const { subscribe } = require("../models/amqpConsumer");

const getMessage = async (req, res) => {
    const consumerEmitter = await subscribe();
    consumerEmitter.on("data", async (message, ack) => {
        console.log(`received: ${message}`);
    });
    res.status(200).send("test");
};

module.exports = {
    getMessage
};
