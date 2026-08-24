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
/// <c>gaussian</c> is the real-valued counterpart: it only makes sense for
/// <c>Chromosome&lt;float&gt;</c>.
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

    /// Draws a random sample from a normal distribution with the given mean and variance,
    /// via the Box-Muller transform. .NET's <c>System.Random</c> only generates uniform
    /// samples, so there is no built-in Gaussian source to call instead. The first uniform
    /// draw is taken as <c>1.0 - NextDouble()</c> rather than <c>NextDouble()</c> directly,
    /// so it lands in <c>(0.0, 1.0]</c> instead of <c>[0.0, 1.0)</c> - <c>NextDouble()</c>
    /// can return exactly <c>0.0</c>, which would make <c>log</c> diverge.
    let private nextGaussian (mean: float) (variance: float) =
        let u1 = 1.0 - System.Random.Shared.NextDouble()
        let u2 = System.Random.Shared.NextDouble()
        let standardNormal = sqrt (-2.0 * log u1) * cos (2.0 * System.Math.PI * u2)
        mean + sqrt variance * standardNormal

    /// <summary>
    /// Mutates a real-valued chromosome by resampling every gene from a normal
    /// distribution fitted to the chromosome's own genes: the mean and variance of the
    /// current gene values are used to draw a fresh, independent value for every gene
    /// position.
    /// </summary>
    /// <remarks>
    /// Unlike the other strategies in this module, which rearrange or flip existing gene
    /// values, Gaussian mutation replaces every gene with a newly sampled value - only the
    /// chromosome's own mean and variance carry over, not the individual gene values
    /// themselves. Only makes sense for real-valued genotypes, so it works on
    /// <c>Chromosome&lt;float&gt;</c> specifically rather than any <c>'Gene</c> type, and
    /// (like <c>flip</c>) always mutates every gene - there is no per-gene rate to
    /// configure.
    /// </remarks>
    /// <param name="chromosome">The chromosome to mutate.</param>
    /// <returns>
    /// A new chromosome with every gene independently resampled from a normal
    /// distribution fitted to the original genes.
    /// </returns>
    let gaussian (chromosome: Chromosome<float>) =
        let genes = chromosome.Genes
        let mu = Array.average genes
        let variance = genes |> Array.averageBy (fun x -> (mu - x) * (mu - x))

        { chromosome with
            Genes = genes |> Array.map (fun _ -> nextGaussian mu variance) }
