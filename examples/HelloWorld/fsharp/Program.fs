open GeneticAlgorithms

let target = "helloworld"

let randomChar () =
    char (System.Random.Shared.Next(int 'a', int 'z' + 1))

let genotype () =
    let genes = Array.init target.Length (fun _ -> randomChar ())

    { Genes = genes
      Fitness = 0.0
      Age = 0 }

let fitness_function (chromosome: Chromosome<char>) =
    let matches =
        chromosome.Genes
        |> Array.indexed
        |> Array.filter (fun (i, gene) -> gene = target.[i])
        |> Array.length

    float matches / float target.Length

let terminate (population: seq<Chromosome<char>>) (_generation: int) (_temperature: float) =
    population |> Seq.exists (fun chromosome -> chromosome.Fitness >= 1.0)

let problem: Problem<char> =
    { Genotype = genotype
      FitnessFunction = fitness_function
      Terminate = terminate }

let options =
    { PopulationSize = 100
      SelectionRate = 0.8
      SelectionFn = Selection.elite
      CrossoverFn = Crossover.singlePoint
      MutationRate = 0.05
      OnGeneration = Genetic.printProgress }

let solution = Genetic.run problem options

printfn "Best solution: %s (fitness: %f)" (System.String solution.Genes) solution.Fitness
