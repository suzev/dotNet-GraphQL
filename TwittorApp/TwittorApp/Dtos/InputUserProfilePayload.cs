using TwittorApp.Models;

namespace TwittorApp.Dtos
{
    public class InputUserProfilePayload
    {
        public InputUserProfilePayload(Profile profile)
        {
            Profile = profile;
        }

        public Profile Profile { get; }
    }
}
