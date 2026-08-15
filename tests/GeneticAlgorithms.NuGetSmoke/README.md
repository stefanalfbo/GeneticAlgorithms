# GeneticAlgorithms NuGet Smoke Test

Verifies that a published version of the `GeneticAlgorithms` package installs from nuget.org and runs correctly. Unlike `GeneticAlgorithms.CSharpSmoke`, which references the library via a `ProjectReference` to the local source, this project pulls the package by version through a `PackageReference`, exactly as a consumer would.

It is deliberately excluded from `genetic-algorithms.sln` so that ordinary builds and CI runs don't depend on a specific package version already being available on nuget.org.

## Running

The version must be supplied explicitly:

```powershell
dotnet run --project tests/GeneticAlgorithms.NuGetSmoke -p:GeneticAlgorithmsVersion=1.0.0
```

Note that a freshly published package can sit under "Unlisted" on nuget.org for a few minutes while it goes through validation and indexing, so restoring immediately after a publish may fail until that completes.

The project runs a trivial one-gene genetic algorithm and throws if the result isn't the expected fitness, so a non-zero exit code means the package is broken.
