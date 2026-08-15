using GeneticAlgorithms;

var solution = GeneticAlgorithm.Run(
    genotype: () => GeneticAlgorithm.CreateChromosome(new[] { Random.Shared.Next(0, 2) }),
    fitnessFunction: chromosome => chromosome.Genes[0],
    terminate: (population, generation, temperature) =>
        population.Any(chromosome => chromosome.Fitness >= 1.0) || generation >= 10,
    populationSize: 8);

if (solution.Fitness < 1.0)
{
    throw new Exception($"Smoke test failed: expected fitness 1.0, got {solution.Fitness}");
}

Console.WriteLine($"Smoke test passed - GeneticAlgorithms package installs and runs correctly (fitness: {solution.Fitness:F1})");
