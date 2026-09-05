# ContinuedFractions

A .NET 10 class library for exploring continued fractions, with an xUnit test project.

## Projects

- `ContinuedFractions` - main library
- `ContinuedFractions.Tests` - xUnit tests

## Building

**This repository does not build standalone.** It references
`BigRationalLibrary` by `ProjectReference`, and that reference escapes the
repo:

```
..\..\BigRationalLibrary\BigRationalLibrary\BigRationalLibrary.csproj
```

That resolves only when this checkout sits beside a `BigRationalLibrary`
checkout, as it does inside the umbrella:

```
Math/
  BigRationalLibrary/
  ContinuedFractions/       <- here
```

A clone of this repository alone cannot restore, and will fail at restore
rather than at compile. This is the accepted price of the umbrella's
`ProjectReference` ruling, not an oversight.

```powershell
dotnet build
```

## Test

```powershell
dotnet test
```
