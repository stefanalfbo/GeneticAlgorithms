using GeneticAlgorithms;

const int n = 8;
const double maxFitness = n;

var options = GeneticAlgorithm.CreateOptions<int>(
    populationSize: 100,
    selectionFn: Selection.elite,
    crossoverFn: Crossover.orderOneCrossover,
    mutationFn: Mutation.scramble,
    reinsertionFn: Reinsertion.pure,
    probe: Probes.printProgress);

var solution = GeneticAlgorithm.Run(
    genotype: () => GeneticAlgorithm.CreateChromosome(
        Enumerable.Range(0, n)
            .OrderBy(_ => Random.Shared.Next())
            .ToArray()),
    fitnessFunction: chromosome =>
    {
        var genes = chromosome.Genes;
        var diagClashes = 0;

        for (var i = 0; i < n; i++)
        {
            for (var j = 0; j < n; j++)
            {
                if (i != j && Math.Abs(i - j) == Math.Abs(genes[i] - genes[j]))
                {
                    diagClashes++;
                }
            }
        }

        var distinctGenes = genes.Distinct().Count();

        return distinctGenes - diagClashes;
    },
    terminate: (population, generation, temperature) =>
        population.Any(chromosome => chromosome.Fitness >= maxFitness),
    options: options);

Console.WriteLine($"Best solution: [{string.Join("; ", solution.Genes)}] (fitness: {solution.Fitness:F6} / {maxFitness:F6})");
