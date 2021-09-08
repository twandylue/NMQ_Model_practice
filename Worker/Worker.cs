using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Worker
{
    class Program
    {
        static void Main(string[] args)
        {
            new MyRabbitMQ().getMessage();
            Console.WriteLine("DONE!");
        }
    }

    class MyRabbitMQ
    {
        public void getMessage()
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
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );
                channel.BasicQos(
                    prefetchSize: 0,
                    prefetchCount: 1,
                    global: false
                );
                Console.WriteLine(" [*] Waiting for messages.");
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    MyTask task = JsonSerializer.Deserialize<MyTask>(message);
                    this.queue.Add(task); // put task in queue
                    Console.WriteLine(" [x] Done: putting job in queue");
                    channel.BasicAck(
                        deliveryTag: ea.DeliveryTag,
                        multiple: false
                    );
                };
                channel.BasicConsume(
                    queue: "task_queue",
                    autoAck: false,
                    consumer: consumer
                );
                StartRun(); // Start threads 
                Console.WriteLine(" Press [enter] to exit");
                Console.ReadLine();
            }
        }
        private void StartRun()
        {
            // open 10 thread
            for (int i = 0; i < 5; i++)
            {
                Task.Run(() => { this.DoAllType(); });
            }
        }

        private BlockingCollection<MyTask> queue = new BlockingCollection<MyTask>();

        private void DoAllType()
        {
            foreach (var task in this.queue.GetConsumingEnumerable())
            {
                if (task.type != 1 && task.type != 2)
                {
                    Console.WriteLine("Wrong Task type.");
                    return;
                }
                Console.WriteLine($"Execute type {task.type} task.");
                if (task.type == 1) Thread.Sleep(2000);
                if (task.type == 2) Thread.Sleep(5000);
            }
            // this.queue.CompleteAdding();
        }
    }

    class MyTask
    {
        public string name { get; set; }
        public int type { get; set; }
    }
}
