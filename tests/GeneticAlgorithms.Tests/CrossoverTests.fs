module GeneticAlgorithms.Tests.CrossoverTests

open Expecto
open GeneticAlgorithms

let private makeChromosome genes = { Genes = genes; Fitness = 0.0; Age = 0 }
let private rng = System.Random.Shared

[<Tests>]
let singlePointTests =
    testList
        "Crossover.singlePoint"
        [ testCase "children have the same length as the parents"
          <| fun _ ->
              let p1 = makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7 |]
              let p2 = makeChromosome [| 10; 11; 12; 13; 14; 15; 16; 17 |]

              for _ in 1..100 do
                  let c1, c2 = Crossover.singlePoint rng p1 p2

                  Expect.equal c1.Genes.Length p1.Genes.Length "first child should match parent length"
                  Expect.equal c2.Genes.Length p2.Genes.Length "second child should match parent length"

          testCase "at every position, each child's gene comes from one of the two parents"
          <| fun _ ->
              let p1 = makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7 |]
              let p2 = makeChromosome [| 10; 11; 12; 13; 14; 15; 16; 17 |]

              for _ in 1..100 do
                  let c1, c2 = Crossover.singlePoint rng p1 p2

                  for i in 0 .. p1.Genes.Length - 1 do
                      Expect.isTrue
                          (c1.Genes.[i] = p1.Genes.[i] || c1.Genes.[i] = p2.Genes.[i])
                          "the first child's gene should come from one of the two parents"

                      Expect.isTrue
                          (c2.Genes.[i] = p1.Genes.[i] || c2.Genes.[i] = p2.Genes.[i])
                          "the second child's gene should come from one of the two parents"

          testCase "does not mutate the parent chromosomes"
          <| fun _ ->
              let p1 = makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7 |]
              let p2 = makeChromosome [| 10; 11; 12; 13; 14; 15; 16; 17 |]
              let p1GenesBefore = Array.copy p1.Genes
              let p2GenesBefore = Array.copy p2.Genes

              Crossover.singlePoint rng p1 p2 |> ignore

              Expect.equal p1.Genes p1GenesBefore "first parent's genes should be unchanged"
              Expect.equal p2.Genes p2GenesBefore "second parent's genes should be unchanged"

          testCase "regression: rejects parents with different lengths"
          <| fun _ ->
              // Bug: singlePoint used to draw its cut point from p1's length alone and apply
              // it to both parents regardless of p2's actual length, silently returning
              // children whose lengths didn't match either parent (or crashing with an
              // unrelated "array too short" exception if p1 was the longer parent) - breaking
              // this module's own documented guarantee that every strategy besides
              // messySinglePoint preserves parent length.
              let p1 = makeChromosome [| 0; 1 |]
              let p2 = makeChromosome [| 10; 11; 12 |]

              Expect.throwsT<System.ArgumentException>
                  (fun () -> Crossover.singlePoint rng p1 p2 |> ignore)
                  "parents with different lengths should be rejected"

          testCase "resets Age and Fitness for both children, regardless of the parents'"
          <| fun _ ->
              let p1 = { makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7 |] with Fitness = 99.0; Age = 7 }
              let p2 = { makeChromosome [| 10; 11; 12; 13; 14; 15; 16; 17 |] with Fitness = 42.0; Age = 3 }

              let c1, c2 = Crossover.singlePoint rng p1 p2

              Expect.equal c1.Age 0 "the first child should start at Age 0, not inherit p1's Age"
              Expect.equal c2.Age 0 "the second child should start at Age 0, not inherit p2's Age"
              Expect.equal c1.Fitness 0.0 "the first child's Fitness should be reset, not inherit p1's"
              Expect.equal c2.Fitness 0.0 "the second child's Fitness should be reset, not inherit p2's" ]

