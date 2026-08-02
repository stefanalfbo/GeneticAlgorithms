namespace GeneticAlgorithms

module Genetic =

    let evaluate population fitness_function =
        population |> Array.sortBy fitness_function

    let select population =
        population
        |> Array.chunkBySize 2
        |> Array.map (fun chunk ->
            match chunk with
            | [| a; b |] -> a, b
            | [| a |] -> a, a
            | _ -> failwith "Invalid chunk")

    let crossover (population: ('T array * 'T array) array) =
        population
        |> Array.collect (fun (p1, p2) ->
            let cxPoint = System.Random.Shared.Next(1, Array.length p1)

            let h1 = p1 |> Array.take cxPoint
            let t1 = p1 |> Array.skip cxPoint

            let h2 = p2 |> Array.take cxPoint
            let t2 = p2 |> Array.skip cxPoint

            [| Array.append h1 t2; Array.append h2 t1 |])

    let mutation (population: int array array) =
        let shuffle xs =
            xs |> Array.sortBy (fun _ -> System.Random.Shared.Next())

        population
        |> Array.map (fun chromosome ->
            if System.Random.Shared.NextDouble() < 0.05 then
                shuffle chromosome
            else
                chromosome)

    let rec evolve fitness_function genotype max_fitness population =
        let next_generation = evaluate population fitness_function

        let best = next_generation.[0]

        printfn "Current Best %d" (fitness_function best)

        if fitness_function best = max_fitness then
            best
        else
            next_generation
            |> select
            |> crossover
            |> mutation
            |> evolve fitness_function genotype max_fitness

    let initialize genotype =
        // Population of 100 individuals, each with 1000 genes (0 or 1)
        Array.init 100 (fun _ -> genotype ())

    let run fitness_function genotype max_fitness =
        let population = initialize genotype

        population |> evolve fitness_function genotype max_fitness
