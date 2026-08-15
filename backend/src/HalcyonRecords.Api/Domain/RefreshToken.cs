namespace HalcyonRecords.Api.Domain;

public class RefreshToken
{
    public int Id { get; set; }
    public required string TokenHash { get; set; }
    public int UserId { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public int? ReplacedByTokenId { get; set; }
}
