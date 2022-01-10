using TwittorApp.Models;

namespace TwittorApp.Dtos
{
    public class InputTwittorPayload
    {
        public InputTwittorPayload(Twittor twittor)
        {
            Twittor = twittor;
        }

        public Twittor Twittor { get; }
    }
}
