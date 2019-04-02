// Variables
var express = require('express')
var app = express()
var http = require('http').Server(app);
var port = process.env.PORT || 3000;
var io = require('socket.io')(http);

// Defining routes
var telemetry = require('./routes/telemetry');

//Base route
app.get('/', function(req, res){
    res.send("Success");
})

//Connecting routes
app.use('/telemetry', telemetry);

http.listen(port, function(){
    console.log('Server listening on ', port);
})
module.exports.io = io;
var ioHelper = require('./helpers/ioHelper');




