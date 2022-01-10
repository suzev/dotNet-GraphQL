using TwittorApp.Models;

namespace TwittorApp.Dtos
{
    public class InputUserRolePayload
    {
        public InputUserRolePayload(UserRole userRole)
        {
            UserRole = userRole;
        }

        public UserRole UserRole { get; }
    }
}
