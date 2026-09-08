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

    /// A chromosome can only ever win a tournament of size <c>tournamentSize</c> if fewer
    /// than <c>population.Length - tournamentSize + 1</c> other chromosomes are strictly
    /// fitter than it - otherwise every tournament it's part of is forced to also include at
    /// least one strictly fitter chromosome (there aren't enough non-fitter chromosomes left
    /// to fill the remaining <c>tournamentSize - 1</c> seats without one), which always wins
    /// instead. This count is unaffected by ties: two chromosomes with equal fitness don't
    /// make each other unreachable, since <c>Array.maxBy</c> can return either one of them
    /// depending on tournament draw order.
    let private isTournamentReachable (tournamentSize: int) (population: Chromosome<'Gene> array) (candidate: Chromosome<'Gene>) =
        let strictlyFitterCount = population |> Array.filter (fun c -> c.Fitness > candidate.Fitness) |> Array.length
        strictlyFitterCount <= population.Length - tournamentSize

    /// <summary>
    /// Like <c>tournament</c>, but keeps running tournaments until <paramref name="n"/>
    /// distinct chromosomes have been selected.
    /// </summary>
    /// <remarks>
    /// Not every chromosome in <paramref name="population"/> can necessarily win a
    /// tournament of size <paramref name="tournamentSize"/> - for example, with
    /// <paramref name="tournamentSize"/> equal to <paramref name="population"/>'s length,
    /// every tournament draws the whole population, so only the single fittest chromosome
    /// can ever win, no matter how many times it's retried. Requesting more distinct
    /// chromosomes than are actually reachable this way would otherwise retry forever, so
    /// this validates <paramref name="n"/> against the number of chromosomes that can
    /// actually win some tournament of the given size, and fails fast instead.
    /// </remarks>
    /// <param name="tournamentSize">The number of chromosomes competing in each tournament.</param>
    /// <param name="population">The population to select from.</param>
    /// <param name="n">The number of distinct chromosomes to select.</param>
    /// <returns><paramref name="n"/> distinct tournament winners.</returns>
    /// <exception cref="System.ArgumentException">
    /// Thrown when <paramref name="n"/> exceeds the number of chromosomes that can actually
    /// win a tournament of size <paramref name="tournamentSize"/> over this population.
    /// </exception>
    let tournamentNoDuplicates (tournamentSize: int) (population: Chromosome<'Gene> array) (n: int) =
        let maxReachable =
            population
            |> Array.distinct
            |> Array.filter (isTournamentReachable tournamentSize population)
            |> Array.length

        if n > maxReachable then
            invalidArg
                (nameof n)
                $"Cannot select {n} distinct chromosomes: with a tournament size of {tournamentSize} over this \
                  population, at most {maxReachable} distinct chromosome(s) can ever win a tournament, so the \
                  retry loop would never terminate."

        let selected = System.Collections.Generic.HashSet<Chromosome<'Gene>>()

        while selected.Count < n do
            let chosen = random population tournamentSize |> Array.maxBy (fun c -> c.Fitness)
            selected.Add chosen |> ignore

        selected |> Seq.toArray

    /// Picks a single chromosome at random, with probability proportional to its weight
    /// (<paramref name="weights"/>.[i] corresponds to <paramref name="population"/>.[i]).
    /// Falls back to a uniform random pick when every weight is <c>0.0</c> - the cumulative
    /// walk below can never find a weight that pushes it past a random draw of exactly
    /// <c>0.0</c>, so without this it would always fall through to and return the very last
    /// chromosome, deterministically, rather than choosing without preference as a total
    /// weight of zero implies.
    let private pickWeighted (population: Chromosome<'Gene> array) (weights: float array) =
        let totalWeight = Array.sum weights

        if totalWeight = 0.0 then
            population.[System.Random.Shared.Next(population.Length)]
        else
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
    /// sensitive to fitness magnitude. If every chromosome has fitness <c>0.0</c> - a valid
    /// state under this function's own contract, e.g. an early generation where every
    /// candidate happens to be infeasible - each pick falls back to uniform random selection
    /// instead of favoring any particular chromosome.
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
    /// <remarks>
    /// Assumes non-negative fitness values. If every chromosome has fitness <c>0.0</c> - a
    /// valid state under this function's own contract, e.g. an early generation where every
    /// candidate happens to be infeasible - every pointer would otherwise sit at the same
    /// zero offset and the walk below would stop advancing at the very first chromosome it
    /// reaches, returning that one chromosome for every pick rather than selecting without
    /// preference. This falls back to <paramref name="n"/> independent uniform random picks
    /// instead in that case.
    /// </remarks>
    /// <param name="population">The population to select from.</param>
    /// <param name="n">The number of chromosomes to select.</param>
    /// <returns><paramref name="n"/> chromosomes, possibly with duplicates.</returns>
    let stochasticUniversalSampling (population: Chromosome<'Gene> array) (n: int) =
        let weights = population |> Array.map (fun c -> c.Fitness)
        let totalWeight = Array.sum weights

        if totalWeight = 0.0 then
            Array.init n (fun _ -> population.[System.Random.Shared.Next(population.Length)])
        else
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
    /// Splits <paramref name="population"/> into parent pairs, the chromosomes consumed as
    /// parents, and leftover chromosomes for one generation: selects
    /// <c>SelectionRate * population.Length</c> chromosomes (rounded up to an even number)
    /// using <c>opts.SelectionFn</c>, pairs them up, and returns whatever wasn't selected as
    /// leftover.
    /// </summary>
    /// <remarks>
    /// The rounded-up count is capped at the largest even number that doesn't exceed
    /// <paramref name="population"/>'s length - without this, a <c>SelectionRate</c> of 1.0
    /// (or population sizes that make rounding land above the population itself) would ask
    /// <c>opts.SelectionFn</c> for more chromosomes than exist, which fails for
    /// implementations like <c>elite</c> that take a fixed slice.
    ///
    /// Chromosomes are compared by value, not by which physical population slot they came
    /// from, which makes "how many chromosomes were actually used as parents" ambiguous
    /// whenever the same value shows up more than once in the raw selection - and that can
    /// happen for two very different reasons. <c>tournament</c>, <c>roulette</c>,
    /// <c>boltzmann</c>, and <c>stochasticUniversalSampling</c> can legitimately redraw the
    /// same individual more than once (independent draws with replacement) - the same
    /// physical individual pairing with two different partners is a normal, intentional part
    /// of tournament/fitness-proportionate selection, and should count as one parent, not
    /// two. But <c>elite</c>'s deterministic top-N slice can just as legitimately select
    /// several genuinely distinct population slots that happen to hold value-identical
    /// chromosomes - unsurprising once a population has substantially converged, since
    /// <c>Reinsertion.elitist</c> is specifically designed to carry the fittest chromosome(s)
    /// forward unchanged generation after generation - and those are separate individuals
    /// that should each count once, not be collapsed into one. A plain
    /// <c>Array.distinct</c> over the raw selection cannot tell these two cases apart, and
    /// picks the wrong answer for the second: undercounting parents shrinks
    /// <c>parents.Length + leftover.Length</c> below <paramref name="population"/>'s size,
    /// which compounds every generation elitism converges the population - this is exactly
    /// what caused <c>OneMaxProblem</c> to hang partway to its target.
    ///
    /// The second element of the returned tuple resolves this by counting, per distinct
    /// value, how many times it was actually selected, then walking
    /// <paramref name="population"/> once and consuming physical slots up to that count per
    /// value (never more than actually exist) - each slot consumed this way becomes one
    /// entry in <c>parents</c>, so a value that legitimately came from several distinct
    /// population slots (the <c>elite</c> case) is represented that many times, while a value
    /// that was redrawn more often than the population physically holds it (the
    /// <c>roulette</c>/<c>tournament</c> case) is capped at how many slots actually exist.
    /// Either way, <c>parents.Length + leftover.Length</c> always equals
    /// <paramref name="population"/>'s length, which is what <c>Reinsertion</c> strategies
    /// that combine the two (e.g. <c>elitist</c>, <c>uniform</c>) rely on. <c>parentPairs</c>
    /// (the first element) is built from the raw, possibly-duplicated selection instead,
    /// since crossover pairing is exactly where a repeated individual is meant to participate
    /// more than once.
    /// </remarks>
    /// <param name="opts">Provides <c>SelectionRate</c> and <c>SelectionFn</c>.</param>
    /// <param name="population">The population to select parents from.</param>
    /// <returns>
    /// A tuple of parent pairs to crossover, the chromosomes consumed as parents, and the
    /// leftover chromosomes that carry over to the next generation unchanged (aside from
    /// mutation).
    /// </returns>
    let select (opts: Options<'Gene>) (population: Chromosome<'Gene> array) =
        let maxN = population.Length - (population.Length % 2)
        let n = int (System.Math.Round(float population.Length * opts.SelectionRate))
        let n = if n % 2 = 0 then n else n + 1
        let n = min n maxN

        let selected = opts.SelectionFn population n

        let remainingBySelection =
            selected
            |> Array.countBy id
            |> Array.map (fun (chromosome, count) -> System.Collections.Generic.KeyValuePair(chromosome, count))
            |> System.Collections.Generic.Dictionary

        let parents = ResizeArray<Chromosome<'Gene>>()
        let leftover = ResizeArray<Chromosome<'Gene>>()

        for chromosome in population do
            match remainingBySelection.TryGetValue chromosome with
            | true, remaining when remaining > 0 ->
                parents.Add chromosome
                remainingBySelection.[chromosome] <- remaining - 1
            | _ -> leftover.Add chromosome

        let parentPairs =
            selected
            |> Array.chunkBySize 2
            |> Array.map (fun chunk ->
                match chunk with
                | [| a; b |] -> a, b
                | _ -> failwith "Invalid chunk size")

        parentPairs, parents.ToArray(), leftover.ToArray()
