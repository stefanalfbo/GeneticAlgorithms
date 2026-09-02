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

    /// <summary>
    /// Combines <paramref name="offspring"/> with the fittest survivors of the previous
    /// generation: pools <paramref name="parents"/> and <paramref name="leftover"/> back
    /// together, and carries over the fittest
    /// <c>floor((parents.Length + leftover.Length) * survivalRate)</c> of them alongside
    /// <paramref name="offspring"/>.
    /// </summary>
    /// <remarks>
    /// Unlike <c>pure</c>, this never discards the previous generation outright - it keeps
    /// whichever of <paramref name="parents"/> and <paramref name="leftover"/> are fittest,
    /// which both preserves good genes that crossover or mutation might otherwise have lost
    /// and gives some control over population size: since <c>parents.Length + leftover.Length</c>
    /// equals the previous generation's population size, choosing
    /// <c>survivalRate</c> around <c>1.0 - SelectionRate - MutationRate</c> keeps the
    /// population roughly stable rather than drifting, as <c>pure</c> does. Assumes
    /// <paramref name="survivalRate"/> is in <c>[0, 1]</c>; this is not validated - a value
    /// above <c>1.0</c> would ask for more survivors than exist. Curry
    /// <paramref name="survivalRate"/> (e.g. <c>Reinsertion.elitist 0.15</c>) to use this as
    /// an <c>Options.ReinsertionFn</c>.
    /// </remarks>
    /// <param name="survivalRate">The fraction of the previous generation to carry over as survivors.</param>
    /// <param name="parents">The chromosomes selected as crossover parents this generation.</param>
    /// <param name="offspring">This generation's crossover children and mutants.</param>
    /// <param name="leftover">The chromosomes not selected as parents this generation.</param>
    /// <returns>
    /// <paramref name="offspring"/> combined with the fittest survivors of
    /// <paramref name="parents"/> and <paramref name="leftover"/>.
    /// </returns>
    let elitist
        (survivalRate: float)
        (parents: Chromosome<'Gene> array)
        (offspring: Chromosome<'Gene> array)
        (leftover: Chromosome<'Gene> array)
        =
        let old = Array.append parents leftover
        let n = int (float old.Length * survivalRate)

        let survivors =
            old |> Array.sortByDescending (fun chromosome -> chromosome.Fitness) |> Array.take n

        Array.append offspring survivors

    /// <summary>
    /// Combines <paramref name="offspring"/> with a uniformly random sample of the previous
    /// generation: pools <paramref name="parents"/> and <paramref name="leftover"/> back
    /// together, and carries over
    /// <c>floor((parents.Length + leftover.Length) * survivalRate)</c> of them, chosen
    /// uniformly at random rather than by fitness.
    /// </summary>
    /// <remarks>
    /// Like <c>elitist</c>, but survivors are drawn without regard to fitness - closer to
    /// <c>Selection.random</c> than to <c>Selection.elite</c>. This keeps population size
    /// under the same control as <c>elitist</c> (see its remarks on choosing
    /// <c>survivalRate</c>), without <c>elitist</c>'s bias toward carrying over the same
    /// fittest chromosomes generation after generation, at the cost of not deliberately
    /// preserving good genes the way <c>elitist</c> does. Assumes
    /// <paramref name="survivalRate"/> is in <c>[0, 1]</c>; this is not validated - a value
    /// above <c>1.0</c> would ask for more survivors than exist. Curry
    /// <paramref name="survivalRate"/> (e.g. <c>Reinsertion.uniform 0.15</c>) to use this as
    /// an <c>Options.ReinsertionFn</c>.
    /// </remarks>
    /// <param name="survivalRate">The fraction of the previous generation to carry over as survivors.</param>
    /// <param name="parents">The chromosomes selected as crossover parents this generation.</param>
    /// <param name="offspring">This generation's crossover children and mutants.</param>
    /// <param name="leftover">The chromosomes not selected as parents this generation.</param>
    /// <returns>
    /// <paramref name="offspring"/> combined with a uniformly random sample of
    /// <paramref name="parents"/> and <paramref name="leftover"/>.
    /// </returns>
    let uniform
        (survivalRate: float)
        (parents: Chromosome<'Gene> array)
        (offspring: Chromosome<'Gene> array)
        (leftover: Chromosome<'Gene> array)
        =
        let old = Array.append parents leftover
        let n = int (float old.Length * survivalRate)

        let survivors =
            old |> Array.sortBy (fun _ -> System.Random.Shared.Next()) |> Array.take n

        Array.append offspring survivors
