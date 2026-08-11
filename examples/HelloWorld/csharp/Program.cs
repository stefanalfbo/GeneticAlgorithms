using GeneticAlgorithms;

const string target = "helloworld";

var solution = GeneticAlgorithm.Run(
    genotype: () => GeneticAlgorithm.CreateChromosome(
        Enumerable.Range(0, target.Length)
            .Select(_ => RandomChar())
            .ToArray()),
    fitnessFunction: chromosome => JaroSimilarity(new string(chromosome.Genes), target),
    terminate: (population, generation, temperature) =>
        population.Any(chromosome => chromosome.Fitness > 0.95),
    populationSize: 100);

Console.WriteLine($"Best solution: {new string(solution.Genes)} (fitness: {solution.Fitness:F6})");

static char RandomChar() =>
    (char)Random.Shared.Next('a', 'z' + 1);

static double JaroSimilarity(string source, string target)
{
    if (source == target)
    {
        return 1.0;
    }

    if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
    {
        return 0.0;
    }

    var matchDistance = Math.Max(0, Math.Max(source.Length, target.Length) / 2 - 1);

    var sourceMatches = new bool[source.Length];
    var targetMatches = new bool[target.Length];

    for (var i = 0; i < source.Length; i++)
    {
        var startIndex = Math.Max(0, i - matchDistance);
        var endIndex = Math.Min(target.Length, i + matchDistance + 1);
        var found = false;

        for (var j = startIndex; j < endIndex && !found; j++)
        {
            if (!targetMatches[j] && source[i] == target[j])
            {
                sourceMatches[i] = true;
                targetMatches[j] = true;
                found = true;
            }
        }
    }

    var sourceChars = source.Where((_, index) => sourceMatches[index]).ToArray();
    var targetChars = target.Where((_, index) => targetMatches[index]).ToArray();

    var matches = sourceChars.Length;

    if (matches == 0)
    {
        return 0.0;
    }

    var transpositions = sourceChars
        .Zip(targetChars, (left, right) => left == right ? 0 : 1)
        .Sum() / 2.0;

    return ((double)matches / source.Length
            + (double)matches / target.Length
            + (matches - transpositions) / matches)
           / 3.0;
}
