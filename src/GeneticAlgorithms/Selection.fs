namespace GeneticAlgorithms

module Selection =

    let elite (population: Chromosome<'Gene> array) (n: int) =
        population |> Array.take n

    let random (population: Chromosome<'Gene> array) (n: int) =
        population
        |> Array.sortBy (fun _ -> System.Random.Shared.Next())
        |> Array.take n

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

