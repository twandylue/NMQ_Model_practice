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
        private IModel _channelPub;
        private string _consumerTag;
        private BlockingCollection<MyTask>[] queues = new BlockingCollection<MyTask>[1 + 2] {
            null,
            new BlockingCollection<MyTask>(Int16.Parse(Environment.GetEnvironmentVariable("TASK1_LIMIT"))),
            new BlockingCollection<MyTask>(Int16.Parse(Environment.GetEnvironmentVariable("TASK2_LIMIT")))
        };
        private BlockingCollection<MyTask> doneTasks_queue = new BlockingCollection<MyTask>();
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
            while (true) // reconnect to rabbitmq if connection failed
            {
                try
                {
                    var factory = new ConnectionFactory()
                    {
                        UserName = Environment.GetEnvironmentVariable("RabbitMQ_UserName"),
                        Password = Environment.GetEnvironmentVariable("RabbitMQ_Password"),
                        VirtualHost = Environment.GetEnvironmentVariable("RabbitMQ_VirtualHost"),
                        HostName = Environment.GetEnvironmentVariable("RabbitMQ_HostName") == null ? "localhost" : Environment.GetEnvironmentVariable("RabbitMQ_HostName"),
                        Port = Int16.Parse(Environment.GetEnvironmentVariable("RabbitMQ_Port"))
                    };
                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();
                    _channel.QueueDeclare(
                        queue: Environment.GetEnvironmentVariable("RabbitMQ_TASK_Queue"),
                        durable: true,
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
                        this.queues[task.type].Add(task); // add tasks into queue by their type
                        _logger.LogInformation($" [x] Done: adding task {task.type} into queue");
                        // launching ackowledgment
                        _channel.BasicAck(
                                    deliveryTag: m.DeliveryTag,
                                    multiple: false
                                );
                    };
                    _consumerTag = _channel.BasicConsume(
                        queue: Environment.GetEnvironmentVariable("RabbitMQ_TASK_Queue"),
                        autoAck: false,
                        consumer: consumer
                    );
                    Task RunTask = Task.Run(() => { this.StartRun(_logger, stoppingToken); }); // start unit threads
                    Task PublishDoneTask = Task.Run(() => { this.PublishDoneTasks(_logger, stoppingToken, _connection); }); // start to publish done task message
                    RunTask.Wait(); // all unit threads stop before publishDonTask thread 
                    this.doneTasks_queue.CompleteAdding(); // closing blockingcollection of done tasks queue
                    PublishDoneTask.Wait();
                    break;
                }
                catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException e)
                {
                    _logger.LogInformation($"error: lose connection with RabbitMQ.");
                    _logger.LogInformation("Trying to re-connect to RabbitMQ in 500 ms later.");
                    if (stoppingToken.WaitHandle.WaitOne(0)) break;
                    Thread.Sleep(500);
                }
            }
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _channel.BasicCancel(_consumerTag);
                _channel.Close();
                _channelPub.Close();
                _connection.Close();
            }
            catch (RabbitMQ.Client.Exceptions.AlreadyClosedException e)
            {
                _logger.LogInformation("Already Closed.");
            }
            catch (Exception)
            {
                _logger.LogInformation("Closed!");
            }
            return base.StopAsync(cancellationToken);
        }
        private void StartRun(ILogger<RabbitMQSubscriber> _logger, CancellationToken stoppingToken)
        {
            List<Task> tasks = new List<Task>();
            int[] counts = {
                    0,
                    Int16.Parse(Environment.GetEnvironmentVariable("TASK1_THREAD_NUMBER")),
                    Int16.Parse(Environment.GetEnvironmentVariable("TASK2_THREAD_NUMBER"))
                }; // thread pool
            for (int i = 0; i < counts.Length; i++)
            {
                for (int j = 0; j < counts[i]; j++)
                {
                    int index = i;
                    Task t = Task.Run(() => { this.DoAllType(index); });
                    tasks.Add(t);
                }
            }

            stoppingToken.WaitHandle.WaitOne(); // waiting for stopping signal
            for (int type = 1; type <= 2; type++)
            {
                this.queues[type].CompleteAdding();
            }
            foreach (var t in tasks) t.Wait();
        }
        private void DoAllType(int type)
        {
            foreach (var task in this.queues[type].GetConsumingEnumerable())
            {
                this.methods[type]().doTask(task.name, task.id, _logger);
                this.doneTasks_queue.Add(task);
            };
        }
        private void PublishDoneTasks(ILogger<RabbitMQSubscriber> _logger, CancellationToken stoppingToken, IConnection conn)
        {
            this._channelPub = conn.CreateModel();
            this._channelPub.QueueDeclare(
                queue: Environment.GetEnvironmentVariable("RabbitMQ_DONE_TASK_Queue"),
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
            foreach (var task in doneTasks_queue.GetConsumingEnumerable())
            {
                string message = JsonSerializer.Serialize<MyTask>(task);
                var body = Encoding.UTF8.GetBytes(message);
                this._channelPub.BasicPublish(
                    exchange: "",
                    routingKey: Environment.GetEnvironmentVariable("RabbitMQ_DONE_TASK_Queue"),
                    basicProperties: null,
                    body: body
                );
                _logger.LogInformation($" [x] Sent {message}");
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
        public void doTask(string name, int id, ILogger<RabbitMQSubscriber> _logger)
        {
            _logger.LogInformation($"Doing {name} | TaskID: {id}");
            // do something in Task 1...
            Thread.Sleep(Int16.Parse(Environment.GetEnvironmentVariable("TASK1_THREAD_PROCESS_TIME")));
        }
    }
    class MyTask2 : IDoTask
    {
        public void doTask(string name, int id, ILogger<RabbitMQSubscriber> _logger)
        {
            _logger.LogInformation($"Doing {name} | TaskID: {id}");
            // do something in Task 2...
            Thread.Sleep(Int16.Parse(Environment.GetEnvironmentVariable("TASK2_THREAD_RPOCESS_TIME")));
        }
    }
    interface IDoTask
    {
        void doTask(string name, int id, ILogger<RabbitMQSubscriber> _logger);
    }
}