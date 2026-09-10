namespace GeneticAlgorithms

/// <summary>
/// Shared argument validation for the library's public API.
/// </summary>
/// <remarks>
/// Every public rate-shaped parameter - a probability, always meant to be a value in
/// <c>[0, 1]</c> - validates itself the same way, via <c>rate</c>, so the range check and
/// its error message stay consistent everywhere instead of being re-implemented ad hoc at
/// each call site: <c>Options.SelectionRate</c> and <c>Options.MutationRate</c> (checked
/// centrally in <c>Genetic.run</c>, the one place both are visible), plus
/// <c>Reinsertion.elitist</c>/<c>Reinsertion.uniform</c>'s <c>survivalRate</c>,
/// <c>Mutation.flipEachGene</c>/<c>Mutation.randomReset</c>'s <c>rate</c>, and
/// <c>Crossover.uniform</c>'s <c>rate</c>. Those last five are each curried directly into
/// an <c>Options</c> function field as a closure (e.g. <c>Reinsertion.elitist 0.15</c>), so
/// unlike <c>SelectionRate</c>/<c>MutationRate</c> there is no single call site that can
/// see the value - each validates itself instead, via this same helper, the moment it's
/// applied.
/// </remarks>
module internal Validation =

    /// <summary>
    /// Validates that <paramref name="value"/> is a well-formed probability: a number in
    /// <c>[0, 1]</c>, and not <c>NaN</c>.
    /// </summary>
    /// <param name="paramName">The name of the parameter being validated, used as the thrown exception's argument name.</param>
    /// <param name="value">The value to validate.</param>
    /// <exception cref="System.ArgumentException">
    /// Thrown when <paramref name="value"/> is <c>NaN</c> or outside <c>[0, 1]</c>.
    /// </exception>
    let rate (paramName: string) (value: float) =
        if System.Double.IsNaN value || value < 0.0 || value > 1.0 then
            invalidArg paramName $"must be a value in [0, 1]; got {value}."
