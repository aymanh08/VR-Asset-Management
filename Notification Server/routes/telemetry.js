// Importing dependencies
var express = require('express')
var bodyParser = require('body-parser');
var dateHelper = require('../helpers/dateHelper');
var app = require('../app');

//Configuring depencies
var router = express.Router();
var parseJSON = bodyParser.json();
var parseUrlEncoded = bodyParser.urlencoded({ extended: false });

//Set universal headers
router.use((req, res, next) => {
    //res.setHeader(<header key>, <header value>);
    res.setHeader('Content-Type', 'application/json');
    next();
})

// define the home page route
router.route("/")

    //create
    .post(parseUrlEncoded, parseJSON, (req, res, next) => {

        const deviceName = req.body.meta.deviceName;
        console.log(deviceName);
        app.io.to(deviceName).emit("NEW_TELEMETRY", req.body);
        var successMessage = {"message":"Success"};
        res.send(JSON.stringify(successMessage));
    })


// Error handler
router.use(function (err, req, res, next) {
    //Handle Error
})

module.exports = router
