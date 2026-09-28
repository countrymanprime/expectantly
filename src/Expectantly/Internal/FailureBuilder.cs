namespace Expectantly.Internal;

/// <summary>
/// Collects the parts of a <see cref="Failure"/> sentence. Values passed to <see cref="Found(object?)"/>
/// are formatted by <see cref="ValueFormatter"/>; phrases are used as written.
/// </summary>
internal sealed class FailureBuilder
{
    private readonly string _subject;
    private readonly string? _because;
    private readonly List<string> _details = [];
    private string _expectation = "to pass";
    private string _outcome = "it did not";
    private string? _which;

    public FailureBuilder(string? expression, string? because)
    {
        _subject = Expressions.Subject(expression);
        _because = Reasons.Normalize(because);
    }

    public FailureBuilder Expected(string expectation)
    {
        _expectation = expectation;
        return this;
    }

    public FailureBuilder Found(object? value) => But("found " + ValueFormatter.Format(value));

    public FailureBuilder But(string outcome)
    {
        _outcome = outcome;
        return this;
    }

    public FailureBuilder Which(string? difference)
    {
        _which = difference;
        return this;
    }

    public FailureBuilder Details(IEnumerable<string> lines)
    {
        _details.AddRange(lines);
        return this;
    }

    public Failure Build() => new(_subject, _expectation, _because, _outcome, _which, _details.ToArray());
}
