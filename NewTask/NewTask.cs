using System;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace NewTask
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory()
            {
                UserName = "root",
                Password = "admin1234",
                VirtualHost = "/",
                HostName = "localhost"
            };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(
                    queue: "task_queue",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                for (int i = 0; i < 20; i++)
                {
                    var message = GetMessage(args, i);
                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(
                        exchange: "",
                        routingKey: "task_queue",
                        basicProperties: null,
                        body: body
                    );
                    Console.WriteLine(" [x] Sent {0}", message);
                }
            }

            Console.WriteLine("Press [enter] to exit");
            Console.ReadLine();
        }
        private static string GetMessage(string[] args, int index)
        {
            MyTask task = new MyTask();
            if (index % 2 == 0)
            {
                task.name = "Logging";
                task.type = 1;
            }
            else
            {
                task.name = "Eating";
                task.type = 2;
            }
            string jsonString = JsonSerializer.Serialize<MyTask>(task);
            return ((args.Length > 0) ? string.Join(" ", args) : jsonString);
        }
    }

    class MyTask
    {
        public string name { get; set; }
        public int type { get; set; }
    }
}
