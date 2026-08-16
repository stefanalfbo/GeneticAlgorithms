open GeneticAlgorithms

let profits = [| 6; 5; 8; 9; 6; 7; 3; 1; 2; 6 |]
let weights = [| 10; 6; 8; 7; 10; 9; 7; 11; 6; 8 |]

let genotype () =
    let genes = Array.init 10 (fun _ -> System.Random.Shared.Next(0, 2))

    { Genes = genes
      Fitness = 0.0
      Age = 0 }

let fitness_function (chromosome: Chromosome<int>) =
    let weight_limit = 40

    let potential_profits =
        chromosome.Genes
        |> Array.zip profits
        |> Array.map (fun (gene, profit) -> gene * profit)
        |> Array.sum

    let over_limit =
        chromosome.Genes
        |> Array.zip weights
        |> Array.map (fun (gene, weight) -> gene * weight)
        |> Array.sum
        |> fun totalWeight -> totalWeight > weight_limit

    if over_limit then 0.0 else float potential_profits

let terminate _population generation _temperature = generation >= 1000

let problem: Problem<int> =
    { Genotype = genotype
      FitnessFunction = fitness_function
      Terminate = terminate }

let options =
    { PopulationSize = 50
      SelectionRate = 0.8
      SelectionFn = Selection.elite
      MutationRate = 0.05
      OnGeneration = Genetic.printProgress }

let solution = Genetic.run problem options

printfn "Best solution: %A (fitness: %f)" solution.Genes solution.Fitness
