using GeneticAlgorithms;

var classNames = new[]
{
    "Algorithms",
    "Artificial Intelligence",
    "Calculus",
    "Chemistry",
    "Data Structures",
    "Discrete Math",
    "History",
    "Literature",
    "Physics",
    "Volleyball"
};

var creditHours = new[] { 3.0, 3.0, 3.0, 4.5, 3.0, 3.0, 3.0, 3.0, 4.5, 1.5 };
var difficulties = new[] { 8.0, 9.0, 4.0, 3.0, 5.0, 2.0, 4.0, 2.0, 6.0, 1.0 };
var usefulness = new[] { 8.0, 9.0, 6.0, 2.0, 8.0, 9.0, 1.0, 2.0, 5.0, 1.0 };
var interest = new[] { 8.0, 8.0, 5.0, 9.0, 7.0, 2.0, 8.0, 2.0, 7.0, 10.0 };

var solution = GeneticAlgorithm.Run(
    genotype: () => GeneticAlgorithm.CreateChromosome(
        Enumerable.Range(0, classNames.Length)
            .Select(_ => Random.Shared.Next(0, 2))
            .ToArray()),
    fitnessFunction: chromosome =>
    {
        var schedule = chromosome.Genes;

        var fitness = schedule
            .Select((selected, index) =>
                selected * (0.3 * usefulness[index] + 0.3 * interest[index] - 0.3 * difficulties[index]))
            .Sum();

        var credits = schedule
            .Select((selected, index) => selected * creditHours[index])
            .Sum();

        return credits > 18.0 ? -99999.0 : fitness;
    },
    terminate: (population, generation, temperature) => generation == 1000,
    populationSize: 100);

var selectedClasses = solution.Genes
    .Zip(classNames)
    .Where(pair => pair.First == 1)
    .Select(pair => pair.Second);

Console.WriteLine($"Best schedule: [{string.Join("; ", solution.Genes)}] (fitness: {solution.Fitness:F6})");
Console.WriteLine($"Classes:       {string.Join(", ", selectedClasses)}");
