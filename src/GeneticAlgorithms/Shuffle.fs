namespace GeneticAlgorithms

/// <summary>
/// A general-purpose Fisher-Yates shuffle, for anywhere this library needs a uniformly
/// random permutation of an array.
/// </summary>
/// <remarks>
/// Selection, crossover, mutation, and reinsertion strategies that need to shuffle or sample
/// without regard to fitness go through this rather than
/// <c>Array.sortBy (fun _ -> System.Random.Shared.Next())</c>: sorting on random keys is
/// O(n log n) just to produce a permutation, whereas Fisher-Yates does it in O(n) - and
/// while sorting on independently drawn keys is uniform in practice (collisions between two
/// elements' keys are vanishingly unlikely with .NET's 32-bit <c>Random.Next()</c>),
/// Fisher-Yates is exactly uniform by construction, with no such edge case to reason about.
/// </remarks>
module Shuffle =

    /// <summary>
    /// Returns a new array containing the same elements as <paramref name="items"/>, in a
    /// uniformly random order.
    /// </summary>
    /// <param name="items">The items to shuffle.</param>
    /// <returns>A new array with the same elements as <paramref name="items"/>, shuffled.</returns>
    let fisherYates (items: 'T array) : 'T array =
        let result = Array.copy items

        for i in result.Length - 1 .. -1 .. 1 do
            let j = System.Random.Shared.Next(i + 1)
            let temp = result.[i]
            result.[i] <- result.[j]
            result.[j] <- temp

        result
