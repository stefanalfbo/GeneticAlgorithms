namespace GeneticAlgorithms

/// <summary>
/// Convenience builders for <c>Options&lt;'Gene&gt;</c>, for F# consumers who want sensible
/// defaults without spelling out every field by hand.
/// </summary>
/// <remarks>
/// This is the F#-native counterpart to <c>GeneticAlgorithm.CreateOptions</c> in
/// <c>Interop.fs</c>. That type's overloaded static factories and <c>Func</c>/<c>Action</c>-
/// wrapped fields exist for C# consumers, where overload resolution is the normal way to
/// express "give me defaults, but let me override some of them." F# already has a more
/// natural way to do the same thing - record-update syntax - so <c>create</c> returns a plain
/// <c>Options&lt;'Gene&gt;</c> record instead of adding more overloads:
/// <c>{ Options.create 100 with SelectionFn = Selection.tournament 3 }</c>.
/// </remarks>
module Options =

    /// <summary>
    /// Builds an <c>Options&lt;'Gene&gt;</c> with <paramref name="populationSize"/> and
    /// sensible defaults for every other field.
    /// </summary>
    /// <remarks>
    /// <paramref name="populationSize"/> has no sensible default - every genotype and problem
    /// needs its own appropriately sized population - so it's the one required argument,
    /// rather than being defaulted like every other field. The defaults otherwise match
    /// <c>GeneticAlgorithm.CreateOptions</c>'s own: <c>SelectionRate</c> 0.8 +
    /// <c>MutationRate</c> 0.05 + <c>Reinsertion.elitist</c>'s survival rate 0.15 sum to 1.0,
    /// which keeps population size stable across generations (see <c>Reinsertion.elitist</c>'s
    /// own remarks for why that matters). <paramref name="populationSize"/> itself is not
    /// validated here - <c>Genetic.run</c> rejects a non-positive value wherever the resulting
    /// options end up being used to run an evolution.
    /// </remarks>
    /// <param name="populationSize">The number of chromosomes in the population.</param>
    /// <returns>
    /// An <c>Options&lt;'Gene&gt;</c> with <paramref name="populationSize"/> and default
    /// values for every other field, ready to override via record-update syntax.
    /// </returns>
    let create (populationSize: int) : Options<'Gene> =
        { PopulationSize = populationSize
          SelectionRate = 0.8
          SelectionFn = Selection.elite
          CrossoverFn = Crossover.singlePoint
          MutationRate = 0.05
          MutationFn = Mutation.scramble
          ReinsertionFn = Reinsertion.elitist 0.15
          Probe = Probes.noop }
