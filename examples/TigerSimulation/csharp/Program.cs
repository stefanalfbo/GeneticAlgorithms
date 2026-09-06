using System.Globalization;
using GeneticAlgorithms;

// A tiger genotype: 8 binary traits - Size, Swimming Ability, Fur Color, Fat Stores,
// Activity Period, Hunting Ground, Fur Thickness, Tail Length, in the same order as the
// scoring tables below. Each gene is 0 or 1; the descriptive labels are purely for
// display (see Describe below) - the fitness function only ever sees the raw 0/1 values.
const int numberOfTraits = 8;
const int lastGeneration = 1000;

Func<Chromosome<int>> genotype = () =>
    GeneticAlgorithm.CreateChromosome(
        Enumerable.Range(0, numberOfTraits)
            .Select(_ => Random.Shared.Next(0, 2))
            .ToArray());

var traitNames = new[]
{
    "Size", "Swimming Ability", "Fur Color", "Fat Stores",
    "Activity Period", "Hunting Ground", "Fur Thickness", "Tail Length",
};

// (label for gene = 0, label for gene = 1). Fur Color's 0/1 meaning wasn't specified
// alongside the other traits, so it's inferred here from the score table below: gene = 1
// scores +2.0 in the tropics and -2.0 in the tundra, so it's assigned "dark" (good
// camouflage in dense, dark vegetation), while gene = 0 - the safer choice in the tundra,
// where it avoids that penalty - is assigned "light" (camouflage in snow).
var traitLabels = new[]
{
    ("smaller", "larger"),
    ("low", "high"),
    ("light", "dark"),
    ("less", "more"),
    ("diurnal", "nocturnal"),
    ("smaller", "larger"),
    ("less thick", "more thick"),
    ("smaller", "larger"),
};

string Describe(Chromosome<int> chromosome) =>
    string.Join(
        ", ",
        traitNames.Zip(traitLabels, chromosome.Genes)
            .Select(t => $"{t.First}: {(t.Third == 0 ? t.Second.Item1 : t.Second.Item2)}"));

// Trait scores per environment, in the same trait order as above. A gene of 0 always
// contributes nothing to fitness (0 * score = 0) regardless of environment - only a gene
// of 1 is rewarded or penalized, and by how much depends on which environment it's
// evaluated in. Tail Length scores 0.0 in both environments, so it's a neutral trait: not
// selected for either way, free to drift.
var tropicalScores = new[] { 0.0, 3.0, 2.0, 1.0, 0.5, 1.0, -1.0, 0.0 };
var tundraScores = new[] { 1.0, 3.0, -2.0, -1.0, 0.5, 2.0, 1.0, 0.0 };

Func<double[], Func<Chromosome<int>, double>> fitnessFunction =
    scores => chromosome => chromosome.Genes.Zip(scores, (gene, score) => gene * score).Sum();

Func<IEnumerable<Chromosome<int>>, int, double, bool> terminate =
    (_, generation, _) => generation == lastGeneration;

var environmentNames = new[] { "Tropical", "Tundra" };
var environmentScores = new[] { tropicalScores, tundraScores };

// Runs the genetic algorithm once for the given environment's score table, recording mean
// fitness, mean age, and best fitness every generation along the way.
(string Name, Chromosome<int> Solution, List<GenerationStats> History) RunEnvironment(string name, double[] scores)
{
    var history = new List<GenerationStats>();

    // SelectionRate is fixed at 0.8 by GeneticAlgorithm.CreateOptions, leaving 20% of the
    // population unselected as `leftover` each generation. Reinsertion.elitist with a
    // survivalRate of 0.15 carries the fittest 15% of (parents + leftover) forward
    // alongside this generation's offspring, so 0.8 + 0.05 (MutationRate) + 0.15 keeps the
    // population size roughly stable across all 1000 generations, instead of collapsing
    // the way the simplest Reinsertion.pure strategy would over a run this long.
    var options = GeneticAlgorithm.CreateOptions<int>(
        populationSize: 100,
        selectionFn: Selection.elite,
        crossoverFn: Crossover.singlePoint,
        mutationFn: Mutation.scramble,
        reinsertionFn: (parents, offspring, leftover) => Reinsertion.elitist(0.15, parents, offspring, leftover),
        // The stats probe records every generation, unthrottled, so the CSV has complete
        // data; printProgress only runs every 100th generation so the console stays
        // readable across 1000 generations. There's no C#-facing equivalent of the F#
        // Probes.combine/everyNth combinators - composing two probes is just a plain
        // lambda calling both.
        probe: info =>
        {
            history.Add(
                new GenerationStats(
                    info.Generation,
                    info.Population.Select(c => c.Fitness).Average(),
                    info.Population.Select(c => (double)c.Age).Average(),
                    info.Best.Fitness));

            if (info.Generation % 100 == 0)
            {
                Probes.printProgress(info);
            }
        });

    var solution = GeneticAlgorithm.Run(
        genotype: genotype,
        fitnessFunction: fitnessFunction(scores),
        terminate: terminate,
        options: options);

    return (name, solution, history);
}

void WriteCsv(string path, List<GenerationStats> history)
{
    // A CSV is an interchange format, not display text - the numbers must use "." for a
    // decimal point regardless of the machine's locale, or a comma-decimal culture (like
    // this one) silently corrupts the file by adding extra commas into what should be a
    // single field.
    var lines = new List<string> { "Generation,MeanFitness,MeanAge,BestFitness" };

    lines.AddRange(
        history.Select(s =>
            string.Create(
                CultureInfo.InvariantCulture,
                $"{s.Generation},{s.MeanFitness:F6},{s.MeanAge:F6},{s.BestFitness:F6}")));

    File.WriteAllLines(path, lines);
}

Console.WriteLine($"Simulating tiger evolution over {lastGeneration} generations in two environments...\n");

var results = environmentNames.Zip(environmentScores, RunEnvironment).ToArray();

foreach (var (name, _, history) in results)
{
    WriteCsv($"{name.ToLowerInvariant()}_stats.csv", history);
}

Console.WriteLine();
Console.WriteLine("Mean fitness / mean age by generation (sampled every 100 generations):");
Console.WriteLine($"{"Generation",10} | {"Tropical Fit.",14} | {"Tropical Age",12} | {"Tundra Fit.",14} | {"Tundra Age",12}");

for (var generation = 0; generation <= lastGeneration; generation += 100)
{
    var tropicalStats = results[0].History[generation];
    var tundraStats = results[1].History[generation];

    Console.WriteLine(
        $"{generation,10} | {tropicalStats.MeanFitness,14:F2} | {tropicalStats.MeanAge,12:F2} | " +
        $"{tundraStats.MeanFitness,14:F2} | {tundraStats.MeanAge,12:F2}");
}

Console.WriteLine();
Console.WriteLine("Final results:");

foreach (var (name, solution, history) in results)
{
    var final = history[^1];

    Console.WriteLine($"{name}:");
    Console.WriteLine($"  Final mean fitness: {final.MeanFitness:F2}, final mean age: {final.MeanAge:F2}");
    Console.WriteLine($"  Fittest tiger (fitness {solution.Fitness:F2}): {Describe(solution)}");
}

Console.WriteLine();
Console.WriteLine("Full per-generation statistics written to tropical_stats.csv and tundra_stats.csv");

/// One generation's tracked statistics: mean fitness and mean age across the whole
/// population, plus the fittest chromosome's fitness for that generation.
record GenerationStats(int Generation, double MeanFitness, double MeanAge, double BestFitness);
