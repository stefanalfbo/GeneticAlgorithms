module GeneticAlgorithms.Tests.MutationTests

open Expecto
open GeneticAlgorithms

let private makeChromosome genes : Chromosome<int> =
    { Genes = genes; Fitness = 3.0; Age = 2 }

let private makeFloatChromosome genes : Chromosome<float> =
    { Genes = genes; Fitness = 3.0; Age = 2 }

let private rng = System.Random.Shared

[<Tests>]
let scrambleTests =
    testList
        "Mutation.scramble"
        [ testCase "preserves the number of genes"
          <| fun _ ->
              let chromosome = makeChromosome [| 1; 2; 3; 4; 5 |]

              let result = Mutation.scramble rng chromosome

              Expect.equal result.Genes.Length chromosome.Genes.Length "gene count should be preserved"

          testCase "produces a permutation of the original genes"
          <| fun _ ->
              let chromosome = makeChromosome [| 1; 2; 3; 4; 5 |]

              for _ in 1..100 do
                  let result = Mutation.scramble rng chromosome

                  Expect.containsAll
                      result.Genes
                      chromosome.Genes
                      "mutated genes should be a permutation of the original genes"

          testCase "leaves fitness and age unchanged"
          <| fun _ ->
              let chromosome = makeChromosome [| 1; 2; 3; 4; 5 |]

              let result = Mutation.scramble rng chromosome

              Expect.equal result.Fitness chromosome.Fitness "fitness should be unchanged"
              Expect.equal result.Age chromosome.Age "age should be unchanged"

          testCase "does not mutate the original chromosome"
          <| fun _ ->
              let chromosome = makeChromosome [| 1; 2; 3; 4; 5 |]
              let genesBefore = Array.copy chromosome.Genes

              Mutation.scramble rng chromosome |> ignore

              Expect.equal chromosome.Genes genesBefore "original chromosome's genes should be unchanged" ]

