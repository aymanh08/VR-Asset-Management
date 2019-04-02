var io = require('../app').io;
io.on('connection', (socket) => {
    socket.emit('Connected');
    socket.on('subscribe', (deviceName) => {
        socket.join(deviceName);
        socket.emit('Subscribed')
    })
    socket.on('unsubscribe', (deviceName) =>{
        socket.leave(deviceName);
        socket.emit('Unsubscribed')
    })
})