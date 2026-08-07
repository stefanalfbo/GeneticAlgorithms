namespace GeneticAlgorithms

open System
open System.Collections.Generic

[<AbstractClass; Sealed>]
type Interop =
    static member CreateChromosome<'Gene>(genes: 'Gene array) : Chromosome<'Gene> =
        if isNull genes then
            nullArg "genes"

        { genes = Array.copy genes
          size = genes.Length
          fitness = 0.0
          age = 0 }

    static member CreateOptions(populationSize: int) : Options = { population_size = populationSize }

    static member CreateProblem<'Gene>
        (
            genotype: Func<Chromosome<'Gene>>,
            fitnessFunction: Func<Chromosome<'Gene>, float>,
            terminate: Func<IEnumerable<Chromosome<'Gene>>, int, float, bool>
        ) : Problem<'Gene> =
        if isNull genotype then
            nullArg "genotype"

        if isNull fitnessFunction then
            nullArg "fitnessFunction"

        if isNull terminate then
            nullArg "terminate"

        { genotype = fun () -> genotype.Invoke()
          fitness_function = fun chromosome -> fitnessFunction.Invoke chromosome
          terminate = fun population generation temperature -> terminate.Invoke(population, generation, temperature) }

    static member Run<'Gene>(problem: Problem<'Gene>, populationSize: int) : Chromosome<'Gene> =
        Genetic.run problem { population_size = populationSize }

    static member Run<'Gene>(problem: Problem<'Gene>, options: Options) : Chromosome<'Gene> =
        Genetic.run problem options
