using System;

namespace TwittorApp.Models
{
    public class UserData
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
