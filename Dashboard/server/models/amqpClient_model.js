require("dotenv").config();
const amqp = require("amqplib");
const EventEmitter = require("events");
const { Task } = require("../models/tasks");

// eslint-disable-next-line camelcase
const { RabbitMQ_UserName, RabbitMQ_Password, RabbitMQ_VirtualHost, RabbitMQ_HostName, RabbitMQ_Port, RabbitMQ_taskQueue, RabbitMQ_doneQueue } = process.env;
const opt = {
    hostname: RabbitMQ_HostName,
    port: RabbitMQ_Port,
    username: RabbitMQ_UserName,
    password: RabbitMQ_Password,
    vhost: RabbitMQ_VirtualHost
};
const subscribe = async () => {
    const connect = await amqp.connect(opt);
    const channel = await connect.createChannel();
    const consumeEmitter = new EventEmitter();
    try {
        channel.assertQueue(RabbitMQ_doneQueue, { durable: false }); // create queue if not exist
        channel.consume(RabbitMQ_doneQueue, message => {
            if (message !== null) {
                consumeEmitter.emit("data", message.content.toString());
            } else {
                const error = new Error("NullMessageException");
                consumeEmitter("error", error);
            }
            channel.ack(message);
        }, { noAck: false });
    } catch (error) {
        consumeEmitter("consume error", error);
    }
    return consumeEmitter;
};

const publish = async (data) => {
    const taskQueue = await PutTasksInQueue(data);
    const connect = await amqp.connect(opt);
    const channel = await connect.createChannel();
    channel.assertQueue(RabbitMQ_taskQueue, { durable: false });
    while (taskQueue.length > 0) {
        try {
            const msg = taskQueue.shift();
            channel.sendToQueue(RabbitMQ_taskQueue, Buffer.from(msg));
        } catch (e) {
            console.log(`erroe in publish: ${e}`);
        }
    }
};

const PutTasksInQueue = (data) => {
    return new Promise((resolve, reject) => {
        const queue = [];
        for (const item of data) {
            for (let i = 0; i < item.number; i++) {
                const task = new Task(i, item.type);
                queue.push(JSON.stringify(task));
            }
        }
        resolve(queue);
    });
};

module.exports = {
    subscribe,
    publish
};
