const router = require("express").Router();
const { getMessage } = require("../controllers/amqp_controller");
router.route("/test").get(getMessage);
module.exports = router;
