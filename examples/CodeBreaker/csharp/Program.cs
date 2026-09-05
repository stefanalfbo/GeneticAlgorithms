using GeneticAlgorithms;

const string target = "ILoveGeneticAlgorithms";
const string encrypted = "LIjs`B`k`qlfDibjwlqmhv";

// The simplest Run overload defaults to Reinsertion.pure, which discards 20% of the
// population (SelectionRate's leftover) every generation with nothing to replace it -
// the population collapses to a tiny, stuck equilibrium long before finding a solution.
// Reinsertion.elitist with a survivalRate of 0.15 keeps the population roughly stable
// instead (SelectionRate 0.8 + MutationRate 0.05 + survivalRate 0.15 = 1.0).
var options = GeneticAlgorithm.CreateOptions<int>(
    populationSize: 100,
    selectionFn: Selection.elite,
    crossoverFn: Crossover.singlePoint,
    mutationFn: Mutation.scramble,
    reinsertionFn: (parents, offspring, leftover) => Reinsertion.elitist(0.15, parents, offspring, leftover),
    probe: Probes.printProgress);

var solution = GeneticAlgorithm.Run(
    genotype: () => GeneticAlgorithm.CreateChromosome(
        Enumerable.Range(0, target.Length)
            .Select(_ => Random.Shared.Next(0, 2))
            .ToArray()),
    fitnessFunction: chromosome => Distance.jaroSimilarity(target, Cipher(encrypted, ChromosomeKey(chromosome))),
    terminate: (population, generation, temperature) =>
        population.Any(chromosome => chromosome.Fitness >= 1.0),
    options: options);

var key = ChromosomeKey(solution);
var decoded = Cipher(encrypted, key);

Console.WriteLine($"Target:   {target}");
Console.WriteLine($"Decoded:  {decoded}");
Console.WriteLine($"Key:      {key} ([{string.Join("; ", solution.Genes)}])");
Console.WriteLine($"Fitness:  {solution.Fitness:F6}");

static string Cipher(string text, int key) =>
    new(text.Select(character => (char)((character ^ key) % 32768)).ToArray());

static int ChromosomeKey(Chromosome<int> chromosome) =>
    chromosome.Genes.Aggregate(0, (key, gene) => (key << 1) | gene);
