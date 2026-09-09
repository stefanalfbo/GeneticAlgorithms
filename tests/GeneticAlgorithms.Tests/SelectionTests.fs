module GeneticAlgorithms.Tests.SelectionTests

open System
open Expecto
open GeneticAlgorithms

let private makeChromosome fitness : Chromosome<int> =
    { Genes = [| 0 |]; Fitness = fitness; Age = 0 }

let private population =
    [| makeChromosome 4.0
       makeChromosome 3.0
       makeChromosome 2.0
       makeChromosome 1.0 |]

// Every chromosome shares Fitness = 0.0 (a valid state under roulette/SUS's own
// non-negative-fitness contract), but distinct Genes so a regression test can tell which
// ones were actually selected - makeChromosome's [| 0 |] genes would otherwise make every
// zero-fitness chromosome structurally identical.
let private zeroFitnessPopulation =
    Array.init 4 (fun i -> { Genes = [| i |]; Fitness = 0.0; Age = 0 })

let private rng = System.Random.Shared

let private opts: Options<int> =
    { PopulationSize = population.Length
      SelectionRate = 0.8
      SelectionFn = Selection.elite
      CrossoverFn = Crossover.singlePoint
      MutationRate = 0.05
      MutationFn = Mutation.scramble
      ReinsertionFn = Reinsertion.``pure``
      Probe = fun _ -> ()
      Random = rng }

[<Tests>]
let eliteTests =
    testList
        "Selection.elite"
        [ testCase "takes the first n chromosomes"
          <| fun _ ->
              let result = Selection.elite rng population 2

              Expect.equal result [| population.[0]; population.[1] |] "should keep the given order"

          testCase "returns n chromosomes"
          <| fun _ ->
              let result = Selection.elite rng population 3

              Expect.equal result.Length 3 "should return exactly n chromosomes" ]

[<Tests>]
let randomTests =
    testList
        "Selection.random"
        [ testCase "returns n chromosomes"
          <| fun _ ->
              let result = Selection.random rng population 2

              Expect.equal result.Length 2 "should return exactly n chromosomes"

          testCase "only returns chromosomes from the population"
          <| fun _ ->
              let result = Selection.random rng population 3

              Expect.all result (fun c -> Array.contains c population) "every chromosome should come from the population" ]

[<Tests>]
let tournamentTests =
    testList
        "Selection.tournament"
        [ testCase "returns n chromosomes"
          <| fun _ ->
              let result = Selection.tournament 2 rng population 3

              Expect.equal result.Length 3 "should return exactly n chromosomes"

          testCase "always picks the fittest chromosome when tournament_size covers the whole population"
          <| fun _ ->
              let result = Selection.tournament population.Length rng population 3

              Expect.all result (fun c -> c = population.[0]) "every round should pick the fittest chromosome" ]

[<Tests>]
let tournamentNoDuplicatesTests =
    testList
        "Selection.tournamentNoDuplicates"
        [ testCase "returns n distinct chromosomes"
          <| fun _ ->
              let result = Selection.tournamentNoDuplicates 2 rng population 3

              Expect.equal result.Length 3 "should return exactly n chromosomes"
              Expect.equal (result |> Array.distinct |> Array.length) 3 "should not repeat chromosomes"

          testCase "when tournamentSize covers the whole population, only 1 distinct chromosome is reachable"
          <| fun _ ->
              // With tournamentSize = population.Length, every tournament draws the entire
              // population, so Array.maxBy always returns the same fittest chromosome
              // (population.[0], confirmed deterministic by Selection.tournament's own
              // "always picks the fittest..." test above) - so n = 1 is the largest request
              // that's actually reachable here.
              let result = Selection.tournamentNoDuplicates population.Length rng population 1

              Expect.equal result [| population.[0] |] "the only reachable winner is the fittest chromosome"

          testCase "regression: throws instead of looping forever when n exceeds what's reachable"
          <| fun _ ->
              // Regression test for a bug: tournamentNoDuplicates used to retry forever
              // here, since a tournamentSize equal to the whole population always picks the
              // single fittest chromosome (see the test above), so 2 distinct winners could
              // never be collected - the `while selected.Count < n` loop had no feasibility
              // check and just spun forever.
              //
              // Run it on a background thread with a short timeout rather than calling it
              // directly, so that if this regresses in the future, this test fails fast
              // instead of hanging the whole suite again.
              let task =
                  System.Threading.Tasks.Task.Run(fun () ->
                      Selection.tournamentNoDuplicates population.Length rng population 2 |> ignore)

              let completedInTime =
                  try
                      task.Wait(System.TimeSpan.FromMilliseconds 200.0)
                  with :? AggregateException ->
                      true

              Expect.isTrue completedInTime "should fail fast with an exception rather than loop forever"
              Expect.isTrue task.IsFaulted "should throw rather than loop forever"

              let innerException = task.Exception.Flatten().InnerExceptions |> Seq.exactlyOne

              Expect.isTrue
                  (innerException :? ArgumentException)
                  "should throw an ArgumentException explaining why n is infeasible, not hang" ]

