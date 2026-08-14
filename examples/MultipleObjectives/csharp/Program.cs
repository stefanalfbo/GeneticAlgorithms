using GeneticAlgorithms;

const double targetFitness = 180.0;

var solution = GeneticAlgorithm.Run(
    genotype: () => GeneticAlgorithm.CreateChromosome(
        Enumerable.Range(0, 10)
            .Select(_ => (Roi: Random.Shared.Next(1, 11), Risk: Random.Shared.Next(1, 11)))
            .ToArray()),
    fitnessFunction: chromosome => chromosome.Genes.Sum(gene => 2 * gene.Roi - gene.Risk),
    terminate: (population, _, _) => population.Any(chromosome => chromosome.Fitness >= targetFitness),
    populationSize: 125);

Console.WriteLine(
    $"Best solution: [{string.Join("; ", solution.Genes.Select(gene => $"({gene.Roi}, {gene.Risk})"))}] " +
    $"(fitness: {solution.Fitness:F6})");
