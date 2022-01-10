using System;

namespace TwittorApp.Dtos
{
    public record InputComment
    (
      int TwittorId,
      int UserId,
      string Comment1,
      DateTime? Created,
      DateTime? Updated
    );
}