[<Tests>]
let multiPointTests =
    testList
        "Crossover.multiPoint"
        [ testCase "children have the same length as the parents"
          <| fun _ ->
              let p1 = makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7 |]
              let p2 = makeChromosome [| 10; 11; 12; 13; 14; 15; 16; 17 |]

              for _ in 1..100 do
                  let c1, c2 = Crossover.multiPoint 3 rng p1 p2

                  Expect.equal c1.Genes.Length p1.Genes.Length "first child should match parent length"
                  Expect.equal c2.Genes.Length p2.Genes.Length "second child should match parent length"

          testCase "at pointCount 0, children exactly match the parents"
          <| fun _ ->
              // With no cut points, the whole array is a single segment - fully
              // deterministic, not just overwhelmingly likely.
              let p1 = makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7 |]
              let p2 = makeChromosome [| 10; 11; 12; 13; 14; 15; 16; 17 |]

              let c1, c2 = Crossover.multiPoint 0 rng p1 p2

              Expect.equal c1.Genes p1.Genes "first child should equal the first parent"
              Expect.equal c2.Genes p2.Genes "second child should equal the second parent"

          testCase "at the maximum pointCount, every position alternates deterministically"
          <| fun _ ->
              // With pointCount = length - 1, every valid cut position is used, so there's
              // no randomness left in *which* points get picked - fully deterministic.
              let p1 = makeChromosome [| 0; 1; 2; 3 |]
              let p2 = makeChromosome [| 10; 11; 12; 13 |]

              let c1, c2 = Crossover.multiPoint 3 rng p1 p2

              Expect.equal c1.Genes [| 0; 11; 2; 13 |] "first child should alternate starting with the first parent"
              Expect.equal c2.Genes [| 10; 1; 12; 3 |] "second child should alternate starting with the second parent"

          testCase "at every position, each child's gene comes from one of the two parents"
          <| fun _ ->
              let p1 = makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7 |]
              let p2 = makeChromosome [| 10; 11; 12; 13; 14; 15; 16; 17 |]

              for _ in 1..100 do
                  let c1, c2 = Crossover.multiPoint 3 rng p1 p2

                  for i in 0 .. p1.Genes.Length - 1 do
                      Expect.isTrue
                          (c1.Genes.[i] = p1.Genes.[i] || c1.Genes.[i] = p2.Genes.[i])
                          "the first child's gene should come from one of the two parents"

                      Expect.isTrue
                          (c2.Genes.[i] = p1.Genes.[i] || c2.Genes.[i] = p2.Genes.[i])
                          "the second child's gene should come from one of the two parents"

          testCase "does not mutate the parent chromosomes"
          <| fun _ ->
              let p1 = makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7 |]
              let p2 = makeChromosome [| 10; 11; 12; 13; 14; 15; 16; 17 |]
              let p1GenesBefore = Array.copy p1.Genes
              let p2GenesBefore = Array.copy p2.Genes

              Crossover.multiPoint 3 rng p1 p2 |> ignore

              Expect.equal p1.Genes p1GenesBefore "first parent's genes should be unchanged"
              Expect.equal p2.Genes p2GenesBefore "second parent's genes should be unchanged"

          testCase "resets Age and Fitness for both children, regardless of the parents'"
          <| fun _ ->
              let p1 = { makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7 |] with Fitness = 99.0; Age = 7 }
              let p2 = { makeChromosome [| 10; 11; 12; 13; 14; 15; 16; 17 |] with Fitness = 42.0; Age = 3 }

              let c1, c2 = Crossover.multiPoint 3 rng p1 p2

              Expect.equal c1.Age 0 "the first child should start at Age 0, not inherit p1's Age"
              Expect.equal c2.Age 0 "the second child should start at Age 0, not inherit p2's Age"
              Expect.equal c1.Fitness 0.0 "the first child's Fitness should be reset, not inherit p1's"
              Expect.equal c2.Fitness 0.0 "the second child's Fitness should be reset, not inherit p2's" ]

