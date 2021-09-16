const router = require("express").Router();
const { getMessage } = require("../controllers/amqpController");
router.route("/").get(getMessage);
module.exports = router;
