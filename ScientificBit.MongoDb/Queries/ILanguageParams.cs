namespace ScientificBit.MongoDb.Queries;

/// <summary>
/// Defines params for locale support
/// </summary>
public interface ILanguageParams
{
    /// <summary>
    /// Language code
    /// </summary>
    string? Lang { get; }
}