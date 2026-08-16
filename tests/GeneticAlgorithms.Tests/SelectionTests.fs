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
let boltzmannTests =
    testList
        "Selection.boltzmann"
        [ testCase "returns n chromosomes"
          <| fun _ ->
              let result = Selection.boltzmann 1.0 population 3

              Expect.equal result.Length 3 "should return exactly n chromosomes"

          testCase "raises for a non-positive temperature"
          <| fun _ ->
              Expect.throwsT<System.ArgumentException>
                  (fun () -> Selection.boltzmann 0.0 population 1 |> ignore)
                  "temperature must be positive"

          testCase "strongly favors the fittest chromosome at a low temperature"
          <| fun _ ->
              let weighted = [| makeChromosome 10.0; makeChromosome 0.0 |]

              let result = Selection.boltzmann 0.1 weighted 5

              Expect.all result (fun c -> c = weighted.[0]) "should almost always pick the fittest chromosome"

          testCase "does not overflow for a large fitness gap at a very low temperature"
          <| fun _ ->
              let weighted = [| makeChromosome 1_000_000.0; makeChromosome 0.0 |]

              let result = Selection.boltzmann 0.001 weighted 5

              Expect.all result (fun c -> c = weighted.[0]) "should deterministically pick the fittest chromosome without producing NaN or Infinity weights" ]

[<Tests>]
let stochasticUniversalSamplingTests =
    testList
        "Selection.stochasticUniversalSampling"
        [ testCase "returns n chromosomes"
          <| fun _ ->
              let result = Selection.stochasticUniversalSampling population 3

              Expect.equal result.Length 3 "should return exactly n chromosomes"

          testCase "returns the only chromosome when the population has a single member"
          <| fun _ ->
              let single = [| makeChromosome 1.0 |]

              let result = Selection.stochasticUniversalSampling single 3

              Expect.all result (fun c -> c = single.[0]) "should always return the only chromosome"

          testCase "selects each chromosome exactly as many times as its share of total fitness"
          <| fun _ ->
              // Evenly spaced pointers guarantee an exact count per chromosome (not just an
              // expected value), regardless of the random starting offset: with fitnesses
              // 4/3/2/1 (summing to 10) and n = 10, the pointer spacing is 1.0, so each
              // chromosome's fitness share always contains exactly that many pointers.
              let weighted =
                  [| makeChromosome 4.0
                     makeChromosome 3.0
                     makeChromosome 2.0
                     makeChromosome 1.0 |]

              let result = Selection.stochasticUniversalSampling weighted 10
              let countOf c = result |> Array.filter ((=) c) |> Array.length

              Expect.equal (countOf weighted.[0]) 4 "the chromosome with 4/10 of the fitness should be picked exactly 4 times"
              Expect.equal (countOf weighted.[1]) 3 "the chromosome with 3/10 of the fitness should be picked exactly 3 times"
              Expect.equal (countOf weighted.[2]) 2 "the chromosome with 2/10 of the fitness should be picked exactly 2 times"
              Expect.equal (countOf weighted.[3]) 1 "the chromosome with 1/10 of the fitness should be picked exactly 1 time" ]

[<Tests>]
let rankTests =
    testList
        "Selection.rank"
        [ testCase "returns n chromosomes"
          <| fun _ ->
              let result = Selection.rank population 3

              Expect.equal result.Length 3 "should return exactly n chromosomes"

          testCase "returns the only chromosome when the population has a single member"
          <| fun _ ->
              let single = [| makeChromosome 1.0 |]

              let result = Selection.rank single 3

              Expect.all result (fun c -> c = single.[0]) "should always return the only chromosome"

          testCase "weighs by rank rather than raw fitness, so an extreme outlier does not dominate"
          <| fun _ ->
              // Ranks here are 1/2/3 (worst to best) regardless of the fitness gap, so the
              // best chromosome has only half the total weight instead of ~all of it, and the
              // worst still has a real (1/6) chance. Over 200 draws the odds of either of the
              // following failing by chance are astronomically small (~(5/6)^200 and ~0.5^200).
              let weighted = [| makeChromosome 1000.0; makeChromosome 2.0; makeChromosome 1.0 |]

              let result = Selection.rank weighted 200

              Expect.isTrue
                  (result |> Array.exists ((=) weighted.[2]))
                  "the lowest-fitness chromosome should still be picked sometimes"

              Expect.isTrue
                  (result |> Array.exists (fun c -> c <> weighted.[0]))
                  "the extreme fitness outlier should not win every single pick" ]

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
