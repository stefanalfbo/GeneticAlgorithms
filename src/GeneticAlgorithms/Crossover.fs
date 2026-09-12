namespace GeneticAlgorithms

/// <summary>
/// Crossover strategies that combine two parent chromosomes into two children.
/// </summary>
/// <remarks>
/// Every strategy has the shape
/// <c>System.Random -&gt; Chromosome&lt;'Gene&gt; -&gt; Chromosome&lt;'Gene&gt; -&gt; Chromosome&lt;'Gene&gt; * Chromosome&lt;'Gene&gt;</c>
/// (a source of randomness, then two parents in, two children out), so any of them can be
/// plugged in as <c>Options.CrossoverFn</c>. Every strategy that needs randomness draws it
/// from the given <c>System.Random</c> rather than <c>System.Random.Shared</c>, so an entire
/// run is reproducible end to end when <c>Options.Random</c> is seeded - strategies that
/// don't need randomness at all (<c>cycleCrossover</c>, <c>wholeArithmeticCrossover</c>)
/// still accept it, purely to match this shared shape. <c>singlePoint</c>, <c>multiPoint</c>,
/// and <c>uniform</c> work for any gene array, and <c>singlePoint</c> is the usual default;
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
/// either parent's. A chromosome with no genes at all is a trivial no-op for every
/// strategy here - it produces another empty chromosome rather than being rejected, the
/// same way an empty array is already a perfectly valid (if degenerate) permutation or
/// gene sequence. Every strategy's two children start at <c>Age = 0</c> and
/// <c>Fitness = 0.0</c>, regardless of either parent's values - combining two parents
/// produces a genuinely new individual, so it is "born" rather than continuing either
/// parent's <c>Age</c>. See <c>Chromosome.Age</c>'s own remarks for the full picture,
/// including why <c>Mutation</c> strategies do not reset it the same way.
/// </remarks>
module Crossover =

    /// A newly created child: takes its Genes from the given array, but starts fresh at
    /// Age 0 and Fitness 0.0 rather than inheriting parent's values - combining two
    /// parents' genes produces a genuinely new individual, not a continuation of parent's
    /// own lineage. Every strategy below uses this to build both of its children.
    let private newborn (parent: Chromosome<'Gene>) (genes: 'Gene array) : Chromosome<'Gene> =
        { parent with
            Genes = genes
            Fitness = 0.0
            Age = 0 }

    /// Validates that both parents have the same Genes length - shared by every strategy
    /// that assumes equal-length parents but doesn't need a full permutation check.
    /// Without this, a length mismatch can silently produce a wrong-length child (if the
    /// operation happens not to index out of bounds) or crash with an unrelated exception
    /// (if it does), instead of a clear, immediate error naming the actual problem.
    let private validateEqualLength (p1: Chromosome<'Gene>) (p2: Chromosome<'Gene>) =
        if p1.Genes.Length <> p2.Genes.Length then
            invalidArg
                (nameof p2)
                $"Both parents must have the same Genes length to preserve it in the children; \
                  got {p1.Genes.Length} and {p2.Genes.Length}. Use messySinglePoint instead if \
                  children are allowed to differ in length."

    /// Validates that both parents are permutations of the exact same set of gene values -
    /// the same length, no duplicate genes in either parent, and the same distinct value
    /// set - the precondition every permutation-based strategy (orderOneCrossover,
    /// cycleCrossover) relies on to stay well-defined. Without this, a non-permutation or
    /// mismatched input can silently produce a wrong-length child, throw an unrelated
    /// exception partway through, or - for cycleCrossover specifically, whose cycle-tracing
    /// recursion assumes a bijection between p1's and p2's positions - recurse without ever
    /// terminating.
    let private validatePermutation (p1: Chromosome<'Gene>) (p2: Chromosome<'Gene>) =
        if p1.Genes.Length <> p2.Genes.Length then
            invalidArg
                (nameof p2)
                $"Both parents must have the same Genes length; got {p1.Genes.Length} and {p2.Genes.Length}."

        if Array.distinct p1.Genes |> Array.length <> p1.Genes.Length then
            invalidArg (nameof p1) "p1 must be a permutation: every gene value must appear exactly once."

        if Array.distinct p2.Genes |> Array.length <> p2.Genes.Length then
            invalidArg (nameof p2) "p2 must be a permutation: every gene value must appear exactly once."

        if Set.ofArray p1.Genes <> Set.ofArray p2.Genes then
            invalidArg (nameof p2) "p1 and p2 must be permutations of the same set of gene values."

    /// Draws a random interior cut point in [1, length - 1], or 0 when length is 0 - there
    /// is no interior position to cut an empty array at, and 0 is the only cut point that
    /// keeps both the "head" and "tail" slices empty, making the no-op for an empty
    /// chromosome fall out of the normal head/tail-splitting logic rather than needing its
    /// own special case.
    let private cutPoint (rng: System.Random) (length: int) =
        if length = 0 then 0 else rng.Next(1, length)

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
    /// <c>messySinglePoint</c> instead if children are allowed to differ in length. Two
    /// empty parents produce two empty children - there is no interior position to cut an
    /// empty array at, so the cut point is 0 in that case, keeping both the head and tail
    /// slices empty.
    /// </remarks>
    /// <param name="rng">The source of randomness.</param>
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
    let singlePoint (rng: System.Random) (p1: Chromosome<'Gene>) (p2: Chromosome<'Gene>) =
        validateEqualLength p1 p2

        let crossoverPoint = cutPoint rng p1.Genes.Length

        let parent1Head = p1.Genes |> Array.take crossoverPoint
        let parent1Tail = p1.Genes |> Array.skip crossoverPoint

        let parent2Head = p2.Genes |> Array.take crossoverPoint
        let parent2Tail = p2.Genes |> Array.skip crossoverPoint

        newborn p1 (Array.append parent1Head parent2Tail), newborn p2 (Array.append parent2Head parent1Tail)

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
    /// instead. An empty parent has no interior position to cut at, so its own cut point is
    /// always 0 - that parent contributes nothing to either child, but the other parent's
    /// (possibly non-empty) cut still applies independently, exactly as this function's own
    /// "cuts are chosen independently" contract already implies.
    /// </remarks>
    /// <param name="rng">The source of randomness.</param>
    /// <param name="p1">The first parent.</param>
    /// <param name="p2">The second parent.</param>
    /// <returns>
    /// Two children built from independently chosen cut points in each parent - their
    /// lengths may differ from the parents' and from each other.
    /// </returns>
    let messySinglePoint (rng: System.Random) (p1: Chromosome<'Gene>) (p2: Chromosome<'Gene>) =
        let cut1 = cutPoint rng p1.Genes.Length
        let cut2 = cutPoint rng p2.Genes.Length

        let c1Genes = Array.append p1.Genes.[0 .. cut1 - 1] p2.Genes.[cut2 ..]
        let c2Genes = Array.append p2.Genes.[0 .. cut2 - 1] p1.Genes.[cut1 ..]

        newborn p1 c1Genes, newborn p2 c2Genes

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
    /// and 0 returns children identical to the parents. Both parents must have the same
    /// <c>Genes</c> length - this is validated, the same way <c>singlePoint</c> validates
    /// it. Curry <paramref name="pointCount"/> (e.g. <c>Crossover.multiPoint 3</c>) to use
    /// this as an <c>Options.CrossoverFn</c>.
    /// </remarks>
    /// <param name="pointCount">The number of cut points to use.</param>
    /// <param name="rng">The source of randomness.</param>
    /// <param name="p1">The first parent.</param>
    /// <param name="p2">The second parent.</param>
    /// <returns>
    /// Two children, with contiguous segments alternately taken from each parent between
    /// the chosen cut points.
    /// </returns>
    /// <exception cref="System.ArgumentException">
    /// Thrown when <paramref name="p1"/> and <paramref name="p2"/> have different
    /// <c>Genes</c> lengths.
    /// </exception>
    let multiPoint (pointCount: int) (rng: System.Random) (p1: Chromosome<'Gene>) (p2: Chromosome<'Gene>) =
        validateEqualLength p1 p2

        let length = p1.Genes.Length

        let points =
            [| 1 .. length - 1 |]
            |> Shuffle.fisherYates rng
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

        newborn p1 c1, newborn p2 c2

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
    /// Both parents must be permutations of the same set of gene values - the same length,
    /// no duplicate genes in either parent, and the same distinct values in both - this is
    /// validated. Without it, a length mismatch or duplicate-containing "almost
    /// permutation" could silently produce a wrong-length child instead of the valid
    /// permutation this strategy promises. Two empty parents are a trivial permutation of
    /// the empty set and produce two empty children - there is no interior slice to pick
    /// from an empty array, so this is handled directly rather than by drawing a
    /// meaningless slice from a range that doesn't exist.
    /// </remarks>
    /// <param name="rng">The source of randomness.</param>
    /// <param name="p1">The first parent.</param>
    /// <param name="p2">The second parent.</param>
    /// <returns>
    /// Two children: the first built from a slice of <paramref name="p1"/>'s genes filled
    /// out with <paramref name="p2"/>'s remaining genes in order, and the second the other
    /// way around.
    /// </returns>
    /// <exception cref="System.ArgumentException">
    /// Thrown when <paramref name="p1"/> and <paramref name="p2"/> are not permutations of
    /// the same set of gene values.
    /// </exception>
    let orderOneCrossover (rng: System.Random) (p1: Chromosome<'Gene>) (p2: Chromosome<'Gene>) =
        validatePermutation p1 p2

        if p1.Genes.Length = 0 then
            newborn p1 [||], newborn p2 [||]
        else
            let lim = p1.Genes.Length - 1

            let i1, i2 =
                let a = rng.Next(1, lim + 1)
                let b = rng.Next(1, lim + 1)
                if a <= b then a, b else b, a

            let slice1 = p1.Genes.[i1..i2]
            let slice1Set = System.Collections.Generic.HashSet<'Gene>(slice1)
            let p2Contrib = p2.Genes |> Array.filter (slice1Set.Contains >> not)
            let head1, tail1 = Array.splitAt i1 p2Contrib

            let slice2 = p2.Genes.[i1..i2]
            let slice2Set = System.Collections.Generic.HashSet<'Gene>(slice2)
            let p1Contrib = p1.Genes |> Array.filter (slice2Set.Contains >> not)
            let head2, tail2 = Array.splitAt i1 p1Contrib

            newborn p1 (Array.concat [ head1; slice1; tail1 ]), newborn p2 (Array.concat [ head2; slice2; tail2 ])

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
    /// Both parents must be permutations of the same, non-empty gene set - the same
    /// length, no duplicate genes in either parent, and the same distinct values in both -
    /// this is validated (empty parents are not; see <c>validatePermutation</c>'s own
    /// remarks for why). Without it, a duplicate-containing "almost permutation" can make
    /// the cycle-tracing recursion below loop back to a position it has already visited
    /// without ever returning to its start, recursing without ever terminating. Ignores
    /// <paramref name="rng"/> - which parent contributes each cycle alternates
    /// deterministically, but still accepts a source of randomness to match every other
    /// <c>CrossoverFn</c>'s shape.
    /// </remarks>
    /// <param name="rng">The source of randomness. Ignored.</param>
    /// <param name="p1">The first parent.</param>
    /// <param name="p2">The second parent.</param>
    /// <returns>
    /// Two children: for each cycle of positions, the first child copies that cycle from
    /// whichever parent alternation lands on (starting with <paramref name="p1"/> for the
    /// first cycle), and the second child copies the other parent for that same cycle.
    /// </returns>
    /// <exception cref="System.ArgumentException">
    /// Thrown when <paramref name="p1"/> and <paramref name="p2"/> are not permutations of
    /// the same set of gene values.
    /// </exception>
    let cycleCrossover (_rng: System.Random) (p1: Chromosome<'Gene>) (p2: Chromosome<'Gene>) =
        validatePermutation p1 p2

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

        newborn p1 c1, newborn p2 c2

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
    /// gene (and the second child keeps the second parent's) rather than swapping. Must be
    /// in [0, 1].
    /// </param>
    /// <param name="rng">The source of randomness.</param>
    /// <param name="p1">The first parent.</param>
    /// <param name="p2">The second parent.</param>
    /// <returns>
    /// Two children, with each gene position independently drawn from one parent or the
    /// other.
    /// </returns>
    /// <exception cref="System.ArgumentException">
    /// Thrown when <paramref name="rate"/> is outside <c>[0, 1]</c>.
    /// </exception>
    let uniform (rate: float) (rng: System.Random) (p1: Chromosome<'Gene>) (p2: Chromosome<'Gene>) =
        Validation.rate (nameof rate) rate

        let c1, c2 =
            Array.zip p1.Genes p2.Genes
            |> Array.map (fun (x, y) -> if rng.NextDouble() < rate then x, y else y, x)
            |> Array.unzip

        newborn p1 c1, newborn p2 c2

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
    /// validated. Ignores <paramref name="rng"/> - blending is purely arithmetic, but still
    /// accepts a source of randomness to match every other <c>CrossoverFn</c>'s shape. Curry
    /// <paramref name="alpha"/> (e.g. <c>Crossover.wholeArithmeticCrossover 0.5</c>) to use
    /// this as an <c>Options&lt;float&gt;.CrossoverFn</c>.
    /// </remarks>
    /// <param name="alpha">The blend weight, typically in the range [0, 1].</param>
    /// <param name="rng">The source of randomness. Ignored.</param>
    /// <param name="p1">The first parent.</param>
    /// <param name="p2">The second parent.</param>
    /// <returns>
    /// Two children, each gene position a weighted blend of the two parents' genes at that
    /// position.
    /// </returns>
    let wholeArithmeticCrossover (alpha: float) (_rng: System.Random) (p1: Chromosome<float>) (p2: Chromosome<float>) =
        let c1, c2 =
            Array.zip p1.Genes p2.Genes
            |> Array.map (fun (x, y) -> x * alpha + y * (1.0 - alpha), x * (1.0 - alpha) + y * alpha)
            |> Array.unzip

        newborn p1 c1, newborn p2 c2
