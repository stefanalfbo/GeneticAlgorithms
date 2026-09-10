namespace GeneticAlgorithms

/// <summary>
/// A candidate solution: a sequence of genes, its most recently computed fitness, and how
/// many generations it has existed.
/// </summary>
/// <remarks>
/// A chromosome is "born" - and <c>Age</c> resets to <c>0</c> - when <c>Crossover</c>
/// produces it from two parents, or when a fresh genotype is generated (the initial
/// population, or padding a shortfall back to <c>PopulationSize</c>; see
/// <c>Genetic.resizeToPopulationSize</c>). <c>Genetic.mutation</c> only changes an existing
/// chromosome's <c>Genes</c> in place, so mutation does not reset <c>Age</c> - it is still
/// the same individual, one generation older, with modified genes, the same way a
/// <c>Reinsertion</c>-carried-over survivor keeps aging without being "reborn".
/// <c>Genetic.evaluate</c> increments <c>Age</c> by exactly one every generation, before
/// <c>Options.Probe</c> or <c>Problem.Terminate</c> ever see that generation's population -
/// so the minimum <c>Age</c> ever observable in a <c>GenerationInfo.Population</c> snapshot
/// is <c>1</c>, not <c>0</c>, for both the very first generation and any chromosome born
/// since.
/// </remarks>
type Chromosome<'T> =
    { Genes: 'T array
      Fitness: float
      Age: int }

    member this.Size = this.Genes.Length
