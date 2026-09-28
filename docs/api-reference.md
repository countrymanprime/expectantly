# API reference

Everything public in `Expectantly` today. There's no generated API site yet, so this page is
maintained by hand. Update it in the same PR as any change to the public surface.

## `Expectantly.Expect` (static)

| Member | Signature | Notes |
|---|---|---|
| `That<T>` | `ObjectAssertions<T> That<T>(T actual, string? because = null, params object[] becauseArgs)` | Wraps a concrete value. |
| `That<T>` | `ObjectAssertions<T> That<T>(Func<T> actualFactory, string? because = null, params object[] becauseArgs)` | Invokes `actualFactory` once, immediately. Throws `ArgumentNullException` if `actualFactory` is `null`. |

## `Expectantly.ObjectAssertions<TActual>`

Implements `IAssertion<TActual>`. `Expect.That` is the intended way to create one, but the class
is public, unsealed, and has a public constructor
`ObjectAssertions(TActual actual, IAssertionContext context)`.

| Member | Type/Signature | Behavior |
|---|---|---|
| `Actual` | `TActual` | The value under test. |
| `Context` | `IAssertionContext` | Metadata for message formatting. |
| `Is(TActual expected)` | `AndConstraint<ObjectAssertions<TActual>>` | Throws unless `Equals(Actual, expected)`. |
| `IsNot(TActual unexpected)` | `AndConstraint<ObjectAssertions<TActual>>` | Throws if `Equals(Actual, unexpected)`. |
| `IsNull()` | `AndConstraint<ObjectAssertions<TActual>>` | Throws unless `Actual is null`. |
| `IsNotNull()` | `AndConstraint<ObjectAssertions<TActual>>` | Throws if `Actual is null`. |
| `IsSameAs(TActual instance)` | `AndConstraint<ObjectAssertions<TActual>>` | Throws unless `ReferenceEquals(Actual, instance)`. Always fails for value types; see [Known limitations](#known-limitations). |
| `IsAssignableTo<TExpected>()` | `AndConstraint<ObjectAssertions<TActual>>` | Throws unless `Actual is TExpected`. |
| `IsTrue()` | `AndConstraint<ObjectAssertions<TActual>>` | Throws unless `Actual` is a `bool` and `true`. |
| `IsFalse()` | `AndConstraint<ObjectAssertions<TActual>>` | Throws unless `Actual` is a `bool` and `false`. |

Every failed check throws `InvalidOperationException`. The message ends with
`Context.FormatBecauseClause()`, and values in it are shown as `<null>` or `<value>`.

## `Expectantly.AndConstraint<TSelf>`

Sealed. Implements `IAndConstraint<TSelf>`, with a public constructor `AndConstraint(TSelf and)`.

| Member | Type | Notes |
|---|---|---|
| `And` | `TSelf` | Returns the wrapped assertion object, enabling `.Is(x).And.IsNotNull()`. |

## `Expectantly.Abstractions.IAssertion<out TActual>`

| Member | Type |
|---|---|
| `Actual` | `TActual` |
| `Context` | `IAssertionContext` |

## `Expectantly.Abstractions.IAssertionContext`

| Member | Type/Signature | Notes |
|---|---|---|
| `Because` | `string?` | Raw, unformatted reason text. |
| `BecauseArgs` | `IReadOnlyList<object?>` | Format arguments for `Because`. |
| `FormatBecauseClause()` | `string` | Returns `""` or `" because <formatted reason>"`. |

## `Expectantly.Abstractions.IAndConstraint<out TSelf>`

| Member | Type |
|---|---|
| `And` | `TSelf` |

## Internal (not part of the public contract)

- `Expectantly.Internal.AssertionContext`: the `IAssertionContext` implementation `Expect.That`
  uses. When `BecauseArgs` is non-empty, it formats `Because` with
  `string.Format(CultureInfo.InvariantCulture, ...)`.

## Known limitations

- **`IsSameAs` always fails for value types.** It calls `ReferenceEquals(Actual, instance)`, which
  boxes each value-type operand separately, so two equal structs never compare as the same
  reference. The failure message ("Expected references to match") doesn't say why. `TActual`
  can't be constrained to `class` on this one method; see
  [Core API design notes](design/core-api.md#overload-and-parameter-consistency).
- **A brace in `because` can hide the real failure.** When `becauseArgs` is non-empty, `because`
  goes through `string.Format`. A stray `{` or `}` in the reason throws `FormatException` while the
  failure message is being built, and that exception replaces the assertion failure.
