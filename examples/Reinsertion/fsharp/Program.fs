open GeneticAlgorithms

// Same class-scheduling problem as the Schedule example - see its README for the full
// explanation of the domain. Reused here unchanged so the only thing that varies between
// the three runs below is the reinsertion strategy, not the problem being solved.

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

let lastGeneration = 1000

let terminate (_population: seq<Chromosome<int>>) (generation: int) (_temperature: float) =
    generation = lastGeneration

let problem: Problem<int> =
    { Genotype = genotype
      FitnessFunction = fitness_function
      Terminate = terminate }

// SelectionRate leaves 20% of the population unselected as `leftover` each generation -
// that's the pool `elitist` and `uniform` draw survivors from. `pure` ignores that pool
// entirely, so with SelectionRate = 0.8 it loses 20% of the population every generation
// until it settles into a small, mostly-static equilibrium - the point of this example is
// to make that visible next to the other two strategies.
//
// `elitist` and `uniform` are given a survivalRate chosen so that
// SelectionRate + MutationRate + survivalRate = 1.0 (0.8 + 0.05 + 0.15), which keeps their
// population size roughly stable across generations instead of drifting like `pure` does.
let survivalRate = 0.15

let baseOptions: Options<int> =
    { PopulationSize = 100
      SelectionRate = 0.8
      SelectionFn = Selection.elite
      CrossoverFn = Crossover.singlePoint
      MutationRate = 0.05
      MutationFn = Mutation.scramble
      ReinsertionFn = Reinsertion.``pure``
      OnGeneration = fun _ _ -> () }

let strategies: (string * (Chromosome<int> array -> Chromosome<int> array -> Chromosome<int> array -> Chromosome<int> array)) list =
    [ "pure", Reinsertion.``pure``
      "elitist", Reinsertion.elitist survivalRate
      "uniform", Reinsertion.uniform survivalRate ]

// Runs the genetic algorithm once with the given reinsertion strategy, recording the best
// fitness seen at every generation along the way so the three runs can be compared
// side by side afterward.
let runStrategy (name: string, reinsertionFn) =
    let fitnessByGeneration = Array.zeroCreate<float> (lastGeneration + 1)

    let options =
        { baseOptions with
            ReinsertionFn = reinsertionFn
            OnGeneration = fun best generation -> fitnessByGeneration.[generation] <- best.Fitness }

    let solution = Genetic.run problem options
    name, solution, fitnessByGeneration

let results = strategies |> List.map runStrategy

printfn "Best fitness by generation (sampled every 100 generations):"
printfn "%10s | %8s | %8s | %8s" "Generation" "pure" "elitist" "uniform"

for generation in 0 .. 100 .. lastGeneration do
    let fitnessAt (_, _, fitnessByGeneration: float array) = fitnessByGeneration.[generation]

    printfn
        "%10d | %8.2f | %8.2f | %8.2f"
        generation
        (fitnessAt results.[0])
        (fitnessAt results.[1])
        (fitnessAt results.[2])

printfn ""
printfn "Final schedules:"

for name, solution, _ in results do
    let selectedClasses =
        Array.zip solution.Genes classNames
        |> Array.choose (fun (selected, className) -> if selected = 1 then Some className else None)

    printfn "%-8s fitness: %8.2f  classes: %s" name solution.Fitness (String.concat ", " selectedClasses)
