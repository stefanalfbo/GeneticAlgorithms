# Code Breaker

This example uses the `GeneticAlgorithms` library to discover the key that decrypts an XOR-encoded message.

## The problem

The program starts with a known target and its encrypted representation:

```text
Target:    ILoveGeneticAlgorithms
Encrypted: LIjs`B`k`qlfDibjwlqmhv
```

Each chromosome contains a binary representation of a candidate key. The genes are folded from left to right into an integer, which is then used to decrypt every character:

```fsharp
let cipher (text: string) key =
    text
    |> Seq.map (fun character -> char ((int character ^^^ key) % 32768))
    |> Seq.toArray
    |> String
```

Because the cipher reduces character values modulo `32768`, only the lower 15 bits of a candidate key affect the decoded message. Different chromosomes can therefore represent equivalent effective keys.

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

The genetic algorithm uses a population of 100 chromosomes, an 80% selection rate, and a 5% mutation rate.

## Running the example

From the repository root:

```powershell
dotnet run --project examples/CodeBreaker/fsharp
```

## Example output

The generation count and discovered chromosome vary between runs because the algorithm is randomized. A successful run ends with output similar to:

```text
Current Best 0.409091
Current Best 1.000000
Target:   ILoveGeneticAlgorithms
Decoded:  ILoveGeneticAlgorithms
Key:      1507333 ([|0; 1; 0; 1; 1; 1; 0; 0; 0; 0; 0; 0; 0; 0; 0; 0; 0; 0; 0; 1; 0; 1|])
Fitness:  1.000000
```

The displayed key may differ between runs. Any successful key has the same relevant lower bits and decrypts the message to the target text.
