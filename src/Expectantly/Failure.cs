using System.Text;

namespace Expectantly;

/// <summary>
/// Describes a failed expectation as the parts of one readable sentence:
/// <c>Expected &lt;subject&gt; &lt;expectation&gt; [because &lt;reason&gt;], but &lt;outcome&gt;[, which &lt;difference&gt;].</c>
/// </summary>
/// <remarks>
/// <see cref="Message"/> joins the parts. Detail lines follow the sentence only for information a
/// sentence cannot hold, such as a pointer into a string; they never restate the expected or found value.
/// </remarks>
public sealed class Failure
{
    internal Failure(
        string subject,
        string expectation,
        string? because,
        string outcome,
        string? which,
        IReadOnlyList<string> details)
    {
        Subject = subject;
        Expectation = expectation;
        Because = because;
        Outcome = outcome;
        Which = which;
        Details = details;
        Message = Compose();
    }

    /// <summary>
    /// Gets what was checked: the source expression passed to <see cref="Expect.That{T}(T, string?)"/>,
    /// or <c>value</c> when no expression is available or it was a literal.
    /// </summary>
    public string Subject { get; }

    /// <summary>
    /// Gets what was expected, as a phrase that follows the subject, for example <c>to be 43</c>.
    /// </summary>
    public string Expectation { get; }

    /// <summary>
    /// Gets the reason supplied through a <c>because</c> argument, or <see langword="null"/> when none was given.
    /// </summary>
    public string? Because { get; }

    /// <summary>
    /// Gets what happened instead, as a phrase that follows <c>but</c>, for example <c>found 42</c>.
    /// </summary>
    public string Outcome { get; }

    /// <summary>
    /// Gets a phrase that names the difference and follows <c>which</c>, for example <c>differs at index 3</c>,
    /// or <see langword="null"/> when the outcome says enough.
    /// </summary>
    public string? Which { get; }

    /// <summary>
    /// Gets extra lines shown after the sentence. The list is empty for most failures.
    /// </summary>
    public IReadOnlyList<string> Details { get; }

    /// <summary>
    /// Gets the complete failure message: the sentence, followed by any detail lines indented four spaces.
    /// Lines are always separated by <c>\n</c>, whatever the operating system.
    /// </summary>
    public string Message { get; }

    /// <inheritdoc />
    public override string ToString() => Message;

    private string Compose()
    {
        var message = new StringBuilder("Expected ")
            .Append(Subject)
            .Append(' ')
            .Append(Expectation);

        if (Because is not null)
        {
            message.Append(" because ").Append(Because);
        }

        message.Append(", but ").Append(Outcome);

        if (Which is not null)
        {
            message.Append(", which ").Append(Which);
        }

        message.Append(Details.Count > 0 ? ':' : '.');

        foreach (var detail in Details)
        {
            message.Append('\n').Append("    ").Append(detail);
        }

        return message.ToString();
    }
}
