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

let options =
    { PopulationSize = 100
      SelectionRate = 1.0
      SelectionFn = Selection.elite
      CrossoverFn = Crossover.singlePoint
      MutationRate = 0.05
      MutationFn = Mutation.scramble
      ReinsertionFn = Reinsertion.``pure``
      Probe = Probes.printProgress }

let solution = Genetic.run problem options

printfn "Best solution: %A" solution.Fitness
