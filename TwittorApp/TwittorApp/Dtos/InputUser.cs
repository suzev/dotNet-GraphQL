using System;

namespace TwittorApp.Dtos
{
    public record InputUser
    (
        string? FullName,
        string? Email,
        string? Username,
        string? Password,
        DateTime? Created,
        DateTime? Update,
        bool IsLocked
    );
}
