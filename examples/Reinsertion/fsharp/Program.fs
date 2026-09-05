open GeneticAlgorithms

// A OneMax-style problem: maximize the number of 1s in a binary chromosome. Deliberately
// simple and large (see numberOfGenes below) so the only thing that determines how well a
// run does is how well it explores and preserves diversity - which is exactly what
// reinsertion controls. A small search space (like the Schedule example's 10 genes) lets
// even a badly shrunk population stumble onto a near-optimal answer by luck, which hides
// the differences between strategies rather than showing them.

let numberOfGenes = 500
let maxFitness = float numberOfGenes

let genotype () =
    let genes = Array.init numberOfGenes (fun _ -> System.Random.Shared.Next(0, 2))

    { Genes = genes
      Fitness = 0.0
      Age = 0 }

let fitness_function (chromosome: Chromosome<int>) = chromosome.Genes |> Array.sum |> float

let lastGeneration = 300

let terminate (_population: seq<Chromosome<int>>) (generation: int) (_temperature: float) =
    generation = lastGeneration

let problem: Problem<int> =
    { Genotype = genotype
      FitnessFunction = fitness_function
      Terminate = terminate }

// SelectionRate leaves 20% of the population unselected as `leftover` each generation -
// that's the pool `elitist` and `uniform` draw survivors from. `pure` ignores that pool
// entirely, so with SelectionRate = 0.8 it loses 20% of the population every generation
// until it settles into a small, mostly-static equilibrium - the point of this example is
// to make that visible next to the other two strategies.
//
// `elitist` and `uniform` are given a survivalRate chosen so that
// SelectionRate + MutationRate + survivalRate = 1.0 (0.8 + 0.05 + 0.15), which keeps their
// population size roughly stable across generations instead of drifting like `pure` does.
let survivalRate = 0.15

let baseOptions: Options<int> =
    { PopulationSize = 100
      SelectionRate = 0.8
      SelectionFn = Selection.elite
      CrossoverFn = Crossover.singlePoint
      MutationRate = 0.05
      MutationFn = Mutation.scramble
      ReinsertionFn = Reinsertion.``pure``
      Probe = Probes.noop }

let strategies: (string * (Chromosome<int> array -> Chromosome<int> array -> Chromosome<int> array -> Chromosome<int> array)) list =
    [ "pure", Reinsertion.``pure``
      "elitist", Reinsertion.elitist survivalRate
      "uniform", Reinsertion.uniform survivalRate ]

// Runs the genetic algorithm once with the given reinsertion strategy, recording the best
// fitness seen at every generation along the way so the three runs can be compared
// side by side afterward.
let runStrategy (name: string, reinsertionFn) =
    let fitnessByGeneration = Array.zeroCreate<float> (lastGeneration + 1)

    let options =
        { baseOptions with
            ReinsertionFn = reinsertionFn
            Probe = fun info -> fitnessByGeneration.[info.Generation] <- info.Best.Fitness }

    let solution = Genetic.run problem options
    name, solution, fitnessByGeneration

let results = strategies |> List.map runStrategy

printfn "Maximum possible fitness: %.0f (all %d genes set to 1)" maxFitness numberOfGenes
printfn ""
printfn "Best fitness by generation (sampled every 30 generations):"
printfn "%10s | %8s | %8s | %8s" "Generation" "pure" "elitist" "uniform"

for generation in 0 .. 30 .. lastGeneration do
    let fitnessAt (_, _, fitnessByGeneration: float array) = fitnessByGeneration.[generation]

    printfn
        "%10d | %8.1f | %8.1f | %8.1f"
        generation
        (fitnessAt results.[0])
        (fitnessAt results.[1])
        (fitnessAt results.[2])

printfn ""
printfn "Final results:"

for name, solution, _ in results do
    printfn
        "%-8s fitness: %5.1f / %.0f (%5.1f%% of maximum)"
        name
        solution.Fitness
        maxFitness
        (100.0 * solution.Fitness / maxFitness)
