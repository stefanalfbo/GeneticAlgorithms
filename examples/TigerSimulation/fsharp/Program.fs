open GeneticAlgorithms

// A tiger genotype: 8 binary traits - Size, Swimming Ability, Fur Color, Fat Stores,
// Activity Period, Hunting Ground, Fur Thickness, Tail Length, in the same order as the
// scoring tables below. Each gene is 0 or 1; the descriptive labels are purely for
// display (see `describe` below) - the fitness function only ever sees the raw 0/1
// values.
let numberOfTraits = 8

let genotype () =
    let genes = Array.init numberOfTraits (fun _ -> System.Random.Shared.Next(0, 2))

    { Genes = genes
      Fitness = 0.0
      Age = 0 }

let private traitNames =
    [| "Size"
       "Swimming Ability"
       "Fur Color"
       "Fat Stores"
       "Activity Period"
       "Hunting Ground"
       "Fur Thickness"
       "Tail Length" |]

// (label for gene = 0, label for gene = 1). Fur Color's 0/1 meaning wasn't specified
// alongside the other traits, so it's inferred here from the score table instead: gene = 1
// scores +2.0 in the tropics and -2.0 in the tundra, so it's assigned "dark" (good
// camouflage in dense, dark vegetation), while gene = 0 - the safer choice in the tundra,
// where it avoids that penalty - is assigned "light" (camouflage in snow).
let private traitLabels =
    [| "smaller", "larger"
       "low", "high"
       "light", "dark"
       "less", "more"
       "diurnal", "nocturnal"
       "smaller", "larger"
       "less thick", "more thick"
       "smaller", "larger" |]

let describe (chromosome: Chromosome<int>) =
    Array.zip3 traitNames traitLabels chromosome.Genes
    |> Array.map (fun (name, (lo, hi), gene) -> sprintf "%s: %s" name (if gene = 0 then lo else hi))
    |> String.concat ", "

// Trait scores per environment, in the same trait order as above. A gene of 0 always
// contributes nothing to fitness (0 * score = 0) regardless of environment - only a gene
// of 1 is rewarded or penalized, and by how much depends on which environment it's
// evaluated in. Tail Length scores 0.0 in both environments, so it's a neutral trait: not
// selected for either way, free to drift.
let private tropicalScores = [| 0.0; 3.0; 2.0; 1.0; 0.5; 1.0; -1.0; 0.0 |]
let private tundraScores = [| 1.0; 3.0; -2.0; -1.0; 0.5; 2.0; 1.0; 0.0 |]

let fitnessFunction (scores: float array) (chromosome: Chromosome<int>) =
    Array.zip chromosome.Genes scores |> Array.sumBy (fun (gene, score) -> float gene * score)

let lastGeneration = 1000

let terminate (_population: seq<Chromosome<int>>) (generation: int) (_temperature: float) =
    generation = lastGeneration

// Options.create's defaults (SelectionRate 0.8 + MutationRate 0.05 + Reinsertion.elitist's
// survivalRate 0.15 summing to 1.0) keep population size stable across all 1000
// generations instead of collapsing, the way the simplest `pure` reinsertion strategy
// would over a run this long.
let baseOptions: Options<int> = Options.create 100

/// One generation's tracked statistics: mean fitness and mean age across the whole
/// population, plus the fittest chromosome's fitness for that generation.
type GenerationStats =
    { Generation: int
      MeanFitness: float
      MeanAge: float
      BestFitness: float }

let private mean (values: float array) = Array.average values

/// Builds a probe that records this generation's mean fitness, mean age, and best fitness
/// into `history` - a plain in-memory collector written for this example, not provided by
/// the library (see GeneticAlgorithms.Probes for why: gathering statistics is left to
/// whoever plugs a probe in, not imposed by the library).
let statsProbe (history: ResizeArray<GenerationStats>) : GenerationInfo<int> -> unit =
    fun info ->
        history.Add
            { Generation = info.Generation
              MeanFitness = info.Population |> Array.map (fun c -> c.Fitness) |> mean
              MeanAge = info.Population |> Array.map (fun c -> float c.Age) |> mean
              BestFitness = info.Best.Fitness }

let writeCsv (path: string) (history: ResizeArray<GenerationStats>) =
    let header = "Generation,MeanFitness,MeanAge,BestFitness"

    let rows =
        history
        |> Seq.map (fun s -> sprintf "%d,%f,%f,%f" s.Generation s.MeanFitness s.MeanAge s.BestFitness)

    System.IO.File.WriteAllLines(path, Seq.append [ header ] rows)

let runEnvironment (name: string) (scores: float array) =
    let history = ResizeArray<GenerationStats>()

    let problem: Problem<int> =
        { Genotype = genotype
          FitnessFunction = fitnessFunction scores
          Terminate = terminate }

    let options =
        { baseOptions with
            // The stats probe records every generation, unthrottled, so the CSV has
            // complete data; printProgress is throttled separately so the console stays
            // readable across 1000 generations.
            Probe = Probes.combine [ statsProbe history; Probes.everyNth 100 Probes.printProgress ] }

    let solution = Genetic.run problem options

    name, solution, history

printfn "Simulating tiger evolution over %d generations in two environments...\n" lastGeneration

let tropical = runEnvironment "Tropical" tropicalScores
let tundra = runEnvironment "Tundra" tundraScores

let environments = [ tropical; tundra ]

for name, _, history in environments do
    writeCsv (sprintf "%s_stats.csv" (name.ToLowerInvariant())) history

printfn ""
printfn "Mean fitness / mean age by generation (sampled every 100 generations):"

printfn
    "%10s | %14s | %12s | %14s | %12s"
    "Generation"
    "Tropical Fit."
    "Tropical Age"
    "Tundra Fit."
    "Tundra Age"

for generation in 0 .. 100 .. lastGeneration do
    let statsAt (_, _, history: ResizeArray<GenerationStats>) = history.[generation]
    let tropicalStats = statsAt tropical
    let tundraStats = statsAt tundra

    printfn
        "%10d | %14.2f | %12.2f | %14.2f | %12.2f"
        generation
        tropicalStats.MeanFitness
        tropicalStats.MeanAge
        tundraStats.MeanFitness
        tundraStats.MeanAge

printfn ""
printfn "Final results:"

for name, solution, history in environments do
    let final = history.[history.Count - 1]

    printfn "%s:" name
    printfn "  Final mean fitness: %.2f, final mean age: %.2f" final.MeanFitness final.MeanAge
    printfn "  Fittest tiger (fitness %.2f): %s" solution.Fitness (describe solution)

printfn ""
printfn "Full per-generation statistics written to tropical_stats.csv and tundra_stats.csv"
