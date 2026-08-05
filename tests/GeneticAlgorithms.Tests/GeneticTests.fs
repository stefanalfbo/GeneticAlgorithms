module GeneticAlgorithms.Tests.GeneticTests

open Expecto
open GeneticAlgorithms

let private makeChromosome genes : Chromosome<int> =
    { genes = genes
      size = Array.length genes
      fitness = 0.0
      age = 0 }

let private opts = { population_size = 4 }

[<Tests>]
let evaluateTests =
    testList
        "Genetic.evaluate"
        [ testCase "applies the fitness function to each chromosome"
          <| fun _ ->
              let population = [| makeChromosome [| 1 |]; makeChromosome [| 2 |] |]
              let fitness (c: Chromosome<int>) = float c.genes.[0]

              let result = Genetic.evaluate population fitness opts

              Expect.all result (fun c -> c.fitness = float c.genes.[0]) "fitness should match the fitness function"

          testCase "increments the age of each chromosome"
          <| fun _ ->
              let population = [| makeChromosome [| 1 |] |]

              let result = Genetic.evaluate population (fun _ -> 0.0) opts

              Expect.equal result.[0].age 1 "age should be incremented by one"

          testCase "sorts the population by descending fitness"
          <| fun _ ->
              let population =
                  [| makeChromosome [| 1 |]; makeChromosome [| 3 |]; makeChromosome [| 2 |] |]

              let fitness (c: Chromosome<int>) = float c.genes.[0]

              let result = Genetic.evaluate population fitness opts

              Expect.equal (result |> Array.map (fun c -> c.genes.[0])) [| 3; 2; 1 |] "should be sorted descending" ]

[<Tests>]
let selectTests =
    testList
        "Genetic.select"
        [ testCase "pairs up an even population"
          <| fun _ ->
              let population =
                  [| makeChromosome [| 1 |]
                     makeChromosome [| 2 |]
                     makeChromosome [| 3 |]
                     makeChromosome [| 4 |] |]

              let result = Genetic.select opts population

              Expect.equal result.Length 2 "should produce population.Length / 2 pairs"
              Expect.equal result.[0] (population.[0], population.[1]) "first pair should be the first two chromosomes"
              Expect.equal result.[1] (population.[2], population.[3]) "second pair should be the last two chromosomes"

          testCase "pairs the leftover chromosome with itself for an odd population"
          <| fun _ ->
              let population =
                  [| makeChromosome [| 1 |]; makeChromosome [| 2 |]; makeChromosome [| 3 |] |]

              let result = Genetic.select opts population

              Expect.equal result.Length 2 "should produce two pairs"
              Expect.equal result.[1] (population.[2], population.[2]) "the leftover chromosome should pair with itself" ]

[<Tests>]
let crossoverTests =
    testList
        "Genetic.crossover"
        [ testCase "produces two children per pair"
          <| fun _ ->
              let p1 = makeChromosome [| 1; 2; 3; 4 |]
              let p2 = makeChromosome [| 5; 6; 7; 8 |]

              let result = Genetic.crossover opts [| (p1, p2) |]

              Expect.equal result.Length 2 "should produce two children per pair of parents"

          testCase "children genes have the same length as the parents"
          <| fun _ ->
              let p1 = makeChromosome [| 1; 2; 3; 4 |]
              let p2 = makeChromosome [| 5; 6; 7; 8 |]

              let result = Genetic.crossover opts [| (p1, p2) |]

              Expect.all result (fun c -> c.genes.Length = p1.genes.Length) "gene count should be preserved"

          testCase "children genes are recombined from both parents"
          <| fun _ ->
              let p1 = makeChromosome [| 1; 2; 3; 4 |]
              let p2 = makeChromosome [| 5; 6; 7; 8 |]

              let result = Genetic.crossover opts [| (p1, p2) |]

              let allGenes = result |> Array.collect (fun c -> c.genes) |> Set.ofArray
              let expectedGenes = Array.append p1.genes p2.genes |> Set.ofArray

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
                  Expect.equal c.genes.Length chromosome.genes.Length "gene count should be preserved"

                  Expect.containsAll
                      c.genes
                      chromosome.genes
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

              let result = Genetic.initialize genotype { population_size = 5 }

              Expect.equal result.Length 5 "should create population_size chromosomes"

              Expect.equal
                  (result |> Array.map (fun c -> c.genes.[0]))
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
                  { genotype = genotype
                    fitness_function = fun c -> float c.genes.[0]
                    terminate = fun _ _ _ -> true }

              let result = Genetic.run problem { population_size = genes.Length }

              Expect.equal result.genes.[0] 9 "should return the chromosome with the highest fitness"

          testCase "passes the current generation to terminate"
          <| fun _ ->
              let observedGenerations = System.Collections.Generic.List<int>()

              let genotype () = makeChromosome [| 0; 1 |]

              let problem =
                  { genotype = genotype
                    fitness_function = fun _ -> 0.0
                    terminate =
                      fun _ generation _ ->
                          observedGenerations.Add generation
                          generation >= 2 }

              Genetic.run problem { population_size = 4 } |> ignore

              Expect.sequenceEqual
                  observedGenerations
                  [ 0; 1; 2 ]
                  "terminate should see each generation in order starting from zero"

          testCase "passes the current temperature to terminate"
          <| fun _ ->
              let observedTemperatures = System.Collections.Generic.List<float>()

              let genotype () = makeChromosome [| 9 |]

              let problem =
                  { genotype = genotype
                    fitness_function = fun c -> float c.genes.[0]
                    terminate =
                      fun _ generation temperature ->
                          observedTemperatures.Add temperature
                          generation >= 2 }

              Genetic.run problem { population_size = 4 } |> ignore

              let roundedTemperatures =
                  observedTemperatures
                  |> Seq.map (fun value -> System.Math.Round(value, 3))
                  |> Seq.toList

              Expect.sequenceEqual
                  roundedTemperatures
                  [ 7.2; 5.76; 4.608 ]
                  "terminate should receive the computed temperature for each generation" ]
