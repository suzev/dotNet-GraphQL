using Confluent.Kafka;
using KafkaApp.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KafkaApp
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                    .AddJsonFile($"appsettings.json", true, true);

            var config = builder.Build();

            var ServerConfig = new ConsumerConfig
            {
                BootstrapServers = config["Settings:KafkaServer"],
                GroupId = "tester",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true; // prevent the process from terminating (Ctrl +C)
                cts.Cancel();
            };

            Console.WriteLine("----------------.NET Application-------------");
            using (var consumer = new ConsumerBuilder<string, string>(ServerConfig).Build())
            {
                Console.WriteLine("KafkaApp Connected");
                var topics = new string[] { "comment", "profile", "role", "twittor", "user", "userRole" };
                consumer.Subscribe(topics);

                Console.WriteLine("Waiting messages....");
                try
                {
                    while (true)
                    {
                        var cr = consumer.Consume(cts.Token);
                        Console.WriteLine($"Consumed record with Topic: {cr.Topic} key: {cr.Message.Key} and value: {cr.Message.Value}");

                        using (var dbcontext = new Kasus2DbContext())
                        {
                            if (cr.Topic == "comment")
                            {
                                Comment comment = JsonConvert.DeserializeObject<Comment>(cr.Message.Value);
                                dbcontext.Comments.Add(comment);
                            }
                            if (cr.Topic == "profile")
                            {
                                Profile profile = JsonConvert.DeserializeObject<Profile>(cr.Message.Value);
                                dbcontext.Profiles.Add(profile);
                            }
                            if (cr.Topic == "role")
                            {
                                Role role = JsonConvert.DeserializeObject<Role>(cr.Message.Value);
                                dbcontext.Roles.Add(role);
                            }
                            if (cr.Topic == "twittor")
                            {
                                Twittor twittor = JsonConvert.DeserializeObject<Twittor>(cr.Message.Value);
                                dbcontext.Twittors.Add(twittor);
                            }
                            if (cr.Topic == "user")
                            {
                                User user = JsonConvert.DeserializeObject<User>(cr.Message.Value);
                                dbcontext.Users.Add(user);
                            }
                            if (cr.Topic == "userRole")
                            {
                                UserRole userRole = JsonConvert.DeserializeObject<UserRole>(cr.Message.Value);
                                dbcontext.UserRoles.Add(userRole);
                            }

                            await dbcontext.SaveChangesAsync();
                            Console.WriteLine("Data was saved into database");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Ctrl+C was pressed.
                }
                finally
                {
                    consumer.Close();
                }

            }

            return 1;

        }
    }
}
