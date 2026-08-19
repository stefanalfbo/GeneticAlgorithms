module GeneticAlgorithms.Tests.CrossoverTests

open Expecto
open GeneticAlgorithms

let private makeChromosome genes = { Genes = genes; Fitness = 0.0; Age = 0 }

[<Tests>]
let orderOneCrossoverTests =
    testList
        "Crossover.orderOneCrossover"
        [ testCase "children have the same length as the parents"
          <| fun _ ->
              let p1 = makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7 |]
              let p2 = makeChromosome [| 7; 6; 5; 4; 3; 2; 1; 0 |]

              for _ in 1..100 do
                  let c1, c2 = Crossover.orderOneCrossover p1 p2

                  Expect.equal c1.Genes.Length p1.Genes.Length "first child should match parent length"
                  Expect.equal c2.Genes.Length p2.Genes.Length "second child should match parent length"

          testCase "each child is a permutation of the parents' genes"
          <| fun _ ->
              let p1 = makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7 |]
              let p2 = makeChromosome [| 7; 6; 5; 4; 3; 2; 1; 0 |]
              let expectedGenes = Set.ofArray p1.Genes

              for _ in 1..100 do
                  let c1, c2 = Crossover.orderOneCrossover p1 p2

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

              Crossover.orderOneCrossover p1 p2 |> ignore

              Expect.equal p1.Genes p1GenesBefore "first parent's genes should be unchanged"
              Expect.equal p2.Genes p2GenesBefore "second parent's genes should be unchanged" ]

[<Tests>]
let uniformTests =
    testList
        "Crossover.uniform"
        [ testCase "children have the same length as the parents"
          <| fun _ ->
              let p1 = makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7 |]
              let p2 = makeChromosome [| 10; 11; 12; 13; 14; 15; 16; 17 |]

              for _ in 1..100 do
                  let c1, c2 = Crossover.uniform 0.5 p1 p2

                  Expect.equal c1.Genes.Length p1.Genes.Length "first child should match parent length"
                  Expect.equal c2.Genes.Length p2.Genes.Length "second child should match parent length"

          testCase "at rate 1.0, children exactly match the parents"
          <| fun _ ->
              // NextDouble() never returns 1.0, so "< 1.0" is always true - this is fully
              // deterministic, not just overwhelmingly likely.
              let p1 = makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7 |]
              let p2 = makeChromosome [| 10; 11; 12; 13; 14; 15; 16; 17 |]

              let c1, c2 = Crossover.uniform 1.0 p1 p2

              Expect.equal c1.Genes p1.Genes "first child should always take the first parent's genes"
              Expect.equal c2.Genes p2.Genes "second child should always take the second parent's genes"

          testCase "at rate 0.0, children are exactly swapped"
          <| fun _ ->
              // NextDouble() never returns a negative value, so "< 0.0" is always false -
              // fully deterministic, not just overwhelmingly likely.
              let p1 = makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7 |]
              let p2 = makeChromosome [| 10; 11; 12; 13; 14; 15; 16; 17 |]

              let c1, c2 = Crossover.uniform 0.0 p1 p2

              Expect.equal c1.Genes p2.Genes "first child should always take the second parent's genes"
              Expect.equal c2.Genes p1.Genes "second child should always take the first parent's genes"

          testCase "at every position, the two children take opposite parents' genes"
          <| fun _ ->
              let p1 = makeChromosome [| 0; 1; 2; 3; 4; 5; 6; 7 |]
              let p2 = makeChromosome [| 10; 11; 12; 13; 14; 15; 16; 17 |]

              for _ in 1..100 do
                  let c1, c2 = Crossover.uniform 0.5 p1 p2

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

              Crossover.uniform 0.5 p1 p2 |> ignore

              Expect.equal p1.Genes p1GenesBefore "first parent's genes should be unchanged"
              Expect.equal p2.Genes p2GenesBefore "second parent's genes should be unchanged" ]

[<Tests>]
let wholeArithmeticCrossoverTests =
    testList
        "Crossover.wholeArithmeticCrossover"
        [ testCase "children have the same length as the parents"
          <| fun _ ->
              let p1 = makeChromosome [| 0.0; 1.0; 2.0; 3.0 |]
              let p2 = makeChromosome [| 10.0; 11.0; 12.0; 13.0 |]

              let c1, c2 = Crossover.wholeArithmeticCrossover 0.3 p1 p2

              Expect.equal c1.Genes.Length p1.Genes.Length "first child should match parent length"
              Expect.equal c2.Genes.Length p2.Genes.Length "second child should match parent length"

          testCase "at alpha 1.0, children exactly match the parents"
          <| fun _ ->
              // This is arithmetic, not random, so this is exactly deterministic:
              // x*1 + y*0 = x and x*0 + y*1 = y for every position.
              let p1 = makeChromosome [| 0.0; 1.0; 2.0; 3.0 |]
              let p2 = makeChromosome [| 10.0; 11.0; 12.0; 13.0 |]

              let c1, c2 = Crossover.wholeArithmeticCrossover 1.0 p1 p2

              Expect.equal c1.Genes p1.Genes "first child should equal the first parent"
              Expect.equal c2.Genes p2.Genes "second child should equal the second parent"

          testCase "at alpha 0.0, children are exactly swapped"
          <| fun _ ->
              let p1 = makeChromosome [| 0.0; 1.0; 2.0; 3.0 |]
              let p2 = makeChromosome [| 10.0; 11.0; 12.0; 13.0 |]

              let c1, c2 = Crossover.wholeArithmeticCrossover 0.0 p1 p2

              Expect.equal c1.Genes p2.Genes "first child should equal the second parent"
              Expect.equal c2.Genes p1.Genes "second child should equal the first parent"

          testCase "at alpha 0.5, both children are the pointwise average of the parents"
          <| fun _ ->
              let p1 = makeChromosome [| 0.0; 1.0; 2.0; 3.0 |]
              let p2 = makeChromosome [| 10.0; 11.0; 12.0; 13.0 |]
              let expected = [| 5.0; 6.0; 7.0; 8.0 |]

              let c1, c2 = Crossover.wholeArithmeticCrossover 0.5 p1 p2

              Expect.equal c1.Genes expected "first child should be the pointwise average"
              Expect.equal c2.Genes expected "second child should be the pointwise average"

          testCase "does not mutate the parent chromosomes"
          <| fun _ ->
              let p1 = makeChromosome [| 0.0; 1.0; 2.0; 3.0 |]
              let p2 = makeChromosome [| 10.0; 11.0; 12.0; 13.0 |]
              let p1GenesBefore = Array.copy p1.Genes
              let p2GenesBefore = Array.copy p2.Genes

              Crossover.wholeArithmeticCrossover 0.3 p1 p2 |> ignore

              Expect.equal p1.Genes p1GenesBefore "first parent's genes should be unchanged"
              Expect.equal p2.Genes p2GenesBefore "second parent's genes should be unchanged" ]
