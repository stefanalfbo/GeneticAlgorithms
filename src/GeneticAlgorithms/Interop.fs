namespace GeneticAlgorithms

open System
open System.Collections.Generic

[<AbstractClass; Sealed>]
type GeneticAlgorithm =
    static member CreateChromosome<'Gene>(genes: 'Gene array) : Chromosome<'Gene> =
        if isNull genes then
            nullArg "genes"

        { genes = Array.copy genes
          size = genes.Length
          fitness = 0.0
          age = 0 }

    static member CreateOptions<'Gene>(populationSize: int) : Options<'Gene> = { population_size = populationSize; selection_rate = 0.8; selection_fn = Selection.elite }

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

    static member Run<'Gene when 'Gene: equality>
        (
            genotype: Func<Chromosome<'Gene>>,
            fitnessFunction: Func<Chromosome<'Gene>, float>,
            terminate: Func<IEnumerable<Chromosome<'Gene>>, int, float, bool>,
            populationSize: int
        ) : Chromosome<'Gene> =
        let problem = GeneticAlgorithm.CreateProblem(genotype, fitnessFunction, terminate)
        let options = GeneticAlgorithm.CreateOptions(populationSize)

        Genetic.run problem options

    static member Run<'Gene when 'Gene: equality>
        (
            genotype: Func<Chromosome<'Gene>>,
            fitnessFunction: Func<Chromosome<'Gene>, float>,
            terminate: Func<IEnumerable<Chromosome<'Gene>>, int, float, bool>,
            options: Options<'Gene>
        ) : Chromosome<'Gene> =
        if isNull (box options) then
            nullArg "options"

        let problem = GeneticAlgorithm.CreateProblem(genotype, fitnessFunction, terminate)

        Genetic.run problem options

    static member Run<'Gene when 'Gene: equality>(problem: Problem<'Gene>, populationSize: int) : Chromosome<'Gene> =
        if isNull (box problem) then
            nullArg "problem"

        Genetic.run problem { population_size = populationSize; selection_rate = 0.8; selection_fn = Selection.elite }

    static member Run<'Gene when 'Gene: equality>(problem: Problem<'Gene>, options: Options<'Gene>) : Chromosome<'Gene> =
        if isNull (box problem) then
            nullArg "problem"

        if isNull (box options) then
            nullArg "options"

        Genetic.run problem options

[<AbstractClass; Sealed>]
type Interop =
    static member CreateChromosome<'Gene>(genes: 'Gene array) : Chromosome<'Gene> =
        GeneticAlgorithm.CreateChromosome genes

    static member CreateOptions<'Gene>(populationSize: int) : Options<'Gene> =
        GeneticAlgorithm.CreateOptions populationSize

    static member CreateProblem<'Gene>
        (
            genotype: Func<Chromosome<'Gene>>,
            fitnessFunction: Func<Chromosome<'Gene>, float>,
            terminate: Func<IEnumerable<Chromosome<'Gene>>, int, float, bool>
        ) : Problem<'Gene> =
        GeneticAlgorithm.CreateProblem(genotype, fitnessFunction, terminate)

    static member Run<'Gene when 'Gene: equality>
        (
            genotype: Func<Chromosome<'Gene>>,
            fitnessFunction: Func<Chromosome<'Gene>, float>,
            terminate: Func<IEnumerable<Chromosome<'Gene>>, int, float, bool>,
            populationSize: int
        ) : Chromosome<'Gene> =
        GeneticAlgorithm.Run(genotype, fitnessFunction, terminate, populationSize)

    static member Run<'Gene when 'Gene: equality>
        (
            genotype: Func<Chromosome<'Gene>>,
            fitnessFunction: Func<Chromosome<'Gene>, float>,
            terminate: Func<IEnumerable<Chromosome<'Gene>>, int, float, bool>,
            options: Options<'Gene>
        ) : Chromosome<'Gene> =
        GeneticAlgorithm.Run(genotype, fitnessFunction, terminate, options)

    static member Run<'Gene when 'Gene: equality>(problem: Problem<'Gene>, populationSize: int) : Chromosome<'Gene> =
        GeneticAlgorithm.Run(problem, populationSize)

    static member Run<'Gene when 'Gene: equality>(problem: Problem<'Gene>, options: Options<'Gene>) : Chromosome<'Gene> =
        GeneticAlgorithm.Run(problem, options)
