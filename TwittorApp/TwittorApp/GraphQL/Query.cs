using HotChocolate;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;
using TwittorApp.Kafka;
using TwittorApp.Models;

namespace TwittorApp.GraphQL
{
    public class Query
    {
        public async Task<IQueryable<Twittor>> GetTwits(
            [Service] Kasus2DbContext context,
            [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var key = "GetAllTwit-" + DateTime.Now.ToString();
            var val = JObject.FromObject(new { Message = "GraphQL Query GetAllTwit" }).ToString(Formatting.None);

            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            return context.Twittors;
        }
            

        public IQueryable<Twittor> GetTwitsByUserId(
            [Service] Kasus2DbContext context,
            int userId, 
            [Service] IOptions<KafkaSettings> kafkaSettings)
            {
                var key = "GetTwitByUserId-" + DateTime.Now.ToString();
                var val = JObject.FromObject(new { Message = "GraphQL Query GetTwitByUserId" }).ToString(Formatting.None);

                _ = KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

                var twittors = context.Twittors.Where(p => p.UserId == userId);
                return twittors;
            }

        public IQueryable<Comment> GetCommentByTwittorId(
            [Service] Kasus2DbContext context,
            int twittorId,
            [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var key = "GetCommentByTwitId-" + DateTime.Now.ToString();
            var val = JObject.FromObject(new { Message = "GraphQL Query GetCommentByTwitId" }).ToString(Formatting.None);

            _ = KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var comments = context.Comments.Where(c => c.TwittorId == twittorId);

            return comments;
        }

        public IQueryable<UserData> GetUsers(
            [Service] Kasus2DbContext context,
            [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var key = "GetAllUser-" + DateTime.Now.ToString();
            var val = JObject.FromObject(new { Message = "GraphQL Query GetAllUser" }).ToString(Formatting.None);

            _ = KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var users = context.Users.Select(p => new UserData()
            {
                Id = p.Id,
                FullName = p.FullName,
                Email = p.Email,
                Username = p.Username
            });
            return users;
        }
    }
}
