require("dotenv").config();
const { subscribe } = require("../models/amqpClient_model");

const socket = async (io) => {
    const consumerEmitter = await subscribe();
    io.on("connection", async (socket) => {
        console.log("WebSocket Connected!");
        consumerEmitter.on("data", (message) => {
            console.log(`On socket: ${message}`);
            socket.emit("newData", message);
        });
        socket.on("disconnect", () => {
            console.log("Disconnected!");
        });
    });
};
module.exports = {
    socket
};
