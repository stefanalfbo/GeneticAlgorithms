namespace GeneticAlgorithms

module Genetic =

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
        |> Shuffle.fisherYates opts.Random
        |> Array.take n
        |> Array.map (opts.MutationFn opts.Random)

    /// Selection, mutation, and reinsertion each round their own fractional share of
    /// PopulationSize independently, so their combined output can drift a chromosome or two
    /// away from PopulationSize even when the configured rates are meant to sum to 1.0 (see
    /// Reinsertion.elitist's remarks). This corrects that drift back to exactly
    /// PopulationSize every generation: a shortfall is padded with freshly generated
    /// genotypes, the same mechanism `initialize` uses for the first generation, and any
    /// surplus is truncated.
    let private resizeToPopulationSize
        (opts: Options<'Gene>)
        (problem: Problem<'Gene>)
        (population: Chromosome<'Gene> array)
        =
        let diff = opts.PopulationSize - population.Length

        if diff > 0 then
            Array.append population (Array.init diff (fun _ -> problem.Genotype opts.Random))
        elif diff < 0 then
            Array.truncate opts.PopulationSize population
        else
            population

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

        opts.Probe
            { Generation = generation
              Population = nextPopulation
              Best = best
              Temperature = newTemperature }

        if problem.Terminate nextPopulation generation newTemperature then
            best
        else
            let parentPairs, parents, leftover = Selection.select opts nextPopulation
            let children = crossover (opts.CrossoverFn opts.Random) parentPairs
            let mutants = mutation opts nextPopulation

            opts.ReinsertionFn opts.Random parents (Array.append children mutants) leftover
            |> resizeToPopulationSize opts problem
            |> evolve opts problem (generation + 1) best.Fitness newTemperature

    let initialize genotype (opts: Options<'Gene>) =
        Array.init opts.PopulationSize (fun _ -> genotype opts.Random)

    let run (problem: Problem<'Gene>) (opts: Options<'Gene>) =
        if opts.PopulationSize <= 0 then
            invalidArg (nameof opts.PopulationSize) $"PopulationSize must be positive; got {opts.PopulationSize}."

        if isNull opts.Random then
            invalidArg (nameof opts.Random) "Random must not be null."

        Validation.rate (nameof opts.SelectionRate) opts.SelectionRate
        Validation.rate (nameof opts.MutationRate) opts.MutationRate

        let population = initialize problem.Genotype opts
        let firstGeneration = 0
        let temperature = 0.0
        let firstMaxFitness = 0.0

        population |> evolve opts problem firstGeneration firstMaxFitness temperature