[<Tests>]
let messySinglePointTests =
    testList
        "Crossover.messySinglePoint"
        [ testCase "each child is a prefix of one parent followed by a suffix of the other"
          <| fun _ ->
              // p1 is all 1s and p2 is all 2s, so regardless of where each parent's
              // independent cut point lands, the first child must be some number of 1s
              // (from p1's head) followed by some number of 2s (from p2's tail) - never
              // decreasing. The second child is the mirror image - p2's head (2s)
              // followed by p1's tail (1s) - never increasing.
              let p1 = makeChromosome (Array.create 8 1)
              let p2 = makeChromosome (Array.create 8 2)

              for _ in 1..100 do
                  let c1, c2 = Crossover.messySinglePoint rng p1 p2

                  let isSorted comparer (genes: int array) =
                      genes |> Array.pairwise |> Array.forall comparer

                  Expect.isTrue (isSorted (fun (a, b) -> a <= b) c1.Genes) "first child should be 1s followed by 2s, never decreasing"
                  Expect.isTrue (isSorted (fun (a, b) -> a >= b) c2.Genes) "second child should be 2s followed by 1s, never increasing"
                  Expect.contains c1.Genes 1 "first child should contain at least one gene from p1's head"
                  Expect.contains c1.Genes 2 "first child should contain at least one gene from p2's tail"

          testCase "children's lengths can differ from the parents' length"
          <| fun _ ->
              // Cut points are chosen independently in each parent, each uniform over
              // 1..7 for an 8-gene chromosome. The original length (8) is only reproduced
              // when both cut points happen to be equal, so across 100 independent trials
              // the odds of *never* seeing a different length are astronomically small
              // (roughly (1/7)^100) - not literally deterministic, but as close to it as a
              // randomized test gets.
              let p1 = makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7 |]
              let p2 = makeChromosome [| 10; 11; 12; 13; 14; 15; 16; 17 |]

              let lengths =
                  [ for _ in 1..100 ->
                        let c1, c2 = Crossover.messySinglePoint rng p1 p2
                        c1.Genes.Length, c2.Genes.Length ]

              Expect.isTrue
                  (lengths |> List.exists (fun (l1, l2) -> l1 <> p1.Genes.Length || l2 <> p2.Genes.Length))
                  "at least one trial should produce a child whose length differs from the original parent length"

          testCase "does not mutate the parent chromosomes"
          <| fun _ ->
              let p1 = makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7 |]
              let p2 = makeChromosome [| 10; 11; 12; 13; 14; 15; 16; 17 |]
              let p1GenesBefore = Array.copy p1.Genes
              let p2GenesBefore = Array.copy p2.Genes

              Crossover.messySinglePoint rng p1 p2 |> ignore

              Expect.equal p1.Genes p1GenesBefore "first parent's genes should be unchanged"
              Expect.equal p2.Genes p2GenesBefore "second parent's genes should be unchanged"

          testCase "resets Age and Fitness for both children, regardless of the parents'"
          <| fun _ ->
              let p1 = { makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7 |] with Fitness = 99.0; Age = 7 }
              let p2 = { makeChromosome [| 10; 11; 12; 13; 14; 15; 16; 17 |] with Fitness = 42.0; Age = 3 }

              let c1, c2 = Crossover.messySinglePoint rng p1 p2

              Expect.equal c1.Age 0 "the first child should start at Age 0, not inherit p1's Age"
              Expect.equal c2.Age 0 "the second child should start at Age 0, not inherit p2's Age"
              Expect.equal c1.Fitness 0.0 "the first child's Fitness should be reset, not inherit p1's"
              Expect.equal c2.Fitness 0.0 "the second child's Fitness should be reset, not inherit p2's" ]

