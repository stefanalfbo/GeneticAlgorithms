using GeneticAlgorithms;

const string target = "ILoveGeneticAlgorithms";
const string encrypted = "LIjs`B`k`qlfDibjwlqmhv";

var solution = GeneticAlgorithm.Run(
    genotype: () => GeneticAlgorithm.CreateChromosome(
        Enumerable.Range(0, target.Length)
            .Select(_ => Random.Shared.Next(0, 2))
            .ToArray()),
    fitnessFunction: chromosome => Distance.jaro(target, Cipher(encrypted, ChromosomeKey(chromosome))),
    terminate: (population, generation, temperature) =>
        population.Any(chromosome => chromosome.Fitness >= 1.0),
    populationSize: 100);

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
