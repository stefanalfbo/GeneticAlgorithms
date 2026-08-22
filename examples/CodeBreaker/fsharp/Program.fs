open System
open GeneticAlgorithms

let target = "ILoveGeneticAlgorithms"
let encrypted = "LIjs`B`k`qlfDibjwlqmhv"

let cipher (text: string) key =
    text
    |> Seq.map (fun character -> char ((int character ^^^ key) % 32768))
    |> Seq.toArray
    |> String

let chromosomeKey (chromosome: Chromosome<int>) =
    chromosome.Genes
    |> Array.fold (fun key gene -> (key <<< 1) ||| gene) 0

let jaroDistance (left: string) (right: string) =
    if left = right then
        1.0
    elif String.IsNullOrEmpty(left) || String.IsNullOrEmpty(right) then
        0.0
    else
        let matchDistance = max (max left.Length right.Length / 2 - 1) 0
        let leftMatches = Array.create left.Length false
        let rightMatches = Array.create right.Length false

        for leftIndex in 0 .. left.Length - 1 do
            let firstRightIndex = max 0 (leftIndex - matchDistance)
            let lastRightIndex = min (leftIndex + matchDistance) (right.Length - 1)
            let mutable rightIndex = firstRightIndex
            let mutable matched = false

            while rightIndex <= lastRightIndex && not matched do
                if not rightMatches.[rightIndex] && left.[leftIndex] = right.[rightIndex] then
                    leftMatches.[leftIndex] <- true
                    rightMatches.[rightIndex] <- true
                    matched <- true

                rightIndex <- rightIndex + 1

        let matchedLeft =
            left
            |> Seq.mapi (fun index character -> index, character)
            |> Seq.choose (fun (index, character) ->
                if leftMatches.[index] then Some character else None)
            |> Seq.toArray

        let matchedRight =
            right
            |> Seq.mapi (fun index character -> index, character)
            |> Seq.choose (fun (index, character) ->
                if rightMatches.[index] then Some character else None)
            |> Seq.toArray

        let matches = matchedLeft.Length

        if matches = 0 then
            0.0
        else
            let transpositions =
                Array.zip matchedLeft matchedRight
                |> Array.sumBy (fun (leftCharacter, rightCharacter) ->
                    if leftCharacter = rightCharacter then 0 else 1)
                |> fun count -> count / 2

            let matches = float matches

            (matches / float left.Length
             + matches / float right.Length
             + (matches - float transpositions) / matches)
            / 3.0

let genotype () =
    { Genes = Array.init target.Length (fun _ -> Random.Shared.Next(0, 2))
      Fitness = 0.0
      Age = 0 }

let fitnessFunction (chromosome: Chromosome<int>) =
    chromosome
    |> chromosomeKey
    |> cipher encrypted
    |> jaroDistance target

let terminate (population: seq<Chromosome<int>>) (_generation: int) (_temperature: float) =
    population |> Seq.exists (fun chromosome -> chromosome.Fitness >= 1.0)

let problem: Problem<int> =
    { Genotype = genotype
      FitnessFunction = fitnessFunction
      Terminate = terminate }

let options =
    { PopulationSize = 100
      SelectionRate = 0.8
      SelectionFn = Selection.elite
      CrossoverFn = Crossover.singlePoint
      MutationRate = 0.05
      MutationFn = Mutation.shuffle
      OnGeneration = Genetic.printProgress }

let solution = Genetic.run problem options
let key = chromosomeKey solution
let decoded = cipher encrypted key

printfn "Target:   %s" target
printfn "Decoded:  %s" decoded
printfn "Key:      %d (%A)" key solution.Genes
printfn "Fitness:  %f" solution.Fitness
