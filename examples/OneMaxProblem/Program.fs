open GeneticAlgorithms

let max_fitness = 1000.0

let genotype () =
    let genes = Array.init 1000 (fun _ -> System.Random.Shared.Next(0, 2))

    { genes = genes
      size = genes.Length
      fitness = 0.0
      age = 0 }

let fitness_function (chromosome: Chromosome<int>) = chromosome.genes |> Array.sum |> float

let terminate (population: seq<Chromosome<int>>) (_generation: int) =
    population |> Seq.exists (fun chromosome -> chromosome.fitness >= max_fitness)

let problem: Problem<int> =
    { genotype = genotype
      fitness_function = fitness_function
      terminate = terminate }

let options = { population_size = 100 }

let solution = Genetic.run problem options

printfn "Best solution: %A" solution.fitness
