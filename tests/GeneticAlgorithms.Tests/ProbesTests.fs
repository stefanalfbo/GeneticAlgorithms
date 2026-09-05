module GeneticAlgorithms.Tests.ProbesTests

open Expecto
open GeneticAlgorithms

let private makeChromosome fitness : Chromosome<int> =
    { Genes = [| 0 |]; Fitness = fitness; Age = 0 }

let private makeInfo generation : GenerationInfo<int> =
    { Generation = generation
      Population = [| makeChromosome 1.0 |]
      Best = makeChromosome 1.0
      Temperature = 0.0 }

[<Tests>]
let noopTests =
    testList
        "Probes.noop"
        [ testCase "has no observable effect"
          <| fun _ ->
              // If this throws, the test fails - that's the whole assertion.
              Probes.noop (makeInfo 0) ]

[<Tests>]
let combineTests =
    testList
        "Probes.combine"
        [ testCase "runs every probe in order for each generation"
          <| fun _ ->
              let calls = ResizeArray<string>()

              let combined =
                  Probes.combine
                      [ (fun _ -> calls.Add "first")
                        (fun _ -> calls.Add "second")
                        (fun _ -> calls.Add "third") ]

              combined (makeInfo 0)

              Expect.sequenceEqual calls [ "first"; "second"; "third" ] "probes should run in the given order"

          testCase "passes the same info to every probe"
          <| fun _ ->
              let observed = ResizeArray<int>()
              let info = makeInfo 42

              let combined =
                  Probes.combine [ (fun i -> observed.Add i.Generation); (fun i -> observed.Add i.Generation) ]

              combined info

              Expect.sequenceEqual observed [ 42; 42 ] "every probe should observe the same generation info"

          testCase "an empty list of probes is a no-op"
          <| fun _ ->
              let combined = Probes.combine []

              // If this throws, the test fails - that's the whole assertion.
              combined (makeInfo 0) ]

[<Tests>]
let everyNthTests =
    testList
        "Probes.everyNth"
        [ testCase "fires on generation 0 and every subsequent multiple of n"
          <| fun _ ->
              let observed = ResizeArray<int>()
              let throttled = Probes.everyNth 3 (fun info -> observed.Add info.Generation)

              for generation in 0..9 do
                  throttled (makeInfo generation)

              Expect.sequenceEqual observed [ 0; 3; 6; 9 ] "the probe should only fire on multiples of n"

          testCase "skips every generation that is not a multiple of n"
          <| fun _ ->
              let mutable callCount = 0
              let throttled = Probes.everyNth 5 (fun _ -> callCount <- callCount + 1)

              for generation in 1..4 do
                  throttled (makeInfo generation)

              Expect.equal callCount 0 "no generation from 1 to 4 is a multiple of 5" ]
