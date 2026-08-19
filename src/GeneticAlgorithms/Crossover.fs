namespace GeneticAlgorithms

/// <summary>
/// Crossover strategies that combine two parent chromosomes into two children.
/// </summary>
/// <remarks>
/// Every strategy has the shape
/// <c>Chromosome&lt;'Gene&gt; -&gt; Chromosome&lt;'Gene&gt; -&gt; Chromosome&lt;'Gene&gt; * Chromosome&lt;'Gene&gt;</c>
/// (two parents in, two children out), so any of them can be plugged in as
/// <c>Options.CrossoverFn</c>. <c>singlePoint</c> and <c>uniform</c> work for any gene
/// array, and <c>singlePoint</c> is the usual default; <c>orderOneCrossover</c> is built
/// specifically for permutation genotypes - chromosomes where every gene value must appear
/// exactly once (for example, one queen per row in <c>NQueens</c>, or one city per visit in
/// a routing problem). A single-point cut on a permutation would usually produce children
/// with duplicate and missing genes, which is what <c>orderOneCrossover</c> avoids.
/// <c>wholeArithmeticCrossover</c> is different again: it only works for real-valued
/// (<c>float</c>) genotypes, since it blends parent genes arithmetically instead of
/// swapping or copying them outright.
/// </remarks>
module Crossover =

    /// <summary>
    /// Combines two parents into two children by picking a single random cut point and
    /// swapping the tails: the first child gets the first parent's head and the second
    /// parent's tail, and the second child gets the reverse.
    /// </summary>
    /// <remarks>
    /// Works for any gene array, but does not preserve permutations - if the parents are
    /// permutations of the same values (as in <c>NQueens</c>), the children generally
    /// won't be. Use <c>orderOneCrossover</c> for permutation genotypes instead.
    /// </remarks>
    /// <param name="p1">The first parent.</param>
    /// <param name="p2">The second parent.</param>
    /// <returns>
    /// Two children: the first with <paramref name="p1"/>'s head and
    /// <paramref name="p2"/>'s tail, and the second with <paramref name="p2"/>'s head and
    /// <paramref name="p1"/>'s tail.
    /// </returns>
    let singlePoint (p1: Chromosome<'Gene>) (p2: Chromosome<'Gene>) =
        let crossoverPoint = System.Random.Shared.Next(1, p1.Genes.Length)

        let parent1Head = p1.Genes |> Array.take crossoverPoint
        let parent1Tail = p1.Genes |> Array.skip crossoverPoint

        let parent2Head = p2.Genes |> Array.take crossoverPoint
        let parent2Tail = p2.Genes |> Array.skip crossoverPoint

        { p1 with
            Genes = Array.append parent1Head parent2Tail },
        { p2 with
            Genes = Array.append parent2Head parent1Tail }

    /// <summary>
    /// Combines two permutation-encoded parents into two children using order-one
    /// crossover (OX1): a random slice of genes is copied from one parent as-is, and the
    /// remaining positions are filled, in order, with the genes from the other parent that
    /// aren't already in that slice.
    /// </summary>
    /// <remarks>
    /// Because each child's genes are a fixed slice of one parent plus the other parent's
    /// remaining genes with duplicates removed, both children are guaranteed to stay valid
    /// permutations of the same gene set as the parents - unlike <c>singlePoint</c>, which
    /// can produce a chromosome with repeated and missing genes. This makes order-one
    /// crossover a good fit for problems like <c>NQueens</c>, where a chromosome's genes
    /// represent a permutation (each row used exactly once) rather than independent
    /// values.
    ///
    /// Both parents are expected to have the same, non-empty <c>Genes</c> length; this is
    /// not validated.
    /// </remarks>
    /// <param name="p1">The first parent.</param>
    /// <param name="p2">The second parent.</param>
    /// <returns>
    /// Two children: the first built from a slice of <paramref name="p1"/>'s genes filled
    /// out with <paramref name="p2"/>'s remaining genes in order, and the second the other
    /// way around.
    /// </returns>
    let orderOneCrossover (p1: Chromosome<'Gene>) (p2: Chromosome<'Gene>) =
        let lim = p1.Genes.Length - 1

        let i1, i2 =
            let a = System.Random.Shared.Next(1, lim + 1)
            let b = System.Random.Shared.Next(1, lim + 1)
            if a <= b then a, b else b, a

        let slice1 = p1.Genes.[i1..i2]
        let slice1Set = System.Collections.Generic.HashSet<'Gene>(slice1)
        let p2Contrib = p2.Genes |> Array.filter (slice1Set.Contains >> not)
        let head1, tail1 = Array.splitAt i1 p2Contrib

        let slice2 = p2.Genes.[i1..i2]
        let slice2Set = System.Collections.Generic.HashSet<'Gene>(slice2)
        let p1Contrib = p1.Genes |> Array.filter (slice2Set.Contains >> not)
        let head2, tail2 = Array.splitAt i1 p1Contrib

        { p1 with
            Genes = Array.concat [ head1; slice1; tail1 ] },
        { p2 with
            Genes = Array.concat [ head2; slice2; tail2 ] }

    /// <summary>
    /// Combines two parents into two children by considering each gene position
    /// independently: with probability <paramref name="rate"/> the first child keeps the
    /// first parent's gene at that position (and the second child keeps the second
    /// parent's), and otherwise the two are swapped.
    /// </summary>
    /// <remarks>
    /// Unlike <c>singlePoint</c>, which swaps one contiguous tail, uniform crossover mixes
    /// genes independently at every position. Like <c>singlePoint</c>, it does not preserve
    /// permutations - if the parents are permutations of the same values (as in
    /// <c>NQueens</c>), the children generally won't be. Both parents are expected to have
    /// the same <c>Genes</c> length; this is not validated. Curry
    /// <paramref name="rate"/> (e.g. <c>Crossover.uniform 0.5</c>) to use this as an
    /// <c>Options.CrossoverFn</c>.
    /// </remarks>
    /// <param name="rate">
    /// The probability, per gene position, that the first child keeps the first parent's
    /// gene (and the second child keeps the second parent's) rather than swapping.
    /// </param>
    /// <param name="p1">The first parent.</param>
    /// <param name="p2">The second parent.</param>
    /// <returns>
    /// Two children, with each gene position independently drawn from one parent or the
    /// other.
    /// </returns>
    let uniform (rate: float) (p1: Chromosome<'Gene>) (p2: Chromosome<'Gene>) =
        let c1, c2 =
            Array.zip p1.Genes p2.Genes
            |> Array.map (fun (x, y) -> if System.Random.Shared.NextDouble() < rate then x, y else y, x)
            |> Array.unzip

        { p1 with Genes = c1 }, { p2 with Genes = c2 }

    /// <summary>
    /// Combines two real-valued parents into two children by blending each gene position
    /// as a weighted average: for genes <c>x</c> (from the first parent) and <c>y</c>
    /// (from the second), the first child gets <c>x * alpha + y * (1 - alpha)</c> and the
    /// second gets <c>x * (1 - alpha) + y * alpha</c>.
    /// </summary>
    /// <remarks>
    /// Unlike the other strategies in this module, whole arithmetic crossover only makes
    /// sense for real-valued genotypes, so it works on <c>Chromosome&lt;float&gt;</c>
    /// specifically rather than any <c>'Gene</c> type - it blends gene values
    /// arithmetically rather than swapping or copying them outright. An
    /// <paramref name="alpha"/> of 0.5 makes both children the pointwise average of the two
    /// parents; values closer to 0 or 1 bias each child toward one parent or the other.
    /// Both parents are expected to have the same <c>Genes</c> length; this is not
    /// validated. Curry <paramref name="alpha"/> (e.g.
    /// <c>Crossover.wholeArithmeticCrossover 0.5</c>) to use this as an
    /// <c>Options&lt;float&gt;.CrossoverFn</c>.
    /// </remarks>
    /// <param name="alpha">The blend weight, typically in the range [0, 1].</param>
    /// <param name="p1">The first parent.</param>
    /// <param name="p2">The second parent.</param>
    /// <returns>
    /// Two children, each gene position a weighted blend of the two parents' genes at that
    /// position.
    /// </returns>
    let wholeArithmeticCrossover (alpha: float) (p1: Chromosome<float>) (p2: Chromosome<float>) =
        let c1, c2 =
            Array.zip p1.Genes p2.Genes
            |> Array.map (fun (x, y) -> x * alpha + y * (1.0 - alpha), x * (1.0 - alpha) + y * alpha)
            |> Array.unzip

        { p1 with Genes = c1 }, { p2 with Genes = c2 }
