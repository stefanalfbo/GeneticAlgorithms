module GeneticAlgorithms.Tests.SelectionTests

open Expecto
open GeneticAlgorithms

let private makeChromosome fitness : Chromosome<int> =
    { Genes = [| 0 |]; Fitness = fitness; Age = 0 }

let private population =
    [| makeChromosome 4.0
       makeChromosome 3.0
       makeChromosome 2.0
       makeChromosome 1.0 |]

let private opts: Options<int> =
    { PopulationSize = population.Length
      SelectionRate = 0.8
      SelectionFn = Selection.elite
      MutationRate = 0.05
      OnGeneration = fun _ _ -> () }

[<Tests>]
let eliteTests =
    testList
        "Selection.elite"
        [ testCase "takes the first n chromosomes"
          <| fun _ ->
              let result = Selection.elite population 2

              Expect.equal result [| population.[0]; population.[1] |] "should keep the given order"

          testCase "returns n chromosomes"
          <| fun _ ->
              let result = Selection.elite population 3

              Expect.equal result.Length 3 "should return exactly n chromosomes" ]

[<Tests>]
let randomTests =
    testList
        "Selection.random"
        [ testCase "returns n chromosomes"
          <| fun _ ->
              let result = Selection.random population 2

              Expect.equal result.Length 2 "should return exactly n chromosomes"

          testCase "only returns chromosomes from the population"
          <| fun _ ->
              let result = Selection.random population 3

              Expect.all result (fun c -> Array.contains c population) "every chromosome should come from the population" ]

[<Tests>]
let tournamentTests =
    testList
        "Selection.tournament"
        [ testCase "returns n chromosomes"
          <| fun _ ->
              let result = Selection.tournament 2 population 3

              Expect.equal result.Length 3 "should return exactly n chromosomes"

          testCase "always picks the fittest chromosome when tournament_size covers the whole population"
          <| fun _ ->
              let result = Selection.tournament population.Length population 3

              Expect.all result (fun c -> c = population.[0]) "every round should pick the fittest chromosome" ]

[<Tests>]
let tournamentNoDuplicatesTests =
    testList
        "Selection.tournamentNoDuplicates"
        [ testCase "returns n distinct chromosomes"
          <| fun _ ->
              let result = Selection.tournamentNoDuplicates 2 population 3

              Expect.equal result.Length 3 "should return exactly n chromosomes"
              Expect.equal (result |> Array.distinct |> Array.length) 3 "should not repeat chromosomes" ]

[<Tests>]
let rouletteTests =
    testList
        "Selection.roulette"
        [ testCase "returns the only chromosome when the population has a single member"
          <| fun _ ->
              let single = [| makeChromosome 1.0 |]

              let result = Selection.roulette single 3

              Expect.all result (fun c -> c = single.[0]) "should always return the only chromosome"

          testCase "always picks the chromosome holding all the fitness weight"
          <| fun _ ->
              let weighted = [| makeChromosome 100.0; makeChromosome 0.0 |]

              let result = Selection.roulette weighted 5

              Expect.all result (fun c -> c = weighted.[0]) "should always pick the chromosome with all the fitness" ]

[<Tests>]
let selectTests =
    testList
        "Selection.select"
        [ testCase "splits the population into parent pairs and leftover using selection_rate"
          <| fun _ ->
              let parentPairs, leftover = Selection.select { opts with SelectionRate = 0.5 } population

              Expect.equal parentPairs.Length 1 "half the population should be paired up"
              Expect.equal leftover.Length 2 "the rest should be left over"

          testCase "rounds an odd selection count up to the next even number"
          <| fun _ ->
              let parentPairs, leftover = Selection.select { opts with SelectionRate = 0.75 } population

              Expect.equal parentPairs.Length 2 "the selection count should be rounded up to stay even"
              Expect.equal leftover.Length 0 "no chromosomes should be left over" ]
