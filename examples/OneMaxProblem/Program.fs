let genotype _ =
    Array.init 1000 (fun _ -> System.Random.Shared.Next(0, 2))

open GeneticAlgorithms

let fitness_function (chromosome: int array) = Array.sum chromosome

let max_fitness = 1000

let options = { population_size = 100 }

let solution = Genetic.run fitness_function genotype max_fitness options

printfn "Best solution: %A" (fitness_function solution)
