module GeneticAlgorithms.Tests.ReinsertionTests

open Expecto
open GeneticAlgorithms

let private makeChromosome genes : Chromosome<int> = { Genes = genes; Fitness = 0.0; Age = 0 }

[<Tests>]
let pureTests =
    testList
        "Reinsertion.``pure``"
        [ testCase "returns the offspring unchanged"
          <| fun _ ->
              let offspring = [| makeChromosome [| 1 |]; makeChromosome [| 2 |] |]

              let result = Reinsertion.``pure`` [||] offspring [||]

              Expect.equal result offspring "offspring should be returned unchanged"

          testCase "ignores parents"
          <| fun _ ->
              let parents = [| makeChromosome [| 9 |] |]
              let offspring = [| makeChromosome [| 1 |] |]

              let result = Reinsertion.``pure`` parents offspring [||]

              Expect.equal result offspring "parents should not appear in the result"

          testCase "ignores leftover"
          <| fun _ ->
              let leftover = [| makeChromosome [| 9 |] |]
              let offspring = [| makeChromosome [| 1 |] |]

              let result = Reinsertion.``pure`` [||] offspring leftover

              Expect.equal result offspring "leftover should not appear in the result"

          testCase "returns an empty population when there is no offspring"
          <| fun _ ->
              let parents = [| makeChromosome [| 1 |] |]
              let leftover = [| makeChromosome [| 2 |] |]

              let result = Reinsertion.``pure`` parents [||] leftover

              Expect.isEmpty result "an empty offspring array should produce an empty next population" ]
