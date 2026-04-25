# Quickstart: Should() Type-Aware Dispatch

**Feature**: 00018-should-type-dispatch  
**Date**: 2026-04-25

This guide shows what changes for callers once the three new `.Should()` overloads
are in place.

---

## Read-Only Collections

### Before (broken)

```csharp
IReadOnlyList<string> names = GetNames();

// Compiler resolves to ObjectAssertions<IReadOnlyList<string>> — no collection methods!
names.Should().HaveCount(3); // ❌ CS1061: 'ObjectAssertions<...>' does not contain 'HaveCount'
```

### After (fixed)

```csharp
IReadOnlyList<string> names = GetNames(); // ["Alice", "Bob", "Charlie"]

names.Should().HaveCount(3);              // ✅
names.Should().Contain("Alice");          // ✅
names.Should().BeEmpty();                 // ✅ (fails here — 3 items — used for illustration)
names.Should().ContainSingle();           // ✅ (fails — 3 items)
names.Should().AllSatisfy(n => n.Length > 2);   // ✅
names.Should().BeEquivalentTo(["Alice", "Bob", "Charlie"]); // ✅
```

Same pattern applies to `IReadOnlyCollection<T>`:

```csharp
IReadOnlyCollection<int> ids = GetIds(); // [1, 2, 3]
ids.Should().HaveCount(3); // ✅
```

### Chaining with `.Which`

```csharp
IReadOnlyList<string> single = ["only-one"];

string item = single.Should()
    .ContainSingle().Which; // ✅ — .Which is the matched element

item.Should().StartWith("only"); // ✅ — further assertions on the element
```

---

## Non-Async Delegates (`Func<T>`)

### Before (broken)

```csharp
Func<string> riskyOp = () => throw new InvalidOperationException("boom");

// Compiler resolves to ObjectAssertions<Func<string>> — no Throw() method!
riskyOp.Should().Throw<InvalidOperationException>(); // ❌ CS1061
```

### After (fixed)

```csharp
Func<string> riskyOp = () => throw new InvalidOperationException("boom");

// Compiler resolves to ActionAssertions ✅
riskyOp.Should().Throw<InvalidOperationException>(); // ✅

// Chaining via .Which for further exception inspection
riskyOp.Should()
    .Throw<InvalidOperationException>().Which
    .Message.Should().Contain("boom"); // ✅

// Non-throwing case
Func<int> safeOp = () => 42;
safeOp.Should().NotThrow(); // ✅ — NotThrow() is terminal; return value is not exposed
```

### Null guard

```csharp
Func<string>? nullFunc = null;
nullFunc!.Should(); // ❌ throws ArgumentNullException immediately at .Should() call site
```

---

## Read-Only Dictionaries (Already Working)

`IReadOnlyDictionary<K,V>` already dispatches correctly in the existing codebase.
No change is needed for callers:

```csharp
IReadOnlyDictionary<string, int> scores = GetScores();

scores.Should().ContainKey("Alice"); // ✅ already worked before this feature
scores.Should().HaveCount(5);        // ✅
scores.Should().NotBeNull();         // ✅
```

---

## Writing New Tests (Pattern Reference)

```csharp
// Dispatch regression test pattern (ShouldDispatchTests.cs)
[Fact]
public void Should_IReadOnlyListSubject_ReturnsGenericCollectionAssertions()
{
    IReadOnlyList<string> subject = ["a"];
    var result = subject.Should();
    Assert.IsType<GenericCollectionAssertions<string>>(result);
}

[Fact]
public void Should_FuncTSubject_ReturnsActionAssertions()
{
    Func<string> subject = () => "value";
    var result = subject.Should();
    Assert.IsType<ActionAssertions>(result);
}
```

---

## Migration Notes

If your codebase called `ObjectAssertions`-only methods (e.g., `.BeNull()`,
`.BeOfType<T>()`) on a variable declared as `IReadOnlyList<T>`, `IReadOnlyCollection<T>`,
or `Func<T>`, those calls will now produce a **compile error** after this feature ships,
because the resolved assertion type changes from `ObjectAssertions<T>` to the correct
specialized type. Fix by using the appropriate assertion method on the new type, or
cast the subject to `object` before calling `.Should()` if you genuinely need
`ObjectAssertions` (rare; this should not occur in normal test code).
