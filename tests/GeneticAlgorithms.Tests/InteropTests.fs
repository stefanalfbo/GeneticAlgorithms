module GeneticAlgorithms.Tests.InteropTests

open System
open System.Collections.Generic
open Expecto
open GeneticAlgorithms

let private chromosome genes : Chromosome<int> =
    { Genes = genes
      Fitness = 0.0
      Age = 0 }

let private genotype = Func<Chromosome<int>>(fun () -> chromosome [| 7 |])
let private fitness = Func<Chromosome<int>, float>(fun candidate -> float candidate.Genes.[0])

let private terminate =
    Func<IEnumerable<Chromosome<int>>, int, float, bool>(fun _ _ _ -> true)

let private problem: Problem<int> =
    { Genotype = fun () -> chromosome [| 7 |]
      FitnessFunction = fun candidate -> float candidate.Genes.[0]
      Terminate = fun _ _ _ -> true }

let private options: Options<int> =
    { PopulationSize = 4
      SelectionRate = 0.8
      SelectionFn = Selection.elite
      CrossoverFn = Crossover.singlePoint
      MutationRate = 0.05
      MutationFn = Mutation.scramble
      OnGeneration = fun _ _ -> () }

let private expectSolution (solution: Chromosome<int>) =
    Expect.equal solution.Genes [| 7 |] "the configured genotype should be returned"
    Expect.equal solution.Fitness 7.0 "the configured fitness function should be applied"

[<Tests>]
let createChromosomeTests =
    testList
        "GeneticAlgorithm.CreateChromosome"
        [ testCase "initializes a chromosome"
          <| fun _ ->
              let result = GeneticAlgorithm.CreateChromosome [| 1; 2; 3 |]

              Expect.equal result.Genes [| 1; 2; 3 |] "genes should be preserved"
              Expect.equal result.Fitness 0.0 "fitness should start at zero"
              Expect.equal result.Age 0 "age should start at zero"

          testCase "copies the supplied gene array"
          <| fun _ ->
              let genes = [| 1; 2; 3 |]
              let result = GeneticAlgorithm.CreateChromosome genes

              genes.[0] <- 99

              Expect.equal result.Genes [| 1; 2; 3 |] "later changes to the input should not affect the chromosome"

          testCase "rejects null genes"
          <| fun _ ->
              Expect.throwsT<ArgumentNullException>
                  (fun _ -> GeneticAlgorithm.CreateChromosome(null: int array) |> ignore)
                  "genes should be required" ]

[<Tests>]
let createOptionsTests =
    testList
        "GeneticAlgorithm.CreateOptions"
        [ testCase "uses the documented defaults"
          <| fun _ ->
              let result = GeneticAlgorithm.CreateOptions<int> 25

              Expect.equal result.PopulationSize 25 "population size should be preserved"
              Expect.equal result.SelectionRate 0.8 "selection rate should use the default"
              Expect.equal result.MutationRate 0.05 "mutation rate should use the default"

          testCase "adapts custom delegates"
          <| fun _ ->
              let selection =
                  Func<Chromosome<int> array, int, Chromosome<int> array>(fun population count ->
                      population |> Array.take count)

              let crossover =
                  Func<Chromosome<int>, Chromosome<int>, Chromosome<int> * Chromosome<int>>(fun left right ->
                      right, left)

              let mutation =
                  Func<Chromosome<int>, Chromosome<int>>(fun candidate ->
                      { candidate with Age = candidate.Age + 1 })

              let result = GeneticAlgorithm.CreateOptions(10, selection, crossover, mutation)
              let first = chromosome [| 1 |]
              let second = chromosome [| 2 |]

              Expect.equal (result.SelectionFn [| first; second |] 1) [| first |] "selection delegate should be invoked"
              Expect.equal (result.CrossoverFn first second) (second, first) "crossover delegate should be invoked"
              Expect.equal (result.MutationFn first).Age 1 "mutation delegate should be invoked"

          testCase "rejects null custom delegates"
          <| fun _ ->
              let selection =
                  Func<Chromosome<int> array, int, Chromosome<int> array>(fun population _ -> population)

              let crossover =
                  Func<Chromosome<int>, Chromosome<int>, Chromosome<int> * Chromosome<int>>(fun left right ->
                      left, right)

              let mutation = Func<Chromosome<int>, Chromosome<int>>(fun candidate -> candidate)

              Expect.throwsT<ArgumentNullException>
                  (fun _ -> GeneticAlgorithm.CreateOptions(10, null, crossover, mutation) |> ignore)
                  "selection delegate should be required"

              Expect.throwsT<ArgumentNullException>
                  (fun _ -> GeneticAlgorithm.CreateOptions(10, selection, null, mutation) |> ignore)
                  "crossover delegate should be required"

              Expect.throwsT<ArgumentNullException>
                  (fun _ -> GeneticAlgorithm.CreateOptions(10, selection, crossover, null) |> ignore)
                  "mutation delegate should be required" ]

