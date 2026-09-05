# ContinuedFractions

> Collaboration contract → `../AGENTS.md`.
> Cross-cutting status & dependency graph → `../INSTRUCTIONS.md`.
> Mission, principles & repo conventions → `../VISION.md`.

The deep working reference for this submodule.

## Stack

A C# class library and its xUnit test project; language version, target
framework and namespace conventions are umbrella-wide and live in
`../VISION.md` and `Directory.Build.props`.

## Solution

`D:\Users\Hal\Documents\Visual Studio 2026\Projects\Math\ContinuedFractions\ContinuedFractions.slnx`

## Repo

`https://github.com/halheinrich/ContinuedFractions`, branch `main`.

## Depends on

- **BigRationalLibrary** — `HalHeinrich.Numerics.BigRational`, and **the edge
  is one constructor wide**. The library uses `BigRational` as the convergent
  type and calls `new BigRational(BigInteger, BigInteger)` at exactly one site,
  in `ContinuedFraction.EnsureComputedThrough`. Nothing else in the library
  touches it; the tests additionally rely on its value equality to assert
  convergents. Reduction is a no-op at that call site — consecutive convergents
  satisfy `p_n·q_{n-1} − p_{n-1}·q_n = ±1`, so every `p_n / q_n` is already in
  lowest terms.

  By **`ProjectReference`**, like every sibling, since halheinrich/Math#10
  carried migration step 7's first half. The path escapes the repo, so a clone
  of this repository alone cannot restore. See § Pitfalls.

## Layout

- **`ContinuedFractions`** — the library. The pattern generators, the two
  recognisers, and the value types they exchange.
- **`ContinuedFractions.Tests`** — xUnit, and **not only the controls**. It
  holds the discovery hunt as well as the tests; § Architecture says why that
  matters and § Pitfalls says what it costs. It sees `internal` members
  (`QuadraticIrrationalExpander`) by `InternalsVisibleTo`, which names the
  *assembly*, not the namespace.

## Architecture

**This is a discovery bench, not a library that happens to have tests.** Its
public types read like a continued-fraction library and describing it that way
is a mischaracterisation — one inferred from type names rather than read from
intent. What the repository is *for* is enumerating a parameterised space of
continued fractions and asking which of them converge to a function of `e`, `π`
or another common irrational, in the hope of a match not yet known. Everything
below is shaped by that: the generators enumerate the space, the recognisers
clear away the already-known, and what neither claims is the discovery surface.

### The pattern is the only construction surface

`ContinuedFraction` takes a `PatternCFCoefficientGenerator` and nothing else.
There is no coefficient-list constructor and no `IEnumerable<BigInteger>`
overload: to express a value you build the pattern, directly or through a
factory on `Patterns`. The reason is that the hunt enumerates *patterns*, and a
CF that arrived as a bare coefficient list would carry no structure to
enumerate, classify, or print — the classifiers below would have nothing to
read.

A pattern is a **pre-period** — a finite list of explicit coefficients emitted
once — followed by a cycle of **lanes**, each of which emits one coefficient
per visit and evolves its value across visits by an `Operation`: `Const`,
`Plus` (arithmetic progression) or `Multiply` (geometric). Position `i` past
the pre-period resolves to `lanes[j % lanes.Count].ValueAt(j / lanes.Count)`
with `j = i − preperiod.Count`. Euler's number is the motivating case:
`e = [2; 1, 2, 1, 1, 4, 1, 1, 6, …]` is one pre-period entry and three lanes,
`Const(1), Plus(2, +2), Const(1)`.

### The structural classification is the hunt's gate

`PatternCFCoefficientGenerator` partitions everything it can produce into three
disjoint classes, derived from the constructor inputs alone in `O(lanes)`, with
no iteration and no arithmetic on the value:

| class | condition | what it means |
| --- | --- | --- |
| `IsRational` | empty lane cycle | finite expansion; the value is rational |
| `IsQuadraticIrrational` | non-empty cycle, every lane `Const` | eventually periodic, so by Lagrange the value is a quadratic irrational |
| neither | some lane is `Plus` or `Multiply` | lane values grow, the CF is aperiodic, the value is something else — often transcendental |

**The third bucket is the discovery surface**, and the first two exist to keep
it clean. A hunt that had to *identify* every rational and every quadratic surd
before reaching an interesting candidate would spend its budget on the known.

### Lane validation makes `Const` the unique signal