[<Tests>]
let rouletteTests =
    testList
        "Selection.roulette"
        [ testCase "returns the only chromosome when the population has a single member"
          <| fun _ ->
              let single = [| makeChromosome 1.0 |]

              let result = Selection.roulette rng single 3

              Expect.all result (fun c -> c = single.[0]) "should always return the only chromosome"

          testCase "always picks the chromosome holding all the fitness weight"
          <| fun _ ->
              let weighted = [| makeChromosome 100.0; makeChromosome 0.0 |]

              let result = Selection.roulette rng weighted 5

              Expect.all result (fun c -> c = weighted.[0]) "should always pick the chromosome with all the fitness"

          testCase "regression: falls back to uniform selection when every chromosome has zero fitness"
          <| fun _ ->
              // Bug: with a total weight of exactly 0.0, the underlying cumulative walk's
              // termination check (w + sum > u, where u is also always 0.0) never triggers,
              // so it always fell through to and returned the very last population member,
              // deterministically, rather than picking without preference. 100 picks over 4
              // chromosomes makes still seeing only 1 distinct result astronomically
              // unlikely if the fallback to uniform selection is working.
              let result = Selection.roulette rng zeroFitnessPopulation 100

              Expect.isTrue
                  (result |> Array.distinct |> Array.length > 1)
                  "zero fitness should fall back to uniform selection, not always the last chromosome" ]

[<Tests>]
let boltzmannTests =
    testList
        "Selection.boltzmann"
        [ testCase "returns n chromosomes"
          <| fun _ ->
              let result = Selection.boltzmann 1.0 rng population 3

              Expect.equal result.Length 3 "should return exactly n chromosomes"

          testCase "raises for a non-positive temperature"
          <| fun _ ->
              Expect.throwsT<System.ArgumentException>
                  (fun () -> Selection.boltzmann 0.0 rng population 1 |> ignore)
                  "temperature must be positive"

          testCase "strongly favors the fittest chromosome at a low temperature"
          <| fun _ ->
              let weighted = [| makeChromosome 10.0; makeChromosome 0.0 |]

              let result = Selection.boltzmann 0.1 rng weighted 5

              Expect.all result (fun c -> c = weighted.[0]) "should almost always pick the fittest chromosome"

          testCase "does not overflow for a large fitness gap at a very low temperature"
          <| fun _ ->
              let weighted = [| makeChromosome 1_000_000.0; makeChromosome 0.0 |]

              let result = Selection.boltzmann 0.001 rng weighted 5

              Expect.all result (fun c -> c = weighted.[0]) "should deterministically pick the fittest chromosome without producing NaN or Infinity weights" ]

