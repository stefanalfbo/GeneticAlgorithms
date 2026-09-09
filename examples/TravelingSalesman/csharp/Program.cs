using GeneticAlgorithms;

// A small, fixed set of cities with illustrative (x, y) coordinates - not real
// geographic positions, just a 2D layout loosely echoing where these cities sit relative
// to one another, good enough to make "the tour crosses back over itself" visibly wrong.
var cities = new (string Name, (double X, double Y) Coordinates)[]
{
    ("Stockholm", (65.0, 80.0)),
    ("Oslo", (55.0, 85.0)),
    ("Helsinki", (75.0, 82.0)),
    ("Copenhagen", (58.0, 70.0)),
    ("Berlin", (60.0, 60.0)),
    ("Amsterdam", (48.0, 62.0)),
    ("London", (38.0, 58.0)),
    ("Paris", (45.0, 50.0)),
    ("Vienna", (65.0, 55.0)),
    ("Rome", (58.0, 30.0)),
    ("Madrid", (20.0, 25.0)),
    ("Lisbon", (10.0, 25.0)),
};

var cityNames = cities.Select(c => c.Name).ToArray();
var coordinates = cities.Select(c => c.Coordinates).ToArray();
var numberOfCities = cities.Length;

// A chromosome's genes are a permutation of city indices - the order in which the tour
// visits them. Genes[i] and Genes[i + 1] are consecutive stops, and the tour is a closed
// loop: the last city connects back to the first.
double TourDistance(Chromosome<int> chromosome)
{
    var order = chromosome.Genes;
    var total = 0.0;

    for (var i = 0; i < numberOfCities; i++)
    {
        var (x1, y1) = coordinates[order[i]];
        var (x2, y2) = coordinates[order[(i + 1) % numberOfCities]];
        total += Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
    }

    return total;
}

const int lastGeneration = 500;

// Options.create's (and GeneticAlgorithm.CreateOptions's) defaults - SelectionRate 0.8 +
// MutationRate 0.05 + Reinsertion.elitist's survivalRate 0.15 summing to 1.0 - keep
// population size stable across all 500 generations.
//
// crossoverFn is orderOneCrossover rather than the default singlePoint: genes here are a
// permutation (each city visited exactly once), and a single-point cut would generally
// produce a tour with a city missing and another repeated.
var options = GeneticAlgorithm.CreateOptions<int>(
    populationSize: 100,
    selectionFn: Selection.elite,
    crossoverFn: Crossover.orderOneCrossover,
    mutationFn: Mutation.scramble,
    reinsertionFn: (rng, parents, offspring, leftover) => Reinsertion.elitist(0.15, rng, parents, offspring, leftover),
    probe: info =>
    {
        if (info.Generation % 50 == 0)
        {
            Console.WriteLine($"Current best distance: {-info.Best.Fitness:F2}");
        }
    });

var solution = GeneticAlgorithm.Run(
    genotype: rng => GeneticAlgorithm.CreateChromosome(
        Enumerable.Range(0, numberOfCities)
            .OrderBy(_ => rng.Next())
            .ToArray()),
    // The genetic algorithm always maximizes fitness, but a tour's quality is its total
    // distance, which we want to minimize - so fitness is simply the negated distance: the
    // shortest tour has the fitness closest to zero (the largest, least negative value).
    fitnessFunction: chromosome => -TourDistance(chromosome),
    terminate: (population, generation, temperature) => generation == lastGeneration,
    options: options);

var tourNames = solution.Genes.Select(cityIndex => cityNames[cityIndex]).ToArray();

Console.WriteLine();
Console.WriteLine($"Best tour found (total distance: {TourDistance(solution):F2}):");
Console.WriteLine($"{string.Join(" -> ", tourNames)} -> {tourNames[0]}");
