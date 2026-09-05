using GeneticAlgorithms;

const double MaxFitness = 1000.0;

// The simplest Run overload defaults to Reinsertion.pure, which discards 20% of the
// population (SelectionRate's leftover) every generation with nothing to replace it -
// fine for a small search space, but for 1000 genes the population collapses to a tiny,
// stuck equilibrium long before finding a solution. Reinsertion.elitist with a
// survivalRate of 0.15 keeps the population roughly stable instead (SelectionRate 0.8 +
// MutationRate 0.05 + survivalRate 0.15 = 1.0).
var options = GeneticAlgorithm.CreateOptions<int>(
    populationSize: 100,
    selectionFn: Selection.elite,
    crossoverFn: Crossover.singlePoint,
    mutationFn: Mutation.scramble,
    reinsertionFn: (parents, offspring, leftover) => Reinsertion.elitist(0.15, parents, offspring, leftover),
    probe: Probes.printProgress);

var solution = GeneticAlgorithm.Run(
    genotype: () => GeneticAlgorithm.CreateChromosome(
        Enumerable.Range(0, 1000)
            .Select(_ => Random.Shared.Next(0, 2))
            .ToArray()),
    fitnessFunction: chromosome => chromosome.Genes.Sum(),
    terminate: (population, generation, temperature) =>
        population.Any(chromosome => chromosome.Fitness >= MaxFitness),
    options: options);

Console.WriteLine($"Best solution: {solution.Fitness:F0}");