Every degenerate operand that would silently reduce an operation to a constant
is rejected at construction: `Plus 0`, `Multiply 1`, `Multiply 0`, and every
negative operand. `InitialValue ≥ 1` is required outright. Two things follow,
and both are load-bearing:

- **Every emission is a valid partial quotient (`≥ 1`) at every visit**, for
  any visit count, without the consumer checking. A negative `Plus` operand
  would satisfy the invariant for a while and then break it at a visit depth
  nobody tested.
- **`Operation == Const` is the unique signal of a constant lane**, which is
  what lets `IsQuadraticIrrational` be a structural test rather than a
  value-level one. If `Plus 0` were constructible, an all-`Plus` pattern could
  be a quadratic irrational that the classifier called aperiodic.

The `default` value of `Lane` (`InitialValue = 0`, `Const`, operand `0`)
violates `InitialValue ≥ 1`; the struct has no way to prevent it and it should
not be used.

### `ContinuedFraction` — convergents, memoized, unbounded

The type is `IEnumerable<BigRational>` over its **convergents**, not over its
coefficients — the coefficients are the generator's, reachable through
`Generator`. Convergents come from the standard recurrence
`p_n = a_n·p_{n-1} + p_{n-2}`, `q_n = a_n·q_{n-1} + q_{n-2}` seeded at
`p_{-1} = 1, p_{-2} = 0, q_{-1} = 0, q_{-2} = 1`, computed in `BigInteger` and
cached, so re-iteration and repeated indexing are cheap and the depth-`n`
convergent costs `O(n)` only once.

The CF invariant on partial quotients is enforced **here** rather than on the
generator: a generator is a general integer-sequence source, and a non-positive
`a_n` with `n ≥ 1` throws `InvalidOperationException` when a convergent needs
it. Iteration of an unbounded CF never terminates on its own; the consumer
stops it.

**No value equality.** Two continued fractions that represent the same number
are not equal, because equality on generator-backed CFs is undecidable in
general. Reference equality is inherited and that is deliberate.

### Two recognisers, two strategies

Both derive from `CFIdentifier`, whose `TryIdentify` null-checks and delegates
to a protected `TryIdentifyCore` — so no implementation repeats the guard.

`EFamilyShapeIdentifier` **reads the answer off the coefficients**. It knows
four shapes and recovers the integer parameter from a single position:
`tanh(1/m)` from `[0; m, 3m, 5m, …]`, `coth(1/m)` from `[m; 3m, 5m, …]`,
`e + n` from `[k; 1, 2, 1, 1, 4, …]` with `n = k − 2`, and `1/(e + n)` from
`[0, k; 1, 2, 1, 1, 4, …]`. Recognition is therefore `O(depth)` per shape and
parameterised — it catches *every* `m`, not a table of them. An alias table
renames a handful of results to the algebraic form a reader expects:
`tanh(1/2)` reports as `(e − 1)/(e + 1)`.

`QuadraticIrrationalIdentifier` **searches**. It enumerates candidate triples
`(d, p, q)` in levels `k = 1, 2, 3, …`, each level being three sweeps in lex
order — the square-root sweep `(d, 0, q)`, then the general sweep with `p > 0`,
then the same triples with `p < 0`. The negative-`p` sweep is not an
afterthought: `φ = (1 + √5)/2` is `(√5 − (−1))/2`, so canonical forms with
`p < 0` are ordinary rather than exotic. Each candidate is expanded by
Lagrange's algorithm (`QuadraticIrrationalExpander`, `internal`) and compared
coefficient-by-coefficient. Square roots are subsumed: `√n` is the triple
`(n, 0, 1)`, which appears in level `n`'s first sweep.

**Comparison runs far past `preperiod + period`, and must.** A candidate with
period `[4]` agrees with a great many inputs on their first two coefficients,
so a comparison bounded by the candidate's own cycle length would declare
accidental prefix matches. `DefaultMaxComparisonDepth` (64) is what rules them
out, and it also bounds the expander's recurrence budget — candidates whose
pre-period plus period would exceed it are skipped rather than half-checked.

### Two result types, one answer

`IdentificationResult` carries `(Match, Identification, Depth)` with a
**string** identification, and is the contract every `CFIdentifier` satisfies —
it is what a heterogeneous harness can dispatch through.
`QuadraticIdentificationResult` carries the recovered `QuadraticIrrational` as
structured `(D, P, Q)` data for a caller who wants to compute with it rather
than print it; `QuadraticIrrationalIdentifier.TryIdentifyQuadratic` is the
typed entry point and `TryIdentifyCore` formats its result down to the string
form. Both are `readonly record struct`s with public positional constructors,
and both direct callers to the `Matched` / `NotMatched` factories, which are
what enforce the invariants — a matched result carries a non-null payload and
the depth is non-negative.