[<Tests>]
let scrambleSliceTests =
    testList
        "Mutation.scrambleSlice"
        [ testCase "preserves the number of genes"
          <| fun _ ->
              let chromosome = makeChromosome [| 0 .. 9 |]

              for _ in 1..100 do
                  let result = Mutation.scrambleSlice 4 rng chromosome

                  Expect.equal result.Genes.Length chromosome.Genes.Length "gene count should be preserved"

          testCase "produces a permutation of the original genes"
          <| fun _ ->
              let chromosome = makeChromosome [| 0 .. 9 |]

              for _ in 1..100 do
                  let result = Mutation.scrambleSlice 4 rng chromosome

                  Expect.containsAll
                      result.Genes
                      chromosome.Genes
                      "mutated genes should be a permutation of the original genes"

          testCase "leaves fitness and age unchanged"
          <| fun _ ->
              let chromosome = makeChromosome [| 0 .. 9 |]

              let result = Mutation.scrambleSlice 4 rng chromosome

              Expect.equal result.Fitness chromosome.Fitness "fitness should be unchanged"
              Expect.equal result.Age chromosome.Age "age should be unchanged"

          testCase "does not mutate the original chromosome"
          <| fun _ ->
              let chromosome = makeChromosome [| 0 .. 9 |]
              let genesBefore = Array.copy chromosome.Genes

              Mutation.scrambleSlice 4 rng chromosome |> ignore

              Expect.equal chromosome.Genes genesBefore "original chromosome's genes should be unchanged"

          testCase "regression: the window can reach both the first and the last gene"
          <| fun _ ->
              // Bug: the window's start used to be drawn from Random.Next(1, n), which
              // doesn't reference the chromosome's size at all - for a 10-gene chromosome
              // with a window of 4, only starts 1, 2, or 3 were ever drawn (windows [1,5),
              // [2,6), [3,7)), so gene 0 and genes 7-9 could never be touched no matter how
              // many times this ran. The valid range is 0..size-n (0..6 here) - every
              // position should be reachable, including both ends.
              //
              // 200 trials makes the odds of never drawing the window that covers gene 0
              // (lo = 0) or the one that covers gene 9 (lo = 6), out of 7 equally likely
              // positions, (6/7)^200 - astronomically small, not just "overwhelmingly
              // likely."
              let chromosome = makeChromosome [| 0 .. 9 |]

              let touchesGene index =
                  Seq.init 200 (fun _ -> Mutation.scrambleSlice 4 rng chromosome)
                  |> Seq.exists (fun result -> result.Genes.[index] <> chromosome.Genes.[index])

              Expect.isTrue (touchesGene 0) "the window should sometimes reach the first gene"
              Expect.isTrue (touchesGene 9) "the window should sometimes reach the last gene"

          testCase "when the window size equals the chromosome size, scrambles every gene"
          <| fun _ ->
              // With a 5-gene chromosome and a window of 5, size - n = 0, so the window's
              // start is always 0 (the only value Random.Next(0, 1) can return) and spans
              // the entire chromosome - deterministic, not just overwhelmingly likely.
              let chromosome = makeChromosome [| 0 .. 4 |]

              for _ in 1..100 do
                  let result = Mutation.scrambleSlice 5 rng chromosome

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

              let result = Mutation.flip rng chromosome

              Expect.equal result.Genes [| 1; 0; 0; 1; 0 |] "every gene should be flipped"

          testCase "preserves the number of genes"
          <| fun _ ->
              let chromosome = makeChromosome [| 0; 1; 1; 0; 1 |]

              let result = Mutation.flip rng chromosome

              Expect.equal result.Genes.Length chromosome.Genes.Length "gene count should be preserved"

          testCase "leaves fitness and age unchanged"
          <| fun _ ->
              let chromosome = makeChromosome [| 0; 1; 1; 0; 1 |]

              let result = Mutation.flip rng chromosome

              Expect.equal result.Fitness chromosome.Fitness "fitness should be unchanged"
              Expect.equal result.Age chromosome.Age "age should be unchanged"

          testCase "does not mutate the original chromosome"
          <| fun _ ->
              let chromosome = makeChromosome [| 0; 1; 1; 0; 1 |]
              let genesBefore = Array.copy chromosome.Genes

              Mutation.flip rng chromosome |> ignore

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

              let result = Mutation.flipEachGene 1.0 rng chromosome

              Expect.equal result.Genes [| 1; 0; 0; 1; 0 |] "every gene should be flipped"

          testCase "at rate 0.0, leaves every gene unchanged"
          <| fun _ ->
              // NextDouble() never returns a negative value, so "< 0.0" is always false -
              // fully deterministic, not just overwhelmingly likely.
              let chromosome = makeChromosome [| 0; 1; 1; 0; 1 |]

              let result = Mutation.flipEachGene 0.0 rng chromosome

              Expect.equal result.Genes chromosome.Genes "no gene should be flipped"

          testCase "preserves the number of genes"
          <| fun _ ->
              let chromosome = makeChromosome [| 0; 1; 1; 0; 1 |]

              let result = Mutation.flipEachGene 0.5 rng chromosome

              Expect.equal result.Genes.Length chromosome.Genes.Length "gene count should be preserved"

          testCase "each gene is either unchanged or flipped"
          <| fun _ ->
              let chromosome = makeChromosome [| 0; 1; 1; 0; 1 |]

              for _ in 1..100 do
                  let result = Mutation.flipEachGene 0.5 rng chromosome

                  for i in 0 .. chromosome.Genes.Length - 1 do
                      Expect.isTrue
                          (result.Genes.[i] = chromosome.Genes.[i] || result.Genes.[i] = (chromosome.Genes.[i] ^^^ 1))
                          "each gene should either be unchanged or flipped"

          testCase "does not mutate the original chromosome"
          <| fun _ ->
              let chromosome = makeChromosome [| 0; 1; 1; 0; 1 |]
              let genesBefore = Array.copy chromosome.Genes

              Mutation.flipEachGene 0.5 rng chromosome |> ignore

              Expect.equal chromosome.Genes genesBefore "original chromosome's genes should be unchanged"

          testCase "regression: rejects a rate outside [0, 1]"
          <| fun _ ->
              let chromosome = makeChromosome [| 0; 1; 1; 0; 1 |]

              Expect.throwsT<System.ArgumentException>
                  (fun () -> Mutation.flipEachGene -0.1 rng chromosome |> ignore)
                  "a negative rate should be rejected"

              Expect.throwsT<System.ArgumentException>
                  (fun () -> Mutation.flipEachGene 1.1 rng chromosome |> ignore)
                  "a rate above 1.0 should be rejected" ]

