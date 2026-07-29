// Population of 100 individuals, each with 1000 genes (0 or 1)
let population =
    Array.init 100 (fun _ -> Array.init 1000 (fun _ -> System.Random.Shared.Next(0, 2)))

let evaluate population = Array.sortBy Array.sum population

let selection population =
    population
    |> Array.chunkBySize 2
    |> Array.map (fun chunk ->
        match chunk with
        | [| a; b |] -> a, b
        | [| a |] -> a, a
        | _ -> failwith "Invalid chunk")

let crossover (population: ('T array * 'T array) array) =
    population
    |> Array.collect (fun (p1, p2) ->
        let cxPoint = System.Random.Shared.Next(1, 1001)

        let h1 = p1 |> Array.take cxPoint
        let t1 = p1 |> Array.skip cxPoint

        let h2 = p2 |> Array.take cxPoint
        let t2 = p2 |> Array.skip cxPoint

        [| Array.append h1 t2; Array.append h2 t1 |])

let mutation (population: int array array) =
    let shuffle xs =
        xs |> Array.sortBy (fun _ -> System.Random.Shared.Next())

    population
    |> Array.map (fun chromosome ->
        if System.Random.Shared.NextDouble() < 0.05 then
            shuffle chromosome
        else
            chromosome)

let rec algorithm population =
    let best = Array.maxBy Array.sum population

    printfn "Best solution so far: %A" (Array.sum best)

    if Array.sum best = 1000 then
        best
    else
        population |> evaluate |> selection |> crossover |> mutation |> algorithm

let solution = algorithm population

printfn "Best solution: %A" (Array.sum solution)
