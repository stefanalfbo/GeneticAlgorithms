module GeneticAlgorithms.Tests.MutationTests

open Expecto
open GeneticAlgorithms

let private makeChromosome genes : Chromosome<int> =
    { Genes = genes; Fitness = 3.0; Age = 2 }

let private makeFloatChromosome genes : Chromosome<float> =
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

              Expect.equal chromosome.Genes genesBefore "original chromosome's genes should be unchanged"

          testCase "when the chosen window would extend past the end, shifts it back to stay in bounds"
          <| fun _ ->
              // With a 6-gene chromosome and a window of 5, start + n is at least 1 + 5 = 6,
              // which is always >= size (6) - the "shift back" branch is taken on every draw,
              // not just overwhelmingly likely. The shifted window is always [1..5], so gene 0
              // is always left untouched.
              let chromosome = makeChromosome [| 0 .. 5 |]

              for _ in 1..100 do
                  let result = Mutation.scrambleSlice 5 chromosome

                  Expect.equal result.Genes.[0] chromosome.Genes.[0] "gene before the shifted window should be untouched"

                  Expect.containsAll
                      result.Genes.[1..]
                      chromosome.Genes.[1..]
                      "shifted window should contain the same genes, reordered"

          testCase "when the window size equals the chromosome size, scrambles every gene"
          <| fun _ ->
              // With a 5-gene chromosome and a window of 5, the shift-back branch always fires
              // and the shifted window spans the entire chromosome (lo = 0, hi = size) -
              // deterministic, not just overwhelmingly likely.
              let chromosome = makeChromosome [| 0 .. 4 |]

              for _ in 1..100 do
                  let result = Mutation.scrambleSlice 5 chromosome

                  Expect.equal result.Genes.Length chromosome.Genes.Length "gene count should be preserved"

                  Expect.containsAll
                      result.Genes
                      chromosome.Genes
                      "mutated genes should be a permutation of the original genes" ]

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

[<Tests>]
let gaussianTests =
    testList
        "Mutation.gaussian"
        [ testCase "preserves the number of genes"
          <| fun _ ->
              let chromosome = makeFloatChromosome [| 1.0; 2.0; 3.0; 4.0; 5.0 |]

              let result = Mutation.gaussian chromosome

              Expect.equal result.Genes.Length chromosome.Genes.Length "gene count should be preserved"

          testCase "leaves fitness and age unchanged"
          <| fun _ ->
              let chromosome = makeFloatChromosome [| 1.0; 2.0; 3.0; 4.0; 5.0 |]

              let result = Mutation.gaussian chromosome

              Expect.equal result.Fitness chromosome.Fitness "fitness should be unchanged"
              Expect.equal result.Age chromosome.Age "age should be unchanged"

          testCase "does not mutate the original chromosome"
          <| fun _ ->
              let chromosome = makeFloatChromosome [| 1.0; 2.0; 3.0; 4.0; 5.0 |]
              let genesBefore = Array.copy chromosome.Genes

              Mutation.gaussian chromosome |> ignore

              Expect.equal chromosome.Genes genesBefore "original chromosome's genes should be unchanged"

          testCase "when every gene is identical, the variance is zero and every mutated gene equals that value"
          <| fun _ ->
              // With zero variance the Box-Muller draw always lands exactly on the mean,
              // regardless of randomness - fully deterministic, not just overwhelmingly likely.
              let chromosome = makeFloatChromosome (Array.create 10 5.0)

              let result = Mutation.gaussian chromosome

              Expect.equal result.Genes chromosome.Genes "every gene should equal the shared value"

          testCase "resampled genes have approximately the same mean as the original genes"
          <| fun _ ->
              let chromosome = makeFloatChromosome (Array.init 1000 (fun i -> float (i % 100)))
              let expectedMean = Array.average chromosome.Genes

              let result = Mutation.gaussian chromosome
              let actualMean = Array.average result.Genes

              Expect.isTrue
                  (abs (actualMean - expectedMean) < 5.0)
                  $"resampled mean {actualMean} should be close to the original mean {expectedMean}" ]
