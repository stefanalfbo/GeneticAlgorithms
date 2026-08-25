module GeneticAlgorithms.Tests.ChromosomeTests

open Expecto
open GeneticAlgorithms

let private chromosome genes : Chromosome<'Gene> =
    { Genes = genes
      Fitness = 0.0
      Age = 0 }

[<Tests>]
let chromosomeTests =
    testList
        "Chromosome"
        [ testCase "Size returns the number of genes"
          <| fun _ ->
              let chromosome = chromosome [| 1; 2; 3; 4 |]

              Expect.equal chromosome.Size 4 "Size should match the length of Genes"

          testCase "Size is zero when the chromosome has no genes"
          <| fun _ ->
              let chromosome = chromosome Array.empty<int>

              Expect.equal chromosome.Size 0 "an empty chromosome should have size zero"

          testCase "supports non-numeric gene types"
          <| fun _ ->
              let chromosome = chromosome [| "red"; "green"; "blue" |]

              Expect.equal chromosome.Size 3 "Size should work independently of the gene type" ]
