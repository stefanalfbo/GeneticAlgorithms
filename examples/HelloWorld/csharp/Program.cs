using GeneticAlgorithms;
using Microsoft.FSharp.Core;

const string target = "helloworld";

var options = GeneticAlgorithm.CreateOptions<char>(
    populationSize: 100,
    selectionFn: Selection.elite,
    crossoverFn: Crossover.singlePoint,
    mutationFn: (rng, chromosome) => Mutation.randomReset(0.1, FuncConvert.FromFunc<Random, char>(RandomChar), rng, chromosome),
    reinsertionFn: (rng, parents, offspring, leftover) => Reinsertion.elitist(0.15, rng, parents, offspring, leftover),
    probe: Probes.printProgress);

var solution = GeneticAlgorithm.Run(
    genotype: rng => GeneticAlgorithm.CreateChromosome(
        Enumerable.Range(0, target.Length)
            .Select(_ => RandomChar(rng))
            .ToArray()),
    fitnessFunction: chromosome => Fitness(chromosome.Genes),
    terminate: (population, generation, temperature) =>
        population.Any(chromosome => chromosome.Fitness >= 1.0),
    options: options);

Console.WriteLine($"Best solution: {new string(solution.Genes)} (fitness: {solution.Fitness:F6})");

static char RandomChar(Random rng) =>
    (char)rng.Next('a', 'z' + 1);

static double Fitness(char[] genes)
{
    var matches = genes.Where((gene, index) => gene == target[index]).Count();

    return (double)matches / target.Length;
}