`QuadraticIrrational` is the canonical form `(√D − P)/Q` with `D ≥ 0`, `Q ≥ 1`,
`P` **signed**, and `Q | D − P²`. The divisibility constraint is not
decoration: it is exactly what keeps Lagrange's recurrence in integers at every
step. Signed `P` is what makes the form total — `P = 0` gives plain roots,
`P > 0` gives `(√7 − 2)/3`, `P < 0` gives `φ`.

### `IntegerMath` — a primitive that is only visiting

`IntegerMath.Sqrt` is an exact integer square root with `Floor` / `Ceiling` /
`Nearest` rounding, computed by Newton-Raphson in `BigInteger` with **no
floating-point intermediate**. `Nearest` has no tie-break because it needs
none: for non-square `n`, `√n` is irrational, so the midpoint between two
consecutive integers is never attained.

It is a general-purpose numeric primitive sitting in a discovery bench, which
is halheinrich/Math#10 — it leaves for `BigRationalLibrary` with
`IntegerSqrtRounding` and its tests. Do not build on it here.

## Public API

Namespace `HalHeinrich.Numerics.ContinuedFractions`, with the generators in
`HalHeinrich.Numerics.ContinuedFractions.Generators`.

```csharp
public abstract class CFCoefficientGenerator : IEnumerable<BigInteger>
{
    public abstract BigInteger this[int index] { get; }
    public virtual int? Length { get; }          // null == unbounded
    public abstract override string ToString();
    public IEnumerator<BigInteger> GetEnumerator();
}

public sealed class PatternCFCoefficientGenerator : CFCoefficientGenerator
{
    public PatternCFCoefficientGenerator(
        IReadOnlyList<BigInteger> preperiod, IReadOnlyList<Lane> lanes);
    public PatternCFCoefficientGenerator(
        BigInteger[] preperiod, params Lane[] lanes);

    public IReadOnlyList<BigInteger> Preperiod { get; }
    public IReadOnlyList<Lane> Lanes { get; }
    public bool IsRational { get; }              // empty lane cycle
    public bool IsQuadraticIrrational { get; }   // non-empty, all Const
}

public enum Operation { Const, Plus, Multiply }

public readonly record struct Lane
{
    public Lane(BigInteger initialValue, Operation operation, BigInteger operand);

    public BigInteger InitialValue { get; }
    public Operation Operation { get; }
    public BigInteger Operand { get; }           // unused for Const

    public static Lane Const(BigInteger value);
    public static Lane Plus(BigInteger initialValue, BigInteger increment);
    public static Lane Multiply(BigInteger initialValue, BigInteger factor);

    public BigInteger ValueAt(int visit);        // zero-based
}

public static class Patterns
{
    public static PatternCFCoefficientGenerator Phi();          // [1; 1, 1, …]
    public static PatternCFCoefficientGenerator Sqrt2();        // [1; 2, 2, …]
    public static PatternCFCoefficientGenerator EulersNumber(); // [2; 1, 2, 1, …]
    public static PatternCFCoefficientGenerator Rational(
        BigInteger numerator, BigInteger denominator);
}
```

`Lane`'s constructor throws `ArgumentOutOfRangeException` on
`initialValue < 1` or an undefined `operation`, and `ArgumentException` on a
degenerate operand (`Plus` below 1, `Multiply` below 2).
`PatternCFCoefficientGenerator` throws `ArgumentException` when both the
pre-period and the lane cycle are empty, or when a non-leading pre-period entry
is not a valid partial quotient; the leading entry is the integer part and may
be any sign. `Patterns.Rational` throws `ArgumentOutOfRangeException` on a zero
denominator and produces a Euclidean expansion whose entire content is the
pre-period.

```csharp
public sealed class ContinuedFraction : IEnumerable<BigRational>
{
    public ContinuedFraction(PatternCFCoefficientGenerator generator);

    public PatternCFCoefficientGenerator Generator { get; }
    public BigInteger IntegerPart { get; }       // a0
    public BigRational this[int depth] { get; }  // the depth-th convergent
    public IEnumerator<BigRational> GetEnumerator();
}
```

