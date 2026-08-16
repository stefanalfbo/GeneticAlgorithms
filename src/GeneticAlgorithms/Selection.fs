namespace GeneticAlgorithms

module Selection =

    let elite (population: Chromosome<'Gene> array) (n: int) = population |> Array.take n

    let random (population: Chromosome<'Gene> array) (n: int) =
        population
        |> Array.sortBy (fun _ -> System.Random.Shared.Next())
        |> Array.take n

    let tournament (tournamentSize: int) (population: Chromosome<'Gene> array) (n: int) =
        Array.init n (fun _ -> random population tournamentSize |> Array.maxBy (fun c -> c.Fitness))

    let tournamentNoDuplicates (tournamentSize: int) (population: Chromosome<'Gene> array) (n: int) =
        let selected = System.Collections.Generic.HashSet<Chromosome<'Gene>>()

        while selected.Count < n do
            let chosen = random population tournamentSize |> Array.maxBy (fun c -> c.Fitness)
            selected.Add chosen |> ignore

        selected |> Seq.toArray

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

    let roulette (population: Chromosome<'Gene> array) (n: int) =
        let weights = population |> Array.map (fun c -> c.Fitness)
        Array.init n (fun _ -> pickWeighted population weights)

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

    let rank (population: Chromosome<'Gene> array) (n: int) =
        // Weight by position in fitness order (1 for the worst, N for the best) rather
        // than by raw fitness value, so a single extreme fitness outlier can't dominate
        // selection the way it would with fitness-proportionate methods like roulette.
        let ranked = population |> Array.sortBy (fun c -> c.Fitness)
        let weights = Array.init ranked.Length (fun i -> float (i + 1))
        Array.init n (fun _ -> pickWeighted ranked weights)

    let select (opts: Options<'Gene>) (population: Chromosome<'Gene> array) =
        let n = int (System.Math.Round(float population.Length * opts.SelectionRate))
        let n = if n % 2 = 0 then n else n + 1

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
