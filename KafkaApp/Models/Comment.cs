using System;
using System.Collections.Generic;

#nullable disable

namespace KafkaApp.Models
{
    public partial class Comment
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int TwittorId { get; set; }
        public string Comment1 { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public virtual Twittor Twittor { get; set; }
    }
}
