# TigerSimulation in C#

This example uses the `GeneticAlgorithms` library from C# to simulate tiger evolution under two very different environments - **tropical** and **tundra** - over 1000 generations, tracking mean fitness and mean age across the whole population via a custom probe.

This mirrors the F# `TigerSimulation` example; see [its README](../fsharp/README.md) for the full explanation of the genotype, the fitness tables, and the notes on mean age and scope. This README focuses on what's different about the C# version.

## Purpose

Like the C# `Reinsertion` example, this one needs a custom **reinsertion** strategy (`Reinsertion.elitist`, to keep the population stable over 1000 generations) and a custom **probe** (to record statistics every generation) - both already exposed by `GeneticAlgorithm.CreateOptions`. Nothing new needed to be added to the C# facade for this example.

There is one thing C# doesn't have a ready-made equivalent for: the F# version combines two probes with `Probes.combine`/`Probes.everyNth` (the statistics collector running every generation, a throttled `Probes.printProgress` running every 100th). There's no C#-facing version of those combinators, but composing two probes doesn't need one - it's just a lambda that calls both:

```csharp
probe: info =>
{
    history.Add(new GenerationStats(/* ... */));

    if (info.Generation % 100 == 0)
    {
        Probes.printProgress(info);
    }
}
```

## A Note on CSV and Locale

Writing the per-generation statistics to CSV needs `CultureInfo.InvariantCulture` explicitly - on a machine whose locale uses a comma as the decimal separator (as this one does), a plain `$"{value:F6}"` would format `3.88` as `3,88`, silently turning one CSV field into two and corrupting the file. The console output in this example doesn't force a culture (matching every other C# example here), since a comma decimal in a `Console.WriteLine` is just a locale-appropriate display choice - but a CSV is an interchange format read by other programs, not display text, so it can't be allowed to vary with whoever's machine produced it.

## Running the Example

From the repository root:

```powershell
dotnet run --project examples/TigerSimulation/csharp
```

## Expected Output

```text
Simulating tiger evolution over 1000 generations in two environments...

Mean fitness / mean age by generation (sampled every 100 generations):
Generation |  Tropical Fit. | Tropical Age |    Tundra Fit. |   Tundra Age
         0 |           3,50 |         1,00 |           2,34 |         1,00
       100 |           7,38 |         1,21 |           7,30 |         1,23
       ...
      1000 |           7,35 |         1,21 |           7,28 |         1,21

Final results:
Tropical:
  Final mean fitness: 7,35, final mean age: 1,21
  Fittest tiger (fitness 7,50): Size: smaller, Swimming Ability: high, Fur Color: dark, Fat Stores: more, Activity Period: nocturnal, Hunting Ground: larger, Fur Thickness: less thick, Tail Length: smaller
Tundra:
  Final mean fitness: 7,28, final mean age: 1,21
  Fittest tiger (fitness 7,50): Size: larger, Swimming Ability: high, Fur Color: light, Fat Stores: less, Activity Period: nocturnal, Hunting Ground: larger, Fur Thickness: more thick, Tail Length: smaller

Full per-generation statistics written to tropical_stats.csv and tundra_stats.csv
```

As with the F# version, both environments reliably converge on the theoretical maximum fitness of 7.5, and mean age hovers a little above `1.0` (around `1.2` in the sample run above) rather than growing with the generation count - see the F# README's "A Note on Mean Age" for why that reflects `Reinsertion.elitist 0.15`'s mix of freshly born crossover children and older carried-over survivors. Because the algorithm is randomized, exact values vary between runs. (Depending on your machine's locale, the console's decimal numbers may print with a comma instead of a period, as in the sample above - this example doesn't force a specific culture for its console output, matching the other C# examples in this library; the CSV output always uses a period, regardless of locale.)

## Related Projects

This example is part of the GeneticAlgorithms library. It's the C# counterpart to the [`TigerSimulation`](../fsharp/README.md) F# example.
