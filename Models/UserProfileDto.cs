namespace BADEAPORTAL.Models
{
    public sealed class UserProfileDto
    {
        // Formal name: "First Last" when available
        public string? FullName { get; init; }

        // Display name from Entra ID (fallback)
        public string? DisplayName { get; init; }

        // Useful identifier (often UPN/email)
        public string? EmailOrUpn { get; init; }
    }
}
