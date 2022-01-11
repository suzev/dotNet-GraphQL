using System;
using System.Collections.Generic;

#nullable disable

namespace TwittorApp.Models
{
    public partial class Twittor
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Post { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public virtual User User { get; set; }
    }
}
