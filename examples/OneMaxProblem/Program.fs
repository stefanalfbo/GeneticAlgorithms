// Population of 100 individuals, each with 1000 genes (0 or 1)
let population =
    [ for _ in 1..100 -> [ for _ in 1..1000 -> System.Random().Next(0, 2) ] ]

let evaluate population = List.sortBy List.sum population

let selection population =
    population
    |> List.chunkBySize 2
    |> List.map (fun chunk ->
        match chunk with
        | [ a; b ] -> a, b
        | [ a ] -> a, a
        | _ -> failwith "Invalid chunk")

let crossover (population: ('T list * 'T list) list) =
    population
    |> List.collect (fun (p1, p2) ->
        let cxPoint = System.Random.Shared.Next(1, 1001)

        let h1 = p1 |> List.take cxPoint
        let t1 = p1 |> List.skip cxPoint

        let h2 = p2 |> List.take cxPoint
        let t2 = p2 |> List.skip cxPoint

        [ h1 @ t2; h2 @ t1 ])

let mutation (population: int list list) =
    let shuffle xs =
        xs |> List.sortBy (fun _ -> System.Random.Shared.Next())

    population
    |> List.map (fun chromosome ->
        if System.Random.Shared.NextDouble() < 0.05 then
            shuffle chromosome
        else
            chromosome)

let rec algorithm population =
    let best = List.maxBy List.sum population

    printfn "Best solution so far: %A" (List.sum best)

    if List.sum best = 1000 then
        best
    else
        population |> evaluate |> selection |> crossover |> mutation |> algorithm

let solution = algorithm population

printfn "Best solution: %A" (List.sum solution)