[<Tests>]
let orderOneCrossoverTests =
    testList
        "Crossover.orderOneCrossover"
        [ testCase "children have the same length as the parents"
          <| fun _ ->
              let p1 = makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7 |]
              let p2 = makeChromosome [| 7; 6; 5; 4; 3; 2; 1; 0 |]

              for _ in 1..100 do
                  let c1, c2 = Crossover.orderOneCrossover rng p1 p2

                  Expect.equal c1.Genes.Length p1.Genes.Length "first child should match parent length"
                  Expect.equal c2.Genes.Length p2.Genes.Length "second child should match parent length"

          testCase "each child is a permutation of the parents' genes"
          <| fun _ ->
              let p1 = makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7 |]
              let p2 = makeChromosome [| 7; 6; 5; 4; 3; 2; 1; 0 |]
              let expectedGenes = Set.ofArray p1.Genes

              for _ in 1..100 do
                  let c1, c2 = Crossover.orderOneCrossover rng p1 p2

                  Expect.equal (Array.distinct c1.Genes |> Array.length) c1.Genes.Length "first child should have no duplicate genes"
                  Expect.equal (Array.distinct c2.Genes |> Array.length) c2.Genes.Length "second child should have no duplicate genes"
                  Expect.equal (Set.ofArray c1.Genes) expectedGenes "first child should contain exactly the parents' genes"
                  Expect.equal (Set.ofArray c2.Genes) expectedGenes "second child should contain exactly the parents' genes"

          testCase "does not mutate the parent chromosomes"
          <| fun _ ->
              let p1 = makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7 |]
              let p2 = makeChromosome [| 7; 6; 5; 4; 3; 2; 1; 0 |]
              let p1GenesBefore = Array.copy p1.Genes
              let p2GenesBefore = Array.copy p2.Genes

              Crossover.orderOneCrossover rng p1 p2 |> ignore

              Expect.equal p1.Genes p1GenesBefore "first parent's genes should be unchanged"
              Expect.equal p2.Genes p2GenesBefore "second parent's genes should be unchanged"

          testCase "resets Age and Fitness for both children, regardless of the parents'"
          <| fun _ ->
              let p1 = { makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7 |] with Fitness = 99.0; Age = 7 }
              let p2 = { makeChromosome [| 7; 6; 5; 4; 3; 2; 1; 0 |] with Fitness = 42.0; Age = 3 }

              let c1, c2 = Crossover.orderOneCrossover rng p1 p2

              Expect.equal c1.Age 0 "the first child should start at Age 0, not inherit p1's Age"
              Expect.equal c2.Age 0 "the second child should start at Age 0, not inherit p2's Age"
              Expect.equal c1.Fitness 0.0 "the first child's Fitness should be reset, not inherit p1's"
              Expect.equal c2.Fitness 0.0 "the second child's Fitness should be reset, not inherit p2's" ]

