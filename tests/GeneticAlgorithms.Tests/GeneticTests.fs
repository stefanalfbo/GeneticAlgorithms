module GeneticAlgorithms.Tests.GeneticTests

open Expecto
open GeneticAlgorithms

let private makeChromosome genes : Chromosome<int> = { Genes = genes; Fitness = 0.0; Age = 0 }

let private rng = System.Random.Shared

let private opts: Options<int> =
    { PopulationSize = 4
      SelectionRate = 0.8
      SelectionFn = Selection.elite
      CrossoverFn = Crossover.singlePoint
      MutationRate = 0.05
      MutationFn = Mutation.scramble
      ReinsertionFn = Reinsertion.``pure``
      Probe = fun _ -> ()
      Random = rng }

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

              let result = Genetic.crossover (Crossover.singlePoint rng) [| (p1, p2) |]

              Expect.equal result.Length 2 "should produce two children per pair of parents"

          testCase "children genes have the same length as the parents"
          <| fun _ ->
              let p1 = makeChromosome [| 1; 2; 3; 4 |]
              let p2 = makeChromosome [| 5; 6; 7; 8 |]

              let result = Genetic.crossover (Crossover.singlePoint rng) [| (p1, p2) |]

              Expect.all result (fun c -> c.Genes.Length = p1.Genes.Length) "gene count should be preserved"

          testCase "children genes are recombined from both parents"
          <| fun _ ->
              let p1 = makeChromosome [| 1; 2; 3; 4 |]
              let p2 = makeChromosome [| 5; 6; 7; 8 |]

              let result = Genetic.crossover (Crossover.singlePoint rng) [| (p1, p2) |]

              let allGenes = result |> Array.collect (fun c -> c.Genes) |> Set.ofArray
              let expectedGenes = Array.append p1.Genes p2.Genes |> Set.ofArray

              Expect.equal allGenes expectedGenes "children should only contain genes from their parents" ]

