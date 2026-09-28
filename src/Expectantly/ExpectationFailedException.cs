namespace Expectantly;

/// <summary>
/// The exception thrown when an expectation is not met.
/// </summary>
/// <remarks>
/// Test runners report it as a failed test. Tools that need the parts of the message, rather than
/// the text, can read <see cref="Failure"/>.
/// </remarks>
public sealed class ExpectationFailedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExpectationFailedException"/> class.
    /// </summary>
    /// <param name="failure">The failed expectation.</param>
    /// <exception cref="ArgumentNullException"><paramref name="failure"/> is <see langword="null"/>.</exception>
    public ExpectationFailedException(Failure failure)
        : base((failure ?? throw new ArgumentNullException(nameof(failure))).Message)
    {
        Failure = failure;
    }

    /// <summary>
    /// Gets the failed expectation that caused this exception.
    /// </summary>
    public Failure Failure { get; }
}
