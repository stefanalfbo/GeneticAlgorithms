module GeneticAlgorithms.Tests.MutationTests

open Expecto
open GeneticAlgorithms

let private makeChromosome genes : Chromosome<int> =
    { Genes = genes; Fitness = 3.0; Age = 2 }

[<Tests>]
let scrambleTests =
    testList
        "Mutation.scramble"
        [ testCase "preserves the number of genes"
          <| fun _ ->
              let chromosome = makeChromosome [| 1; 2; 3; 4; 5 |]

              let result = Mutation.scramble chromosome

              Expect.equal result.Genes.Length chromosome.Genes.Length "gene count should be preserved"

          testCase "produces a permutation of the original genes"
          <| fun _ ->
              let chromosome = makeChromosome [| 1; 2; 3; 4; 5 |]

              for _ in 1..100 do
                  let result = Mutation.scramble chromosome

                  Expect.containsAll
                      result.Genes
                      chromosome.Genes
                      "mutated genes should be a permutation of the original genes"

          testCase "leaves fitness and age unchanged"
          <| fun _ ->
              let chromosome = makeChromosome [| 1; 2; 3; 4; 5 |]

              let result = Mutation.scramble chromosome

              Expect.equal result.Fitness chromosome.Fitness "fitness should be unchanged"
              Expect.equal result.Age chromosome.Age "age should be unchanged"

          testCase "does not mutate the original chromosome"
          <| fun _ ->
              let chromosome = makeChromosome [| 1; 2; 3; 4; 5 |]
              let genesBefore = Array.copy chromosome.Genes

              Mutation.scramble chromosome |> ignore

              Expect.equal chromosome.Genes genesBefore "original chromosome's genes should be unchanged" ]

[<Tests>]
let scrambleSliceTests =
    testList
        "Mutation.scrambleSlice"
        [ testCase "preserves the number of genes"
          <| fun _ ->
              let chromosome = makeChromosome [| 0 .. 9 |]

              for _ in 1..100 do
                  let result = Mutation.scrambleSlice 4 chromosome

                  Expect.equal result.Genes.Length chromosome.Genes.Length "gene count should be preserved"

          testCase "produces a permutation of the original genes"
          <| fun _ ->
              let chromosome = makeChromosome [| 0 .. 9 |]

              for _ in 1..100 do
                  let result = Mutation.scrambleSlice 4 chromosome

                  Expect.containsAll
                      result.Genes
                      chromosome.Genes
                      "mutated genes should be a permutation of the original genes"

          testCase "leaves fitness and age unchanged"
          <| fun _ ->
              let chromosome = makeChromosome [| 0 .. 9 |]

              let result = Mutation.scrambleSlice 4 chromosome

              Expect.equal result.Fitness chromosome.Fitness "fitness should be unchanged"
              Expect.equal result.Age chromosome.Age "age should be unchanged"

          testCase "does not mutate the original chromosome"
          <| fun _ ->
              let chromosome = makeChromosome [| 0 .. 9 |]
              let genesBefore = Array.copy chromosome.Genes

              Mutation.scrambleSlice 4 chromosome |> ignore

              Expect.equal chromosome.Genes genesBefore "original chromosome's genes should be unchanged" ]

[<Tests>]
let flipTests =
    testList
        "Mutation.flip"
        [ testCase "flips every gene"
          <| fun _ ->
              let chromosome = makeChromosome [| 0; 1; 1; 0; 1 |]

              let result = Mutation.flip chromosome

              Expect.equal result.Genes [| 1; 0; 0; 1; 0 |] "every gene should be flipped"

          testCase "preserves the number of genes"
          <| fun _ ->
              let chromosome = makeChromosome [| 0; 1; 1; 0; 1 |]

              let result = Mutation.flip chromosome

              Expect.equal result.Genes.Length chromosome.Genes.Length "gene count should be preserved"

          testCase "leaves fitness and age unchanged"
          <| fun _ ->
              let chromosome = makeChromosome [| 0; 1; 1; 0; 1 |]

              let result = Mutation.flip chromosome

              Expect.equal result.Fitness chromosome.Fitness "fitness should be unchanged"
              Expect.equal result.Age chromosome.Age "age should be unchanged"

          testCase "does not mutate the original chromosome"
          <| fun _ ->
              let chromosome = makeChromosome [| 0; 1; 1; 0; 1 |]
              let genesBefore = Array.copy chromosome.Genes

              Mutation.flip chromosome |> ignore

              Expect.equal chromosome.Genes genesBefore "original chromosome's genes should be unchanged" ]

[<Tests>]
let flipEachGeneTests =
    testList
        "Mutation.flipEachGene"
        [ testCase "at rate 1.0, flips every gene"
          <| fun _ ->
              // NextDouble() never returns 1.0, so "< 1.0" is always true - fully
              // deterministic, not just overwhelmingly likely.
              let chromosome = makeChromosome [| 0; 1; 1; 0; 1 |]

              let result = Mutation.flipEachGene 1.0 chromosome

              Expect.equal result.Genes [| 1; 0; 0; 1; 0 |] "every gene should be flipped"

          testCase "at rate 0.0, leaves every gene unchanged"
          <| fun _ ->
              // NextDouble() never returns a negative value, so "< 0.0" is always false -
              // fully deterministic, not just overwhelmingly likely.
              let chromosome = makeChromosome [| 0; 1; 1; 0; 1 |]

              let result = Mutation.flipEachGene 0.0 chromosome

              Expect.equal result.Genes chromosome.Genes "no gene should be flipped"

          testCase "preserves the number of genes"
          <| fun _ ->
              let chromosome = makeChromosome [| 0; 1; 1; 0; 1 |]

              let result = Mutation.flipEachGene 0.5 chromosome

              Expect.equal result.Genes.Length chromosome.Genes.Length "gene count should be preserved"

          testCase "each gene is either unchanged or flipped"
          <| fun _ ->
              let chromosome = makeChromosome [| 0; 1; 1; 0; 1 |]

              for _ in 1..100 do
                  let result = Mutation.flipEachGene 0.5 chromosome

                  for i in 0 .. chromosome.Genes.Length - 1 do
                      Expect.isTrue
                          (result.Genes.[i] = chromosome.Genes.[i] || result.Genes.[i] = (chromosome.Genes.[i] ^^^ 1))
                          "each gene should either be unchanged or flipped"

          testCase "does not mutate the original chromosome"
          <| fun _ ->
              let chromosome = makeChromosome [| 0; 1; 1; 0; 1 |]
              let genesBefore = Array.copy chromosome.Genes

              Mutation.flipEachGene 0.5 chromosome |> ignore

              Expect.equal chromosome.Genes genesBefore "original chromosome's genes should be unchanged" ]
