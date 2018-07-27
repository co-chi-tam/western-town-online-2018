// PLAYER
var PlayerData = require('./src/data/player/playerData');
// GAME ROOM
var GameRoom = require('./src/gameRoom');

users = []; // Array User names
rooms = {}; // Rooms

const MAXIMUM_ROOMS = 10; // Maximum rooms

var GameWolves = function (http) {
    var io = require('socket.io')(http); // Require socket.io
    var self = this;
    // On client connect.
    io.on('connection', function(socket) {
        console.log('A user connected ' + (socket.client.id));
        // Welcome message
        socket.emit ('gameInit', { 
            msg: 'Welcome to connect game.',
            version: '0.1',
            // dayJobs: initDayJobs,
            // nightJob: initNightJobs
        });
        // INIT PLAYER
        // Set player name and player job.
        socket.on('setGameSetup', function(data) {
            if (data && data.playerName) {
                var isDuplicateName = false;
                for (let i = 0; i < users.length; i++) {
                    const u = users[i];
                    if (u.playerName == data.playerName) {
                        isDuplicateName = true;    
                        break;
                    }
                }
                if(isDuplicateName) {
                    socket.emit('msgError', { 
                        msg: data.playerName  + ' username is taken! Try some other username.'
                    });
                } else {
                    if (data.playerName.length < 5) {
                        socket.emit('msgError', { 
                            msg: data.playerName  + ' username must longer than 5 character'
                        });
                    } else {
                        var newPlayer = new PlayerData(data);
                        socket.player = newPlayer;
                        users.push(newPlayer);
                        socket.emit('playerGameSet', { 
                            id: socket.client.id,
                            name: newPlayer.playerName,
                            icon: newPlayer.playerIcon
                        });
                        console.log ('playerGameSet: ' + newPlayer.playerName);
                    }
                }
            }
        });
        // Receive beep mesg
        socket.on('beep', function(data) {
            socket.emit('boop');
        })
        // INIT ROOM
        // Get all room status
        socket.on('getRoomsStatus', function() {
            var results = [];
            for (let i = 0; i < MAXIMUM_ROOMS; i++) {
                var roomName = 'room-' + (i + 1);
                var playerCount = typeof (rooms [roomName]) !== 'undefined' 
                                        ? rooms [roomName].length()
                                        : 0;
                var maximumPlayer = typeof (rooms [roomName]) !== 'undefined' 
                                        ? rooms [roomName].size()
                                        : 6;
                var roomStatus = typeof (rooms [roomName]) !== 'undefined' 
                                        ? rooms [roomName].town.status
                                        : 'FREE';
                results.push ({
                    roomName: roomName,
                    roomDisplay: '[' + roomName + ']: ' 
                                    + playerCount + '/' + maximumPlayer
                                    + ' ==> ' + roomStatus,
                    players: playerCount
                });
            }
            socket.emit('updateRoomStatus', {
                rooms: results
            });
        });
        // Join or create room by name. 
        socket.on('joinOrCreateRoom', function(playerJoin) {
            // CLEAR EVENTS
            self.clearEvents(socket);
            if(playerJoin && socket.player) {
                var roomName = playerJoin.roomName;
                if (typeof(rooms [roomName]) === 'undefined') {
                    rooms [roomName] = new GameRoom(roomName);
                }
                if (rooms [roomName].isAvailable(socket)) {
                    socket.room = rooms [roomName];
                    socket.room.join (socket);
                    socket.room.emitAll('newJoinRoom', {
                        roomInfo: socket.room.getInfo()
                    });    
                    socket.on('sendRoomChat', function(msg) {
                        if(msg && socket.room) {
                            socket.room.sendRoomChat({
                                playerName: socket.player.playerName,
                                message: msg.message
                            });
                        }
                    });
                    socket.on('drawACard',  socket.room.drawACard);
                    socket.on('useCard',    socket.room.useCard);
                    console.log ("A player join room. " + socket.room.roomName + " Room: " + socket.room.length());
                } else {
                    socket.emit('joinRoomFailed', {
                        msg: "Room is not available.\n Please try again later."
                    });
                }
            }
        });
        // Receive world chat.
        socket.on('sendWorldChat', function(msg) {
            if(msg) {
                // socket.broadcast.emit => will send the message to all the other clients except the newly created connection
                io.sockets.emit('msgWorldChat', {
                    user: socket.player.playerName,
                    message: msg.message
                });
            }
        });
        // Receive leave room mesg.
        socket.on('leaveRoom', function() {
            // LEAVE ROOM
            self.clearSocketRoom(socket);
        });
        // DISCONNECT
        // Disconnect and clear room.
        socket.on('disconnect', function() {
            console.log ('User disconnect...' + socket.id);
            // LEAVE ROOM
            self.clearSocketRoom(socket);
            // DELETE PLAYER DATA
            self.clearSocketPlayer(socket);
        });
    });
    // CHECK START GAME
    this.checkStartGame = function() {
        setInterval(function() {
            for (let i = 0; i < MAXIMUM_ROOMS; i++) {
                var roomName = 'room-' + (i + 1);
                if (rooms [roomName]) {
                    if (rooms [roomName].canStartGame()) {
                        rooms [roomName].startGame();
                    }
                }
            }
        }, 1000);
    }
    // CLEAR EVENTS
    this.clearEvents = function(socket) {
        socket.removeAllListeners('sendRoomChat');   
        socket.removeAllListeners('drawACard'); 
        socket.removeAllListeners('useCard'); 
    };
    // LEAVE ROOM
    this.clearSocketRoom = function(socket) {
        if (socket.room) {
            self.clearEvents(socket);
            console.log ('Clear player room');
            // LEAVE ROOM
            socket.room.leave(socket);
            // ROOM
            socket.room.emitAll('newLeaveRoom', {
                roomInfo: socket.room.getInfo()
            });
            if (socket.room.length() < 2) {
                var roomName = socket.room.roomName;
                socket.room.clearRoom();
                delete rooms [roomName];
                console.log ('Clear room');
            }
            delete socket.room;
        }
    };
    // DELETE PLAYER DATA
    this.clearSocketPlayer = function(socket) {
        if (socket.player) {
            for (let i = 0; i < users.length; i++) {
                const u = users[i];
                if (u.playerName == socket.player.playerName) {
                    users.splice(i, 1);  
                    break;
                }
            }
            delete socket.player;
            console.log ('Clear socket player');
        }
    };
    return this;
};
// INIT
module.exports = GameWolves;