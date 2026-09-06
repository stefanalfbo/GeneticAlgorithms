open GeneticAlgorithms

// A small, fixed set of cities with illustrative (x, y) coordinates - not real
// geographic positions, just a 2D layout loosely echoing where these cities sit relative
// to one another, good enough to make "the tour crosses back over itself" visibly wrong.
let private cities =
    [| "Stockholm", (65.0, 80.0)
       "Oslo", (55.0, 85.0)
       "Helsinki", (75.0, 82.0)
       "Copenhagen", (58.0, 70.0)
       "Berlin", (60.0, 60.0)
       "Amsterdam", (48.0, 62.0)
       "London", (38.0, 58.0)
       "Paris", (45.0, 50.0)
       "Vienna", (65.0, 55.0)
       "Rome", (58.0, 30.0)
       "Madrid", (20.0, 25.0)
       "Lisbon", (10.0, 25.0) |]

let private cityNames = cities |> Array.map fst
let private coordinates = cities |> Array.map snd
let private numberOfCities = cities.Length

// A chromosome's genes are a permutation of city indices - the order in which the tour
// visits them. Genes.[i] and Genes.[i + 1] are consecutive stops, and the tour is a closed
// loop: the last city connects back to the first.
let genotype () =
    let genes = Array.init numberOfCities id |> Array.sortBy (fun _ -> System.Random.Shared.Next())

    { Genes = genes
      Fitness = 0.0
      Age = 0 }

let tourDistance (chromosome: Chromosome<int>) =
    let order = chromosome.Genes

    [ 0 .. numberOfCities - 1 ]
    |> List.sumBy (fun i ->
        let from = coordinates.[order.[i]]
        let to_ = coordinates.[order.[(i + 1) % numberOfCities]]
        Distance.euclidean from to_)

// The genetic algorithm always maximizes fitness, but a tour's quality is its total
// distance, which we want to minimize - so fitness is simply the negated distance: the
// shortest tour has the fitness closest to zero (the largest, least negative value).
let fitnessFunction (chromosome: Chromosome<int>) = -(tourDistance chromosome)

let lastGeneration = 500

let terminate (_population: seq<Chromosome<int>>) (generation: int) (_temperature: float) =
    generation = lastGeneration

let problem: Problem<int> =
    { Genotype = genotype
      FitnessFunction = fitnessFunction
      Terminate = terminate }

// SelectionRate leaves 20% of the population as leftover each generation; elitist
// reinsertion carries the fittest 15% of (parents + leftover) forward alongside this
// generation's offspring, so 0.8 + 0.05 (MutationRate) + 0.15 keeps the population size
// roughly stable across all 500 generations - the simpler `pure` reinsertion strategy
// would let the population grow without bound over a run this long, since nothing ever
// discards the extra mutants it adds each generation.
//
// CrossoverFn is orderOneCrossover rather than the library's default singlePoint: genes
// here are a permutation (each city visited exactly once), and a single-point cut would
// generally produce a tour with a city missing and another repeated.
let options: Options<int> =
    { PopulationSize = 100
      SelectionRate = 0.8
      SelectionFn = Selection.elite
      CrossoverFn = Crossover.orderOneCrossover
      MutationRate = 0.05
      MutationFn = Mutation.scramble
      ReinsertionFn = Reinsertion.elitist 0.15
      Probe =
        Probes.everyNth 50 (fun info -> printfn "Current best distance: %.2f" (-info.Best.Fitness)) }

let solution = Genetic.run problem options

let tourNames =
    solution.Genes |> Array.map (fun cityIndex -> cityNames.[cityIndex])

printfn ""
printfn "Best tour found (total distance: %.2f):" (tourDistance solution)
printfn "%s -> %s" (String.concat " -> " tourNames) tourNames.[0]
