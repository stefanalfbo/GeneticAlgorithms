using GeneticAlgorithms;

// A OneMax-style problem: maximize the number of 1s in a binary chromosome. Deliberately
// simple and large (see numberOfGenes below) so the only thing that determines how well a
// run does is how well it explores and preserves diversity - which is exactly what
// reinsertion controls. A small search space lets even a badly shrunk population stumble
// onto a near-optimal answer by luck, which hides the differences between strategies
// rather than showing them.

const int numberOfGenes = 500;
const double maxFitness = numberOfGenes;
const int lastGeneration = 300;

// SelectionRate is fixed at 0.8 by GeneticAlgorithm.CreateOptions, leaving 20% of the
// population unselected as `leftover` each generation - that's the pool `elitist` and
// `uniform` draw survivors from. `pure` ignores that pool entirely, so it loses 20% of the
// population every generation until it settles into a small, mostly-static equilibrium -
// the point of this example is to make that visible next to the other two strategies.
//
// `elitist` and `uniform` are given a survivalRate chosen so that
// SelectionRate + MutationRate + survivalRate = 1.0 (0.8 + 0.05 + 0.15), which keeps their
// population size roughly stable across generations instead of drifting like `pure` does.
const double survivalRate = 0.15;

Func<Chromosome<int>> genotype = () =>
    GeneticAlgorithm.CreateChromosome(
        Enumerable.Range(0, numberOfGenes)
            .Select(_ => Random.Shared.Next(0, 2))
            .ToArray());

Func<Chromosome<int>, double> fitnessFunction = chromosome => chromosome.Genes.Sum();

Func<IEnumerable<Chromosome<int>>, int, double, bool> terminate =
    (_, generation, _) => generation == lastGeneration;

var strategies = new (string Name, Func<Chromosome<int>[], Chromosome<int>[], Chromosome<int>[], Chromosome<int>[]> ReinsertionFn)[]
{
    ("pure", Reinsertion.pure),
    ("elitist", (parents, offspring, leftover) => Reinsertion.elitist(survivalRate, parents, offspring, leftover)),
    ("uniform", (parents, offspring, leftover) => Reinsertion.uniform(survivalRate, parents, offspring, leftover)),
};

// Runs the genetic algorithm once with the given reinsertion strategy, recording the best
// fitness seen at every generation along the way so the three runs can be compared
// side by side afterward.
(string Name, Chromosome<int> Solution, double[] FitnessByGeneration) RunStrategy(
    string name,
    Func<Chromosome<int>[], Chromosome<int>[], Chromosome<int>[], Chromosome<int>[]> reinsertionFn)
{
    var fitnessByGeneration = new double[lastGeneration + 1];

    var options = GeneticAlgorithm.CreateOptions<int>(
        populationSize: 100,
        selectionFn: Selection.elite,
        crossoverFn: Crossover.singlePoint,
        mutationFn: Mutation.scramble,
        reinsertionFn: reinsertionFn,
        onGeneration: (best, generation) => fitnessByGeneration[generation] = best.Fitness);

    var solution = GeneticAlgorithm.Run(
        genotype: genotype,
        fitnessFunction: fitnessFunction,
        terminate: terminate,
        options: options);

    return (name, solution, fitnessByGeneration);
}

var results = strategies.Select(s => RunStrategy(s.Name, s.ReinsertionFn)).ToArray();

Console.WriteLine($"Maximum possible fitness: {maxFitness:F0} (all {numberOfGenes} genes set to 1)");
Console.WriteLine();
Console.WriteLine("Best fitness by generation (sampled every 30 generations):");
Console.WriteLine($"{"Generation",10} | {"pure",8} | {"elitist",8} | {"uniform",8}");

for (var generation = 0; generation <= lastGeneration; generation += 30)
{
    var pureFitness = results[0].FitnessByGeneration[generation];
    var elitistFitness = results[1].FitnessByGeneration[generation];
    var uniformFitness = results[2].FitnessByGeneration[generation];

    Console.WriteLine($"{generation,10} | {pureFitness,8:F1} | {elitistFitness,8:F1} | {uniformFitness,8:F1}");
}

Console.WriteLine();
Console.WriteLine("Final results:");

foreach (var (name, solution, _) in results)
{
    var percentage = 100.0 * solution.Fitness / maxFitness;
    Console.WriteLine($"{name,-8} fitness: {solution.Fitness,5:F1} / {maxFitness:F0} ({percentage,5:F1}% of maximum)");
}
