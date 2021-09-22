const { subscribe, publish } = require("../models/amqpClient_model");
const getMessage = async (req, res) => {
    const consumerEmitter = await subscribe();
    consumerEmitter.on("data", (message) => {
        console.log(`received: ${message}`);
    });
    res.status(200).send("Received messages!");
};

const publishMessage = async (req, res) => {
    const { data } = req.body;
    publish(data);
    res.status(200).send("Publish success!");
};

module.exports = {
    getMessage,
    publishMessage
};
