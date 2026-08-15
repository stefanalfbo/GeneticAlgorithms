namespace GeneticAlgorithms

module Genetic =

    let evaluate (population: Chromosome<'Gene> array) (fitnessFunction: Chromosome<'Gene> -> float) (_opts: Options<'Gene>) =
        population
        |> Array.map (fun chromosome ->
            { chromosome with
                fitness = fitnessFunction chromosome
                age = chromosome.age + 1 })
        |> Array.sortByDescending (fun chromosome -> chromosome.fitness)

    let crossover (opts: Options<'Gene>) (population: (Chromosome<'Gene> * Chromosome<'Gene>) array) =
        population
        |> Array.collect (fun (p1, p2) ->
            let cxPoint = System.Random.Shared.Next(1, p1.genes.Length)

            let h1 = p1.genes |> Array.take cxPoint
            let t1 = p1.genes |> Array.skip cxPoint

            let h2 = p2.genes |> Array.take cxPoint
            let t2 = p2.genes |> Array.skip cxPoint

            [| { p1 with genes = Array.append h1 t2 }
               { p2 with genes = Array.append h2 t1 } |])

    let mutation (opts: Options<'Gene>) (population: Chromosome<'Gene> array) =
        let shuffle xs =
            xs |> Array.sortBy (fun _ -> System.Random.Shared.Next())

        population
        |> Array.map (fun chromosome ->
            if System.Random.Shared.NextDouble() < 0.05 then
                { chromosome with
                    genes = shuffle chromosome.genes }
            else
                chromosome)

    let rec evolve
        (opts: Options<'Gene>)
        (problem: Problem<'Gene>)
        (generation: int)
        (last_max_fitness: float)
        (temperature: float)
        (population: Chromosome<'Gene> array)
        =
        let next_population = evaluate population problem.fitness_function opts

        let best = next_population.[0]
        let new_temperature = 0.8 * (temperature + (best.fitness - last_max_fitness))

        printfn "Current Best %f" (problem.fitness_function best)

        if problem.terminate next_population generation new_temperature then
            best
        else
            let parents, leftover = Selection.select opts next_population
            let children = crossover opts parents

            Array.append children leftover
            |> mutation opts
            |> evolve opts problem (generation + 1) best.fitness new_temperature

    let initialize genotype (opts: Options<'Gene>) =
        Array.init opts.population_size (fun _ -> genotype ())

    let run (problem: Problem<'Gene>) (opts: Options<'Gene>) =
        let population = initialize problem.genotype opts
        let first_generation = 0
        let temperature = 0
        let first_max_fitness = 0.0

        population |> evolve opts problem first_generation first_max_fitness temperature
