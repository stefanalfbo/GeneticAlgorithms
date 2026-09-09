module GeneticAlgorithms.Tests.ShuffleTests

open Expecto
open GeneticAlgorithms

[<Tests>]
let fisherYatesTests =
    testList
        "Shuffle.fisherYates"
        [ testCase "preserves the number of elements"
          <| fun _ ->
              let items = [| 0 .. 9 |]

              for _ in 1..100 do
                  let result = Shuffle.fisherYates System.Random.Shared items

                  Expect.equal result.Length items.Length "element count should be preserved"

          testCase "produces a permutation of the original elements"
          <| fun _ ->
              let items = [| 0 .. 9 |]

              for _ in 1..100 do
                  let result = Shuffle.fisherYates System.Random.Shared items

                  Expect.containsAll result items "shuffled elements should be a permutation of the original"

          testCase "does not mutate the original array"
          <| fun _ ->
              let items = [| 0 .. 9 |]
              let itemsBefore = Array.copy items

              Shuffle.fisherYates System.Random.Shared items |> ignore

              Expect.equal items itemsBefore "original array should be unchanged"

          testCase "an empty array shuffles to an empty array"
          <| fun _ ->
              Expect.equal
                  (Shuffle.fisherYates System.Random.Shared Array.empty<int>)
                  Array.empty<int>
                  "empty in, empty out"

          testCase "a single-element array is always unchanged"
          <| fun _ ->
              let items = [| 42 |]

              Expect.equal (Shuffle.fisherYates System.Random.Shared items) items "the only possible order is unchanged"

          testCase "produces more than one distinct ordering across many trials"
          <| fun _ ->
              // Over 100 trials of a 4-element array (24 possible orderings), seeing only 1
              // distinct result would mean this isn't actually shuffling - astronomically
              // unlikely if it is.
              let items = [| 0; 1; 2; 3 |]

              let distinctOrderings =
                  Seq.init 100 (fun _ -> Shuffle.fisherYates System.Random.Shared items)
                  |> Seq.distinct
                  |> Seq.length

              Expect.isTrue (distinctOrderings > 1) "100 trials should produce more than one distinct ordering"

          testCase "regression: the same seed produces the same shuffle"
          <| fun _ ->
              // The whole point of taking an explicit rng - a run seeded the same way must
              // be reproducible.
              let items = [| 0 .. 9 |]

              let first = Shuffle.fisherYates (System.Random(42)) items
              let second = Shuffle.fisherYates (System.Random(42)) items

              Expect.equal first second "the same seed should produce the same shuffle" ]
