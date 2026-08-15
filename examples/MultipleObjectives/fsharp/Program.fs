open GeneticAlgorithms

let target_fitness = 180.0

let genotype () =
    let rnd = System.Random.Shared

    let genes = Array.init 10 (fun _ -> rnd.Next(1, 11), rnd.Next(1, 11))

    { genes = genes
      size = genes.Length
      fitness = 0.0
      age = 0 }

let fitness_function (chromosome: Chromosome<int * int>) =
    chromosome.genes |> Array.sumBy (fun (roi, risk) -> 2 * roi - risk) |> float

let terminate (population: seq<Chromosome<int * int>>) _generation _temperature =
    let max_value = population |> Seq.maxBy (fun c -> c.fitness)
    max_value.fitness >= target_fitness

let problem: Problem<int * int> =
    { genotype = genotype
      fitness_function = fitness_function
      terminate = terminate }

let options = { population_size = 125; selection_rate = 0.8; selection_fn = Selection.elite }
let solution = Genetic.run problem options

printfn "Best solution: %A (fitness: %f)" solution.genes solution.fitness
