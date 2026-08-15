namespace GeneticAlgorithms

module Selection =

    let elite (population: Chromosome<'Gene> array) (n: int) = population |> Array.take n

    let random (population: Chromosome<'Gene> array) (n: int) =
        population
        |> Array.sortBy (fun _ -> System.Random.Shared.Next())
        |> Array.take n

    let tournament (tournament_size: int) (population: Chromosome<'Gene> array) (n: int) =
        Array.init n (fun _ -> random population tournament_size |> Array.maxBy (fun c -> c.fitness))

    let tournamentNoDuplicates (tournament_size: int) (population: Chromosome<'Gene> array) (n: int) =
        let selected = System.Collections.Generic.HashSet<Chromosome<'Gene>>()

        while selected.Count < n do
            let chosen = random population tournament_size |> Array.maxBy (fun c -> c.fitness)
            selected.Add chosen |> ignore

        selected |> Seq.toArray

    let roulette (population: Chromosome<'Gene> array) (n: int) =
        let sumFitness = population |> Array.sumBy (fun c -> c.fitness)

        let pick () =
            let u = System.Random.Shared.NextDouble() * sumFitness

            let rec loop sum i =
                if i >= population.Length - 1 then
                    population.[population.Length - 1]
                else
                    let c = population.[i]

                    if c.fitness + sum > u then
                        c
                    else
                        loop (sum + c.fitness) (i + 1)

            loop 0.0 0

        Array.init n (fun _ -> pick ())

    // TODO: Implement other selection methods:
    // - Boltzmann selection: selection according to a “temperature” function.
    // - Stochastic universal sampling: selection at evenly spaced intervals.
    // - Rank selection: selection based on “rank” in the population.

    let select (opts: Options<'Gene>) (population: Chromosome<'Gene> array) =
        let n = int (System.Math.Round(float population.Length * opts.selection_rate))
        let n = if n % 2 = 0 then n else n + 1

        let parents = opts.selection_fn population n
        let leftover = population |> Seq.except parents |> Seq.toArray

        let parentPairs =
            parents
            |> Array.chunkBySize 2
            |> Array.map (fun chunk ->
                match chunk with
                | [| a; b |] -> a, b
                | _ -> failwith "Invalid chunk size")

        parentPairs, leftover
