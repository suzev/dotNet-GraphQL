using TwittorApp.Models;

namespace TwittorApp.Dtos
{
    public class InputUserPayload
    {
        public InputUserPayload(User user)
        {
            User = user;
        }

        public User User { get; }
    }
}
