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

    let roulette (population: Chromosome<'Gene> array) (n: int) =
        let sumFitness = population |> Array.sumBy (fun c -> c.Fitness)

        let pick () =
            let u = System.Random.Shared.NextDouble() * sumFitness

            let rec loop sum i =
                if i >= population.Length - 1 then
                    population.[population.Length - 1]
                else
                    let c = population.[i]

                    if c.Fitness + sum > u then
                        c
                    else
                        loop (sum + c.Fitness) (i + 1)

            loop 0.0 0

        Array.init n (fun _ -> pick ())

    // TODO: Implement other selection methods:
    // - Boltzmann selection: selection according to a “temperature” function.
    // - Stochastic universal sampling: selection at evenly spaced intervals.
    // - Rank selection: selection based on “rank” in the population.

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
