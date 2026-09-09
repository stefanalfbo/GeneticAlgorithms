module GeneticAlgorithms.Tests.OptionsTests

open Expecto
open GeneticAlgorithms

let private rng = System.Random.Shared

[<Tests>]
let createTests =
    testList
        "Options.create"
        [ testCase "sets the given population size"
          <| fun _ ->
              let result = Options.create 42

              Expect.equal result.PopulationSize 42 "population size should be preserved"

          testCase "uses the same rates as GeneticAlgorithm.CreateOptions's defaults"
          <| fun _ ->
              let result = Options.create 10

              Expect.equal result.SelectionRate 0.8 "selection rate should match the C# facade's default"
              Expect.equal result.MutationRate 0.05 "mutation rate should match the C# facade's default"
              Expect.isFalse (isNull result.Random) "a default Random instance should be supplied"

          testCase "regression: Random can be overridden for a reproducible run"
          <| fun _ ->
              // The whole point of Options.Random - a seeded Random passed via ordinary
              // record-update syntax should be the one actually stored, not silently
              // replaced by a fresh instance.
              let seeded = System.Random(42)
              let result: Options<int> = { Options.create 10 with Random = seeded }

              Expect.isTrue (obj.ReferenceEquals(result.Random, seeded)) "the supplied Random instance should be used as-is"

          testCase "defaults to a population-stable reinsertion strategy, not pure"
          <| fun _ ->
              // Same shape as InteropTests.fs's equivalent regression test for
              // GeneticAlgorithm.CreateOptions: old.Length (parents + leftover) needs to be
              // large enough that floor(old.Length * 0.15) >= 1 to actually distinguish
              // elitist from `` Reinsertion.`pure` ``, which would keep no survivors at all.
              let result: Options<int> = Options.create 20

              let parents =
                  Array.init 10 (fun i -> { Genes = [| i |]; Fitness = float i; Age = 0 })

              let leftover =
                  Array.init 10 (fun i -> { Genes = [| i + 10 |]; Fitness = float (i + 10); Age = 0 })

              let offspring = [| { Genes = [| 99 |]; Fitness = 99.0; Age = 0 } |]

              let nextGeneration = result.ReinsertionFn rng parents offspring leftover

              Expect.isTrue
                  (nextGeneration.Length > offspring.Length)
                  "the default reinsertion strategy should carry some survivors forward, not just offspring"

          testCase "individual fields can be overridden via record-update syntax, leaving the rest at their defaults"
          <| fun _ ->
              let customSelection (_: System.Random) (population: Chromosome<int> array) (n: int) =
                  population |> Array.rev |> Array.take n
              let mutable observedGenerations = 0

              let result =
                  { Options.create 10 with
                      SelectionFn = customSelection
                      Probe = fun info -> observedGenerations <- info.Generation }

              Expect.equal result.PopulationSize 10 "unoverridden fields should keep their default"
              Expect.equal result.MutationRate 0.05 "unoverridden fields should keep their default"

              let population =
                  Array.init 3 (fun i -> { Genes = [| i |]; Fitness = 0.0; Age = 0 })

              Expect.equal
                  (result.SelectionFn rng population 1)
                  [| population.[2] |]
                  "the overridden selection function should be the one actually used"

              result.Probe
                  { Generation = 7
                    Population = [||]
                    Best = { Genes = [| 0 |]; Fitness = 0.0; Age = 0 }
                    Temperature = 0.0 }

              Expect.equal observedGenerations 7 "the overridden probe should be the one actually used" ]
