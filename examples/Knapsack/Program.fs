open GeneticAlgorithms

let profits = [| 6; 5; 8; 9; 6; 7; 3; 1; 2; 6 |]
let weights = [| 10; 6; 8; 7; 10; 9; 7; 11; 6; 8 |]

let genotype () =
    let genes = Array.init 10 (fun _ -> System.Random.Shared.Next(0, 2))

    { genes = genes
      size = genes.Length
      fitness = 0.0
      age = 0 }

let fitness_function (chromosome: Chromosome<int>) =
    let weight_limit = 40

    let potential_profits =
        chromosome.genes
        |> Array.zip profits
        |> Array.map (fun (gene, profit) -> gene * profit)
        |> Array.sum

    let over_limit =
        chromosome.genes
        |> Array.zip weights
        |> Array.map (fun (gene, weight) -> gene * weight)
        |> Array.sum
        |> fun totalWeight -> totalWeight > weight_limit

    if over_limit then 0.0 else float potential_profits

let terminate _population generation _temperature = generation >= 1000

let problem: Problem<int> =
    { genotype = genotype
      fitness_function = fitness_function
      terminate = terminate }

let options = { population_size = 50 }
let solution = Genetic.run problem options

printfn "Best solution: %A (fitness: %f)" solution.genes solution.fitness
