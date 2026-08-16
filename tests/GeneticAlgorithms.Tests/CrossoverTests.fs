module GeneticAlgorithms.Tests.CrossoverTests

open Expecto
open GeneticAlgorithms

let private makeChromosome genes : Chromosome<int> = { Genes = genes; Fitness = 0.0; Age = 0 }

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
