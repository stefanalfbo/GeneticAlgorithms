namespace GeneticAlgorithms

/// <summary>
/// Mutation strategies that transform a single chromosome's genes.
/// </summary>
/// <remarks>
/// Every strategy has the shape <c>Chromosome&lt;'Gene&gt; -&gt; Chromosome&lt;'Gene&gt;</c>
/// (one chromosome in, one mutated chromosome out), so any of them can be plugged in as
/// <c>Options.MutationFn</c>. Whether a given chromosome is mutated at all is decided
/// separately, by <c>Genetic.mutation</c> rolling against <c>Options.MutationRate</c> - the
/// strategies here only decide how to mutate a chromosome once that decision has already
/// been made.
/// </remarks>
module Mutation =

    /// <summary>
    /// Mutates a chromosome by randomly shuffling its genes into a new order.
    /// </summary>
    /// <remarks>
    /// Preserves the exact multiset of gene values, so it never introduces a gene value
    /// that wasn't already present - safe to use with permutation genotypes (as in
    /// <c>NQueens</c>), unlike a strategy that replaces individual genes with newly
    /// generated values.
    /// </remarks>
    /// <param name="chromosome">The chromosome to mutate.</param>
    /// <returns>A new chromosome with the same genes in a randomly shuffled order.</returns>
    let shuffle (chromosome: Chromosome<'Gene>) =
        { chromosome with
            Genes = chromosome.Genes |> Array.sortBy (fun _ -> System.Random.Shared.Next()) }
