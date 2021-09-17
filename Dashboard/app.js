require("dotenv").config();
const { PORT } = process.env;
const path = require("path");
const express = require("express");
const app = express();
const server = require("http").createServer(app);
const SocketServer = require("socket.io").Server;
const io = new SocketServer(server);
const { socket } = require("./server/controllers/socket_controller");
socket(io);

app.use(express.static("public"));
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

app.use([
    require("./server/routes/page_route"),
    require("./server/routes/publish_route")
]);

app.get(["/", "/index.html"], (req, res) => {
    res.sendFile(path.join(__dirname, "/public/dashboard.html"));
});

if (require.main === module) {
    server.listen(PORT, () => {
        console.log(`Server listening on port: ${PORT}`);
    });
}
