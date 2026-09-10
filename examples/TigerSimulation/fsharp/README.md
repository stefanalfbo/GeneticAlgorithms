# TigerSimulation

This project demonstrates how to use the `GeneticAlgorithms` library to simulate tiger evolution under two very different environments - **tropical** and **tundra** - over 1000 generations, and how to build a custom probe to track statistics (mean fitness, mean age) across a long run.

This is the tiger simulation example from *Genetic Algorithms in Elixir* (Chapter 9, "Tracking Genetic Algorithm Statistics").

## The Genotype

Each tiger is a chromosome of 8 binary traits:

| Trait | Gene = 0 | Gene = 1 |
| --- | --- | --- |
| Size | smaller | larger |
| Swimming Ability | low | high |
| Fur Color | light | dark |
| Fat Stores | less | more |
| Activity Period | diurnal | nocturnal |
| Hunting Ground | smaller | larger |
| Fur Thickness | less thick | more thick |
| Tail Length | smaller | larger |

Fur Color's 0/1 meaning isn't pinned down anywhere in the source material this example is based on, so it's inferred here from the score table below: a gene of 1 is rewarded in the tropics and penalized in the tundra, so it's assigned "dark" (better camouflage in dense vegetation); a gene of 0 - the safer choice in the tundra - is assigned "light" (camouflage in snow).

## Fitness

Each trait scores differently depending on the environment:

| Trait | Tropical | Tundra |
| --- | --- | --- |
| Size | 0.0 | 1.0 |
| Swimming Ability | 3.0 | 3.0 |
| Fur Color | 2.0 | -2.0 |
| Fat Stores | 1.0 | -1.0 |
| Activity Period | 0.5 | 0.5 |
| Hunting Ground | 1.0 | 2.0 |
| Fur Thickness | -1.0 | 1.0 |
| Tail Length | 0.0 | 0.0 |

Fitness is `sum(gene * score)` over all 8 traits. Because a gene of 0 always contributes 0 regardless of score, only a gene of 1 is ever rewarded or penalized - and by how much depends entirely on which environment it's scored in. Tail Length scores 0.0 in both environments, so it's a neutral trait: never selected for or against, free to drift purely by chance.

This design also gives each environment a known optimum - the best possible chromosome sets every gene with a positive score to 1 and every gene with a non-positive score to 0. That works out to a maximum fitness of exactly **7.5** in both environments (via a different combination of traits in each case), which this example's runs do reach.

## Purpose

The library's built-in `Probes.printProgress` only reports the single fittest chromosome each generation - not enough to track statistics across a population over a long run. This example builds its own probe on top of `GenerationInfo`, following the library's own design: gathering statistics is left entirely to the developer, not imposed by the library (see `GeneticAlgorithms.Probes`'s doc comments).

```fsharp
type GenerationStats =
    { Generation: int
      MeanFitness: float
      MeanAge: float
      BestFitness: float }

let statsProbe (history: ResizeArray<GenerationStats>) : GenerationInfo<int> -> unit =
    fun info ->
        history.Add
            { Generation = info.Generation
              MeanFitness = info.Population |> Array.map (fun c -> c.Fitness) |> Array.average
              MeanAge = info.Population |> Array.map (fun c -> float c.Age) |> Array.average
              BestFitness = info.Best.Fitness }
```

This probe is combined with a throttled `Probes.printProgress` via `Probes.combine`, so the console stays readable across 1000 generations while the full per-generation history is still recorded for every single generation.

## How It Works

1. Run the same genotype, population size, selection, crossover, and mutation settings in both environments - only the fitness function's score table differs.
2. `SelectionRate = 0.8` combined with `Reinsertion.elitist 0.15` keeps the population size roughly stable across all 1000 generations (`0.8 + 0.05` `MutationRate` `+ 0.15 = 1.0`) - the simpler `` Reinsertion.`pure` `` strategy would let the population collapse over a run this long.
3. Record mean fitness, mean age, and best fitness every generation via the custom probe above.
4. After both runs finish, print a side-by-side comparison table (sampled every 100 generations) and each environment's fittest tiger, and write the complete per-generation history to `tropical_stats.csv` and `tundra_stats.csv` for further analysis or charting.

## Running the Example

From the repository root:

```powershell
dotnet run --project examples/TigerSimulation/fsharp
```

## Expected Output

```text
Simulating tiger evolution over 1000 generations in two environments...

Mean fitness / mean age by generation (sampled every 100 generations):
Generation |  Tropical Fit. | Tropical Age |    Tundra Fit. |   Tundra Age
         0 |           3.42 |         1.00 |           2.35 |         1.00
       100 |           7.36 |         1.21 |           7.21 |         1.20
       ...
      1000 |           7.29 |         1.21 |           7.28 |         1.21

Final results:
Tropical:
  Final mean fitness: 7.29, final mean age: 1.21
  Fittest tiger (fitness 7.50): Size: smaller, Swimming Ability: high, Fur Color: dark, Fat Stores: more, Activity Period: nocturnal, Hunting Ground: larger, Fur Thickness: less thick, Tail Length: smaller
Tundra:
  Final mean fitness: 7.28, final mean age: 1.21
  Fittest tiger (fitness 7.50): Size: larger, Swimming Ability: high, Fur Color: light, Fat Stores: less, Activity Period: nocturnal, Hunting Ground: larger, Fur Thickness: more thick, Tail Length: smaller

Full per-generation statistics written to tropical_stats.csv and tundra_stats.csv
```

Both environments reliably converge on their theoretical maximum fitness of 7.5, but via different trait combinations - tropical tigers evolve dark fur, more fat, and thin fur; tundra tigers evolve light fur, less fat, and thick fur - exactly the traits their respective score tables reward. Tail Length settles on either value at random and stays there, since nothing in either environment selects for or against it. Because the algorithm is randomized, exact intermediate values vary between runs.

## A Note on Mean Age

`Chromosome.Age` counts the number of generations a specific individual has existed - see the library README's "Core Concepts" section for the full definition. In this example, `Reinsertion.elitist 0.15` carries roughly 15% of each generation forward as unchanged survivors (who keep aging), while the rest of the population is replaced by freshly born crossover children (who reset to `Age = 0`, observed as `Age = 1` the first generation they're evaluated). That mix is why mean age hovers a little above `1.0` - around `1.2` in the sample run above - rather than growing with the generation count the way it used to: at any given moment, most of the population is one generation old, and a smaller, roughly constant fraction is older survivors pulling the average up. A higher `survivalRate` would push mean age up further; a lower one would push it closer to `1.0`. It still isn't a rich genealogy signal (it can't distinguish a five-generation survivor from a fifty-generation one without also looking at the age distribution's tail, not just its mean), but it now measures something real: how much of the population turns over each generation, not just how many generations have elapsed since the run started.

## Note on Scope

The task this example is based on also mentions tracking **genealogy** (ancestry across generations). That isn't implemented here: doing it properly would mean tracking parent lineage through crossover and reinsertion, which the `GeneticAlgorithms` library doesn't currently support - it would be a change to the library itself, not something an example alone can add. Mean fitness, mean age, and the fittest chromosome per generation - the statistics this example does track - only needed `GenerationInfo`, which the library already exposes.

## Related Projects

This example is part of the GeneticAlgorithms library and demonstrates writing a custom statistics-tracking probe on top of the `Probes` module, comparing the same genotype and algorithm settings across two different fitness functions.
