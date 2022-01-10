using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaApp.Models
{
    internal class UserData
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool IsLock { get; set; }
    }
}
