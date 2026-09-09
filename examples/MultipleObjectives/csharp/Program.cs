using GeneticAlgorithms;

const double targetFitness = 180.0;

var options = GeneticAlgorithm.CreateOptions<(int Roi, int Risk)>(
    populationSize: 125,
    selectionFn: Selection.elite,
    crossoverFn: Crossover.singlePoint,
    mutationFn: Mutation.scramble,
    reinsertionFn: (rng, parents, offspring, leftover) => Reinsertion.elitist(0.15, rng, parents, offspring, leftover),
    probe: Probes.printProgress);

var solution = GeneticAlgorithm.Run(
    genotype: rng => GeneticAlgorithm.CreateChromosome(
        Enumerable.Range(0, 10)
            .Select(_ => (Roi: rng.Next(1, 11), Risk: rng.Next(1, 11)))
            .ToArray()),
    fitnessFunction: chromosome => chromosome.Genes.Sum(gene => 2 * gene.Roi - gene.Risk),
    terminate: (population, _, _) => population.Any(chromosome => chromosome.Fitness >= targetFitness),
    options: options);

Console.WriteLine(
    $"Best solution: [{string.Join("; ", solution.Genes.Select(gene => $"({gene.Roi}, {gene.Risk})"))}] " +
    $"(fitness: {solution.Fitness:F6})");
