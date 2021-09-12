using System;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BackgroundWorker
{
    public class RabbitMQSubscriber : BackgroundService
    {
        private readonly ILogger<RabbitMQSubscriber> _logger;
        private IConnection _connection;
        private IModel _channel;
        private string _consumerTag;
        // private BlockingCollection<MyTask>[] queues = new BlockingCollection<MyTask>[1 + 2] {
        //     null,
        //     new BlockingCollection<MyTask>(),
        //     new BlockingCollection<MyTask>()
        // };
        private ConcurrentQueue<MyTask>[] queues = new ConcurrentQueue<MyTask>[1 + 2] {
            null,
            new ConcurrentQueue<MyTask>(),
            new ConcurrentQueue<MyTask>()
        };
        private Func<IDoTask>[] methods = new Func<IDoTask>[1 + 2] {
            null,
            ()=> new MyTask1(),
            ()=> new MyTask2()
        };
        public RabbitMQSubscriber(ILogger<RabbitMQSubscriber> logger)
        {
            _logger = logger;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory()
            // env
            {
                UserName = "root",
                Password = "admin1234",
                VirtualHost = "/",
                HostName = "localhost"
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                queue: "task_queue",
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
            _channel.BasicQos(
                prefetchSize: 0,
                prefetchCount: 1,
                global: false
            );
            _logger.LogInformation(" [*] Waiting for messages...");
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (sender, m) =>
            {
                var body = m.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                MyTask task = JsonSerializer.Deserialize<MyTask>(message);
                // this.queues[task.type].Add(task); // add task into queue based on their type
                this.queues[task.type].Enqueue(task);
                _logger.LogInformation($" [x] Done: adding task {task.type} into queue");
                // launching ackowledgment
                _channel.BasicAck(
                    deliveryTag: m.DeliveryTag,
                    multiple: false
                );
                // this.StartRun(); // start threads
            };
            
            _consumerTag = _channel.BasicConsume(
                queue: "task_queue",
                autoAck: false,
                consumer: consumer
            );
            this.StartRun(); // start threads
            // Thread.Sleep(8000);
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _channel.BasicCancel(_consumerTag);
            _channel.Close();
            _connection.Close();
            // for (int type = 1; type <= 2; type++)
            // {
            //     this.queues[type].CompleteAdding();
            // }
            return base.StopAsync(cancellationToken);
        }
        private void StartRun()
        {
            List<Task> tasks = new List<Task>();
            int[] counts = { 0, 2, 3 }; // thread pool
            Parallel.For(1, counts.Length, (type) =>
            {
                for (int i = 0; i < counts[type]; i++)
                {
                    Task t = Task.Run(() => { this.DoAllType(type); });
                    tasks.Add(t);
                }
            });
            foreach (var t in tasks) t.Wait();
        }
        private void DoAllType(int type)
        {
            // foreach (var task in this.queues[type].GetConsumingEnumerable()) // 有問題
            // {
            //     // this.methods[type]().doTask(task.name, task.id);
            // };
            // Console.WriteLine($"new {type}");

            while (!this.queues[type].IsEmpty)
            {
                if (this.queues[type].TryDequeue(out MyTask task))
                {
                    this.methods[type]().doTask(task.name, task.id);
                }
            }
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