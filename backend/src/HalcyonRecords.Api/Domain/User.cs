using Microsoft.AspNetCore.Identity;

namespace HalcyonRecords.Api.Domain;

public class User : IdentityUser<int>
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public bool IsShowcaseAccount { get; set; }
    public DateTimeOffset RegisteredAt { get; set; }
    public DateTimeOffset LastActiveAt { get; set; }
    public Guid PublicId { get; set; } = Guid.NewGuid();
}