[<Tests>]
let stochasticUniversalSamplingTests =
    testList
        "Selection.stochasticUniversalSampling"
        [ testCase "returns n chromosomes"
          <| fun _ ->
              let result = Selection.stochasticUniversalSampling rng population 3

              Expect.equal result.Length 3 "should return exactly n chromosomes"

          testCase "returns the only chromosome when the population has a single member"
          <| fun _ ->
              let single = [| makeChromosome 1.0 |]

              let result = Selection.stochasticUniversalSampling rng single 3

              Expect.all result (fun c -> c = single.[0]) "should always return the only chromosome"

          testCase "selects each chromosome exactly as many times as its share of total fitness"
          <| fun _ ->
              // Evenly spaced pointers guarantee an exact count per chromosome (not just an
              // expected value), regardless of the random starting offset: with fitnesses
              // 4/3/2/1 (summing to 10) and n = 10, the pointer spacing is 1.0, so each
              // chromosome's fitness share always contains exactly that many pointers.
              let weighted =
                  [| makeChromosome 4.0
                     makeChromosome 3.0
                     makeChromosome 2.0
                     makeChromosome 1.0 |]

              let result = Selection.stochasticUniversalSampling rng weighted 10
              let countOf c = result |> Array.filter ((=) c) |> Array.length

              Expect.equal (countOf weighted.[0]) 4 "the chromosome with 4/10 of the fitness should be picked exactly 4 times"
              Expect.equal (countOf weighted.[1]) 3 "the chromosome with 3/10 of the fitness should be picked exactly 3 times"
              Expect.equal (countOf weighted.[2]) 2 "the chromosome with 2/10 of the fitness should be picked exactly 2 times"
              Expect.equal (countOf weighted.[3]) 1 "the chromosome with 1/10 of the fitness should be picked exactly 1 time"

          testCase "regression: falls back to uniform selection when every chromosome has zero fitness"
          <| fun _ ->
              // Bug: with a total weight of exactly 0.0, every pointer sits at the same zero
              // offset, so the walk stops advancing at the very first chromosome it reaches
              // and every pick returned that one chromosome, deterministically, rather than
              // picking without preference. 100 picks over 4 chromosomes makes still seeing
              // only 1 distinct result astronomically unlikely if the fallback to uniform
              // selection is working.
              let result = Selection.stochasticUniversalSampling rng zeroFitnessPopulation 100

              Expect.isTrue
                  (result |> Array.distinct |> Array.length > 1)
                  "zero fitness should fall back to uniform selection, not always the same chromosome" ]

[<Tests>]
let rankTests =
    testList
        "Selection.rank"
        [ testCase "returns n chromosomes"
          <| fun _ ->
              let result = Selection.rank rng population 3

              Expect.equal result.Length 3 "should return exactly n chromosomes"

          testCase "returns the only chromosome when the population has a single member"
          <| fun _ ->
              let single = [| makeChromosome 1.0 |]

              let result = Selection.rank rng single 3

              Expect.all result (fun c -> c = single.[0]) "should always return the only chromosome"

          testCase "weighs by rank rather than raw fitness, so an extreme outlier does not dominate"
          <| fun _ ->
              // Ranks here are 1/2/3 (worst to best) regardless of the fitness gap, so the
              // best chromosome has only half the total weight instead of ~all of it, and the
              // worst still has a real (1/6) chance. Over 200 draws the odds of either of the
              // following failing by chance are astronomically small (~(5/6)^200 and ~0.5^200).
              let weighted = [| makeChromosome 1000.0; makeChromosome 2.0; makeChromosome 1.0 |]

              let result = Selection.rank rng weighted 200

              Expect.isTrue
                  (result |> Array.exists ((=) weighted.[2]))
                  "the lowest-fitness chromosome should still be picked sometimes"

              Expect.isTrue
                  (result |> Array.exists (fun c -> c <> weighted.[0]))
                  "the extreme fitness outlier should not win every single pick" ]

