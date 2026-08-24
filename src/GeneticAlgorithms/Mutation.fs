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
    /// Mutates a chromosome by scrambling the order of genes within a random contiguous
    /// window of size <paramref name="n"/>, leaving every gene outside that window
    /// untouched.
    /// </summary>
    /// <remarks>
    /// Unlike <c>scramble</c>, which reorders every gene, this only disturbs a local
    /// window - a less disruptive mutation for larger chromosomes. If the randomly chosen
    /// window would extend past the end of the chromosome, it is shifted back so it stays
    /// exactly <paramref name="n"/> genes long, rather than being allowed to change the
    /// chromosome's overall length. <paramref name="n"/> must not exceed the chromosome's
    /// <c>Genes</c> length; this is not validated. Curry <paramref name="n"/> (e.g.
    /// <c>Mutation.scrambleSlice 3</c>) to use this as an <c>Options.MutationFn</c>.
    /// </remarks>
    /// <param name="n">The size of the window to scramble.</param>
    /// <param name="chromosome">The chromosome to mutate.</param>
    /// <returns>A new chromosome with a random <paramref name="n"/>-gene window scrambled in place.</returns>
    let scrambleSlice (n: int) (chromosome: Chromosome<'Gene>) =
        let size = chromosome.Genes.Length
        let start = System.Random.Shared.Next(1, n)

        let lo, hi =
            if start + n >= size then
                size - n, size
            else
                start, start + n

        let head = chromosome.Genes.[0 .. lo - 1]
        let mid = chromosome.Genes.[lo .. hi - 1] |> Array.sortBy (fun _ -> System.Random.Shared.Next())
        let tail = chromosome.Genes.[hi..]

        { chromosome with
            Genes = Array.concat [ head; mid; tail ] }

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
