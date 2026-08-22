open GeneticAlgorithms

let target_fitness = 180.0

let genotype () =
    let rnd = System.Random.Shared

    let genes = Array.init 10 (fun _ -> rnd.Next(1, 11), rnd.Next(1, 11))

    { Genes = genes
      Fitness = 0.0
      Age = 0 }

let fitness_function (chromosome: Chromosome<int * int>) =
    chromosome.Genes |> Array.sumBy (fun (roi, risk) -> 2 * roi - risk) |> float

let terminate (population: seq<Chromosome<int * int>>) _generation _temperature =
    let max_value = population |> Seq.maxBy (fun c -> c.Fitness)
    max_value.Fitness >= target_fitness

let problem: Problem<int * int> =
    { Genotype = genotype
      FitnessFunction = fitness_function
      Terminate = terminate }

let options =
    { PopulationSize = 125
      SelectionRate = 0.8
      SelectionFn = Selection.elite
      CrossoverFn = Crossover.singlePoint
      MutationRate = 0.05
      MutationFn = Mutation.shuffle
      OnGeneration = Genetic.printProgress }

let solution = Genetic.run problem options

printfn "Best solution: %A (fitness: %f)" solution.Genes solution.Fitness
