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
        public RabbitMQSubscriber(ILogger<RabbitMQSubscriber> logger)
        {
            _logger = logger;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory()
            {
                UserName = Environment.GetEnvironmentVariable("RabbitMQ_UserName"),
                Password = Environment.GetEnvironmentVariable("RabbitMQ_Password"),
                VirtualHost = Environment.GetEnvironmentVariable("RabbitMQ_VirtualHost"),
                HostName = Environment.GetEnvironmentVariable("RabbitMQ_HostName")
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(
                queue: Environment.GetEnvironmentVariable("RabbitMQ_Queue"),
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
                this.queues[task.type].Add(task); // add task into queue based on their type
                _logger.LogInformation($" [x] Done: adding task {task.type} into queue");
                // launching ackowledgment
                _channel.BasicAck(
                    deliveryTag: m.DeliveryTag,
                    multiple: false
                );
            };
            _consumerTag = _channel.BasicConsume(
                queue: Environment.GetEnvironmentVariable("RabbitMQ_Queue"),
                autoAck: false,
                consumer: consumer
            );
            this.StartRun(_logger, stoppingToken); // start threads
            // Console.WriteLine(stoppingToken.IsCancellationRequested);
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _channel.BasicCancel(_consumerTag);
            _channel.Close();
            _connection.Close();
            return base.StopAsync(cancellationToken);
        }
        private void StartRun(ILogger<RabbitMQSubscriber> _logger, CancellationToken stoppingToken)
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
            while (!stoppingToken.IsCancellationRequested)
            {
            }
            
            _logger.LogInformation("Closing application...");
            for (int type = 1; type <= 2; type++)
            {
                this.queues[type].CompleteAdding();
            }
            foreach (var t in tasks) t.Wait(); // 注意
        }
        private void DoAllType(int type)
        {
            foreach (var task in this.queues[type].GetConsumingEnumerable())
            {
                this.methods[type]().doTask(task.name, task.id, _logger);
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
        public void doTask(string name, int id, ILogger<RabbitMQSubscriber> _logger)
        {
            _logger.LogInformation($"Doing {name} | TaskID: {id}");
            // do something in Task 1...
            Thread.Sleep(300);
        }
    }
    class MyTask2 : IDoTask
    {
        public void doTask(string name, int id, ILogger<RabbitMQSubscriber> _logger)
        {
            _logger.LogInformation($"Doing {name} | TaskID: {id}");
            // do something in Task 2...
            Thread.Sleep(500);
        }
    }
    interface IDoTask
    {
        void doTask(string name, int id, ILogger<RabbitMQSubscriber> _logger);
    }
}