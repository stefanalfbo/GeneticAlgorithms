namespace GeneticAlgorithms

module Genetic =

    let printProgress (chromosome: Chromosome<'Gene>) (_generation: int) =
        printfn "Current Best %f" chromosome.Fitness

    let evaluate (population: Chromosome<'Gene> array) (fitnessFunction: Chromosome<'Gene> -> float) =
        population
        |> Array.map (fun chromosome ->
            { chromosome with
                Fitness = fitnessFunction chromosome
                Age = chromosome.Age + 1 })
        |> Array.sortByDescending (fun chromosome -> chromosome.Fitness)

    let crossover
        (crossoverFn: Chromosome<'Gene> -> Chromosome<'Gene> -> Chromosome<'Gene> * Chromosome<'Gene>)
        (population: (Chromosome<'Gene> * Chromosome<'Gene>) array)
        =
        population
        |> Array.collect (fun (p1, p2) ->
            let c1, c2 = crossoverFn p1 p2
            [| c1; c2 |])

    let mutation (opts: Options<'Gene>) (population: Chromosome<'Gene> array) =
        let n = int (float population.Length * opts.MutationRate)

        population
        |> Array.sortBy (fun _ -> System.Random.Shared.Next())
        |> Array.take n
        |> Array.map opts.MutationFn

    let rec evolve
        (opts: Options<'Gene>)
        (problem: Problem<'Gene>)
        (generation: int)
        (lastMaxFitness: float)
        (temperature: float)
        (population: Chromosome<'Gene> array)
        =
        let nextPopulation = evaluate population problem.FitnessFunction

        let best = nextPopulation.[0]
        let newTemperature = 0.8 * (temperature + (best.Fitness - lastMaxFitness))

        opts.OnGeneration best generation

        if problem.Terminate nextPopulation generation newTemperature then
            best
        else
            let parentPairs, leftover = Selection.select opts nextPopulation
            let children = crossover opts.CrossoverFn parentPairs
            let mutants = mutation opts nextPopulation
            let parents = parentPairs |> Array.collect (fun (p1, p2) -> [| p1; p2 |])

            opts.ReinsertionFn parents (Array.append children mutants) leftover
            |> evolve opts problem (generation + 1) best.Fitness newTemperature

    let initialize genotype (opts: Options<'Gene>) =
        Array.init opts.PopulationSize (fun _ -> genotype ())

    let run (problem: Problem<'Gene>) (opts: Options<'Gene>) =
        let population = initialize problem.Genotype opts
        let firstGeneration = 0
        let temperature = 0.0
        let firstMaxFitness = 0.0

        population |> evolve opts problem firstGeneration firstMaxFitness temperature
