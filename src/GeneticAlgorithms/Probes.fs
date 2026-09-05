namespace GeneticAlgorithms

/// <summary>
/// Composable probes for observing a genetic algorithm as it evolves.
/// </summary>
/// <remarks>
/// Every probe has the shape <c>GenerationInfo&lt;'Gene&gt; -&gt; unit</c>, matching
/// <c>Options.Probe</c>, so any of them - or several combined via <c>combine</c> - can be
/// plugged in directly. <c>Genetic.evolve</c> calls the configured probe once per
/// generation, right after evaluating and sorting that generation's population, before
/// selection ever runs.
///
/// This module only provides generic building blocks - what to do with a
/// <c>GenerationInfo</c> once a probe receives one (print it, collect it in memory, write
/// it to a file or database, push it to a monitoring service) is left entirely to the
/// probe itself; there's no ready-made collector here, by design.
/// </remarks>
module Probes =

    /// <summary>
    /// Does nothing. This is the library's own default - probing is opt-in, not imposed.
    /// </summary>
    /// <param name="_">The generation snapshot. Ignored.</param>
    let noop (_: GenerationInfo<'Gene>) : unit = ()

    /// <summary>
    /// Prints the current generation's best fitness to the console.
    /// </summary>
    /// <param name="info">The generation snapshot to report on.</param>
    let printProgress (info: GenerationInfo<'Gene>) =
        printfn "Current Best %f" info.Best.Fitness

    /// <summary>
    /// Combines multiple probes into one: every probe in <paramref name="observers"/> is
    /// invoked, in order, for each generation.
    /// </summary>
    /// <param name="observers">The probes to run for every generation, in order.</param>
    /// <returns>A single probe that runs every one of <paramref name="observers"/> in turn.</returns>
    let combine (observers: (GenerationInfo<'Gene> -> unit) list) : GenerationInfo<'Gene> -> unit =
        fun info -> observers |> List.iter (fun observe -> observe info)

    /// <summary>
    /// Wraps a probe so it only runs every <paramref name="n"/>th generation (generation 0,
    /// n, 2n, ...), skipping every other call.
    /// </summary>
    /// <remarks>
    /// Useful for throttling a probe that would otherwise be too expensive or too verbose
    /// to run every single generation - writing to a file or calling a remote service, for
    /// example.
    /// </remarks>
    /// <param name="n">Run the probe once every <paramref name="n"/> generations.</param>
    /// <param name="observe">The probe to throttle.</param>
    /// <returns>A probe that only forwards to <paramref name="observe"/> on matching generations.</returns>
    let everyNth (n: int) (observe: GenerationInfo<'Gene> -> unit) : GenerationInfo<'Gene> -> unit =
        fun info -> if info.Generation % n = 0 then observe info
