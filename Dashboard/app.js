require("dotenv").config();
const { PORT } = process.env;
const express = require("express");
const app = express();
app.use(express.static("public"));
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

app.use([
    require("./server/routes/page_route")
]);

if (require.main === module) {
    app.listen(PORT, () => {
        console.log(`Listening on port: ${PORT}`);
    });
}
