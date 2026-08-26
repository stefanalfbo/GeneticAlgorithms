# Schedule in C#

This example uses the `GeneticAlgorithms` library from C# to solve a **class scheduling** problem: choosing which classes to take, under a credit-hour limit, to maximize interest and usefulness while minimizing difficulty.

This is the class scheduling example from *Genetic Algorithms in Elixir* (Chapter 8, "Replacing and Transitioning").

## The problem

Given ten possible classes - Algorithms, Artificial Intelligence, Calculus, Chemistry, Data Structures, Discrete Math, History, Literature, Physics, and Volleyball - decide which ones to take, subject to a limit of 18 credit hours, so that the overall schedule scores as highly as possible on interest and usefulness while scoring as low as possible on difficulty.

Each class has been rated from 1 to 10 on three criteria, plus a fixed credit-hour cost:

| Class | Interest | Usefulness | Difficulty | Credit Hours |
| --- | --- | --- | --- | --- |
| Algorithms | 8.0 | 8.0 | 8.0 | 3.0 |
| Artificial Intelligence | 8.0 | 9.0 | 9.0 | 3.0 |
| Calculus | 5.0 | 6.0 | 4.0 | 3.0 |
| Chemistry | 9.0 | 2.0 | 3.0 | 4.5 |
| Data Structures | 7.0 | 8.0 | 5.0 | 3.0 |
| Discrete Math | 2.0 | 9.0 | 2.0 | 3.0 |
| History | 8.0 | 1.0 | 4.0 | 3.0 |
| Literature | 2.0 | 2.0 | 2.0 | 3.0 |
| Physics | 7.0 | 5.0 | 6.0 | 4.5 |
| Volleyball | 10.0 | 1.0 | 1.0 | 1.5 |

Each chromosome is a fixed-length binary array where the index is a class and the value is whether it's included in the schedule.

## Purpose

This example mirrors the F# Schedule version, but uses the C# facade provided by `GeneticAlgorithms.GeneticAlgorithm`. It uses the library's default selection, crossover, and mutation strategies, so it calls `GeneticAlgorithm.Run` with just a population size rather than building custom `Options` - the same simpler style as the `CodeBreaker` C# example.

## Fitness

For every selected class, `0.3 * usefulness + 0.3 * interest - 0.3 * difficulty` is added to the schedule's fitness, so usefulness and interest raise the score while difficulty lowers it, all weighted equally. The credit hours of every selected class are then added up; if the total exceeds 18, the schedule's fitness becomes `-99999` regardless of how good its weighted score was - a large enough penalty that invalid schedules always lose to valid ones.

Because the optimal fitness value isn't known ahead of time, this example terminates on a fixed generation count (1000) rather than a fitness target.

## Running the example

From the repository root:

```powershell
dotnet run --project examples/Schedule/csharp
```

## Example output

The generation count and discovered schedule vary between runs because the algorithm is randomized. A successful run ends with output similar to:

```text
Current Best 12.900000
Best schedule: [0; 0; 0; 1; 1; 1; 0; 0; 1; 1] (fitness: 12.900000)
Classes:       Chemistry, Data Structures, Discrete Math, Physics, Volleyball
```
