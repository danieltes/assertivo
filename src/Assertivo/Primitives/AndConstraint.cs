namespace Assertivo.Primitives;

/// <summary>
/// Continuation type providing <see cref="And"/> for fluent chaining.
/// </summary>
public readonly struct AndConstraint<TAssertions>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AndConstraint{TAssertions}"/> struct.
    /// </summary>
    public AndConstraint(TAssertions parent)
    {
        And = parent;
    }

    /// <summary>Gets the parent assertion object for continued chaining.</summary>
    public TAssertions And { get; }
}