[<Tests>]
let createProblemTests =
    testList
        "GeneticAlgorithm.CreateProblem"
        [ testCase "adapts all problem delegates"
          <| fun _ ->
              let mutable observedGeneration = -1
              let mutable observedTemperature = 0.0
              let mutable observedPopulationSize = 0

              let termination =
                  Func<IEnumerable<Chromosome<int>>, int, float, bool>(fun population generation temperature ->
                      observedPopulationSize <- population |> Seq.length
                      observedGeneration <- generation
                      observedTemperature <- temperature
                      true)

              let result = GeneticAlgorithm.CreateProblem(genotype, fitness, termination)
              let candidate = result.Genotype()

              Expect.equal candidate.Genes [| 7 |] "genotype delegate should be invoked"
              Expect.equal (result.FitnessFunction candidate) 7.0 "fitness delegate should be invoked"
              Expect.isTrue (result.Terminate [ candidate ] 3 1.25) "termination result should be returned"
              Expect.equal observedPopulationSize 1 "population should be passed to termination"
              Expect.equal observedGeneration 3 "generation should be passed to termination"
              Expect.equal observedTemperature 1.25 "temperature should be passed to termination"

          testCase "rejects null problem delegates"
          <| fun _ ->
              Expect.throwsT<ArgumentNullException>
                  (fun _ -> GeneticAlgorithm.CreateProblem(null, fitness, terminate) |> ignore)
                  "genotype should be required"

              Expect.throwsT<ArgumentNullException>
                  (fun _ -> GeneticAlgorithm.CreateProblem(genotype, null, terminate) |> ignore)
                  "fitness function should be required"

              Expect.throwsT<ArgumentNullException>
                  (fun _ -> GeneticAlgorithm.CreateProblem(genotype, fitness, null) |> ignore)
                  "termination function should be required" ]

[<Tests>]
let runTests =
    testList
        "GeneticAlgorithm.Run"
        [ testCase "supports every overload"
          <| fun _ ->
              expectSolution (GeneticAlgorithm.Run(genotype, fitness, terminate, 4))
              expectSolution (GeneticAlgorithm.Run(genotype, fitness, terminate, options))
              expectSolution (GeneticAlgorithm.Run(problem, 4))
              expectSolution (GeneticAlgorithm.Run(problem, options))

          testCase "rejects null problems and options"
          <| fun _ ->
              let nullProblem = Unchecked.defaultof<Problem<int>>
              let nullOptions = Unchecked.defaultof<Options<int>>

              Expect.throwsT<ArgumentNullException>
                  (fun _ -> GeneticAlgorithm.Run(nullProblem, 4) |> ignore)
                  "problem should be required"

              Expect.throwsT<ArgumentNullException>
                  (fun _ -> GeneticAlgorithm.Run(problem, nullOptions) |> ignore)
                  "options should be required"

              Expect.throwsT<ArgumentNullException>
                  (fun _ -> GeneticAlgorithm.Run(genotype, fitness, terminate, nullOptions) |> ignore)
                  "options should be required" ]

[<Tests>]
let compatibilityFacadeTests =
    testList
        "Interop compatibility facade"
        [ testCase "forwards creation methods"
          <| fun _ ->
              let createdChromosome = Interop.CreateChromosome [| 7 |]
              let createdOptions = Interop.CreateOptions<int> 4
              let createdProblem = Interop.CreateProblem(genotype, fitness, terminate)

              Expect.equal createdChromosome.Genes [| 7 |] "chromosome creation should be forwarded"
              Expect.equal createdOptions.PopulationSize 4 "options creation should be forwarded"
              Expect.equal (createdProblem.Genotype()).Genes [| 7 |] "problem creation should be forwarded"

          testCase "forwards custom option delegates"
          <| fun _ ->
              let selection =
                  Func<Chromosome<int> array, int, Chromosome<int> array>(fun population count ->
                      population |> Array.take count)

              let crossover =
                  Func<Chromosome<int>, Chromosome<int>, Chromosome<int> * Chromosome<int>>(fun left right ->
                      left, right)

              let mutation = Func<Chromosome<int>, Chromosome<int>>(fun candidate -> candidate)
              let result = Interop.CreateOptions(4, selection, crossover, mutation)

              Expect.equal result.PopulationSize 4 "custom options creation should be forwarded"

          testCase "forwards every Run overload"
          <| fun _ ->
              expectSolution (Interop.Run(genotype, fitness, terminate, 4))
              expectSolution (Interop.Run(genotype, fitness, terminate, options))
              expectSolution (Interop.Run(problem, 4))
              expectSolution (Interop.Run(problem, options)) ]
