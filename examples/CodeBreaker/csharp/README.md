# Code Breaker in C#

This example uses the `GeneticAlgorithms` library from C# to discover the key that decrypts an XOR-encoded message.

## The problem

The program starts with a known target and its encrypted representation:

```text
Target:    ILoveGeneticAlgorithms
Encrypted: LIjs`B`k`qlfDibjwlqmhv
```

Each chromosome contains a binary representation of a candidate key. The genes are folded from left to right into an integer, which is then used to decrypt every character:

```csharp
static string Cipher(string text, int key) =>
    new(text.Select(character => (char)((character ^ key) % 32768)).ToArray());
```

Because the cipher reduces character values modulo `32768`, only the lower 15 bits of a candidate key affect the decoded message. Different chromosomes can therefore represent equivalent effective keys.

## Purpose

This example mirrors the F# CodeBreaker version, but uses the C# facade provided by `GeneticAlgorithms.GeneticAlgorithm`. It uses `Distance.jaro` straight from C#, the same way `Selection`/`Crossover`/`Mutation` module functions can be passed as method groups elsewhere in these examples - here it's called directly as an ordinary two-argument static method inside the fitness function.

## Fitness

The fitness function decrypts the message with a chromosome's candidate key and measures its similarity to the target using the Jaro distance:

- `0.0` means that the strings have no similarity.
- `1.0` means that the decoded message exactly matches the target.

Evolution stops as soon as any chromosome reaches a fitness of `1.0`.

## Evolution process

For each generation, the example:

1. Converts every chromosome's binary genes into a candidate key.
2. Decrypts the encrypted message with that key.
3. Calculates the Jaro distance between the decoded message and the target.
4. Selects the best candidates using elite selection.
5. Produces new candidates with single-point crossover and mutation.
6. Repeats until the original message is recovered.

The genetic algorithm uses a population of 100 chromosomes, an 80% selection rate, and a 5% mutation rate - the library's defaults, so this example calls `GeneticAlgorithm.Run` with just a population size rather than building custom `Options`.

## Running the example

From the repository root:

```powershell
dotnet run --project examples/CodeBreaker/csharp
```

## Example output

The generation count and discovered chromosome vary between runs because the algorithm is randomized. A successful run ends with output similar to:

```text
Current Best 1.000000
Target:   ILoveGeneticAlgorithms
Decoded:  ILoveGeneticAlgorithms
Key:      2686981 ([1; 0; 1; 0; 0; 1; 0; 0; 0; 0; 0; 0; 0; 0; 0; 0; 0; 0; 0; 1; 0; 1])
Fitness:  1.000000
```

The displayed key may differ between runs. Any successful key has the same relevant lower bits and decrypts the message to the target text.
