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
        private BlockingCollection<MyTask> queue = new BlockingCollection<MyTask>();
        static void Main(string[] args)
        {
            // one worker
            // Console.WriteLine("Hello World!");
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
                    // Console.WriteLine(" [x] Received {0}", message);
                    MyJob job = JsonSerializer.Deserialize<MyJob>(message);
                    this.queue.Add(new MyTask(job.job_type));
                    // Person ms = JsonSerializer.Deserialize<Person>(message);
                    // Console.WriteLine($"Name: {ms.name}");
                    // Console.WriteLine($"Age: {ms.age}");
                    Console.WriteLine(" [x] Done");
                    channel.BasicAck(
                        deliveryTag: ea.DeliveryTag,
                        multiple: false
                    );
                    // Thread.Sleep(500);
                };
                channel.BasicConsume(
                    queue: "task_queue",
                    autoAck: false,
                    consumer: consumer
                );
                Console.WriteLine(" Press [enter] to exit");
                Console.ReadLine();
            }
        }

        // private static void Unit(int type)
        // {
        //     Console.WriteLine($"Execute type {type} task.");
        //     if (type == 1) Thread.Sleep(1000);
        //     if (type == 2) Thread.Sleep(5000);
        // }
    }

    // class Person
    // {
    //     public string name { get; set; }
    //     public int age { get; set; }
    // }

    class MyJob {
        public string job_name {get; set;}
        public int job_type {get; set;}
    }

    class MyTask
    {
        private int type;
        public MyTask(int type)
        {
            this.type = type;
        }
        public void Run()
        {
            if (this.type != 1 || this.type != 2)
            {
                Console.WriteLine("Wrong Task type.");
                return;
            }
            Console.WriteLine($"Execute type {this.type} task.");
            if (this.type == 1) Thread.Sleep(1000);
            if (this.type == 2) Thread.Sleep(5000);
        }
    }
}
