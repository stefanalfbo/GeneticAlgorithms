module GeneticAlgorithms.Tests.GeneticTests

open Expecto
open GeneticAlgorithms

let private makeChromosome genes : Chromosome<int> = { Genes = genes; Fitness = 0.0; Age = 0 }

let private opts: Options<int> =
    { PopulationSize = 4
      SelectionRate = 0.8
      SelectionFn = Selection.elite
      MutationRate = 0.05
      OnGeneration = fun _ _ -> () }

[<Tests>]
let evaluateTests =
    testList
        "Genetic.evaluate"
        [ testCase "applies the fitness function to each chromosome"
          <| fun _ ->
              let population = [| makeChromosome [| 1 |]; makeChromosome [| 2 |] |]
              let fitness (c: Chromosome<int>) = float c.Genes.[0]

              let result = Genetic.evaluate population fitness

              Expect.all result (fun c -> c.Fitness = float c.Genes.[0]) "fitness should match the fitness function"

          testCase "increments the age of each chromosome"
          <| fun _ ->
              let population = [| makeChromosome [| 1 |] |]

              let result = Genetic.evaluate population (fun _ -> 0.0)

              Expect.equal result.[0].Age 1 "age should be incremented by one"

          testCase "sorts the population by descending fitness"
          <| fun _ ->
              let population =
                  [| makeChromosome [| 1 |]; makeChromosome [| 3 |]; makeChromosome [| 2 |] |]

              let fitness (c: Chromosome<int>) = float c.Genes.[0]

              let result = Genetic.evaluate population fitness

              Expect.equal (result |> Array.map (fun c -> c.Genes.[0])) [| 3; 2; 1 |] "should be sorted descending" ]

[<Tests>]
let crossoverTests =
    testList
        "Genetic.crossover"
        [ testCase "produces two children per pair"
          <| fun _ ->
              let p1 = makeChromosome [| 1; 2; 3; 4 |]
              let p2 = makeChromosome [| 5; 6; 7; 8 |]

              let result = Genetic.crossover [| (p1, p2) |]

              Expect.equal result.Length 2 "should produce two children per pair of parents"

          testCase "children genes have the same length as the parents"
          <| fun _ ->
              let p1 = makeChromosome [| 1; 2; 3; 4 |]
              let p2 = makeChromosome [| 5; 6; 7; 8 |]

              let result = Genetic.crossover [| (p1, p2) |]

              Expect.all result (fun c -> c.Genes.Length = p1.Genes.Length) "gene count should be preserved"

          testCase "children genes are recombined from both parents"
          <| fun _ ->
              let p1 = makeChromosome [| 1; 2; 3; 4 |]
              let p2 = makeChromosome [| 5; 6; 7; 8 |]

              let result = Genetic.crossover [| (p1, p2) |]

              let allGenes = result |> Array.collect (fun c -> c.Genes) |> Set.ofArray
              let expectedGenes = Array.append p1.Genes p2.Genes |> Set.ofArray

              Expect.equal allGenes expectedGenes "children should only contain genes from their parents" ]

[<Tests>]
let mutationTests =
    testList
        "Genetic.mutation"
        [ testCase "preserves the population size"
          <| fun _ ->
              let population = Array.init 20 (fun i -> makeChromosome [| i |])

              let result = Genetic.mutation opts population

              Expect.equal result.Length population.Length "population size should be unchanged"

          testCase "mutated genes are a permutation of the original genes"
          <| fun _ ->
              let chromosome = makeChromosome [| 1; 2; 3; 4; 5 |]
              let population = Array.create 50 chromosome

              let result = Genetic.mutation opts population

              for c in result do
                  Expect.equal c.Genes.Length chromosome.Genes.Length "gene count should be preserved"

                  Expect.containsAll
                      c.Genes
                      chromosome.Genes
                      "mutated genes should be a permutation of the original genes" ]

[<Tests>]
let initializeTests =
    testList
        "Genetic.initialize"
        [ testCase "creates population_size chromosomes using the genotype function"
          <| fun _ ->
              let mutable counter = 0

              let genotype () =
                  counter <- counter + 1
                  makeChromosome [| counter |]

              let result = Genetic.initialize genotype { opts with PopulationSize = 5 }

              Expect.equal result.Length 5 "should create population_size chromosomes"

              Expect.equal
                  (result |> Array.map (fun c -> c.Genes.[0]))
                  [| 1; 2; 3; 4; 5 |]
                  "should call the genotype function for each chromosome" ]

[<Tests>]
let runTests =
    testList
        "Genetic.run"
        [ testCase "returns the fittest chromosome once terminate is true"
          <| fun _ ->
              let genes = [| 3; 7; 1; 9; 4 |]
              let mutable index = -1

              let genotype () =
                  index <- index + 1
                  makeChromosome [| genes.[index] |]

              let problem =
                  { Genotype = genotype
                    FitnessFunction = fun c -> float c.Genes.[0]
                    Terminate = fun _ _ _ -> true }

              let result = Genetic.run problem { opts with PopulationSize = genes.Length }

              Expect.equal result.Genes.[0] 9 "should return the chromosome with the highest fitness"

          testCase "passes the current generation to terminate"
          <| fun _ ->
              let observedGenerations = System.Collections.Generic.List<int>()

              let genotype () = makeChromosome [| 0; 1 |]

              let problem =
                  { Genotype = genotype
                    FitnessFunction = fun _ -> 0.0
                    Terminate =
                      fun _ generation _ ->
                          observedGenerations.Add generation
                          generation >= 2 }

              Genetic.run problem opts |> ignore

              Expect.sequenceEqual
                  observedGenerations
                  [ 0; 1; 2 ]
                  "terminate should see each generation in order starting from zero"

          testCase "passes the current temperature to terminate"
          <| fun _ ->
              let observedTemperatures = System.Collections.Generic.List<float>()

              let genotype () = makeChromosome [| 9 |]

              let problem =
                  { Genotype = genotype
                    FitnessFunction = fun c -> float c.Genes.[0]
                    Terminate =
                      fun _ generation temperature ->
                          observedTemperatures.Add temperature
                          generation >= 2 }

              Genetic.run problem opts |> ignore

              let roundedTemperatures =
                  observedTemperatures
                  |> Seq.map (fun value -> System.Math.Round(value, 3))
                  |> Seq.toList

              Expect.sequenceEqual
                  roundedTemperatures
                  [ 7.2; 5.76; 4.608 ]
                  "terminate should receive the computed temperature for each generation" ]
