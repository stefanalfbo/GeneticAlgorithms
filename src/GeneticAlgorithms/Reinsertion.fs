namespace GeneticAlgorithms

/// <summary>
/// Reinsertion strategies that decide how a generation's parents, offspring, and leftover
/// chromosomes combine into the next population.
/// </summary>
/// <remarks>
/// Every strategy has the shape
/// <c>Chromosome&lt;'Gene&gt; array -&gt; Chromosome&lt;'Gene&gt; array -&gt; Chromosome&lt;'Gene&gt; array -&gt; Chromosome&lt;'Gene&gt; array</c>
/// (parents, then offspring, then leftover, producing the next population), so any of them
/// can be plugged in as <c>Options.ReinsertionFn</c>. <c>Genetic.evolve</c> calls the
/// configured strategy once per generation, after selection, crossover, and mutation have
/// already produced <c>offspring</c> (this generation's crossover children together with
/// <c>Genetic.mutation</c>'s mutants) - the strategies here only decide how the three
/// groups recombine, not how any of them were produced.
/// </remarks>
module Reinsertion =

    /// <summary>
    /// Replaces the population outright with <paramref name="offspring"/>, discarding
    /// <paramref name="parents"/> and <paramref name="leftover"/> entirely.
    /// </summary>
    /// <remarks>
    /// The simplest possible reinsertion strategy - and the only one in this module that
    /// doesn't look at <paramref name="parents"/> or <paramref name="leftover"/> at all.
    /// Because <c>Genetic.mutation</c> draws its mutants from the whole population rather
    /// than replacing genes in place, <paramref name="offspring"/> is not guaranteed to be
    /// the same size as the population it replaces, so population size can drift from one
    /// generation to the next: at <c>SelectionRate &lt; 1.0</c>, discarding
    /// <paramref name="leftover"/> shrinks the population every generation; at
    /// <c>SelectionRate = 1.0</c> (no leftover to lose), the mutants add extra individuals
    /// on top of a full set of crossover children, so the population instead grows without
    /// bound, generation over generation.
    /// </remarks>
    /// <param name="parents">The chromosomes selected as crossover parents this generation. Ignored.</param>
    /// <param name="offspring">This generation's crossover children and mutants.</param>
    /// <param name="leftover">The chromosomes not selected as parents this generation. Ignored.</param>
    /// <returns><paramref name="offspring"/>, unchanged.</returns>
    let ``pure``
        (_parents: Chromosome<'Gene> array)
        (offspring: Chromosome<'Gene> array)
        (_leftover: Chromosome<'Gene> array)
        =
        offspring
