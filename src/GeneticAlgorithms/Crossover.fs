namespace GeneticAlgorithms

/// <summary>
/// Crossover strategies that combine two parent chromosomes into two children.
/// </summary>
/// <remarks>
/// Every strategy has the shape
/// <c>Chromosome&lt;'Gene&gt; -&gt; Chromosome&lt;'Gene&gt; -&gt; Chromosome&lt;'Gene&gt; * Chromosome&lt;'Gene&gt;</c>
/// (two parents in, two children out), so any of them can be plugged in as
/// <c>Options.CrossoverFn</c>. <c>singlePoint</c>, <c>multiPoint</c>, and <c>uniform</c>
/// work for any gene array, and <c>singlePoint</c> is the usual default;
/// <c>orderOneCrossover</c> and <c>cycleCrossover</c> are built specifically for
/// permutation genotypes - chromosomes where every gene value must appear exactly once
/// (for example, one queen per row in <c>NQueens</c>, or one city per visit in a routing
/// problem). A single-point cut on a permutation would usually produce children with
/// duplicate and missing genes, which is what both of those avoid, just via different
/// means: <c>orderOneCrossover</c> copies a contiguous slice from one parent and fills the
/// rest from the other, while <c>cycleCrossover</c> keeps every gene at its original
/// position in whichever parent contributed it.
/// <c>wholeArithmeticCrossover</c> is different again: it only works for real-valued
/// (<c>float</c>) genotypes, since it blends parent genes arithmetically instead of
/// swapping or copying them outright. Every strategy above preserves each parent's
/// <c>Genes</c> length in its children; <c>messySinglePoint</c> is the exception - it's a
/// variant of <c>singlePoint</c> built specifically to let a child's length differ from
/// either parent's.
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
    /// won't be. Use <c>orderOneCrossover</c> for permutation genotypes instead. Both parents
    /// must have the same <c>Genes</c> length - the single cut point is drawn from
    /// <paramref name="p1"/>'s length and applied to both parents, so unlike this module's
    /// other same-length-assuming strategies, this is validated rather than merely assumed:
    /// silently returning children whose length doesn't match either parent (or, if
    /// <paramref name="p1"/> is the longer parent, crashing with an unrelated "array too
    /// short" exception instead) would otherwise contradict this module's own guarantee that
    /// every strategy besides <c>messySinglePoint</c> preserves parent length. Use
    /// <c>messySinglePoint</c> instead if children are allowed to differ in length.
    /// </remarks>
    /// <param name="p1">The first parent.</param>
    /// <param name="p2">The second parent.</param>
    /// <returns>
    /// Two children: the first with <paramref name="p1"/>'s head and
    /// <paramref name="p2"/>'s tail, and the second with <paramref name="p2"/>'s head and
    /// <paramref name="p1"/>'s tail.
    /// </returns>
    /// <exception cref="System.ArgumentException">
    /// Thrown when <paramref name="p1"/> and <paramref name="p2"/> have different
    /// <c>Genes</c> lengths.
    /// </exception>
    let singlePoint (p1: Chromosome<'Gene>) (p2: Chromosome<'Gene>) =
        if p1.Genes.Length <> p2.Genes.Length then
            invalidArg
                (nameof p2)
                $"Both parents must have the same Genes length to preserve it in the children; \
                  got {p1.Genes.Length} and {p2.Genes.Length}. Use messySinglePoint instead if \
                  children are allowed to differ in length."

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
    /// Combines two parents into two children like <c>singlePoint</c>, but picks the cut
    /// point independently in each parent rather than sharing one - so unlike every other
    /// strategy in this module, this one does not preserve chromosome length.
    /// </summary>
    /// <remarks>
    /// The first child is <paramref name="p1"/>'s head (up to its own cut point) followed
    /// by <paramref name="p2"/>'s tail (from its own, independently chosen cut point); the
    /// second child is the reverse. Because the two cut points are chosen independently, a
    /// child's length is <c>(one parent's cut point) + (the other parent's length - that
    /// parent's cut point)</c>, which need not equal either original parent's length - and
    /// the two children need not be the same length as each other either. This is the
    /// defining property of "messy" crossover: unlike <c>singlePoint</c>,
    /// <c>multiPoint</c>, or <c>uniform</c>, a child's length here is an output of the
    /// operation, not an invariant it preserves.
    ///
    /// Works for any gene array, but does not preserve permutations - if the parents are
    /// permutations of the same values (as in <c>NQueens</c>), the children generally
    /// won't be, and generally won't even be the same length as the permutation itself.
    /// Use <c>orderOneCrossover</c> or <c>cycleCrossover</c> for permutation genotypes
    /// instead.
    /// </remarks>
    /// <param name="p1">The first parent.</param>
    /// <param name="p2">The second parent.</param>
    /// <returns>
    /// Two children built from independently chosen cut points in each parent - their
    /// lengths may differ from the parents' and from each other.
    /// </returns>
    let messySinglePoint (p1: Chromosome<'Gene>) (p2: Chromosome<'Gene>) =
        let cut1 = System.Random.Shared.Next(1, p1.Genes.Length)
        let cut2 = System.Random.Shared.Next(1, p2.Genes.Length)

        let c1Genes = Array.append p1.Genes.[0 .. cut1 - 1] p2.Genes.[cut2 ..]
        let c2Genes = Array.append p2.Genes.[0 .. cut2 - 1] p1.Genes.[cut1 ..]

        { p1 with Genes = c1Genes }, { p2 with Genes = c2Genes }

    /// <summary>
    /// Combines two parents into two children by picking <paramref name="pointCount"/>
    /// distinct random cut points and alternating which parent contributes each segment
    /// between them - a generalization of <c>singlePoint</c> to more than one cut.
    /// </summary>
    /// <remarks>
    /// Works for any gene array, but does not preserve permutations - if the parents are
    /// permutations of the same values (as in <c>NQueens</c>), the children generally
    /// won't be. Use <c>orderOneCrossover</c> for permutation genotypes instead.
    ///
    /// <paramref name="pointCount"/> must be less than the parents' <c>Genes</c> length
    /// (there are only <c>Genes.Length - 1</c> valid cut positions); this is not
    /// validated. A <paramref name="pointCount"/> of 1 behaves like <c>singlePoint</c>,
    /// and 0 returns children identical to the parents. Both parents are expected to have
    /// the same <c>Genes</c> length; this is not validated either. Curry
    /// <paramref name="pointCount"/> (e.g. <c>Crossover.multiPoint 3</c>) to use this as
    /// an <c>Options.CrossoverFn</c>.
    /// </remarks>
    /// <param name="pointCount">The number of cut points to use.</param>
    /// <param name="p1">The first parent.</param>
    /// <param name="p2">The second parent.</param>
    /// <returns>
    /// Two children, with contiguous segments alternately taken from each parent between
    /// the chosen cut points.
    /// </returns>
    let multiPoint (pointCount: int) (p1: Chromosome<'Gene>) (p2: Chromosome<'Gene>) =
        let length = p1.Genes.Length

        let points =
            [| 1 .. length - 1 |]
            |> Shuffle.fisherYates
            |> Array.take pointCount
            |> Array.sort

        let boundaries = Array.concat [ [| 0 |]; points; [| length |] ]

        let segments (genes: 'Gene array) =
            boundaries |> Array.pairwise |> Array.map (fun (a, b) -> genes.[a .. b - 1])

        let p1Segments = segments p1.Genes
        let p2Segments = segments p2.Genes

        let c1 =
            Array.init p1Segments.Length (fun i -> if i % 2 = 0 then p1Segments.[i] else p2Segments.[i])
            |> Array.concat

        let c2 =
            Array.init p1Segments.Length (fun i -> if i % 2 = 0 then p2Segments.[i] else p1Segments.[i])
            |> Array.concat

        { p1 with Genes = c1 }, { p2 with Genes = c2 }

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
    /// Combines two permutation-encoded parents into two children using cycle crossover
    /// (CX): every gene position belongs to exactly one "cycle" - follow the value at a
    /// position in <paramref name="p1"/> to wherever that same value sits in
    /// <paramref name="p2"/>, and repeat from there until the cycle returns to its
    /// starting position - and each child copies every position in a given cycle from one
    /// parent, alternating which parent contributes each successive cycle.
    /// </summary>
    /// <remarks>
    /// Unlike <c>orderOneCrossover</c>, which copies a contiguous slice from one parent and
    /// fills the remaining positions from the other, cycle crossover never moves a gene
    /// away from the position it already held in whichever parent contributed its cycle -
    /// every gene in a child sits exactly where it sat in one of the two parents. Both
    /// children are still guaranteed to stay valid permutations of the same gene set as the
    /// parents, so this is a good fit for problems like <c>NQueens</c>, where a
    /// chromosome's genes represent a permutation (each row used exactly once) rather than
    /// independent values.
    ///
    /// Both parents are expected to have the same, non-empty <c>Genes</c> length, and to be
    /// permutations of the same gene set (every value appearing exactly once); this is not
    /// validated.
    /// </remarks>
    /// <param name="p1">The first parent.</param>
    /// <param name="p2">The second parent.</param>
    /// <returns>
    /// Two children: for each cycle of positions, the first child copies that cycle from
    /// whichever parent alternation lands on (starting with <paramref name="p1"/> for the
    /// first cycle), and the second child copies the other parent for that same cycle.
    /// </returns>
    let cycleCrossover (p1: Chromosome<'Gene>) (p2: Chromosome<'Gene>) =
        let length = p1.Genes.Length
        let indexInP2 = System.Collections.Generic.Dictionary<'Gene, int>(length)

        for i in 0 .. length - 1 do
            indexInP2.[p2.Genes.[i]] <- i

        let visited = Array.create length false
        let c1 = Array.zeroCreate<'Gene> length
        let c2 = Array.zeroCreate<'Gene> length
        let mutable takeFromP1 = true

        for start in 0 .. length - 1 do
            if not visited.[start] then
                let rec traceCycle idx =
                    visited.[idx] <- true

                    if takeFromP1 then
                        c1.[idx] <- p1.Genes.[idx]
                        c2.[idx] <- p2.Genes.[idx]
                    else
                        c1.[idx] <- p2.Genes.[idx]
                        c2.[idx] <- p1.Genes.[idx]

                    let next = indexInP2.[p1.Genes.[idx]]

                    if next <> start then
                        traceCycle next

                traceCycle start
                takeFromP1 <- not takeFromP1

        { p1 with Genes = c1 }, { p2 with Genes = c2 }

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