[<Tests>]
let cycleCrossoverTests =
    testList
        "Crossover.cycleCrossover"
        [ testCase "children have the same length as the parents"
          <| fun _ ->
              let p1 = makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7 |]
              let p2 = makeChromosome [| 7; 6; 5; 4; 3; 2; 1; 0 |]

              let c1, c2 = Crossover.cycleCrossover rng p1 p2

              Expect.equal c1.Genes.Length p1.Genes.Length "first child should match parent length"
              Expect.equal c2.Genes.Length p2.Genes.Length "second child should match parent length"

          testCase "each child is a permutation of the parents' genes"
          <| fun _ ->
              let p1 = makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7 |]
              let p2 = makeChromosome [| 7; 6; 5; 4; 3; 2; 1; 0 |]
              let expectedGenes = Set.ofArray p1.Genes

              let c1, c2 = Crossover.cycleCrossover rng p1 p2

              Expect.equal (Array.distinct c1.Genes |> Array.length) c1.Genes.Length "first child should have no duplicate genes"
              Expect.equal (Array.distinct c2.Genes |> Array.length) c2.Genes.Length "second child should have no duplicate genes"
              Expect.equal (Set.ofArray c1.Genes) expectedGenes "first child should contain exactly the parents' genes"
              Expect.equal (Set.ofArray c2.Genes) expectedGenes "second child should contain exactly the parents' genes"

          testCase "at every position, each child's gene comes from one of the two parents at that same position"
          <| fun _ ->
              // Unlike orderOneCrossover, which can move a gene to a different position,
              // cycle crossover always keeps a gene at the index it held in whichever
              // parent contributed it - this is the property that distinguishes it.
              let p1 = makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7 |]
              let p2 = makeChromosome [| 7; 6; 5; 4; 3; 2; 1; 0 |]

              let c1, c2 = Crossover.cycleCrossover rng p1 p2

              for i in 0 .. p1.Genes.Length - 1 do
                  Expect.isTrue
                      (c1.Genes.[i] = p1.Genes.[i] || c1.Genes.[i] = p2.Genes.[i])
                      "the first child's gene should come from one of the two parents at the same position"

                  Expect.isTrue
                      (c2.Genes.[i] = p1.Genes.[i] || c2.Genes.[i] = p2.Genes.[i])
                      "the second child's gene should come from one of the two parents at the same position"

          testCase "produces the well-known worked example's result"
          <| fun _ ->
              // Cycle crossover has no randomness at all - the result is a pure function
              // of the two parents, so this is fully deterministic. Cycles are {0, 8, 9},
              // {1, 2, 4, 5, 6, 7}, and {3}; children alternate which parent contributes
              // each successive cycle, starting with p1.
              let p1 = makeChromosome [| 8; 4; 7; 3; 6; 2; 5; 1; 9; 0 |]
              let p2 = makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7; 8; 9 |]

              let c1, c2 = Crossover.cycleCrossover rng p1 p2

              Expect.equal c1.Genes [| 8; 1; 2; 3; 4; 5; 6; 7; 9; 0 |] "first child should match the worked example"
              Expect.equal c2.Genes [| 0; 4; 7; 3; 6; 2; 5; 1; 8; 9 |] "second child should match the worked example"

          testCase "does not mutate the parent chromosomes"
          <| fun _ ->
              let p1 = makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7 |]
              let p2 = makeChromosome [| 7; 6; 5; 4; 3; 2; 1; 0 |]
              let p1GenesBefore = Array.copy p1.Genes
              let p2GenesBefore = Array.copy p2.Genes

              Crossover.cycleCrossover rng p1 p2 |> ignore

              Expect.equal p1.Genes p1GenesBefore "first parent's genes should be unchanged"
              Expect.equal p2.Genes p2GenesBefore "second parent's genes should be unchanged"

          testCase "resets Age and Fitness for both children, regardless of the parents'"
          <| fun _ ->
              let p1 = { makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7 |] with Fitness = 99.0; Age = 7 }
              let p2 = { makeChromosome [| 7; 6; 5; 4; 3; 2; 1; 0 |] with Fitness = 42.0; Age = 3 }

              let c1, c2 = Crossover.cycleCrossover rng p1 p2

              Expect.equal c1.Age 0 "the first child should start at Age 0, not inherit p1's Age"
              Expect.equal c2.Age 0 "the second child should start at Age 0, not inherit p2's Age"
              Expect.equal c1.Fitness 0.0 "the first child's Fitness should be reset, not inherit p1's"
              Expect.equal c2.Fitness 0.0 "the second child's Fitness should be reset, not inherit p2's" ]

