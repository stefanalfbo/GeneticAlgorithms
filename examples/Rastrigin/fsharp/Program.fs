open GeneticAlgorithms

// The classic Rastrigin benchmark: highly multimodal, with a lattice of local minima
// surrounding the single global minimum f(x) = 0 at x = (0, ..., 0). A good stress test for
// showing a genetic algorithm escaping local optima where plain hill-climbing gets stuck.
let private dimensions = 5
let private lowerBound = -5.12
let private upperBound = 5.12

// Each chromosome's genes are 5 independent real-valued coordinates within the standard
// Rastrigin domain.
let genotype () =
    let genes =
        Array.init dimensions (fun _ ->
            lowerBound + System.Random.Shared.NextDouble() * (upperBound - lowerBound))

    { Genes = genes
      Fitness = 0.0
      Age = 0 }

let rastrigin (genes: float array) =
    let n = float genes.Length

    10.0 * n
    + (genes |> Array.sumBy (fun x -> x * x - 10.0 * cos (2.0 * System.Math.PI * x)))

// The genetic algorithm always maximizes fitness, but Rastrigin is a function to minimize -
// so fitness is simply the negated value: the best chromosome has the fitness closest to
// zero (the largest, least negative value).
let fitnessFunction (chromosome: Chromosome<float>) = -(rastrigin chromosome.Genes)

let lastGeneration = 300

let terminate (_population: seq<Chromosome<float>>) (generation: int) (_temperature: float) =
    generation = lastGeneration

let problem: Problem<float> =
    { Genotype = genotype
      FitnessFunction = fitnessFunction
      Terminate = terminate }

// SelectionRate leaves 20% of the population as leftover each generation; elitist
// reinsertion carries the fittest 5% of (parents + leftover) forward alongside this
// generation's offspring, so 0.8 + 0.15 (MutationRate) + 0.05 keeps the population size
// roughly stable across all 300 generations - this runs a fixed generation count with no
// early-exit fitness target (continuous fitness rarely lands on an exact value), so the
// simpler `pure` reinsertion strategy would let the population grow without bound, just as
// it did for the classic discrete examples earlier this session. MutationRate is higher
// here (0.15) than most other examples' 0.05: Rastrigin's lattice of local minima needs
// more frequent mutants to reliably escape, and the elitist survivalRate is lowered to 0.05
// (rather than the usual 0.15) to keep the same 0.8 + MutationRate + survivalRate = 1.0
// invariant - raising MutationRate without lowering survivalRate to match reintroduces the
// exact unbounded-growth bug fixed elsewhere this session.
//
// CrossoverFn is wholeArithmeticCrossover rather than the library's default singlePoint:
// genes here are real-valued coordinates, not discrete values to swap wholesale - blending
// each gene as a weighted average of both parents makes far more sense for a continuous
// search space. MutationFn is gaussian rather than scramble for the same reason: scramble
// only reorders a chromosome's existing gene values, which is meaningless once genes are
// continuous coordinates rather than a fixed multiset - gaussian instead resamples each
// gene from a normal distribution fitted to the chromosome's own genes, so exploration
// naturally narrows as the population converges toward the optimum.
let options: Options<float> =
    { Options.create 150 with
        CrossoverFn = Crossover.wholeArithmeticCrossover 0.5
        MutationRate = 0.15
        MutationFn = Mutation.gaussian
        ReinsertionFn = Reinsertion.elitist 0.05
        Probe = Probes.everyNth 30 (fun info -> printfn "Current best f(x): %.4f" (-info.Best.Fitness)) }

let solution = Genetic.run problem options

printfn ""
printfn "Best solution found (f(x) = %.4f):" (rastrigin solution.Genes)
printfn "x = %s" (solution.Genes |> Array.map (sprintf "%.4f") |> String.concat ", " |> sprintf "[%s]")