[<Tests>]
let randomResetTests =
    testList
        "Mutation.randomReset"
        [ testCase "at rate 1.0, replaces every gene with a freshly generated value"
          <| fun _ ->
              // NextDouble() never returns 1.0, so "< 1.0" is always true - fully
              // deterministic, not just overwhelmingly likely.
              let chromosome = makeChromosome [| 0; 1; 2; 3; 4 |]

              let result = Mutation.randomReset 1.0 (fun (_: System.Random) -> 99) rng chromosome

              Expect.equal result.Genes [| 99; 99; 99; 99; 99 |] "every gene should be replaced"

          testCase "at rate 0.0, leaves every gene unchanged"
          <| fun _ ->
              // NextDouble() never returns a negative value, so "< 0.0" is always false -
              // fully deterministic, not just overwhelmingly likely.
              let chromosome = makeChromosome [| 0; 1; 2; 3; 4 |]

              let result = Mutation.randomReset 0.0 (fun (_: System.Random) -> 99) rng chromosome

              Expect.equal result.Genes chromosome.Genes "no gene should be replaced"

          testCase "preserves the number of genes"
          <| fun _ ->
              let chromosome = makeChromosome [| 0; 1; 2; 3; 4 |]

              let result = Mutation.randomReset 0.5 (fun (_: System.Random) -> 99) rng chromosome

              Expect.equal result.Genes.Length chromosome.Genes.Length "gene count should be preserved"

          testCase "can introduce a gene value that was never in the original chromosome"
          <| fun _ ->
              let chromosome = makeChromosome [| 0; 1; 2; 3; 4 |]

              let result = Mutation.randomReset 1.0 (fun (_: System.Random) -> 99) rng chromosome

              Expect.isTrue (result.Genes |> Array.forall (fun gene -> gene = 99)) "every gene should be the freshly generated value, absent from the original"

          testCase "each gene is either unchanged or replaced by the generator"
          <| fun _ ->
              let chromosome = makeChromosome [| 0; 1; 2; 3; 4 |]

              for _ in 1..100 do
                  let result = Mutation.randomReset 0.5 (fun (_: System.Random) -> 99) rng chromosome

                  for i in 0 .. chromosome.Genes.Length - 1 do
                      Expect.isTrue
                          (result.Genes.[i] = chromosome.Genes.[i] || result.Genes.[i] = 99)
                          "each gene should either be unchanged or replaced"

          testCase "leaves fitness and age unchanged"
          <| fun _ ->
              let chromosome = makeChromosome [| 0; 1; 2; 3; 4 |]

              let result = Mutation.randomReset 0.5 (fun (_: System.Random) -> 99) rng chromosome

              Expect.equal result.Fitness chromosome.Fitness "fitness should be unchanged"
              Expect.equal result.Age chromosome.Age "age should be unchanged"

          testCase "does not mutate the original chromosome"
          <| fun _ ->
              let chromosome = makeChromosome [| 0; 1; 2; 3; 4 |]
              let genesBefore = Array.copy chromosome.Genes

              Mutation.randomReset 0.5 (fun (_: System.Random) -> 99) rng chromosome |> ignore

              Expect.equal chromosome.Genes genesBefore "original chromosome's genes should be unchanged"

          testCase "regression: rejects a rate outside [0, 1]"
          <| fun _ ->
              let chromosome = makeChromosome [| 0; 1; 2; 3; 4 |]

              Expect.throwsT<System.ArgumentException>
                  (fun () -> Mutation.randomReset -0.1 (fun (_: System.Random) -> 99) rng chromosome |> ignore)
                  "a negative rate should be rejected"

              Expect.throwsT<System.ArgumentException>
                  (fun () -> Mutation.randomReset 1.1 (fun (_: System.Random) -> 99) rng chromosome |> ignore)
                  "a rate above 1.0 should be rejected" ]

[<Tests>]
let gaussianTests =
    testList
        "Mutation.gaussian"
        [ testCase "preserves the number of genes"
          <| fun _ ->
              let chromosome = makeFloatChromosome [| 1.0; 2.0; 3.0; 4.0; 5.0 |]

              let result = Mutation.gaussian rng chromosome

              Expect.equal result.Genes.Length chromosome.Genes.Length "gene count should be preserved"

          testCase "leaves fitness and age unchanged"
          <| fun _ ->
              let chromosome = makeFloatChromosome [| 1.0; 2.0; 3.0; 4.0; 5.0 |]

              let result = Mutation.gaussian rng chromosome

              Expect.equal result.Fitness chromosome.Fitness "fitness should be unchanged"
              Expect.equal result.Age chromosome.Age "age should be unchanged"

          testCase "does not mutate the original chromosome"
          <| fun _ ->
              let chromosome = makeFloatChromosome [| 1.0; 2.0; 3.0; 4.0; 5.0 |]
              let genesBefore = Array.copy chromosome.Genes

              Mutation.gaussian rng chromosome |> ignore

              Expect.equal chromosome.Genes genesBefore "original chromosome's genes should be unchanged"

          testCase "when every gene is identical, the variance is zero and every mutated gene equals that value"
          <| fun _ ->
              // With zero variance the Box-Muller draw always lands exactly on the mean,
              // regardless of randomness - fully deterministic, not just overwhelmingly likely.
              let chromosome = makeFloatChromosome (Array.create 10 5.0)

              let result = Mutation.gaussian rng chromosome

              Expect.equal result.Genes chromosome.Genes "every gene should equal the shared value"

          testCase "resampled genes have approximately the same mean as the original genes"
          <| fun _ ->
              let chromosome = makeFloatChromosome (Array.init 1000 (fun i -> float (i % 100)))
              let expectedMean = Array.average chromosome.Genes

              let result = Mutation.gaussian rng chromosome
              let actualMean = Array.average result.Genes

              Expect.isTrue
                  (abs (actualMean - expectedMean) < 5.0)
                  $"resampled mean {actualMean} should be close to the original mean {expectedMean}" ]
