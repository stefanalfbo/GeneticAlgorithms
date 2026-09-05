using GeneticAlgorithms;

const string target = "helloworld";

var solution = GeneticAlgorithm.Run(
    genotype: () => GeneticAlgorithm.CreateChromosome(
        Enumerable.Range(0, target.Length)
            .Select(_ => RandomChar())
            .ToArray()),
    fitnessFunction: chromosome => Fitness(chromosome.Genes),
    terminate: (population, generation, temperature) =>
        population.Any(chromosome => chromosome.Fitness >= 1.0),
    populationSize: 100,
    probe: Probes.printProgress);

Console.WriteLine($"Best solution: {new string(solution.Genes)} (fitness: {solution.Fitness:F6})");

static char RandomChar() =>
    (char)Random.Shared.Next('a', 'z' + 1);

static double Fitness(char[] genes)
{
    var matches = genes.Where((gene, index) => gene == target[index]).Count();

    return (double)matches / target.Length;
}