The indexer throws `ArgumentOutOfRangeException` on a negative depth or, for a
finite CF, on a depth at or past the generator's length, and
`InvalidOperationException` when computing the convergent required a
non-positive `a_n` with `n ≥ 1`.

```csharp
public abstract class CFIdentifier
{
    public IdentificationResult TryIdentify(ContinuedFraction cf);
    protected abstract IdentificationResult TryIdentifyCore(ContinuedFraction cf);
}

public readonly record struct IdentificationResult(
    bool Match, string? Identification, int Depth)
{
    public static IdentificationResult Matched(string identification, int depth);
    public static IdentificationResult NotMatched(int depth);
}

public sealed class EFamilyShapeIdentifier : CFIdentifier
{
    public const int DefaultMaxComparisonDepth = 64;
    public EFamilyShapeIdentifier(int maxComparisonDepth = DefaultMaxComparisonDepth);
}

public sealed class QuadraticIrrationalIdentifier : CFIdentifier
{
    public const int DefaultMaxTriples = 20_000;
    public const int DefaultMaxComparisonDepth = 64;

    public QuadraticIrrationalIdentifier(
        int maxTriples = DefaultMaxTriples,
        int maxComparisonDepth = DefaultMaxComparisonDepth);

    public QuadraticIdentificationResult TryIdentifyQuadratic(ContinuedFraction cf);
}

public readonly record struct QuadraticIdentificationResult(
    bool Match, QuadraticIrrational? Value, int Depth)
{
    public static QuadraticIdentificationResult Matched(
        QuadraticIrrational value, int depth);
    public static QuadraticIdentificationResult NotMatched(int depth);
}

public readonly record struct QuadraticIrrational
{
    public QuadraticIrrational(BigInteger d, BigInteger p, BigInteger q);

    public BigInteger D { get; }                 // radicand, >= 0
    public BigInteger P { get; }                 // offset, any sign
    public BigInteger Q { get; }                 // denominator, >= 1
}

public enum IntegerSqrtRounding { Floor, Ceiling, Nearest }

public static class IntegerMath
{
    public static BigInteger Sqrt(
        BigInteger value,
        IntegerSqrtRounding rounding = IntegerSqrtRounding.Floor);
}
```

Both identifier constructors throw `ArgumentOutOfRangeException` on a
non-positive budget. `QuadraticIrrational`'s constructor throws
`ArgumentOutOfRangeException` on a negative `d` or non-positive `q`, and
`ArgumentException` when `q` does not divide `d − p²`; its `default` value
violates `Q ≥ 1` and is an invalid sentinel rather than a meaningful value.
`IntegerMath.Sqrt` throws `ArgumentOutOfRangeException` on a negative input or
an undefined rounding mode.

## Pitfalls

- **The hunt lives in the test project, and it is an experiment.**
  `HuntTests` enumerates patterns, dispatches them through the identifier
  chain, prints a table and asserts on a handful of hand-checked rows.
  `AGENTS.md` § Exactness discipline draws the line the other way: a target
  with a known answer is a test and gates CI; a target with an unknown answer
  is an experiment and its output is data. `Collatz` has already separated its
  two into `Collatz.Tests` and `Collatz.Experiments`; this repository has not
  faced the question. Until it does, the suite's runtime and its meaning are
  both hostage to a sweep whose whole point is that nobody knows what it will
  find.
- **`HuntTests` writes into the source tree.** It computes the test project
  root as `AppContext.BaseDirectory` plus three `..` segments and writes
  `hunt-output.txt` there. A test with a filesystem side effect is already
  worth knowing about; the path arithmetic additionally assumes the
  `bin/<config>/<tfm>` output layout, and moves silently the day that changes.
  The files are git-ignored, which is legitimate **here** — this is the repo
  where the hunt actually runs.
- **A `NotMatched` never means "not a member of the family".** It means the
  identifier did not find a match *within its budget* —
  `DefaultMaxTriples = 20_000` triples and `DefaultMaxComparisonDepth = 64`
  coefficients. `√n` needs the triple `(n, 0, 1)`, which sits in level `n`'s
  square-root sweep — the first of that level's three. The default budget runs
  out partway through level 22's *general* sweep, after its square-root sweep
  has already been emitted, so **the default identifies `√n` up to `n = 22` and
  cannot reach `√23` at all** (measured 2026-09-03: every non-square `n` from 2
  to 30 through the public surface, matching from 2 to 22 and failing from 23
  on). Reporting an unmatched candidate as "not quadratic" turns a budget into
  a proof.