[<Tests>]
let selectTests =
    testList
        "Selection.select"
        [ testCase "splits the population into parent pairs and leftover using selection_rate"
          <| fun _ ->
              let parentPairs, parents, leftover = Selection.select { opts with SelectionRate = 0.5 } population

              Expect.equal parentPairs.Length 1 "half the population should be paired up"
              Expect.equal parents.Length 2 "both paired chromosomes should be reported as parents"
              Expect.equal leftover.Length 2 "the rest should be left over"

          testCase "rounds an odd selection count up to the next even number"
          <| fun _ ->
              let parentPairs, parents, leftover = Selection.select { opts with SelectionRate = 0.75 } population

              Expect.equal parentPairs.Length 2 "the selection count should be rounded up to stay even"
              Expect.equal parents.Length 4 "every selected chromosome should be reported as a parent"
              Expect.equal leftover.Length 0 "no chromosomes should be left over"

          testCase "clamps the selection count so rounding up never exceeds an odd population"
          <| fun _ ->
              // With SelectionRate = 1.0 the rounded-up count would be 6 for a 5-chromosome
              // population - one more than exists. It must be clamped to 4 (the largest even
              // number that still fits), leaving the fifth chromosome as leftover.
              let oddPopulation = Array.append population [| makeChromosome 0.5 |]

              let parentPairs, parents, leftover =
                  Selection.select { opts with SelectionRate = 1.0 } oddPopulation

              Expect.equal parentPairs.Length 2 "selection count should be clamped to 4 (2 pairs)"
              Expect.equal parents.Length 4 "every selected chromosome should be reported as a parent"
              Expect.equal leftover.Length 1 "the chromosome that didn't fit should be left over"

          testCase "regression: parents and leftover add up to the population size even when SelectionFn permits duplicates"
          <| fun _ ->
              // Bug: tournament/roulette/boltzmann/stochasticUniversalSampling can legitimately
              // select the same chromosome more than once (e.g. [A; A] out of [A; B; C; D]).
              // leftover is computed as a set difference, so it correctly drops to one fewer
              // element per distinct chromosome selected - but the raw, possibly-duplicated
              // selection has more elements than there are distinct chromosomes behind it. If
              // that raw array were used as "parents" for reinsertion accounting,
              // parents.Length + leftover.Length would exceed population.Length (2 + 3 = 5 for
              // a population of 4), which breaks Reinsertion.elitist/uniform's documented
              // invariant that the two add back up to the previous population's size - a
              // chromosome selected as a parent twice would then also count twice toward "old"
              // survivors, silently growing the population every generation it happens.
              let alwaysSameTwice =
                  fun (_rng: System.Random) (pop: Chromosome<int> array) (n: int) -> Array.create n pop.[0]

              let duplicateSelectionOpts =
                  { opts with
                      SelectionRate = 0.5
                      SelectionFn = alwaysSameTwice }

              let parentPairs, parents, leftover = Selection.select duplicateSelectionOpts population

              Expect.equal parentPairs.Length 1 "should still form one pair for crossover"
              Expect.equal parentPairs.[0] (population.[0], population.[0]) "the pair should use the duplicated selection"
              Expect.equal parents.Length 1 "duplicate selections of the same chromosome should count once"
              Expect.equal leftover.Length 3 "the rest of the population should be left over"
              Expect.equal
                  (parents.Length + leftover.Length)
                  population.Length
                  "parents and leftover should add back up to the population size, even with duplicate selections"

          testCase "regression: distinct population slots that happen to share a value are each counted as a parent"
          <| fun _ ->
              // Bug: the fix above (deduplicating parents by value) went too far - it also
              // collapsed the case where the population itself genuinely contains more than
              // one physically distinct chromosome with the same value, which is exactly what
              // Reinsertion.elitist produces once a population converges (the fittest
              // chromosome(s) carried forward unchanged, generation after generation). If
              // Selection.elite's deterministic top-N slice includes several of those
              // value-identical slots, each one is a real, separate individual and must count
              // once each - collapsing them the same way as a roulette-style repeated draw
              // undercounts parents, so parents.Length + leftover.Length falls short of
              // population.Length and the population silently shrinks every generation this
              // happens. This is exactly what made OneMaxProblem hang partway to its target.
              let fittest = makeChromosome 9.0
              let converged = [| fittest; fittest; makeChromosome 2.0; makeChromosome 1.0 |]

              let parentPairs, parents, leftover =
                  Selection.select { opts with SelectionRate = 0.5; SelectionFn = Selection.elite } converged

              Expect.equal parentPairs.Length 1 "should still form one pair for crossover"
              Expect.equal parentPairs.[0] (fittest, fittest) "the pair should use both value-identical slots"
              Expect.equal parents.Length 2 "both distinct population slots should each count as a parent"
              Expect.equal leftover.Length 2 "only the two unselected chromosomes should be left over"
              Expect.equal
                  (parents.Length + leftover.Length)
                  converged.Length
                  "parents and leftover should add back up to the population size" ]
