namespace GeneticAlgorithms

/// <summary>
/// Parent-selection strategies for a genetic algorithm. Every selection function has the
/// shape <c>Chromosome&lt;'Gene&gt; array -&gt; int -&gt; Chromosome&lt;'Gene&gt; array</c>
/// (population, then the number of chromosomes to select), possibly after currying an
/// extra leading parameter such as <c>tournamentSize</c> or <c>temperature</c> - so any of
/// them can be plugged in as <c>Options.SelectionFn</c>.
/// </summary>
module Selection =

    /// <summary>
    /// Selects the fittest <paramref name="n"/> chromosomes from the population.
    /// </summary>
    /// <remarks>
    /// Assumes <paramref name="population"/> is already sorted by descending fitness (as it
    /// is when produced by <c>Genetic.evaluate</c>); it simply takes the first
    /// <paramref name="n"/> elements without checking fitness itself.
    /// </remarks>
    /// <param name="population">The population to select from, sorted by descending fitness.</param>
    /// <param name="n">The number of chromosomes to select.</param>
    /// <returns>The <paramref name="n"/> fittest chromosomes, in their original order.</returns>
    let elite (population: Chromosome<'Gene> array) (n: int) = population |> Array.take n

    /// <summary>
    /// Selects <paramref name="n"/> chromosomes uniformly at random, without regard to fitness.
    /// </summary>
    /// <param name="population">The population to select from.</param>
    /// <param name="n">The number of chromosomes to select.</param>
    /// <returns><paramref name="n"/> randomly chosen chromosomes.</returns>
    let random (population: Chromosome<'Gene> array) (n: int) =
        population
        |> Array.sortBy (fun _ -> System.Random.Shared.Next())
        |> Array.take n

    /// <summary>
    /// Selects <paramref name="n"/> chromosomes by running <paramref name="n"/> independent
    /// tournaments: each tournament draws <paramref name="tournamentSize"/> chromosomes at
    /// random and keeps the fittest one.
    /// </summary>
    /// <remarks>
    /// Because tournaments are independent, the same chromosome can be selected more than
    /// once. Curry <paramref name="tournamentSize"/> (e.g. <c>Selection.tournament 3</c>) to
    /// use this as an <c>Options.SelectionFn</c>.
    /// </remarks>
    /// <param name="tournamentSize">The number of chromosomes competing in each tournament.</param>
    /// <param name="population">The population to select from.</param>
    /// <param name="n">The number of chromosomes to select.</param>
    /// <returns><paramref name="n"/> tournament winners, possibly with duplicates.</returns>
    let tournament (tournamentSize: int) (population: Chromosome<'Gene> array) (n: int) =
        Array.init n (fun _ -> random population tournamentSize |> Array.maxBy (fun c -> c.Fitness))

    /// <summary>
    /// Like <c>tournament</c>, but keeps running tournaments until <paramref name="n"/>
    /// distinct chromosomes have been selected.
    /// </summary>
    /// <remarks>
    /// Can loop indefinitely if <paramref name="n"/> exceeds the number of distinct
    /// chromosomes reachable through repeated tournaments of the given size - for example,
    /// if the population itself contains fewer than <paramref name="n"/> distinct
    /// chromosomes.
    /// </remarks>
    /// <param name="tournamentSize">The number of chromosomes competing in each tournament.</param>
    /// <param name="population">The population to select from.</param>
    /// <param name="n">The number of distinct chromosomes to select.</param>
    /// <returns><paramref name="n"/> distinct tournament winners.</returns>
    let tournamentNoDuplicates (tournamentSize: int) (population: Chromosome<'Gene> array) (n: int) =
        let selected = System.Collections.Generic.HashSet<Chromosome<'Gene>>()

        while selected.Count < n do
            let chosen = random population tournamentSize |> Array.maxBy (fun c -> c.Fitness)
            selected.Add chosen |> ignore

        selected |> Seq.toArray

    /// Picks a single chromosome at random, with probability proportional to its weight
    /// (<paramref name="weights"/>.[i] corresponds to <paramref name="population"/>.[i]).
    let private pickWeighted (population: Chromosome<'Gene> array) (weights: float array) =
        let totalWeight = Array.sum weights
        let u = System.Random.Shared.NextDouble() * totalWeight

        let rec loop sum i =
            if i >= population.Length - 1 then
                population.[population.Length - 1]
            else
                let w = weights.[i]

                if w + sum > u then
                    population.[i]
                else
                    loop (sum + w) (i + 1)

        loop 0.0 0

    /// <summary>
    /// Selects <paramref name="n"/> chromosomes using fitness-proportionate ("roulette
    /// wheel") selection: each pick is independent, with probability proportional to
    /// fitness.
    /// </summary>
    /// <remarks>
    /// Assumes non-negative fitness values. A single much-fitter chromosome can dominate
    /// selection; see <c>rank</c> or <c>boltzmann</c> for alternatives that are less
    /// sensitive to fitness magnitude.
    /// </remarks>
    /// <param name="population">The population to select from.</param>
    /// <param name="n">The number of chromosomes to select.</param>
    /// <returns><paramref name="n"/> chromosomes, possibly with duplicates.</returns>
    let roulette (population: Chromosome<'Gene> array) (n: int) =
        let weights = population |> Array.map (fun c -> c.Fitness)
        Array.init n (fun _ -> pickWeighted population weights)

    /// <summary>
    /// Selects <paramref name="n"/> chromosomes using Boltzmann selection: like
    /// <c>roulette</c>, but weighted by <c>exp(fitness / temperature)</c> instead of raw
    /// fitness. Lower temperatures sharpen the bias toward fitter chromosomes; higher
    /// temperatures flatten it toward uniform random selection.
    /// </summary>
    /// <remarks>
    /// Weights are computed as <c>exp((fitness - maxFitness) / temperature)</c> rather than
    /// <c>exp(fitness / temperature)</c> directly. Both give identical selection
    /// probabilities, but shifting by the population's max fitness first keeps every
    /// exponent &lt;= 0, which avoids overflowing to <c>Infinity</c> for large fitness
    /// values or low temperatures. Curry <paramref name="temperature"/> (e.g.
    /// <c>Selection.boltzmann 1.0</c>) to use this as an <c>Options.SelectionFn</c>.
    /// </remarks>
    /// <param name="temperature">Controls selection pressure. Must be positive.</param>
    /// <param name="population">The population to select from.</param>
    /// <param name="n">The number of chromosomes to select.</param>
    /// <returns><paramref name="n"/> chromosomes, possibly with duplicates.</returns>
    /// <exception cref="System.ArgumentException">
    /// Thrown when <paramref name="temperature"/> is not positive.
    /// </exception>
    let boltzmann (temperature: float) (population: Chromosome<'Gene> array) (n: int) =
        if temperature <= 0.0 then
            invalidArg "temperature" "Temperature must be positive."

        // Subtract the max fitness before exponentiating so every exponent is <= 0.
        // This keeps exp(...) within (0, 1] regardless of fitness/temperature magnitude,
        // avoiding an overflow to Infinity, while leaving the selection probabilities
        // identical to the unshifted computation.
        let maxFitness = population |> Array.map (fun c -> c.Fitness) |> Array.max

        let weights =
            population |> Array.map (fun c -> exp ((c.Fitness - maxFitness) / temperature))

        Array.init n (fun _ -> pickWeighted population weights)

    /// <summary>
    /// Selects <paramref name="n"/> chromosomes using stochastic universal sampling: a
    /// single random offset plus <paramref name="n"/> evenly spaced pointers are walked
    /// across the population once, so each chromosome's selection count tracks its fitness
    /// share far more tightly than independent draws (as in <c>roulette</c>) would.
    /// </summary>
    /// <param name="population">The population to select from.</param>
    /// <param name="n">The number of chromosomes to select.</param>
    /// <returns><paramref name="n"/> chromosomes, possibly with duplicates.</returns>
    let stochasticUniversalSampling (population: Chromosome<'Gene> array) (n: int) =
        let weights = population |> Array.map (fun c -> c.Fitness)
        let totalWeight = Array.sum weights
        let pointerDistance = totalWeight / float n
        let start = System.Random.Shared.NextDouble() * pointerDistance

        let selected = ResizeArray<Chromosome<'Gene>>(n)
        let mutable sum = weights.[0]
        let mutable i = 0

        for j in 0 .. n - 1 do
            let pointer = start + float j * pointerDistance

            while sum <= pointer && i < population.Length - 1 do
                i <- i + 1
                sum <- sum + weights.[i]

            selected.Add population.[i]

        selected.ToArray()

    /// <summary>
    /// Selects <paramref name="n"/> chromosomes weighted by rank (1 for the worst, N for
    /// the best) rather than by raw fitness, so a single extreme fitness outlier can't
    /// dominate selection the way it would with fitness-proportionate methods like
    /// <c>roulette</c>.
    /// </summary>
    /// <remarks>
    /// Sorts <paramref name="population"/> by fitness internally, so unlike <c>elite</c> it
    /// does not require the caller to have already sorted it.
    /// </remarks>
    /// <param name="population">The population to select from.</param>
    /// <param name="n">The number of chromosomes to select.</param>
    /// <returns><paramref name="n"/> chromosomes, possibly with duplicates.</returns>
    let rank (population: Chromosome<'Gene> array) (n: int) =
        let ranked = population |> Array.sortBy (fun c -> c.Fitness)
        let weights = Array.init ranked.Length (fun i -> float (i + 1))
        Array.init n (fun _ -> pickWeighted ranked weights)

    /// <summary>
    /// Splits <paramref name="population"/> into parent pairs and leftover chromosomes for
    /// one generation: selects <c>SelectionRate * population.Length</c> chromosomes
    /// (rounded up to an even number) using <c>opts.SelectionFn</c>, pairs them up, and
    /// returns whatever wasn't selected as leftover.
    /// </summary>
    /// <remarks>
    /// The rounded-up count is capped at the largest even number that doesn't exceed
    /// <paramref name="population"/>'s length - without this, a <c>SelectionRate</c> of 1.0
    /// (or population sizes that make rounding land above the population itself) would ask
    /// <c>opts.SelectionFn</c> for more chromosomes than exist, which fails for
    /// implementations like <c>elite</c> that take a fixed slice.
    /// </remarks>
    /// <param name="opts">Provides <c>SelectionRate</c> and <c>SelectionFn</c>.</param>
    /// <param name="population">The population to select parents from.</param>
    /// <returns>
    /// A tuple of parent pairs to crossover, and the leftover chromosomes that carry over
    /// to the next generation unchanged (aside from mutation).
    /// </returns>
    let select (opts: Options<'Gene>) (population: Chromosome<'Gene> array) =
        let maxN = population.Length - (population.Length % 2)
        let n = int (System.Math.Round(float population.Length * opts.SelectionRate))
        let n = if n % 2 = 0 then n else n + 1
        let n = min n maxN

        let parents = opts.SelectionFn population n
        let leftover = population |> Seq.except parents |> Seq.toArray

        let parentPairs =
            parents
            |> Array.chunkBySize 2
            |> Array.map (fun chunk ->
                match chunk with
                | [| a; b |] -> a, b
                | _ -> failwith "Invalid chunk size")

        parentPairs, leftover
