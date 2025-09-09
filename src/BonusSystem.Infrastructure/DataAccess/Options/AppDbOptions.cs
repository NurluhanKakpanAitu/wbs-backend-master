namespace BonusSystem.Infrastructure.DataAccess;

/// <summary>
/// Configuration options for the database
/// </summary>
public class AppDbOptions
{
    public const string Position = "AppDb";

    /// <summary>
    /// Type of database to use
    /// </summary>
    public string Type { get; set; } = "InMemory";

    /// <summary>
    /// Connection string for the database
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// JWT secret key for token generation and validation
    /// </summary>
    public string JwtSecret { get; set; } = string.Empty;

    /// <summary>
    /// JWT token expiration time in minutes
    /// </summary>
    public int TokenExpirationMinutes { get; set; } = 60;

    public Guid SystemAccountId { get; set; } = Guid.Parse("00000000-0000-0000-0000-000000000001"); 
}