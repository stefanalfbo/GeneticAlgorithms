open GeneticAlgorithms

let target_fitness = 180.0

let genotype (rng: System.Random) =
    let genes = Array.init 10 (fun _ -> rng.Next(1, 11), rng.Next(1, 11))

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
    { Options.create 124 with
        SelectionRate = 1.0
        ReinsertionFn = Reinsertion.``pure``
        Probe = Probes.printProgress }

let solution = Genetic.run problem options

printfn "Best solution: %A (fitness: %f)" solution.Genes solution.Fitness
