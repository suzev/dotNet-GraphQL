using System;

namespace TwittorApp.Dtos
{
    public record InputTwittor
    (
        int UserId,
        string Post,
        DateTime? Created,
        DateTime? Updated        
    );
}
