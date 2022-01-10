using System;

namespace TwittorApp.Dtos
{
    public record InputComment
    (
      int UserId,
      int TwittorId,
      string? Comment1,
      DateTime? Created,
      DateTime? Updated
    );
}
