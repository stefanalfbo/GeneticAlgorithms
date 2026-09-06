open GeneticAlgorithms

let max_fitness = 1000.0

let genotype () =
    let genes = Array.init 1000 (fun _ -> System.Random.Shared.Next(0, 2))

    { Genes = genes
      Fitness = 0.0
      Age = 0 }

let fitness_function (chromosome: Chromosome<int>) = chromosome.Genes |> Array.sum |> float

let terminate (population: seq<Chromosome<int>>) (_generation: int) (_temperature: float) =
    population |> Seq.exists (fun chromosome -> chromosome.Fitness >= max_fitness)

let problem: Problem<int> =
    { Genotype = genotype
      FitnessFunction = fitness_function
      Terminate = terminate }

// SelectionRate leaves 20% of the population as leftover each generation; elitist
// reinsertion carries the fittest 15% of (parents + leftover) forward alongside this
// generation's offspring, so 0.8 + 0.05 (MutationRate) + 0.15 keeps the population size
// roughly stable across however many generations it takes to converge. The simpler
// `pure` reinsertion strategy, at the library's default SelectionRate of 1.0, lets the
// population grow without bound every generation instead - fine for a handful of
// generations, but this problem's 1000-gene chromosome can take long enough to reach the
// target that the growth compounds into a population far too large to finish in any
// reasonable time.
let options =
    { PopulationSize = 100
      SelectionRate = 0.8
      SelectionFn = Selection.elite
      CrossoverFn = Crossover.singlePoint
      MutationRate = 0.05
      MutationFn = Mutation.scramble
      ReinsertionFn = Reinsertion.elitist 0.15
      Probe = Probes.printProgress }

let solution = Genetic.run problem options

printfn "Best solution: %A" solution.Fitness