[<Tests>]
let uniformTests =
    testList
        "Crossover.uniform"
        [ testCase "children have the same length as the parents"
          <| fun _ ->
              let p1 = makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7 |]
              let p2 = makeChromosome [| 10; 11; 12; 13; 14; 15; 16; 17 |]

              for _ in 1..100 do
                  let c1, c2 = Crossover.uniform 0.5 rng p1 p2

                  Expect.equal c1.Genes.Length p1.Genes.Length "first child should match parent length"
                  Expect.equal c2.Genes.Length p2.Genes.Length "second child should match parent length"

          testCase "at rate 1.0, children exactly match the parents"
          <| fun _ ->
              // NextDouble() never returns 1.0, so "< 1.0" is always true - this is fully
              // deterministic, not just overwhelmingly likely.
              let p1 = makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7 |]
              let p2 = makeChromosome [| 10; 11; 12; 13; 14; 15; 16; 17 |]

              let c1, c2 = Crossover.uniform 1.0 rng p1 p2

              Expect.equal c1.Genes p1.Genes "first child should always take the first parent's genes"
              Expect.equal c2.Genes p2.Genes "second child should always take the second parent's genes"

          testCase "at rate 0.0, children are exactly swapped"
          <| fun _ ->
              // NextDouble() never returns a negative value, so "< 0.0" is always false -
              // fully deterministic, not just overwhelmingly likely.
              let p1 = makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7 |]
              let p2 = makeChromosome [| 10; 11; 12; 13; 14; 15; 16; 17 |]

              let c1, c2 = Crossover.uniform 0.0 rng p1 p2

              Expect.equal c1.Genes p2.Genes "first child should always take the second parent's genes"
              Expect.equal c2.Genes p1.Genes "second child should always take the first parent's genes"

          testCase "at every position, the two children take opposite parents' genes"
          <| fun _ ->
              let p1 = makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7 |]
              let p2 = makeChromosome [| 10; 11; 12; 13; 14; 15; 16; 17 |]

              for _ in 1..100 do
                  let c1, c2 = Crossover.uniform 0.5 rng p1 p2

                  for i in 0 .. p1.Genes.Length - 1 do
                      if c1.Genes.[i] = p1.Genes.[i] then
                          Expect.equal c2.Genes.[i] p2.Genes.[i] "when the first child keeps p1's gene, the second should keep p2's"
                      else
                          Expect.equal c1.Genes.[i] p2.Genes.[i] "the first child's gene should come from one of the two parents"
                          Expect.equal c2.Genes.[i] p1.Genes.[i] "when the first child takes p2's gene, the second should take p1's"

          testCase "does not mutate the parent chromosomes"
          <| fun _ ->
              let p1 = makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7 |]
              let p2 = makeChromosome [| 10; 11; 12; 13; 14; 15; 16; 17 |]
              let p1GenesBefore = Array.copy p1.Genes
              let p2GenesBefore = Array.copy p2.Genes

              Crossover.uniform 0.5 rng p1 p2 |> ignore

              Expect.equal p1.Genes p1GenesBefore "first parent's genes should be unchanged"
              Expect.equal p2.Genes p2GenesBefore "second parent's genes should be unchanged"

          testCase "regression: rejects a rate outside [0, 1]"
          <| fun _ ->
              let p1 = makeChromosome [| 0; 1; 2; 3 |]
              let p2 = makeChromosome [| 10; 11; 12; 13 |]

              Expect.throwsT<System.ArgumentException>
                  (fun () -> Crossover.uniform -0.1 rng p1 p2 |> ignore)
                  "a negative rate should be rejected"

              Expect.throwsT<System.ArgumentException>
                  (fun () -> Crossover.uniform 1.1 rng p1 p2 |> ignore)
                  "a rate above 1.0 should be rejected"

          testCase "resets Age and Fitness for both children, regardless of the parents'"
          <| fun _ ->
              let p1 = { makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7 |] with Fitness = 99.0; Age = 7 }
              let p2 = { makeChromosome [| 10; 11; 12; 13; 14; 15; 16; 17 |] with Fitness = 42.0; Age = 3 }

              let c1, c2 = Crossover.uniform 0.5 rng p1 p2

              Expect.equal c1.Age 0 "the first child should start at Age 0, not inherit p1's Age"
              Expect.equal c2.Age 0 "the second child should start at Age 0, not inherit p2's Age"
              Expect.equal c1.Fitness 0.0 "the first child's Fitness should be reset, not inherit p1's"
              Expect.equal c2.Fitness 0.0 "the second child's Fitness should be reset, not inherit p2's" ]

