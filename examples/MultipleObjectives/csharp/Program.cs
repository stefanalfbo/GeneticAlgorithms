using GeneticAlgorithms;

const double targetFitness = 180.0;

var options = GeneticAlgorithm.CreateOptions<(int Roi, int Risk)>(
    populationSize: 125,
    selectionFn: Selection.elite,
    crossoverFn: Crossover.singlePoint,
    mutationFn: Mutation.scramble,
    reinsertionFn: (parents, offspring, leftover) => Reinsertion.elitist(0.15, parents, offspring, leftover),
    probe: Probes.printProgress);

var solution = GeneticAlgorithm.Run(
    genotype: () => GeneticAlgorithm.CreateChromosome(
        Enumerable.Range(0, 10)
            .Select(_ => (Roi: Random.Shared.Next(1, 11), Risk: Random.Shared.Next(1, 11)))
            .ToArray()),
    fitnessFunction: chromosome => chromosome.Genes.Sum(gene => 2 * gene.Roi - gene.Risk),
    terminate: (population, _, _) => population.Any(chromosome => chromosome.Fitness >= targetFitness),
    options: options);

Console.WriteLine(
    $"Best solution: [{string.Join("; ", solution.Genes.Select(gene => $"({gene.Roi}, {gene.Risk})"))}] " +
    $"(fitness: {solution.Fitness:F6})");
