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
/// been made. <c>flip</c> and <c>flipEachGene</c> only make sense for binary genotypes
/// (<c>Chromosome&lt;int&gt;</c> with genes of <c>0</c> or <c>1</c>), so unlike
/// <c>scramble</c> they work on <c>int</c> specifically rather than any <c>'Gene</c> type.
/// </remarks>
module Mutation =

    /// <summary>
    /// Mutates a chromosome by randomly scrambling its genes into a new order.
    /// </summary>
    /// <remarks>
    /// Preserves the exact multiset of gene values, so it never introduces a gene value
    /// that wasn't already present - safe to use with permutation genotypes (as in
    /// <c>NQueens</c>), unlike a strategy that replaces individual genes with newly
    /// generated values.
    /// </remarks>
    /// <param name="chromosome">The chromosome to mutate.</param>
    /// <returns>A new chromosome with the same genes in a randomly scrambled order.</returns>
    let scramble (chromosome: Chromosome<'Gene>) =
        { chromosome with
            Genes = chromosome.Genes |> Array.sortBy (fun _ -> System.Random.Shared.Next()) }

    /// <summary>
    /// Mutates a binary chromosome by flipping every gene: each <c>0</c> becomes <c>1</c>
    /// and each <c>1</c> becomes <c>0</c>.
    /// </summary>
    /// <remarks>
    /// This is an aggressive mutation - every gene changes, every time. See
    /// <c>flipEachGene</c> for a version that only flips each gene with some probability.
    /// Assumes every gene is <c>0</c> or <c>1</c>; for any other integer value it toggles
    /// the lowest bit, which is unlikely to be meaningful.
    /// </remarks>
    /// <param name="chromosome">The chromosome to mutate.</param>
    /// <returns>A new chromosome with every gene flipped.</returns>
    let flip (chromosome: Chromosome<int>) =
        { chromosome with
            Genes = chromosome.Genes |> Array.map (fun gene -> gene ^^^ 1) }

    /// <summary>
    /// Mutates a binary chromosome by flipping each gene independently with probability
    /// <paramref name="rate"/>: with that probability a <c>0</c> becomes <c>1</c> (and vice
    /// versa), and otherwise the gene is left unchanged.
    /// </summary>
    /// <remarks>
    /// A less aggressive alternative to <c>flip</c>, which always flips every gene.
    /// <paramref name="rate"/> here is a separate, per-gene probability - distinct from
    /// <c>Options.MutationRate</c>, which decides whether a chromosome is mutated at all
    /// before <c>Options.MutationFn</c> ever runs. Assumes every gene is <c>0</c> or
    /// <c>1</c>; for any other integer value it toggles the lowest bit, which is unlikely
    /// to be meaningful. Curry <paramref name="rate"/> (e.g.
    /// <c>Mutation.flipEachGene 0.05</c>) to use this as an <c>Options&lt;int&gt;.MutationFn</c>.
    /// </remarks>
    /// <param name="rate">The probability, per gene, that it gets flipped.</param>
    /// <param name="chromosome">The chromosome to mutate.</param>
    /// <returns>A new chromosome with each gene independently flipped or left as-is.</returns>
    let flipEachGene (rate: float) (chromosome: Chromosome<int>) =
        { chromosome with
            Genes =
                chromosome.Genes
                |> Array.map (fun gene -> if System.Random.Shared.NextDouble() < rate then gene ^^^ 1 else gene) }
