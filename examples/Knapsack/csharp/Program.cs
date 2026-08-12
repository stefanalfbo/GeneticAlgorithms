using GeneticAlgorithms;

var profits = new[] { 6, 5, 8, 9, 6, 7, 3, 1, 2, 6 };
var weights = new[] { 10, 6, 8, 7, 10, 9, 7, 11, 6, 8 };
const int weightLimit = 40;

var solution = GeneticAlgorithm.Run(
    genotype: () => GeneticAlgorithm.CreateChromosome(
        Enumerable.Range(0, 10)
            .Select(_ => Random.Shared.Next(0, 2))
            .ToArray()),
    fitnessFunction: chromosome =>
    {
        var totalProfit = chromosome.Genes
            .Zip(profits, (gene, profit) => gene * profit)
            .Sum();

        var totalWeight = chromosome.Genes
            .Zip(weights, (gene, weight) => gene * weight)
            .Sum();

        return totalWeight > weightLimit ? 0.0 : totalProfit;
    },
    terminate: (population, generation, temperature) => generation >= 1000,
    populationSize: 50);

Console.WriteLine($"Best solution: [{string.Join("; ", solution.Genes)}] (fitness: {solution.Fitness:F6})");
