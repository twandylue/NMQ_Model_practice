require("dotenv").config();
const amqp = require("amqplib");
const EventEmitter = require("events");
// eslint-disable-next-line camelcase
const { RabbitMQ_UserName, RabbitMQ_Password, RabbitMQ_VirtualHost, RabbitMQ_HostName, RabbitMQ_Port, RabbitMQ_Queue } = process.env;
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
        channel.consume(RabbitMQ_Queue, message => {
            if (message !== null) {
                consumeEmitter.emit("data", message.content.toString());
            } else {
                const error = new Error("NullMessageException");
                consumeEmitter("error", error);
            }
            channel.ack(message);
        }, { noAck: false });
    } catch (error) {
        consumeEmitter("error", error);
    }
    return consumeEmitter;
};

module.exports = {
    subscribe
};