[<Tests>]
let wholeArithmeticCrossoverTests =
    testList
        "Crossover.wholeArithmeticCrossover"
        [ testCase "children have the same length as the parents"
          <| fun _ ->
              let p1 = makeChromosome [| 0.0; 1.0; 2.0; 3.0 |]
              let p2 = makeChromosome [| 10.0; 11.0; 12.0; 13.0 |]

              let c1, c2 = Crossover.wholeArithmeticCrossover 0.3 rng p1 p2

              Expect.equal c1.Genes.Length p1.Genes.Length "first child should match parent length"
              Expect.equal c2.Genes.Length p2.Genes.Length "second child should match parent length"

          testCase "at alpha 1.0, children exactly match the parents"
          <| fun _ ->
              // This is arithmetic, not random, so this is exactly deterministic:
              // x*1 + y*0 = x and x*0 + y*1 = y for every position.
              let p1 = makeChromosome [| 0.0; 1.0; 2.0; 3.0 |]
              let p2 = makeChromosome [| 10.0; 11.0; 12.0; 13.0 |]

              let c1, c2 = Crossover.wholeArithmeticCrossover 1.0 rng p1 p2

              Expect.equal c1.Genes p1.Genes "first child should equal the first parent"
              Expect.equal c2.Genes p2.Genes "second child should equal the second parent"

          testCase "at alpha 0.0, children are exactly swapped"
          <| fun _ ->
              let p1 = makeChromosome [| 0.0; 1.0; 2.0; 3.0 |]
              let p2 = makeChromosome [| 10.0; 11.0; 12.0; 13.0 |]

              let c1, c2 = Crossover.wholeArithmeticCrossover 0.0 rng p1 p2

              Expect.equal c1.Genes p2.Genes "first child should equal the second parent"
              Expect.equal c2.Genes p1.Genes "second child should equal the first parent"

          testCase "at alpha 0.5, both children are the pointwise average of the parents"
          <| fun _ ->
              let p1 = makeChromosome [| 0.0; 1.0; 2.0; 3.0 |]
              let p2 = makeChromosome [| 10.0; 11.0; 12.0; 13.0 |]
              let expected = [| 5.0; 6.0; 7.0; 8.0 |]

              let c1, c2 = Crossover.wholeArithmeticCrossover 0.5 rng p1 p2

              Expect.equal c1.Genes expected "first child should be the pointwise average"
              Expect.equal c2.Genes expected "second child should be the pointwise average"

          testCase "does not mutate the parent chromosomes"
          <| fun _ ->
              let p1 = makeChromosome [| 0.0; 1.0; 2.0; 3.0 |]
              let p2 = makeChromosome [| 10.0; 11.0; 12.0; 13.0 |]
              let p1GenesBefore = Array.copy p1.Genes
              let p2GenesBefore = Array.copy p2.Genes

              Crossover.wholeArithmeticCrossover 0.3 rng p1 p2 |> ignore

              Expect.equal p1.Genes p1GenesBefore "first parent's genes should be unchanged"
              Expect.equal p2.Genes p2GenesBefore "second parent's genes should be unchanged"

          testCase "resets Age and Fitness for both children, regardless of the parents'"
          <| fun _ ->
              let p1 = { makeChromosome [| 0.0; 1.0; 2.0; 3.0 |] with Fitness = 99.0; Age = 7 }
              let p2 = { makeChromosome [| 10.0; 11.0; 12.0; 13.0 |] with Fitness = 42.0; Age = 3 }

              let c1, c2 = Crossover.wholeArithmeticCrossover 0.3 rng p1 p2

              Expect.equal c1.Age 0 "the first child should start at Age 0, not inherit p1's Age"
              Expect.equal c2.Age 0 "the second child should start at Age 0, not inherit p2's Age"
              Expect.equal c1.Fitness 0.0 "the first child's Fitness should be reset, not inherit p1's"
              Expect.equal c2.Fitness 0.0 "the second child's Fitness should be reset, not inherit p2's" ]
