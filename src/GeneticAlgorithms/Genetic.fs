namespace GeneticAlgorithms

type Options = { population_size: int }

module Genetic =

    let evaluate population fitness_function (opts: Options) =
        population |> Array.sortBy fitness_function

    let select (opts: Options) population =
        population
        |> Array.chunkBySize 2
        |> Array.map (fun chunk ->
            match chunk with
            | [| a; b |] -> a, b
            | [| a |] -> a, a
            | _ -> failwith "Invalid chunk")

    let crossover (opts: Options) (population: ('T array * 'T array) array) =
        population
        |> Array.collect (fun (p1, p2) ->
            let cxPoint = System.Random.Shared.Next(1, Array.length p1)

            let h1 = p1 |> Array.take cxPoint
            let t1 = p1 |> Array.skip cxPoint

            let h2 = p2 |> Array.take cxPoint
            let t2 = p2 |> Array.skip cxPoint

            [| Array.append h1 t2; Array.append h2 t1 |])

    let mutation (opts: Options) (population: int array array) =
        let shuffle xs =
            xs |> Array.sortBy (fun _ -> System.Random.Shared.Next())

        population
        |> Array.map (fun chromosome ->
            if System.Random.Shared.NextDouble() < 0.05 then
                shuffle chromosome
            else
                chromosome)

    let rec evolve (opts: Options) fitness_function genotype max_fitness population =
        let next_generation = evaluate population fitness_function opts

        let best = next_generation.[0]

        printfn "Current Best %d" (fitness_function best)

        if fitness_function best = max_fitness then
            best
        else
            next_generation
            |> select opts
            |> crossover opts
            |> mutation opts
            |> evolve opts fitness_function genotype max_fitness

    let initialize genotype (opts: Options) =
        Array.init opts.population_size (fun _ -> genotype ())

    let run fitness_function genotype max_fitness (opts: Options) =
        let population = initialize genotype opts

        population |> evolve opts fitness_function genotype max_fitness
