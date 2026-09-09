using System.Globalization;
using GeneticAlgorithms;

// The classic Rastrigin benchmark: highly multimodal, with a lattice of local minima
// surrounding the single global minimum f(x) = 0 at x = (0, ..., 0). A good stress test for
// showing a genetic algorithm escaping local optima where plain hill-climbing gets stuck.
const int dimensions = 5;
const double lowerBound = -5.12;
const double upperBound = 5.12;

// SelectionRate leaves 20% of the population as leftover each generation; elitist
// reinsertion carries the fittest 5% of (parents + leftover) forward alongside this
// generation's offspring, so 0.8 + 0.15 (MutationRate) + 0.05 keeps the population size
// roughly stable across all 300 generations - this runs a fixed generation count with no
// early-exit fitness target (continuous fitness rarely lands on an exact value), so the
// simpler pure reinsertion strategy would let the population grow without bound, just as it
// did for the classic discrete examples earlier this session. MutationRate is higher here
// (0.15) than most other examples' 0.05: Rastrigin's lattice of local minima needs more
// frequent mutants to reliably escape, and the elitist survivalRate is lowered to 0.05
// (rather than the usual 0.15) to keep the same 0.8 + MutationRate + survivalRate = 1.0
// invariant - raising MutationRate without lowering survivalRate to match reintroduces the
// exact unbounded-growth bug fixed elsewhere this session.
//
// CrossoverFn is wholeArithmeticCrossover rather than the library's default singlePoint:
// genes here are real-valued coordinates, not discrete values to swap wholesale - blending
// each gene as a weighted average of both parents makes far more sense for a continuous
// search space. MutationFn is gaussian rather than scramble for the same reason: scramble
// only reorders a chromosome's existing gene values, which is meaningless once genes are
// continuous coordinates rather than a fixed multiset - gaussian instead resamples each
// gene from a normal distribution fitted to the chromosome's own genes, so exploration
// naturally narrows as the population converges toward the optimum.
var options = GeneticAlgorithm.CreateOptions<double>(
    populationSize: 150,
    selectionFn: Selection.elite,
    crossoverFn: (rng, p1, p2) => Crossover.wholeArithmeticCrossover(0.5, rng, p1, p2),
    mutationFn: Mutation.gaussian,
    mutationRate: 0.15,
    reinsertionFn: (rng, parents, offspring, leftover) => Reinsertion.elitist(0.05, rng, parents, offspring, leftover),
    // There's no C#-facing equivalent of the F# Probes.everyNth combinator - throttling to
    // every 30th generation is just a plain conditional in the lambda.
    probe: info =>
    {
        if (info.Generation % 30 == 0)
        {
            Console.WriteLine(string.Create(CultureInfo.InvariantCulture, $"Current best f(x): {-info.Best.Fitness:F4}"));
        }
    });

const int lastGeneration = 300;

var solution = GeneticAlgorithm.Run(
    genotype: rng => GeneticAlgorithm.CreateChromosome(
        Enumerable.Range(0, dimensions)
            .Select(_ => lowerBound + rng.NextDouble() * (upperBound - lowerBound))
            .ToArray()),
    fitnessFunction: chromosome => -Rastrigin(chromosome.Genes),
    terminate: (population, generation, temperature) => generation == lastGeneration,
    options: options);

Console.WriteLine();
Console.WriteLine(string.Create(CultureInfo.InvariantCulture, $"Best solution found (f(x) = {Rastrigin(solution.Genes):F4}):"));
Console.WriteLine($"x = [{string.Join(", ", solution.Genes.Select(x => x.ToString("F4", CultureInfo.InvariantCulture)))}]");

static double Rastrigin(double[] genes)
{
    var n = genes.Length;

    return 10.0 * n + genes.Sum(x => x * x - 10.0 * Math.Cos(2.0 * Math.PI * x));
}
