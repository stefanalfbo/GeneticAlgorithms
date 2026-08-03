open GeneticAlgorithms

let target = "helloworld"

let randomChar () =
    char (System.Random.Shared.Next(int 'a', int 'z' + 1))

let genotype () =
    let genes = Array.init target.Length (fun _ -> randomChar ())

    { genes = genes
      size = genes.Length
      fitness = 0.0
      age = 0 }

let jaro_similarity (source: string) (target: string) =
    if source = target then
        1.0
    elif System.String.IsNullOrEmpty source || System.String.IsNullOrEmpty target then
        0.0
    else
        let matchDistance = max 0 (max source.Length target.Length / 2 - 1)

        let sourceMatches = Array.zeroCreate<bool> source.Length
        let targetMatches = Array.zeroCreate<bool> target.Length

        for i in 0 .. source.Length - 1 do
            let startIndex = max 0 (i - matchDistance)
            let endIndex = min target.Length (i + matchDistance + 1)
            let mutable j = startIndex
            let mutable found = false

            while j < endIndex && not found do
                if not targetMatches.[j] && source.[i] = target.[j] then
                    sourceMatches.[i] <- true
                    targetMatches.[j] <- true
                    found <- true

                j <- j + 1

        let sourceChars =
            [| for i in 0 .. source.Length - 1 do
                   if sourceMatches.[i] then
                       yield source.[i] |]

        let targetChars =
            [| for i in 0 .. target.Length - 1 do
                   if targetMatches.[i] then
                       yield target.[i] |]

        let matches = float sourceChars.Length

        if matches = 0.0 then
            0.0
        else
            let transpositions =
                Array.zip sourceChars targetChars
                |> Array.sumBy (fun (left, right) -> if left <> right then 1 else 0)
                |> fun count -> float count / 2.0

            (matches / float source.Length
             + matches / float target.Length
             + (matches - transpositions) / matches)
            / 3.0

let fitness_function (chromosome: Chromosome<char>) =
    let guess = System.String chromosome.genes

    jaro_similarity guess target

let terminate (population: seq<Chromosome<char>>) =
    population |> Seq.exists (fun chromosome -> chromosome.fitness > 0.95)

let problem: Problem<char> =
    { genotype = genotype
      fitness_function = fitness_function
      terminate = terminate }

let options = { population_size = 100 }

let solution = Genetic.run problem options

printfn "Best solution: %s (fitness: %f)" (System.String solution.genes) solution.fitness
