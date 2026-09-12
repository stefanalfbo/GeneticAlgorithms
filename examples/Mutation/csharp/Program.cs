using GeneticAlgorithms;

// Single-machine weighted tardiness scheduling: given a fixed set of jobs, each with a
// processing time, due date, and importance weight, find the processing ORDER that
// minimizes total weighted tardiness. Unlike a OneMax-style problem (used by the
// Reinsertion example), this one is genuinely order-dependent - reordering the same set of
// jobs changes which ones finish late and by how much. That's essential here: scramble and
// scrambleSlice only differ in HOW they reorder a chromosome's genes, so an
// order-insensitive fitness function (like OneMax's gene sum) would make them
// indistinguishable no matter how differently they mutate.
const int numberOfJobs = 30;
const int lastGeneration = 300;

// The job data itself (processing times, due dates, weights) is a fixed problem instance,
// not part of what's randomized every run - only the search (initial population,
// selection, crossover, mutation) draws from the options' Random. A dedicated Random with
// its own fixed seed generates this instance once, independently of any run's own
// randomness.
var instanceRng = new Random(42);
var processingTimes = Enumerable.Range(0, numberOfJobs).Select(_ => instanceRng.Next(1, 20)).ToArray();
var dueDates = Enumerable.Range(0, numberOfJobs).Select(_ => instanceRng.Next(20, 200)).ToArray();
var weights = Enumerable.Range(0, numberOfJobs).Select(_ => instanceRng.Next(1, 10)).ToArray();

double TotalWeightedTardiness(Chromosome<int> chromosome)
{
    var completionTime = 0;
    var total = 0;

    foreach (var jobIndex in chromosome.Genes)
    {
        completionTime += processingTimes[jobIndex];
        var tardiness = Math.Max(0, completionTime - dueDates[jobIndex]);
        total += weights[jobIndex] * tardiness;
    }

    return total;
}

// A chromosome's genes are a permutation of job indices - the order jobs run in on the
// single machine, back to back with no idle time.
Func<Random, Chromosome<int>> genotype = rng =>
    GeneticAlgorithm.CreateChromosome(
        Enumerable.Range(0, numberOfJobs)
            .OrderBy(_ => rng.Next())
            .ToArray());

// The genetic algorithm always maximizes fitness, but total weighted tardiness is a cost
// to minimize - so fitness is simply the negated cost: a perfect, tardiness-free schedule
// has the fitness closest to zero (the largest, least negative value).
Func<Chromosome<int>, double> fitnessFunction = chromosome => -TotalWeightedTardiness(chromosome);

Func<IEnumerable<Chromosome<int>>, int, double, bool> terminate =
    (_, generation, _) => generation == lastGeneration;

// scrambleSliceWindow is deliberately small relative to numberOfJobs: the whole point of
// this example is comparing scrambleSlice against scramble's full-chromosome reorder, so a
// window that's a small, local fraction of the chromosome is what makes it "less
// disruptive" in the first place - a window close to numberOfJobs would just behave like
// scramble.
const int scrambleSliceWindow = 5;

var strategies = new (string Name, Func<Random, Chromosome<int>, Chromosome<int>> MutationFn)[]
{
    ("scramble", Mutation.scramble),
    ("scrambleSlice", (rng, chromosome) => Mutation.scrambleSlice(scrambleSliceWindow, rng, chromosome)),
};

// Runs the genetic algorithm once with the given mutation strategy, recording the best
// (least tardy) schedule's cost seen at every generation along the way so the two runs can
// be compared side by side afterward.
(string Name, Chromosome<int> Solution, double[] CostByGeneration) RunStrategy(
    string name,
    Func<Random, Chromosome<int>, Chromosome<int>> mutationFn)
{
    var costByGeneration = new double[lastGeneration + 1];

    // Genes here are a permutation (every job run exactly once), so crossoverFn is
    // orderOneCrossover rather than the library's default singlePoint - a single-point cut
    // would generally produce a schedule with one job missing and another repeated.
    // reinsertionFn keeps population size roughly stable across all 300 generations
    // (SelectionRate 0.8 + MutationRate 0.05 + survivalRate 0.15 = 1.0), the same
    // population-stable combination used throughout this library's own examples.
    var options = GeneticAlgorithm.CreateOptions<int>(
        populationSize: 100,
        selectionFn: Selection.elite,
        crossoverFn: Crossover.orderOneCrossover,
        mutationFn: mutationFn,
        reinsertionFn: (rng, parents, offspring, leftover) => Reinsertion.elitist(0.15, rng, parents, offspring, leftover),
        probe: info => costByGeneration[info.Generation] = -info.Best.Fitness);

    var solution = GeneticAlgorithm.Run(
        genotype: genotype,
        fitnessFunction: fitnessFunction,
        terminate: terminate,
        options: options);

    return (name, solution, costByGeneration);
}

var results = strategies.Select(s => RunStrategy(s.Name, s.MutationFn)).ToArray();

Console.WriteLine($"Minimizing total weighted tardiness across {numberOfJobs} jobs over {lastGeneration} generations...");
Console.WriteLine();
Console.WriteLine("Best (lowest) total weighted tardiness by generation (sampled every 30 generations):");
Console.WriteLine($"{"Generation",10} | {"scramble",10} | {"scrambleSlice",13}");

for (var generation = 0; generation <= lastGeneration; generation += 30)
{
    var scrambleCost = results[0].CostByGeneration[generation];
    var scrambleSliceCost = results[1].CostByGeneration[generation];

    Console.WriteLine($"{generation,10} | {scrambleCost,10:F1} | {scrambleSliceCost,13:F1}");
}

Console.WriteLine();
Console.WriteLine("Final results:");

foreach (var (name, solution, _) in results)
{
    Console.WriteLine($"{name,-13} total weighted tardiness: {-solution.Fitness:F1}");
}
