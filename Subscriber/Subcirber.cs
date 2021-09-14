using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Subscriber
{
    class Program
    {
        static void Main(string[] args)
        {
            var root = Directory.GetCurrentDirectory();
            var dotenv = Path.Combine(root.ToString(), ".env");
            DotEnv.Load(dotenv); // loading environment variable

            new RabbitMQSubscriber().getMessage();
            Console.WriteLine(" DONE!");
        }
    }
    class RabbitMQSubscriber
    {
        private BlockingCollection<MyTask>[] queues = new BlockingCollection<MyTask>[1 + 2] {
            null,
            new BlockingCollection<MyTask>(),
            new BlockingCollection<MyTask>()
        };
        private Func<IDoTask>[] methods = new Func<IDoTask>[1 + 2] {
            null,
            ()=> new MyTask1(),
            ()=> new MyTask2()
        };

        public void getMessage()
        {
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
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );
                // wating for cosuming
                channel.BasicQos(
                    prefetchSize: 0,
                    prefetchCount: 1,
                    global: false
                );
                Console.WriteLine(" [*] Waiting for messages...");
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, m) =>
                {
                    var body = m.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    MyTask task = JsonSerializer.Deserialize<MyTask>(message);
                    this.queues[task.type].Add(task); // add task into queue based on their type
                    Console.WriteLine($"[x] Done: adding task {task.type} into queue");
                    // launching ackowledgment
                    channel.BasicAck(
                        deliveryTag: m.DeliveryTag,
                        multiple: false
                    );
                };
                channel.BasicConsume(
                    queue: Environment.GetEnvironmentVariable("RabbitMQ_Queue"),
                    autoAck: false,
                    consumer: consumer
                );
                this.StartRun(); // Start Threads
            }
        }
        private void StartRun()
        {
            List<Task> tasks = new List<Task>();
            int[] counts = { 0, 2, 3 }; // thread pool
            for (int type = 1; type <= 2; type++)
            {
                Parallel.For(1, counts.Length, (type) =>
                {
                    for (int i = 0; i < counts[type]; i++)
                    {
                        Task t = Task.Run(() => { this.DoAllType(type); });
                        tasks.Add(t);
                    }
                });
            }
            foreach (var t in tasks) t.Wait();
        }
        private void DoAllType(int type)
        {
            foreach (var task in this.queues[type].GetConsumingEnumerable())
            {
                this.methods[type]().doTask(task.name, task.id);
            };
        }
    }
    class MyTask
    {
        public int id { get; set; }
        public int type { get; set; }
        public string name { get; set; }
    }
    class MyTask1 : IDoTask
    {
        public void doTask(string name, int id)
        {
            Console.WriteLine($"Doing Task :{name} | TaskID: {id}");
            // do something in Task 1...
            Thread.Sleep(5000);
        }
    }
    class MyTask2 : IDoTask
    {
        public void doTask(string name, int id)
        {
            Console.WriteLine($"Doing Task :{name} | TaskID: {id}");
            // do something in Task 2...
            Thread.Sleep(3000);
        }
    }
    interface IDoTask
    {
        void doTask(string name, int id);
    }
}
