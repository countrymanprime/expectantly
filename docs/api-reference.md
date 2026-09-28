# API reference

Everything public in `Expectantly` today. There's no generated API site yet, so this page is
maintained by hand. Update it in the same PR as any change to the public surface; the build also
checks the surface against [`PublicAPI.Unshipped.txt`](../src/Expectantly/PublicAPI.Unshipped.txt).

## `Expectantly.Expect` (static)

| Member | Signature | Notes |
|---|---|---|
| `That<T>` | `ObjectAssertions<T> That<T>(T actual, [CallerArgumentExpression] string? expression = null)` | Wraps a value. The compiler supplies `expression`, the source text of `actual`, which failure messages use to name it. Don't pass it yourself. |

## `Expectantly.ObjectAssertions<TActual>`

Implements `IAssertion<TActual>`. `Expect.That` is the intended way to create one, but the class
is public, unsealed, and has a public constructor
`ObjectAssertions(TActual actual, string? expression = null)`.

Every check takes an optional `string? because` reason, which the failure message includes. Checks
that take a value also take a `[CallerArgumentExpression]` parameter for it, which the compiler
supplies. Every failed check throws `ExpectationFailedException`.

| Member | Returns | Behavior |
|---|---|---|
| `Actual` | `TActual` | The value under test. |
| `Expression` | `string?` | The source text of the value under test, or `null` if unknown. |
| `Is(TActual expected, string? because = null)` | `AndConstraint<ObjectAssertions<TActual>>` | Fails unless `EqualityComparer<TActual>.Default.Equals(Actual, expected)`. |
| `Is(TActual expected, IEqualityComparer<TActual> comparer, string? because = null)` | `AndConstraint<ObjectAssertions<TActual>>` | Fails unless `comparer.Equals(Actual, expected)`. Throws `ArgumentNullException` if `comparer` is `null`. |
| `IsNot(TActual unexpected, string? because = null)` | `AndConstraint<ObjectAssertions<TActual>>` | Fails if the values are equal by the default comparer. |
| `IsNot(TActual unexpected, IEqualityComparer<TActual> comparer, string? because = null)` | `AndConstraint<ObjectAssertions<TActual>>` | Fails if `comparer.Equals(Actual, unexpected)`. Throws `ArgumentNullException` if `comparer` is `null`. |
| `IsNull(string? because = null)` | `AndConstraint<ObjectAssertions<TActual>>` | Fails unless `Actual` is `null`. |
| `IsNotNull(string? because = null)` | `AndWhichConstraint<ObjectAssertions<TActual>, TActual>` | Fails if `Actual` is `null`. `.Which` is the value. |
| `IsSameInstanceAs(TActual expected, string? because = null)` | `AndConstraint<ObjectAssertions<TActual>>` | Fails unless `ReferenceEquals(Actual, expected)`. Always fails when `TActual` is a value type, with a message that explains why. |
| `IsNotSameInstanceAs(TActual unexpected, string? because = null)` | `AndConstraint<ObjectAssertions<TActual>>` | Fails if `ReferenceEquals(Actual, unexpected)`. Always passes when `TActual` is a value type. |
| `IsAssignableTo<TExpected>(string? because = null)` | `AndWhichConstraint<ObjectAssertions<TActual>, TExpected>` | Fails unless `Actual is TExpected`. `.Which` is the value as a `TExpected`. |
| `IsTrue(string? because = null)` | `AndConstraint<ObjectAssertions<TActual>>` | Fails unless `Actual` is the `bool` `true`. |
| `IsFalse(string? because = null)` | `AndConstraint<ObjectAssertions<TActual>>` | Fails unless `Actual` is the `bool` `false`. |

## Failure messages

A failure message is one sentence:

```text
Expected <subject> <expectation> [because <reason>], but <outcome>[, which <difference>].
```

- **Subject** is the expression passed to `Expect.That`, or `value` when it was a literal.
- **Expected values** are shown by value, with their expression when it names something:
  `to be expectedTotal (43)`.
- **Values** are formatted culture-invariantly: strings quoted and escaped, `null`, `true`/`false`,
  enums as `Type.Member`, types in C# spelling, collections as `[a, b, c]` (at most 10 items, then
  `… (n more)`). Strings longer than 100 characters are shortened around the difference and followed
  by their length.
- **String differences** are named: a missing or extra ending, a difference only in case, or the
  index of the first difference, followed by a pointer:

  ```text
  Expected name to be "Victoria", but found "Vic toria", which differs at index 3:
      "Vic toria"
          ↑
  ```

- **Look-alikes**: when both values print the same, the message says so and names the types:
  `but found 1, which looks the same but is a long, not an int.`

## `Expectantly.ExpectationFailedException`

Sealed; derives from `Exception`.

| Member | Type/Signature | Notes |
|---|---|---|
| constructor | `ExpectationFailedException(Failure failure)` | Throws `ArgumentNullException` if `failure` is `null`. `Message` is `failure.Message`. |
| `Failure` | `Failure` | The failed expectation. |

## `Expectantly.Failure`

Sealed, with no public constructor. Its properties are the parts of the failure sentence.

| Member | Type | Example |
|---|---|---|
| `Subject` | `string` | `order.Total` |
| `Expectation` | `string` | `to be 43` |
| `Because` | `string?` | `tax is included` |
| `Outcome` | `string` | `found 42` |
| `Which` | `string?` | `differs at index 3` |
| `Details` | `IReadOnlyList<string>` | Lines after the sentence; usually empty. |
| `Message` | `string` | The composed message. Lines are joined with `\n`. `ToString()` returns it too. |

## `Expectantly.AndConstraint<TSelf>`

Sealed. Implements `IAndConstraint<TSelf>`, with a public constructor `AndConstraint(TSelf and)`.

| Member | Type | Notes |
|---|---|---|
| `And` | `TSelf` | Returns the wrapped assertion object, enabling `.Is(x).And.IsNotNull()`. |

## `Expectantly.AndWhichConstraint<TSelf, TValue>`

Sealed. Implements `IAndConstraint<TSelf>`, with a public constructor
`AndWhichConstraint(TSelf and, TValue which)`. Returned by checks that narrow the value.

| Member | Type | Notes |
|---|---|---|
| `And` | `TSelf` | Continues on the same subject. |
| `Which` | `TValue` | The narrowed value. |

## `Expectantly.Abstractions.IAssertion<out TActual>`

| Member | Type |
|---|---|
| `Actual` | `TActual` |
| `Expression` | `string?` |

## `Expectantly.Abstractions.IAndConstraint<out TSelf>`

| Member | Type |
|---|---|
| `And` | `TSelf` |

## Known limitations

- **Every check is available on every type.** `IsTrue` compiles on a `string` and fails at run
  time. Type-specific assertion surfaces come in PRD milestone M1.
- **`IsNotNull().Which` keeps the declared nullability.** For a `string?` subject, `.Which` is still
  `string?`, so the compiler warns on dereference. PRD requirement OBJ-3 (M1) addresses it.
- **Comparer-based string checks diff by ordinal characters.** `Is(x, StringComparer.OrdinalIgnoreCase)`
  uses the comparer to decide equality, but the pointer in the message still marks the first ordinal
  difference.
- **Passing a reason in `Expect.That`'s second argument doesn't do what it looks like.**
  `Expect.That(x, "reason")` compiles, but `"reason"` becomes the expression that names the subject.
  Pass reasons to the check instead: `Expect.That(x).Is(1, because: "reason")`.
