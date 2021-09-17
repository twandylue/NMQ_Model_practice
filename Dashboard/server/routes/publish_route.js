const router = require("express").Router();
const { publishMessage } = require("../controllers/amqp_controller");
router.route("/publishMessage").post(publishMessage);
module.exports = router;
