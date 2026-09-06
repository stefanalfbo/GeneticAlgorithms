open GeneticAlgorithms

let numberOfClasses = 10

let genotype () =
    let genes = Array.init numberOfClasses (fun _ -> System.Random.Shared.Next(0, 2))

    { Genes = genes
      Fitness = 0.0
      Age = 0 }

let private classNames =
    [| "Algorithms"
       "Artificial Intelligence"
       "Calculus"
       "Chemistry"
       "Data Structures"
       "Discrete Math"
       "History"
       "Literature"
       "Physics"
       "Volleyball" |]

let private creditHours = [| 3.0; 3.0; 3.0; 4.5; 3.0; 3.0; 3.0; 3.0; 4.5; 1.5 |]
let private difficulties = [| 8.0; 9.0; 4.0; 3.0; 5.0; 2.0; 4.0; 2.0; 6.0; 1.0 |]
let private usefulness = [| 8.0; 9.0; 6.0; 2.0; 8.0; 9.0; 1.0; 2.0; 5.0; 1.0 |]
let private interest = [| 8.0; 8.0; 5.0; 9.0; 7.0; 2.0; 8.0; 2.0; 7.0; 10.0 |]

let fitness_function (chromosome: Chromosome<int>) =
    let schedule = chromosome.Genes

    if schedule.Length <> creditHours.Length then
        invalidArg (nameof chromosome) $"Expected {creditHours.Length} genes, but received {schedule.Length}."

    let fitness =
        schedule
        |> Array.mapi (fun index selected ->
            float selected
            * (0.3 * usefulness.[index] + 0.3 * interest.[index] - 0.3 * difficulties.[index]))
        |> Array.sum

    let credits =
        Array.map2 (fun selected creditHours -> float selected * creditHours) schedule creditHours
        |> Array.sum

    if credits > 18.0 then -99999.0 else fitness

let terminate (_population: seq<Chromosome<int>>) (generation: int) (_temperature: float) = generation = 1000

let problem: Problem<int> =
    { Genotype = genotype
      FitnessFunction = fitness_function
      Terminate = terminate }

// SelectionRate leaves 20% of the population as leftover each generation; elitist
// reinsertion carries the fittest 15% of (parents + leftover) forward alongside this
// generation's offspring, so 0.8 + 0.05 (MutationRate) + 0.15 keeps the population size
// roughly stable across all 1000 generations. This runs a fixed generation count with no
// early-exit fitness target (the optimal schedule isn't known ahead of time), so the
// simpler `pure` reinsertion strategy - which, at the library's default SelectionRate of
// 1.0, lets the population grow without bound every generation - would compound over
// 1000 generations into a population far too large to finish in any reasonable time.
let options =
    { PopulationSize = 100
      SelectionRate = 0.8
      SelectionFn = Selection.elite
      CrossoverFn = Crossover.singlePoint
      MutationRate = 0.05
      MutationFn = Mutation.scramble
      ReinsertionFn = Reinsertion.elitist 0.15
      Probe = Probes.printProgress }

let solution = Genetic.run problem options

let selectedClasses =
    Array.zip solution.Genes classNames
    |> Array.choose (fun (selected, name) -> if selected = 1 then Some name else None)

printfn "Best schedule: %A (fitness: %f)" solution.Genes solution.Fitness
printfn "Classes:       %s" (String.concat ", " selectedClasses)
