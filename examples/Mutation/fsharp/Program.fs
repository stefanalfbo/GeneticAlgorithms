open GeneticAlgorithms

// Single-machine weighted tardiness scheduling: given a fixed set of jobs, each with a
// processing time, due date, and importance weight, find the processing ORDER that
// minimizes total weighted tardiness. Unlike a OneMax-style problem (used by the
// Reinsertion example), this one is genuinely order-dependent - reordering the same set of
// jobs changes which ones finish late and by how much. That's essential here: scramble and
// scrambleSlice only differ in HOW they reorder a chromosome's genes, so an
// order-insensitive fitness function (like OneMax's gene sum) would make them
// indistinguishable no matter how differently they mutate.
let numberOfJobs = 30

// The job data itself (processing times, due dates, weights) is a fixed problem instance,
// not part of what's randomized every run - only the search (initial population,
// selection, crossover, mutation) draws from Options.Random. A dedicated Random with its
// own fixed seed generates this instance once, independently of any run's own randomness.
let private instanceRng = System.Random(42)
let processingTimes = Array.init numberOfJobs (fun _ -> instanceRng.Next(1, 20))
let dueDates = Array.init numberOfJobs (fun _ -> instanceRng.Next(20, 200))
let weights = Array.init numberOfJobs (fun _ -> instanceRng.Next(1, 10))

// A chromosome's genes are a permutation of job indices - the order jobs run on the single
// machine, back to back with no idle time.
let genotype (rng: System.Random) =
    let genes = Array.init numberOfJobs id |> Shuffle.fisherYates rng

    { Genes = genes
      Fitness = 0.0
      Age = 0 }

let totalWeightedTardiness (chromosome: Chromosome<int>) =
    let mutable completionTime = 0
    let mutable total = 0

    for jobIndex in chromosome.Genes do
        completionTime <- completionTime + processingTimes.[jobIndex]
        let tardiness = max 0 (completionTime - dueDates.[jobIndex])
        total <- total + weights.[jobIndex] * tardiness

    total

// The genetic algorithm always maximizes fitness, but total weighted tardiness is a cost
// to minimize - so fitness is simply the negated cost: a perfect, tardiness-free schedule
// has the fitness closest to zero (the largest, least negative value).
let fitnessFunction (chromosome: Chromosome<int>) = -(float (totalWeightedTardiness chromosome))

let lastGeneration = 300

let terminate (_population: seq<Chromosome<int>>) (generation: int) (_temperature: float) =
    generation = lastGeneration

let problem: Problem<int> =
    { Genotype = genotype
      FitnessFunction = fitnessFunction
      Terminate = terminate }

// Genes here are a permutation (every job run exactly once), so CrossoverFn is
// orderOneCrossover rather than the library's default singlePoint - a single-point cut
// would generally produce a schedule with one job missing and another repeated.
// scrambleSlice's window is deliberately small relative to numberOfJobs: the whole point of
// this example is comparing it against scramble's full-chromosome reorder, so a window
// that's a small, local fraction of the chromosome is what makes it "less disruptive" in
// the first place - a window close to numberOfJobs would just behave like scramble.
let scrambleSliceWindow = 5

let baseOptions: Options<int> =
    { Options.create 100 with
        CrossoverFn = Crossover.orderOneCrossover }

let strategies: (string * (System.Random -> Chromosome<int> -> Chromosome<int>)) list =
    [ "scramble", Mutation.scramble
      "scrambleSlice", Mutation.scrambleSlice scrambleSliceWindow ]

// Runs the genetic algorithm once with the given mutation strategy, recording the best
// (least tardy) schedule's cost seen at every generation along the way so the two runs can
// be compared side by side afterward.
let runStrategy (name: string, mutationFn) =
    let costByGeneration = Array.zeroCreate<float> (lastGeneration + 1)

    let options =
        { baseOptions with
            MutationFn = mutationFn
            Probe = fun info -> costByGeneration.[info.Generation] <- -info.Best.Fitness }

    let solution = Genetic.run problem options
    name, solution, costByGeneration

let results = strategies |> List.map runStrategy

printfn "Minimizing total weighted tardiness across %d jobs over %d generations..." numberOfJobs lastGeneration
printfn ""
printfn "Best (lowest) total weighted tardiness by generation (sampled every 30 generations):"
printfn "%10s | %10s | %13s" "Generation" "scramble" "scrambleSlice"

for generation in 0 .. 30 .. lastGeneration do
    let costAt (_, _, costByGeneration: float array) = costByGeneration.[generation]

    printfn "%10d | %10.1f | %13.1f" generation (costAt results.[0]) (costAt results.[1])

printfn ""
printfn "Final results:"

for name, solution, _ in results do
    printfn "%-13s total weighted tardiness: %.1f" name (-solution.Fitness)
