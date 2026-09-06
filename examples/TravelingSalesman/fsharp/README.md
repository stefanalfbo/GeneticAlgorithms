# TravelingSalesman

This project demonstrates how to use the `GeneticAlgorithms` library to solve a small instance of the classic **Traveling Salesman Problem** (TSP): find the shortest closed tour that visits every city exactly once and returns to the start.

## What Is the Traveling Salesman Problem?

Given a fixed set of cities and the distances between them, find the order to visit all of them - starting and ending at the same city - that minimizes total travel distance. It's one of the best-known problems in combinatorial optimization: the number of distinct tours grows factorially with the number of cities, so brute-force search is only feasible for a handful of cities, and finding the exact optimum for larger instances is famously hard (TSP is NP-hard). A genetic algorithm doesn't guarantee the exact optimal tour, but it can find a very good one quickly, even as the number of cities grows.

This example uses 12 cities with illustrative `(x, y)` coordinates - not real geographic positions, just a 2D layout loosely echoing where these cities sit relative to one another, good enough to make a bad tour (one that crosses back over itself) visibly worse than a good one.

## The Genotype

Each chromosome's genes are a permutation of the 12 city indices - the order in which the tour visits them. `Genes.[i]` and `Genes.[i + 1]` are consecutive stops, and the tour is a closed loop: the last city connects back to the first.

## Fitness

`Genetic.run` always maximizes fitness, but a tour's quality is its total distance, which we want to *minimize*. Fitness is simply the negated total distance (`Distance.euclidean` summed around the closed loop): the shortest tour has the fitness closest to zero, the largest (least negative) value. `Distance.euclidean` is a small addition to the library's `Distance` module alongside `hammingDistance` and `jaroSimilarity`, since a fitness function that minimizes a spatial distance is a different (and common) kind of building block from the existing string-comparison functions.

## Why `orderOneCrossover`?

A chromosome here is a **permutation** - every city must be visited exactly once - so this example uses `Crossover.orderOneCrossover` instead of the library's default `Crossover.singlePoint`. A single-point cut on a permutation would generally produce a tour with one city missing and another repeated; order-one crossover copies a slice from one parent and fills the remaining stops from the other, in order, so both children stay valid tours of all 12 cities.

## Why `Reinsertion.elitist` Instead of the Default?

`SelectionRate = 0.8` combined with `Reinsertion.elitist 0.15` keeps the population size roughly stable across all 500 generations (`0.8 + 0.05` `MutationRate` `+ 0.15 = 1.0`). This matters here specifically because the run is a **fixed generation count** with no early-exit fitness target (there's no way to know the true shortest tour ahead of time to check against) - the simpler `` Reinsertion.`pure` `` strategy, at the library's default `SelectionRate` of 1.0, would let mutants accumulate as pure population growth every generation with nothing to check it, compounding over hundreds of generations into a population far too large to finish in any reasonable time.

## Running the Example

From the repository root:

```powershell
dotnet run --project examples/TravelingSalesman/fsharp
```

## Expected Output

```text
Current best distance: 315.45
Current best distance: 246.26
Current best distance: 246.26
...

Best tour found (total distance: 246.26):
Berlin -> Amsterdam -> Copenhagen -> Oslo -> Helsinki -> Stockholm -> Paris -> London -> Madrid -> Lisbon -> Rome -> Vienna -> Berlin
```

Because the algorithm is randomized, the exact tour and its distance will vary between runs - typically converging somewhere in the low-to-mid 200s for this particular layout, well down from the 300+ of a random starting tour. The genetic algorithm doesn't promise the true shortest possible tour, only a good one found quickly.

## Related Projects

This example is part of the GeneticAlgorithms library and serves as the reference implementation for permutation-encoded optimization with a real-valued, minimized cost function - complementing `NQueens`, which is also permutation-encoded but maximizes a constraint-satisfaction count instead.