- **`EFamilyShapeIdentifier` returns early on short finite CFs**, before any
  recogniser runs, with a depth of `max(len − 1, 0)`. That is correct — a
  finite CF is rational and no shape can match it — but the returned depth
  measures the input, not the work done, and reads differently from the
  budget-exhausted depth the other paths report.
- **`QuadraticIrrational` accepts a perfect-square `D`.** The canonical form is
  validated, not the irrationality, so `(√9 − 1)/2` constructs happily and is
  the rational `1`. Anything that assumes an infinite periodic expansion must
  check; `QuadraticIrrationalExpander` returns an empty period for that case
  and `QuadraticIrrationalIdentifier` skips it.
- **`IntegerMath` and `IntegerSqrtRounding` are leaving** for
  `BigRationalLibrary` under halheinrich/Math#10, together with their tests and
  their current callers' expectations. Do not grow them here, and do not add a
  second caller that would have to be migrated too.
- **Namespaces are rooted at `HalHeinrich.Numerics.ContinuedFractions`** — the
  sub-namespace, not the flat root the shared machinery uses. `BigRational` and
  `RationalApproximation` sit at `HalHeinrich.Numerics` because they *are* the
  shared machinery; a bench's recognisers, generators and `QuadraticIrrational`
  would crowd that surface with things no other member should reach for.
  `Collatz` set the precedent. `RootNamespace` is set on both projects so a new
  file lands correctly by default; the `InternalsVisibleTo` still reads
  `ContinuedFractions.Tests` because that is an assembly name and assemblies
  were not renamed.
- **This repository does not build standalone.** Its `ProjectReference` to
  `BigRationalLibrary` escapes the repository and resolves only beside a
  `BigRationalLibrary` checkout, as inside the umbrella. Restore fails, not
  compile, so the error names a missing project rather than a missing type.
  This was not always so — the edge was a `PackageReference` at
  `0.1.0-preview1` until halheinrich/Math#10 — so treat any surviving claim
  that a clone of this repo restores on its own as stale.
- **`nuget.config` here is the umbrella's only correct
  `packageSourceMapping`.** It declares the `github` source *and* maps
  `HalHeinrich.*` to it, which is what makes the dependency-confusion defence
  its comment describes real. The other members inherited the comment without
  the source, which is halheinrich/Math#30 — *their* defect, not this file's.
  Change nothing here to match them.
- **`RestoreLockedMode` is unconditional, so a deliberate package change fails
  the restore.** It used to be gated on `ContinuousIntegrationBuild`, which
  nothing here sets and no workflow has ever existed to set, so the lock files
  were honoured by convention only (halheinrich/Math#29). They are enforced
  now: changing a version in a `.csproj` gives `NU1004` rather than a quietly
  rewritten lock. Recover with `dotnet restore -p:RestoreForceEvaluate=true`
  and commit the regenerated file — and note that NuGet writes it with CRLF
  while `.gitattributes` pins it to LF, so `git status` will then report a
  change `git diff` cannot show. It no longer guards the BigRational edge:
  halheinrich/Math#10 made that a `ProjectReference`, and a project-edge
  constraint is written into the lock and never read (halheinrich/Math#44). The
  hazard went with the protection, since a path reference has no version to
  drift. What the lock still guards is the test project's tooling graph.
- **`.gitattributes` pins `*.cs` to `eol=crlf`**, and that bites mechanical
  edits. A tool that rewrites a source file with LF endings leaves a working
  tree disagreeing with what checkout produces — git normalises on staging, so
  the committed blob is unaffected and `git diff` looks clean while every
  subsequent git command warns. Restore the endings rather than reading the
  clean diff as proof nothing happened.

## Subproject-internal next steps

The backlog lives in the umbrella tracker rather than here: halheinrich/Math#10
takes `IntegerMath` out and converts the BigRational edge in the same arc, and
§ Pitfalls above says what each open item costs a reader today.

The one question this repository owns and has not answered is the
controls-versus-experiments split — whether `HuntTests` becomes a
`ContinuedFractions.Experiments` project as `Collatz`'s sweeps did. It is not
filed, because the decision is worth taking against a real workflow rather than
in the abstract, and this member has none yet.

Cross-cutting items — the build-and-test workflow this member still lacks, and
the lock-file gate that stays dormant without it — are `../INSTRUCTIONS.md`'s.
