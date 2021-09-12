using System;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Publisher
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
                for (int i = 0; i < 50; i++)
                {
                    var message = GetMessage(i);
                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(
                        exchange: "",
                        routingKey: "task_queue",
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
