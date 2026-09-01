module GeneticAlgorithms.Tests.ReinsertionTests

open Expecto
open GeneticAlgorithms

let private makeChromosome genes : Chromosome<int> = { Genes = genes; Fitness = 0.0; Age = 0 }

let private makeChromosomeWithFitness fitness genes : Chromosome<int> =
    { Genes = genes; Fitness = fitness; Age = 0 }

[<Tests>]
let pureTests =
    testList
        "Reinsertion.pure"
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

[<Tests>]
let elitistTests =
    testList
        "Reinsertion.elitist"
        [ testCase "keeps every offspring"
          <| fun _ ->
              let offspring = [| makeChromosome [| 1 |]; makeChromosome [| 2 |] |]
              let parents = [| makeChromosomeWithFitness 5.0 [| 9 |] |]

              let result = Reinsertion.elitist 1.0 parents offspring [||]

              Expect.containsAll result offspring "every offspring chromosome should be in the result"

          testCase "carries over floor((parents + leftover) * survivalRate) survivors"
          <| fun _ ->
              let parents =
                  [| makeChromosomeWithFitness 4.0 [| 1 |]; makeChromosomeWithFitness 3.0 [| 2 |] |]

              let leftover =
                  [| makeChromosomeWithFitness 2.0 [| 3 |]; makeChromosomeWithFitness 1.0 [| 4 |] |]

              let result = Reinsertion.elitist 0.5 parents [||] leftover

              Expect.equal result.Length 2 "4 old chromosomes * 0.5 survival rate = 2 survivors"

          testCase "keeps the fittest of parents and leftover as survivors"
          <| fun _ ->
              let parents =
                  [| makeChromosomeWithFitness 4.0 [| 1 |]; makeChromosomeWithFitness 1.0 [| 2 |] |]

              let leftover =
                  [| makeChromosomeWithFitness 3.0 [| 3 |]; makeChromosomeWithFitness 2.0 [| 4 |] |]

              let result = Reinsertion.elitist 0.5 parents [||] leftover

              Expect.containsAll
                  result
                  [| parents.[0]; leftover.[0] |]
                  "the two fittest chromosomes (fitness 4.0 and 3.0) should survive"

          testCase "at survivalRate 0.0, behaves like pure - no survivors carried over"
          <| fun _ ->
              let parents = [| makeChromosomeWithFitness 5.0 [| 9 |] |]
              let leftover = [| makeChromosomeWithFitness 4.0 [| 8 |] |]
              let offspring = [| makeChromosome [| 1 |] |]

              let result = Reinsertion.elitist 0.0 parents offspring leftover

              Expect.equal result offspring "no survivors should be carried over at a 0.0 survival rate"

          testCase "at survivalRate 1.0, carries over every parent and leftover chromosome"
          <| fun _ ->
              let parents = [| makeChromosomeWithFitness 5.0 [| 9 |] |]
              let leftover = [| makeChromosomeWithFitness 4.0 [| 8 |] |]
              let offspring = [| makeChromosome [| 1 |] |]

              let result = Reinsertion.elitist 1.0 parents offspring leftover

              Expect.equal result.Length 3 "offspring plus every parent and leftover chromosome should survive"

              Expect.containsAll
                  result
                  (Array.concat [ offspring; parents; leftover ])
                  "every chromosome should be present in the result" ]
