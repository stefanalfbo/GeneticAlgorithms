namespace GeneticAlgorithms

/// <summary>
/// Mutation strategies that transform a single chromosome's genes.
/// </summary>
/// <remarks>
/// Every strategy has the shape
/// <c>System.Random -&gt; Chromosome&lt;'Gene&gt; -&gt; Chromosome&lt;'Gene&gt;</c> (a source
/// of randomness, then one chromosome in, one mutated chromosome out), so any of them can be
/// plugged in as <c>Options.MutationFn</c>. Every strategy draws its randomness from the
/// given <c>System.Random</c> rather than <c>System.Random.Shared</c>, so an entire run is
/// reproducible end to end when <c>Options.Random</c> is seeded - <c>flip</c>, the only
/// strategy that needs no randomness at all, still accepts it to match this shared shape.
/// Whether a given chromosome is mutated at all is decided separately, by
/// <c>Genetic.mutation</c> rolling against <c>Options.MutationRate</c> - the strategies here
/// only decide how to mutate a chromosome once that decision has already been made.
/// <c>flip</c> and <c>flipEachGene</c> only make sense for binary genotypes
/// (<c>Chromosome&lt;int&gt;</c> with genes of <c>0</c> or <c>1</c>), so unlike
/// <c>scramble</c> they work on <c>int</c> specifically rather than any <c>'Gene</c> type.
/// <c>gaussian</c> is the real-valued counterpart: it only makes sense for
/// <c>Chromosome&lt;float&gt;</c>. <c>scramble</c> and <c>scrambleSlice</c> can only
/// rearrange gene values that already exist somewhere in the chromosome; if selection
/// drives a needed value to extinction across the entire population, no amount of
/// reordering can bring it back. <c>randomReset</c> is the general-purpose strategy that
/// can, by replacing genes with freshly generated values instead of just reordering them.
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
    /// <param name="rng">The source of randomness.</param>
    /// <param name="chromosome">The chromosome to mutate.</param>
    /// <returns>A new chromosome with the same genes in a randomly scrambled order.</returns>
    let scramble (rng: System.Random) (chromosome: Chromosome<'Gene>) =
        { chromosome with
            Genes = chromosome.Genes |> Shuffle.fisherYates rng }

    /// <summary>
    /// Mutates a chromosome by scrambling the order of genes within a random contiguous
    /// window of size <paramref name="n"/>, leaving every gene outside that window
    /// untouched.
    /// </summary>
    /// <remarks>
    /// Unlike <c>scramble</c>, which reorders every gene, this only disturbs a local
    /// window - a less disruptive mutation for larger chromosomes. The window's start
    /// position is drawn uniformly from every position where a <paramref name="n"/>-gene
    /// window fits entirely within the chromosome (positions <c>0</c> through
    /// <c>Genes.Length - n</c>, inclusive), so it always stays exactly <paramref name="n"/>
    /// genes long without needing to be shifted, and every gene - including the very first
    /// and very last - has an equal chance of falling inside it. <paramref name="n"/> must
    /// not exceed the chromosome's <c>Genes</c> length; this is not validated. Curry
    /// <paramref name="n"/> (e.g. <c>Mutation.scrambleSlice 3</c>) to use this as an
    /// <c>Options.MutationFn</c>.
    /// </remarks>
    /// <param name="n">The size of the window to scramble.</param>
    /// <param name="rng">The source of randomness.</param>
    /// <param name="chromosome">The chromosome to mutate.</param>
    /// <returns>A new chromosome with a random <paramref name="n"/>-gene window scrambled in place.</returns>
    let scrambleSlice (n: int) (rng: System.Random) (chromosome: Chromosome<'Gene>) =
        let size = chromosome.Genes.Length
        let lo = rng.Next(0, size - n + 1)
        let hi = lo + n

        let head = chromosome.Genes.[0 .. lo - 1]
        let mid = chromosome.Genes.[lo .. hi - 1] |> Shuffle.fisherYates rng
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
    /// the lowest bit, which is unlikely to be meaningful. Ignores <paramref name="rng"/> -
    /// flipping every gene needs no randomness, but still accepts it to match every other
    /// <c>MutationFn</c>'s shape.
    /// </remarks>
    /// <param name="rng">The source of randomness. Ignored.</param>
    /// <param name="chromosome">The chromosome to mutate.</param>
    /// <returns>A new chromosome with every gene flipped.</returns>
    let flip (_rng: System.Random) (chromosome: Chromosome<int>) =
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
    /// <param name="rate">The probability, per gene, that it gets flipped. Must be in [0, 1].</param>
    /// <param name="rng">The source of randomness.</param>
    /// <param name="chromosome">The chromosome to mutate.</param>
    /// <returns>A new chromosome with each gene independently flipped or left as-is.</returns>
    /// <exception cref="System.ArgumentException">
    /// Thrown when <paramref name="rate"/> is outside <c>[0, 1]</c>.
    /// </exception>
    let flipEachGene (rate: float) (rng: System.Random) (chromosome: Chromosome<int>) =
        Validation.rate (nameof rate) rate

        { chromosome with
            Genes =
                chromosome.Genes
                |> Array.map (fun gene -> if rng.NextDouble() < rate then gene ^^^ 1 else gene) }

    /// <summary>
    /// Mutates a chromosome by replacing each gene, independently with probability
    /// <paramref name="rate"/>, with a freshly generated value from
    /// <paramref name="generator"/>.
    /// </summary>
    /// <remarks>
    /// Unlike <c>scramble</c> and <c>scrambleSlice</c>, which only reorder a chromosome's
    /// existing genes, this can introduce a gene value that was never present in the
    /// chromosome - or anywhere in the population - to begin with. That makes it the only
    /// strategy in this module able to recover an allele that selection has driven to
    /// extinction across the whole population; scramble-based strategies can never do
    /// this, no matter how many generations run, since they only rearrange values that
    /// already exist. A <paramref name="generator"/> is required because, unlike
    /// <c>flip</c>/<c>flipEachGene</c> (fixed to <c>0</c>/<c>1</c>) or <c>gaussian</c>
    /// (fitted to the chromosome's own genes), there is no way to synthesize a fresh value
    /// for an arbitrary <c>'Gene</c> without the caller supplying how to produce one - it
    /// should match whatever the genotype's own generator uses, so replaced genes stay
    /// within the same domain. <paramref name="generator"/> is given the same
    /// <paramref name="rng"/> this function receives, rather than drawing from
    /// <c>System.Random.Shared</c> itself, so the freshly generated values stay reproducible
    /// too. Curry both arguments (e.g. <c>Mutation.randomReset 0.1 randomChar</c>) to use
    /// this as an <c>Options&lt;'Gene&gt;.MutationFn</c>.
    /// </remarks>
    /// <param name="rate">The probability, per gene, that it gets replaced. Must be in [0, 1].</param>
    /// <param name="generator">Produces a fresh, random gene value from the given source of randomness.</param>
    /// <param name="rng">The source of randomness.</param>
    /// <param name="chromosome">The chromosome to mutate.</param>
    /// <returns>A new chromosome with each gene independently replaced or left as-is.</returns>
    /// <exception cref="System.ArgumentException">
    /// Thrown when <paramref name="rate"/> is outside <c>[0, 1]</c>.
    /// </exception>
    let randomReset (rate: float) (generator: System.Random -> 'Gene) (rng: System.Random) (chromosome: Chromosome<'Gene>) =
        Validation.rate (nameof rate) rate

        { chromosome with
            Genes =
                chromosome.Genes
                |> Array.map (fun gene -> if rng.NextDouble() < rate then generator rng else gene) }

    /// Draws a random sample from a normal distribution with the given mean and variance,
    /// via the Box-Muller transform. <paramref name="rng"/> only generates uniform samples,
    /// so there is no built-in Gaussian source to call instead. The first uniform draw is
    /// taken as <c>1.0 - rng.NextDouble()</c> rather than <c>rng.NextDouble()</c> directly,
    /// so it lands in <c>(0.0, 1.0]</c> instead of <c>[0.0, 1.0)</c> - <c>NextDouble()</c>
    /// can return exactly <c>0.0</c>, which would make <c>log</c> diverge.
    let private nextGaussian (rng: System.Random) (mean: float) (variance: float) =
        let u1 = 1.0 - rng.NextDouble()
        let u2 = rng.NextDouble()
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
    /// <param name="rng">The source of randomness.</param>
    /// <param name="chromosome">The chromosome to mutate.</param>
    /// <returns>
    /// A new chromosome with every gene independently resampled from a normal
    /// distribution fitted to the original genes.
    /// </returns>
    let gaussian (rng: System.Random) (chromosome: Chromosome<float>) =
        let genes = chromosome.Genes
        let mu = Array.average genes
        let variance = genes |> Array.averageBy (fun x -> (mu - x) * (mu - x))

        { chromosome with
            Genes = genes |> Array.map (fun _ -> nextGaussian rng mu variance) }
