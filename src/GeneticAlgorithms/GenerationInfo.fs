namespace GeneticAlgorithms

/// <summary>
/// A snapshot of one generation's state, passed to <c>Options.Probe</c>.
/// </summary>
/// <remarks>
/// It's up to the probe what to do with this - print it, collect it in memory, write it
/// to a file or database, push it to a monitoring service, and so on. The library only
/// decides when a probe fires and what it carries; everything else is up to the developer
/// plugging one in.
/// </remarks>
type GenerationInfo<'Gene> =
    { /// The current generation number, starting from zero.
      Generation: int
      /// This generation's population, evaluated and sorted by descending fitness.
      Population: Chromosome<'Gene> array
      /// The fittest chromosome this generation - equivalent to <c>Population.[0]</c>.
      Best: Chromosome<'Gene>
      /// The temperature computed for this generation, the same value passed to
      /// <c>Problem.Terminate</c>.
      Temperature: float }