[<Tests>]
let mutationTests =
    testList
        "Genetic.mutation"
        [ testCase "returns floor(population size * mutation rate) mutants"
          <| fun _ ->
              let population = Array.init 20 (fun i -> makeChromosome [| i |])

              let result = Genetic.mutation opts population

              Expect.equal result.Length 1 "20 * 0.05 = 1 mutant"

          testCase "rounds the sample size down rather than up"
          <| fun _ ->
              let population = Array.init 5 (fun i -> makeChromosome [| i |])

              let result = Genetic.mutation opts population

              Expect.equal result.Length 0 "5 * 0.05 = 0.25, floored to 0"

          testCase "mutated genes are a permutation of the original genes"
          <| fun _ ->
              let chromosome = makeChromosome [| 1; 2; 3; 4; 5 |]
              let population = Array.create 50 chromosome

              let result = Genetic.mutation opts population

              Expect.equal result.Length 2 "50 * 0.05 = 2 mutants"

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

              let genotype (_: System.Random) =
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

              let genotype (_: System.Random) =
                  index <- index + 1
                  makeChromosome [| genes.[index] |]

              let problem =
                  { Genotype = genotype
                    FitnessFunction = fun c -> float c.Genes.[0]
                    Terminate = fun _ _ _ -> true }

              let result = Genetic.run problem { opts with PopulationSize = genes.Length }

              Expect.equal result.Genes.[0] 9 "should return the chromosome with the highest fitness"

          testCase "regression: rejects a zero PopulationSize"
          <| fun _ ->
              // Bug: PopulationSize = 0 used to make Genetic.initialize build an empty
              // population silently, which Genetic.evolve then indexed into (`nextPopulation.
              // [0]`) before problem.Terminate was ever called, crashing with an unrelated
              // IndexOutOfRangeException instead of a clear, immediate error.
              let mutable terminateWasCalled = false

              let problem =
                  { Genotype = fun _ -> makeChromosome [| 0 |]
                    FitnessFunction = fun _ -> 0.0
                    Terminate =
                      fun _ _ _ ->
                          terminateWasCalled <- true
                          true }

              Expect.throwsT<System.ArgumentException>
                  (fun () -> Genetic.run problem { opts with PopulationSize = 0 } |> ignore)
                  "a zero PopulationSize should be rejected"

              Expect.isFalse terminateWasCalled "terminate should never be called for an invalid PopulationSize"

          testCase "regression: rejects a negative PopulationSize"
          <| fun _ ->
              let problem =
                  { Genotype = fun _ -> makeChromosome [| 0 |]
                    FitnessFunction = fun _ -> 0.0
                    Terminate = fun _ _ _ -> true }

              Expect.throwsT<System.ArgumentException>
                  (fun () -> Genetic.run problem { opts with PopulationSize = -1 } |> ignore)
                  "a negative PopulationSize should be rejected"

          testCase "regression: rejects a null Random"
          <| fun _ ->
              let problem =
                  { Genotype = fun _ -> makeChromosome [| 0 |]
                    FitnessFunction = fun _ -> 0.0
                    Terminate = fun _ _ _ -> true }

              Expect.throwsT<System.ArgumentException>
                  (fun () -> Genetic.run problem { opts with Random = null } |> ignore)
                  "a null Random should be rejected"

          testCase "regression: rejects a SelectionRate outside [0, 1]"
          <| fun _ ->
              let problem =
                  { Genotype = fun _ -> makeChromosome [| 0 |]
                    FitnessFunction = fun _ -> 0.0
                    Terminate = fun _ _ _ -> true }

              Expect.throwsT<System.ArgumentException>
                  (fun () -> Genetic.run problem { opts with SelectionRate = -0.1 } |> ignore)
                  "a negative SelectionRate should be rejected"

              Expect.throwsT<System.ArgumentException>
                  (fun () -> Genetic.run problem { opts with SelectionRate = 1.1 } |> ignore)
                  "a SelectionRate above 1.0 should be rejected"

          testCase "regression: rejects a MutationRate outside [0, 1]"
          <| fun _ ->
              let problem =
                  { Genotype = fun _ -> makeChromosome [| 0 |]
                    FitnessFunction = fun _ -> 0.0
                    Terminate = fun _ _ _ -> true }

              Expect.throwsT<System.ArgumentException>
                  (fun () -> Genetic.run problem { opts with MutationRate = -0.1 } |> ignore)
                  "a negative MutationRate should be rejected"

              Expect.throwsT<System.ArgumentException>
                  (fun () -> Genetic.run problem { opts with MutationRate = 1.1 } |> ignore)
                  "a MutationRate above 1.0 should be rejected"

          testCase "passes the current generation to terminate"
          <| fun _ ->
              let observedGenerations = System.Collections.Generic.List<int>()

              let genotype (_: System.Random) = makeChromosome [| 0; 1 |]

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

              let genotype (_: System.Random) = makeChromosome [| 9 |]

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
                  "terminate should receive the computed temperature for each generation"

          testCase "passes the full generation snapshot to the probe"
          <| fun _ ->
              let observedGenerations = System.Collections.Generic.List<int>()
              let observedPopulationSizes = System.Collections.Generic.List<int>()
              let observedBestFitnesses = System.Collections.Generic.List<float>()
              let observedTemperatures = System.Collections.Generic.List<float>()

              let genotype (_: System.Random) = makeChromosome [| 9 |]

              let problem =
                  { Genotype = genotype
                    FitnessFunction = fun c -> float c.Genes.[0]
                    Terminate = fun _ generation _ -> generation >= 2 }

              let probeOpts =
                  { opts with
                      Probe =
                          fun info ->
                              observedGenerations.Add info.Generation
                              observedPopulationSizes.Add info.Population.Length
                              observedBestFitnesses.Add info.Best.Fitness
                              observedTemperatures.Add info.Temperature }

              Genetic.run problem probeOpts |> ignore

              Expect.sequenceEqual
                  observedGenerations
                  [ 0; 1; 2 ]
                  "the probe should see each generation in order starting from zero"

              Expect.all
                  observedPopulationSizes
                  ((=) probeOpts.PopulationSize)
                  "the probe should see the full, unfiltered population every generation"

              Expect.all
                  observedBestFitnesses
                  ((=) 9.0)
                  "the probe should see the best chromosome's fitness"

              let roundedTemperatures =
                  observedTemperatures |> Seq.map (fun value -> System.Math.Round(value, 3)) |> Seq.toList

              Expect.sequenceEqual
                  roundedTemperatures
                  [ 7.2; 5.76; 4.608 ]
                  "the probe should see the same computed temperature terminate does"

          testCase "regression: reinsertion accounting stays correct for a generation with a SelectionFn that permits duplicates"
          <| fun _ ->
              // End-to-end regression test: with SelectionRate 0.8 + MutationRate 0.05 +
              // Reinsertion.elitist's survivalRate 0.15 summing to 1.0, population size is
              // meant to stay stable from one generation to the next. That arithmetic relies
              // on Selection.select reporting distinct parents, not raw (possibly duplicated)
              // selections, when it hands off to ReinsertionFn - otherwise Selection.roulette
              // (which can legitimately select the same chromosome more than once, just like
              // tournament/boltzmann/stochasticUniversalSampling) would inflate "old"
              // survivors and grow the population whenever that happens.
              //
              // This only checks a single generation transition (0 -> 1), not population
              // stability over many generations: elitist reinsertion is specifically
              // designed to carry the fittest chromosomes forward unchanged, so over enough
              // generations the population will end up genuinely holding more than one
              // structurally-identical copy of the same chromosome. Once that happens,
              // Seq.except (a value-based set difference) can no longer distinguish "the one
              // copy that was selected" from "the other copies that weren't", and starts
              // excluding all of them from leftover - a separate, pre-existing limitation of
              // comparing chromosomes by value rather than identity, not something this fix
              // addresses. Genes are wide-range random values (not a small counter)
              // specifically so this single generation doesn't hit that separate issue by
              // coincidence.
              let genotype (rng: System.Random) =
                  makeChromosome (Array.init 10 (fun _ -> rng.Next(0, 1_000_000)))

              let observedPopulationSizes = System.Collections.Generic.List<int>()

              let problem =
                  { Genotype = genotype
                    FitnessFunction = fun c -> c.Genes |> Array.sumBy float
                    Terminate = fun _ generation _ -> generation >= 1 }

              // PopulationSize = 20 keeps every intermediate count (0.8/0.05/0.15 of 20)
              // an exact integer - population.Length - population.Length%2, MutationRate,
              // and the elitist survivalRate all divide evenly, so nothing here is masked
              // or caused by ordinary floor-rounding of fractional counts elsewhere in the
              // pipeline.
              let duplicateSelectionOpts =
                  { opts with
                      PopulationSize = 20
                      SelectionFn = Selection.roulette
                      ReinsertionFn = Reinsertion.elitist 0.15
                      Probe = fun info -> observedPopulationSizes.Add info.Population.Length }

              Genetic.run problem duplicateSelectionOpts |> ignore

              Expect.all
                  observedPopulationSizes
                  ((=) duplicateSelectionOpts.PopulationSize)
                  "population size should stay stable across this generation, even though roulette selection can pick the same chromosome more than once"

          testCase "regression: PopulationSize stays exact despite independent rounding in selection, mutation, and reinsertion"
          <| fun _ ->
              // Bug: Options.create's defaults (SelectionRate 0.8 + MutationRate 0.05 +
              // Reinsertion.elitist's survivalRate 0.15) are meant to sum to 1.0 and keep
              // population size stable, but Selection.select, Genetic.mutation, and
              // Reinsertion.elitist each round their own fractional share of PopulationSize
              // independently. For PopulationSize = 8: selection rounds 6.4 up to the
              // nearest even number (6) and consumes exactly 6 parents as crossover
              // children, mutation floors 0.4 down to 0 mutants, and elitist floors
              // (parents + leftover) * 0.15 = 1.2 down to 1 survivor - 6 + 0 + 1 = 7, one
              // short of 8, every generation, with no rate combination or population size
              // able to guarantee otherwise by construction.
              let genotype (rng: System.Random) = makeChromosome [| rng.Next(0, 1_000_000) |]

              let observedPopulationSizes = System.Collections.Generic.List<int>()

              let problem =
                  { Genotype = genotype
                    FitnessFunction = fun c -> float c.Genes.[0]
                    Terminate = fun _ generation _ -> generation >= 5 }

              let smallOpts =
                  { Options.create 8 with
                      Probe = fun info -> observedPopulationSizes.Add info.Population.Length }

              Genetic.run problem smallOpts |> ignore

              Expect.all
                  observedPopulationSizes
                  ((=) 8)
                  "population size should stay exactly 8 every generation, not drift to 7"

          testCase "regression: identical seeds reproduce an entire run bit-for-bit, not just Shuffle.fisherYates in isolation"
          <| fun _ ->
              // RNG injection touches every stage of the pipeline - initial population,
              // selection, crossover, mutation, and reinsertion - so this deliberately
              // exercises randomness at every one of those stages (roulette selection,
              // single-point crossover, scramble mutation, uniform reinsertion) rather than
              // a mostly-deterministic combination like SelectionFn = elite /
              // ReinsertionFn = pure, to prove the whole run reproduces end to end when
              // Options.Random is seeded - not just that Shuffle.fisherYates alone does.
              let genotype (rng: System.Random) =
                  makeChromosome (Array.init 10 (fun _ -> rng.Next(0, 1_000_000)))

              let problem =
                  { Genotype = genotype
                    FitnessFunction = fun c -> c.Genes |> Array.sumBy float
                    Terminate = fun _ generation _ -> generation >= 10 }

              let makeOpts (seed: int) (observedPopulations: ResizeArray<Chromosome<int> array>) =
                  { PopulationSize = 20
                    SelectionRate = 0.8
                    SelectionFn = Selection.roulette
                    CrossoverFn = Crossover.singlePoint
                    MutationRate = 0.1
                    MutationFn = Mutation.scramble
                    ReinsertionFn = Reinsertion.uniform 0.15
                    Probe = fun info -> observedPopulations.Add info.Population
                    Random = System.Random(seed) }

              let firstPopulations = ResizeArray<Chromosome<int> array>()
              let secondPopulations = ResizeArray<Chromosome<int> array>()

              let firstResult = Genetic.run problem (makeOpts 42 firstPopulations)
              let secondResult = Genetic.run problem (makeOpts 42 secondPopulations)

              Expect.equal secondResult firstResult "identical seeds should reproduce an identical final chromosome"

              Expect.equal
                  (secondPopulations |> Seq.toList)
                  (firstPopulations |> Seq.toList)
                  "identical seeds should reproduce an identical population at every generation, not just the final result"

              let thirdPopulations = ResizeArray<Chromosome<int> array>()
              let thirdResult = Genetic.run problem (makeOpts 43 thirdPopulations)

              Expect.notEqual
                  thirdResult
                  firstResult
                  "a different seed should not coincidentally reproduce the same result - otherwise this test would pass even if Options.Random were ignored entirely" ]
