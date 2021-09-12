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

                    this.queues[task.type].Add(task); // put task in queue

                    Console.WriteLine($" [x] Done: putting task {task.type} in queue");
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
                this.StartRun(); // Start threads 
                Console.WriteLine(" Press [enter] to exit");
                Console.ReadLine();
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

        private BlockingCollection<MyTask>[] queues = new BlockingCollection<MyTask>[1 + 2]{
            null,
            new BlockingCollection<MyTask>(), // type 1
            new BlockingCollection<MyTask>() // type 2
        };

        private Func<IDoTask>[] TaskType = new Func<IDoTask>[1 + 2] {
            null,
            () => new MyTask1(),
            () => new MyTask2()
        };

        private void DoAllType(int type)
        {
            foreach (var task in this.queues[type].GetConsumingEnumerable())
            {
                this.TaskType[type]().finishTask(task.name, task.id);
            }
        }
    }

    class MyTask
    {
        public int id { get; set; }
        public string name { get; set; }
        public int type { get; set; }
    }

    class MyTask1 : IDoTask
    {
        public void finishTask(string name, int id)
        {
            Console.WriteLine($"Doing Task name: {name}...");
            Console.WriteLine($"Doing Task ID: {id}...");
            // do something about Task 1...
            Thread.Sleep(5000);
        }
    }

    class MyTask2 : IDoTask
    {
        public void finishTask(string name, int id)
        {
            Console.WriteLine($"Doing Task name: {name}...");
            Console.WriteLine($"Doing Task ID: {id}...");
            // do something about Task 2...
            Thread.Sleep(12000);
        }
    }

    interface IDoTask
    {
        void finishTask(string name, int id);
    }
}
