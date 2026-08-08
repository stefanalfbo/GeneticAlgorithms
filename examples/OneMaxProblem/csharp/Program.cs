using GeneticAlgorithms;

const double MaxFitness = 1000.0;

var solution = GeneticAlgorithm.Run(
    genotype: () => GeneticAlgorithm.CreateChromosome(
        Enumerable.Range(0, 1000)
            .Select(_ => Random.Shared.Next(0, 2))
            .ToArray()),
    fitnessFunction: chromosome => chromosome.Genes.Sum(),
    terminate: (population, generation, temperature) =>
        population.Any(chromosome => chromosome.Fitness >= MaxFitness),
    populationSize: 100);

Console.WriteLine($"Best solution: {solution.Fitness:F0}");
