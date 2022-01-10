using TwittorApp.Models;

namespace TwittorApp.Dtos
{
    public class InputCommentPayload
    {
        public InputCommentPayload(Comment comment)
        {
            Comment = comment;
        }

        public Comment Comment { get; }
    }
}
