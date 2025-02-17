namespace ScientificBit.MongoDb.Entities.Common;

public class ActivityUserInfo
{
    public string? UserId { get; set; }

    public string? UserName { get; set; }

    public string? DisplayName { get; set; }

    public string? UserType { get; set; }

    public string[] Roles { get; set; } = new string[] {};

    public string? TenantId { get; set; }

    public string GetUserName() => !string.IsNullOrEmpty(UserName) ? UserName : UserId ?? $"{TenantId}/{DisplayName}";
}