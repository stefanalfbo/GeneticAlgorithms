module GeneticAlgorithms.Tests.MutationTests

open Expecto
open GeneticAlgorithms

let private makeChromosome genes : Chromosome<int> =
    { Genes = genes; Fitness = 3.0; Age = 2 }

[<Tests>]
let shuffleTests =
    testList
        "Mutation.shuffle"
        [ testCase "preserves the number of genes"
          <| fun _ ->
              let chromosome = makeChromosome [| 1; 2; 3; 4; 5 |]

              let result = Mutation.shuffle chromosome

              Expect.equal result.Genes.Length chromosome.Genes.Length "gene count should be preserved"

          testCase "produces a permutation of the original genes"
          <| fun _ ->
              let chromosome = makeChromosome [| 1; 2; 3; 4; 5 |]

              for _ in 1..100 do
                  let result = Mutation.shuffle chromosome

                  Expect.containsAll
                      result.Genes
                      chromosome.Genes
                      "mutated genes should be a permutation of the original genes"

          testCase "leaves fitness and age unchanged"
          <| fun _ ->
              let chromosome = makeChromosome [| 1; 2; 3; 4; 5 |]

              let result = Mutation.shuffle chromosome

              Expect.equal result.Fitness chromosome.Fitness "fitness should be unchanged"
              Expect.equal result.Age chromosome.Age "age should be unchanged"

          testCase "does not mutate the original chromosome"
          <| fun _ ->
              let chromosome = makeChromosome [| 1; 2; 3; 4; 5 |]
              let genesBefore = Array.copy chromosome.Genes

              Mutation.shuffle chromosome |> ignore

              Expect.equal chromosome.Genes genesBefore "original chromosome's genes should be unchanged" ]
