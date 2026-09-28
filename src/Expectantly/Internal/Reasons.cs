namespace Expectantly.Internal;

/// <summary>Cleans up a <c>because</c> reason so it reads naturally inside the failure sentence.</summary>
internal static class Reasons
{
    private const string Word = "because";

    /// <summary>
    /// Trims the reason, drops a leading <c>because</c> so it is never doubled, and drops trailing
    /// full stops. Returns <see langword="null"/> when nothing is left.
    /// </summary>
    public static string? Normalize(string? because)
    {
        var reason = because?.Trim();
        if (string.IsNullOrEmpty(reason))
        {
            return null;
        }

        if (reason!.StartsWith(Word, StringComparison.OrdinalIgnoreCase)
            && (reason.Length == Word.Length || char.IsWhiteSpace(reason[Word.Length])))
        {
            reason = reason.Substring(Word.Length).TrimStart();
        }

        reason = reason.TrimEnd('.', ' ');
        return reason.Length == 0 ? null : reason;
    }
}
