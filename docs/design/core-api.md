# Core API design notes

## Naming conventions

Expectantly follows a predictable fluent naming pattern:

- `IsX` for state/value predicates (`Is`, `IsNull`, `IsTrue`).
- `HasX` for structural or property ownership (for collections/entities: `HasCount`, `HasId`, `HasItem`).
- `ContainsX`/`Contains` for membership/content checks where a containment verb reads best.

## Return types and fluent chaining

Assertion methods that represent a check and then continue the same assertion surface return
`AndConstraint<TAssertion>`. This keeps IntelliSense focused on discoverable follow-up methods while
making chain intent explicit (`.Is(...).And.IsNot(...)`).

Methods may return assertion types directly only when changing to a different assertion surface
(for example, projecting to a dedicated collection assertion object).

## Overload and parameter consistency

- Public entry points and cross-cutting assertion methods should expose optional `because` and
  `becauseArgs` parameters in the same order.
- Deferred execution overloads (`Func<T>`) should mirror direct value overloads and preserve context.
- Generic constraints should be used when they improve compile-time guidance (`IsSameAs` requires
  reference types).
- Nullable annotations should always reflect the intended contract for null acceptance and null return.
