namespace Assertivo.Primitives;

/// <summary>
/// Continuation type providing <see cref="And"/> and <see cref="Which"/> for fluent drill-down chaining.
/// </summary>
public readonly struct AndWhichConstraint<TAssertions, TSubject>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AndWhichConstraint{TAssertions, TSubject}"/> struct.
    /// </summary>
    public AndWhichConstraint(TAssertions parent, TSubject subject)
    {
        And = parent;
        Which = subject;
    }

    /// <summary>Gets the parent assertion object for continued chaining.</summary>
    public TAssertions And { get; }
    /// <summary>Gets the extracted sub-subject for further assertions.</summary>
    public TSubject Which { get; }
}
