namespace GeneticAlgorithms

open System

/// <summary>
/// Similarity and distance functions for comparing two strings, useful as building blocks
/// for fitness functions that compare a candidate string against a target - for example,
/// decoding a candidate key and scoring how close the result is to a known message.
/// </summary>
module Distance =

    /// <summary>
    /// Computes the Jaro similarity between two strings: <c>1.0</c> means the strings are
    /// identical, and <c>0.0</c> means they share no similarity at all.
    /// </summary>
    /// <remarks>
    /// Jaro similarity counts matching characters within a distance window that scales with
    /// the strings' lengths, then penalizes matched characters that appear out of order
    /// (transpositions). It tolerates minor reordering and length differences better than an
    /// exact, position-by-position comparison, which makes it a reasonable fitness signal
    /// for problems where a candidate string only needs to be "close to" a target rather
    /// than matched character-for-character in place.
    /// </remarks>
    /// <param name="left">The first string to compare.</param>
    /// <param name="right">The second string to compare.</param>
    /// <returns>A similarity score between <c>0.0</c> and <c>1.0</c>.</returns>
    let jaro (left: string) (right: string) =
        if left = right then
            1.0
        elif String.IsNullOrEmpty(left) || String.IsNullOrEmpty(right) then
            0.0
        else
            let matchDistance = max (max left.Length right.Length / 2 - 1) 0
            let leftMatches = Array.create left.Length false
            let rightMatches = Array.create right.Length false

            for leftIndex in 0 .. left.Length - 1 do
                let firstRightIndex = max 0 (leftIndex - matchDistance)
                let lastRightIndex = min (leftIndex + matchDistance) (right.Length - 1)
                let mutable rightIndex = firstRightIndex
                let mutable matched = false

                while rightIndex <= lastRightIndex && not matched do
                    if not rightMatches.[rightIndex] && left.[leftIndex] = right.[rightIndex] then
                        leftMatches.[leftIndex] <- true
                        rightMatches.[rightIndex] <- true
                        matched <- true

                    rightIndex <- rightIndex + 1

            let matchedLeft =
                left
                |> Seq.mapi (fun index character -> index, character)
                |> Seq.choose (fun (index, character) ->
                    if leftMatches.[index] then Some character else None)
                |> Seq.toArray

            let matchedRight =
                right
                |> Seq.mapi (fun index character -> index, character)
                |> Seq.choose (fun (index, character) ->
                    if rightMatches.[index] then Some character else None)
                |> Seq.toArray

            let matches = matchedLeft.Length

            if matches = 0 then
                0.0
            else
                let transpositions =
                    Array.zip matchedLeft matchedRight
                    |> Array.sumBy (fun (leftCharacter, rightCharacter) ->
                        if leftCharacter = rightCharacter then 0 else 1)
                    |> fun count -> count / 2

                let matches = float matches

                (matches / float left.Length
                 + matches / float right.Length
                 + (matches - float transpositions) / matches)
                / 3.0
