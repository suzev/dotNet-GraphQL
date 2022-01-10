﻿using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;

namespace KafkaListeningApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                   .AddJsonFile($"appsettings.json", true, true);

            var config = builder.Build();

            var ServerConfig = new ConsumerConfig
            {
                // BootstrapServers = config["Settings:KafkaServer"],
                // GroupId = "tester",
                // AutoOffsetReset = AutoOffsetReset.Earliest
                BootstrapServers = config["Settings:KafkaServer"],
                GroupId = "tester",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            var topic = "logging";
            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) => {
                e.Cancel = true; // prevent the process from terminating (Ctrl+C)
                cts.Cancel();
            };

            using (var consumer = new ConsumerBuilder<string, string>(ServerConfig).Build())
            {
                Console.WriteLine("KafkaListeningApp");
                consumer.Subscribe(topic);
                Console.WriteLine("Waiting messages....");
                try
                {
                    while (true)
                    {
                        var cr = consumer.Consume(cts.Token);
                        Console.WriteLine($"Consumed record with key: {cr.Message.Key} and value: {cr.Message.Value}");
                    }
                }
                catch (OperationCanceledException)
                {
                    // Ctrl-C was pressed.
                }
                finally
                {
                    consumer.Close();
                }

            }
        }
    }
}
