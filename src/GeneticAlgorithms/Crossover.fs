namespace GeneticAlgorithms

/// <summary>
/// Crossover strategies that combine two parent chromosomes into two children.
/// </summary>
/// <remarks>
/// Unlike <c>Genetic.crossover</c>, which recombines chromosomes with a single-point cut
/// and works for any gene array, the strategies here are built for permutation genotypes -
/// chromosomes where every gene value must appear exactly once (for example, one queen per
/// row in <c>NQueens</c>, or one city per visit in a routing problem). A single-point cut
/// on a permutation would usually produce children with duplicate and missing genes, so
/// these strategies take care to preserve the permutation instead.
/// </remarks>
module Crossover =

    /// <summary>
    /// Combines two permutation-encoded parents into two children using order-one
    /// crossover (OX1): a random slice of genes is copied from one parent as-is, and the
    /// remaining positions are filled, in order, with the genes from the other parent that
    /// aren't already in that slice.
    /// </summary>
    /// <remarks>
    /// Because each child's genes are a fixed slice of one parent plus the other parent's
    /// remaining genes with duplicates removed, both children are guaranteed to stay valid
    /// permutations of the same gene set as the parents - unlike a single-point crossover,
    /// which can produce a chromosome with repeated and missing genes. This makes
    /// order-one crossover a good fit for problems like <c>NQueens</c>, where a
    /// chromosome's genes represent a permutation (each row used exactly once) rather than
    /// independent values.
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
