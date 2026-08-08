using GeneticAlgorithms;

var solution = GeneticAlgorithm.Run(
    genotype: () => GeneticAlgorithm.CreateChromosome(new[] { Random.Shared.Next(0, 2) }),
    fitnessFunction: chromosome => chromosome.Genes[0],
    terminate: (population, generation, temperature) =>
        population.Any(chromosome => chromosome.Fitness >= 1.0) || generation >= 10,
    populationSize: 8);

Console.WriteLine($"Best solution: {solution.Genes[0]} (fitness: {solution.Fitness:F1})");
