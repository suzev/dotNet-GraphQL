using TwittorApp.Models;

namespace TwittorApp.Dtos
{
    public class ConfirmUserPayload
    {
        public ConfirmUserPayload (User user, UserRole userRole)
        {
            User = user;
            UserRole = userRole;
        }

        public User User { get; }
        public UserRole UserRole { get; }
    }
}
