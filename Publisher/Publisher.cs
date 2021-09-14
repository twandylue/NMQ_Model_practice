using System;
using System.IO;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace Publisher
{
    class Program
    {
        static void Main(string[] args)
        {
            var root = Directory.GetCurrentDirectory();
            var dotenv = Path.Combine(root.ToString(), ".env");
            DotEnv.Load(dotenv); // loading environment variable

            var factory = new ConnectionFactory()
            {
                UserName = Environment.GetEnvironmentVariable("RabbitMQ_UserName"),
                Password = Environment.GetEnvironmentVariable("RabbitMQ_Password"),
                VirtualHost = Environment.GetEnvironmentVariable("RabbitMQ_VirtualHost"),
                HostName = Environment.GetEnvironmentVariable("RabbitMQ_HostName")
            };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(
                    queue: Environment.GetEnvironmentVariable("RabbitMQ_Queue"),
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );
                for (int i = 0; i < 50; i++)
                {
                    var message = GetMessage(i);
                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(
                        exchange: "",
                        routingKey: Environment.GetEnvironmentVariable("RabbitMQ_Queue"),
                        basicProperties: null,
                        body: body
                    );
                    Console.WriteLine(" [x] Sent {0}", message);
                }

                Console.WriteLine(" Press [enter] to exit");
                Console.ReadLine();
            }
        }
        private static string GetMessage(int index)
        {
            MyTask task = new MyTask();
            task.id = index;
            if (index % 2 == 0)
            {
                task.type = 1;
                task.name = "Task 1";
            }
            else
            {
                task.type = 2;
                task.name = "Task 2";
            }
            string jsonString = JsonSerializer.Serialize<MyTask>(task);
            return jsonString;
        }
    }

    class MyTask
    {
        public int id { get; set; }
        public int type { get; set; }
        public string name { get; set; }
    }
}
