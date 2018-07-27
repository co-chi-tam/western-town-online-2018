// Town DATA
var TownData = require('../src/data/town/townData');
// Card Data
var CardData = require('../src/data/card/cardData');
// Bomb Data
var BombData = require('../src/data/bomb/bombData');
// INIT
module.exports = class GameRoom {
    // Current room name.
    get roomName () {
        return this.town.townName;
    }
    set roomName (value) {
        this.town.townName = value;
    }
    // All players 
    get players() {
        return this.town.townMembers;
    }
    set players (value) {
        this.town.townMembers = value;
    }
    // Ghosts
    get ghosts() {
        return this.town.ghostMembers;
    }
    set ghosts (value) {
        this.town.ghostMembers = value;
    }
    constructor(name) {
        // town data
        this.town = new TownData({ 
            townName: name,
            maximumMembers: 3, 
            townMembers: [],
            townTime: 0,
            status: 'FREE'
        });
        // Card list
        this.cardList = [
            new CardData({ cardName: 'Move bomb +1', cardIcon: 'card_move', cardDescription: 'Move bomb plus 1', cardValue: 1 }),
            new CardData({ cardName: 'Move bomb +1', cardIcon: 'card_move', cardDescription: 'Move bomb plus 1', cardValue: 1 }),
            new CardData({ cardName: 'Move bomb +1', cardIcon: 'card_move', cardDescription: 'Move bomb plus 1', cardValue: 1 }),
            new CardData({ cardName: 'Move bomb +1', cardIcon: 'card_move', cardDescription: 'Move bomb plus 1', cardValue: 1 }),
            new CardData({ cardName: 'Move bomb +2', cardIcon: 'card_move', cardDescription: 'Move bomb plus 2', cardValue: 2 }),
            new CardData({ cardName: 'Move bomb +2', cardIcon: 'card_move', cardDescription: 'Move bomb plus 2', cardValue: 2 }),
            new CardData({ cardName: 'Move bomb +2', cardIcon: 'card_move', cardDescription: 'Move bomb plus 2', cardValue: 2 }),
            new CardData({ cardName: 'Move bomb +3', cardIcon: 'card_move', cardDescription: 'Move bomb plus 3', cardValue: 3 }),
            new CardData({ cardName: 'Move bomb +3', cardIcon: 'card_move', cardDescription: 'Move bomb plus 3', cardValue: 3 }),
            new CardData({ cardName: 'Hold on', cardIcon: 'card_move', cardDescription: 'Not move bomb', cardValue: 0 }),
            new CardData({ cardName: 'Hold on', cardIcon: 'card_move', cardDescription: 'Not move bomb', cardValue: 0 }),
            new CardData({ cardName: 'Gold +1', cardIcon: 'card_gold', cardDescription: 'Gold plus 1', goldValue: 1 }),
            new CardData({ cardName: 'Gold +1', cardIcon: 'card_gold', cardDescription: 'Gold plus 1', goldValue: 1 }),
            new CardData({ cardName: 'Gold +5', cardIcon: 'card_gold', cardDescription: 'Gold plus 5', goldValue: 5 }),
            new CardData({ cardName: 'Gold +5', cardIcon: 'card_gold', cardDescription: 'Gold plus 5', goldValue: 5 })
        ];
        // BOMB
        this.bomb = new BombData({
            turnIndex: 0,
            bombIndex: 0,
            currentTimer: 0,
            maxTimer: 10
        });
        // SHORT INFO
        this.shortInfoPlayers = [];
        // START GAME COUNTDOWN
        this.startGameCountDown = 5;
    }
    // Join room and set turn index for player
    join (player) {
        if (this.players.indexOf (player) == -1) {
            player.player.active = true;  
            player.player.status = 'ALIVE'; 
            player.player.cardList.length = 0;        
            this.players.push (player);
            for (let i = 0; i < this.players.length; i++) {
                const player = this.players[i];
                player.player.turnIndex = i;
            }
        }
    };
    sendRoomChat(msg) {
        // Receive client chat in current room.
        this.emitAll('msgChatRoom', {
            playerName: msg.playerName,
            message: msg.message
        });
    }
    // Clear room
    clearRoom () {
        for (let i = 0; i < this.players.length; i++) {
            const ply = this.players[i];
            ply.emit('clearRoom', {
                msg: "Room is empty or player quit."
            });
            delete ply.room;
        }
        for (let i = 0; i < this.ghosts.length; i++) {
            const ply = this.ghosts[i];
            ply.emit('clearRoom', {
                msg: "Room is empty or player quit."
            });
            delete ply.room;
        }
        // Change town status
        this.town.status = 'FREE';
        this.startGameCountDown = 5;
    };
    // Leave room 
    leave (player) {
        // MEMBERS
        if (this.players.indexOf (player) > -1) {
            this.players.splice (this.players.indexOf (player), 1);
            console.log ('B. Delete User in ROOM...' + player.player.playerName);
            player.player.active = false;
            // RETURN CARD
            if (player.player.cardList.length > 0) {
                var cards = player.player.cardList;
                for (let i = 0; i < cards.length; i++) {
                    const ca = cards[i];
                    this.cardList.push (ca);
                }
            }
        }
        // GHOST
        if (this.ghosts.indexOf (player) > -1) {
            this.ghosts.splice (this.ghosts.indexOf (player), 1);
            player.player.active = false;
            // RETURN CARD
            if (player.player.cardList.length > 0) {
                var cards = player.player.cardList;
                for (let i = 0; i < cards.length; i++) {
                    const ca = cards[i];
                    this.cardList.push (ca);
                }
            }
        }
        // RESET TURN IF PLAYED
        if (this.town.status != 'FREE') {
            this.resetPlayerTurn();
        }
    };
    // Send all mesg for players in room.
    emitAll (name, obj) {
        // PLAYER
        for (let i = 0; i < this.players.length; i++) {
            const player = this.players[i];
            player.emit(name, obj);
        }
        // GHOST
        for (let i = 0; i < this.ghosts.length; i++) {
            const ghost = this.ghosts[i];
            ghost.emit(name, obj);
        }
    };
    // Send each player in room
    emitEach(name, withGhost, item) {
        for (let i = 0; i < this.players.length; i++) {
            const socket = this.players[i];
            if (item) {
                socket.emit(name, item(i, socket));
            }
        }
        if (withGhost) {
            for (let i = 0; i < this.ghosts.length; i++) {
                const socket = this.ghosts[i];
                if (item) {
                    socket.emit(name, item(i, socket));
                }
            }
        }
    };
    // Get room info.
    getInfo () {
        var playerInfoes = [];
        for (let i = 0; i < this.players.length; i++) {
            const player = this.players[i];
            const playerData = player.player;
            playerInfoes.push (playerData);
        }
        return {
            roomName: this.roomName,
            maxPlayer: this.town.maximumMembers,
            players: playerInfoes
        };
    }
    // Get town info.
    // Short info.
    getTownInfo() {
        this.shortInfoPlayers.length = 0;
        for (let i = 0; i < this.players.length; i++) {
            const player = this.players[i];
            const playerData = player.player;
            this.shortInfoPlayers.push ({
                turnIndex:  playerData.turnIndex,
                playerName: playerData.playerName,
                playerIcon: playerData.playerIcon,
                gold:       playerData.gold,
                status:     playerData.status
            });
        }
        return {
            townName: this.town.townName,
            townMembers: this.shortInfoPlayers
        };
    }  
    // Start game
    startGame() {
        this.startGameCountDown--;
        if (this.startGameCountDown <= 0) {
            // 1. Change town status
            this.town.status = 'ACTIVE';
            // 2. Random bomb index
            this.bomb.bombIndex = this.randomIntRange(0, this.players.length - 1);
            // 3. Shuffle card list and split card to player.
            this.shuffleArray(this.cardList);
            this.emitEach('enterGame', true, (index, socket) => {
                const firstCard = this.shiftCard (this.cardList);
                socket.player.cardList.push (firstCard);
                return {
                    player: socket.player.getInfo(),
                    turnIndex: this.bomb.turnIndex,
                    bombIndex: this.bomb.bombIndex,
                    bombTimer: this.bomb.currentTimer,
                    maxTimer: this.bomb.maxTimer,
                    town: this.getTownInfo()
                }
            });
        }
    } 
    // Draw a card
    // 1. When card list available
    // 2. When turn index is correct
    drawACard() {
        if (this.room) {
            var room = this.room;
            var bomb = room.bomb;
            if (bomb.isExplosion()) {
                // Bomb was explision
                this.emit('messageError', {
                    msg: 'Bomb was explision.'
                });
            } else {
                var cardList = room.cardList;
                if (this.player 
                    && cardList.length > 0) {
                    if (this.player.turnIndex == bomb.turnIndex
                        && this.player.drawCardCount > 0) {
                        var firstCard = room.shiftCard (cardList);
                        this.player.cardList.push (firstCard);
                        this.player.drawCardCount -= 1;
                        // UPDATE USER CARD LIST
                        this.emit('updatePlayer', {
                            player: this.player.getInfo()
                        });
                    } else {
                        // Wrong turn
                        this.emit('messageError', {
                            msg: 'This is not your turn.'
                        });
                    }
                } else {
                    console.log('[ERROR] CARD LIST IS EMPTY.');
                }
            }
        } else {
            // Bomb was explision
            this.emit('messageError', {
                msg: 'You must join a room first.'
            });
        }
    }
    // USE CARD
    useCard(value) {
        if (this.room) {
            var room = this.room;
            var bomb = room.bomb;
            var cardList = room.cardList;
            if (this.player && value) {
                if (this.player.turnIndex == bomb.turnIndex 
                    && this.player.status != 'DEATH') {
                    // INDEX CARD
                    var cardIndex = value.cardIndex;
                    if (cardIndex < this.player.cardList.length) {
                        var card = this.player.cardList[cardIndex];
                        // USE CARD VALUE
                        bomb.bombIndex = bomb.bombIndex + card.cardValue;
                        bomb.currentTimer += 1;
                        // RETURN CARD LIST
                        cardList.push(card);
                        // USE CARD
                        this.player.cardList.splice(cardIndex, 1);
                        // Update gold
                        this.player.gold += card.goldValue;
                        // Update turn
                        this.player.drawCardCount = 1;
                        bomb.turnIndex = (bomb.turnIndex + 1) % room.players.length;
                        // UPDATE USER TURN
                        room.emitEach('updateTurn', true, (index, socket) => {
                            return {
                                player: socket.player.getInfo(),
                                turnIndex: bomb.turnIndex,
                                bombIndex: bomb.bombIndex,
                                bombTimer: bomb.currentTimer,
                                maxTimer: bomb.maxTimer,
                                town: room.getTownInfo()
                            };
                        });
                        // BOMB EXPLOSION 
                        if (bomb.isExplosion()) {
                            var exPlosionIndex = bomb.bombIndex % room.players.length;
                            var explosionSocket = room.players[exPlosionIndex];
                            var explosionPlayer = explosionSocket.player;
                            explosionPlayer.status = 'DEATH';
                            // BECOME GHOST
                            room.becomeGhost(explosionSocket);
                            room.resetPlayerTurn();
                            room.emitEach('explosionPlayer', true, (index, socket) => {
                                return {
                                    player: socket.player.getInfo(),
                                    explosionPlayer: explosionPlayer.getInfo(),
                                    town: room.getTownInfo()
                                }
                            });
                            bomb.reset();
                        } 
                    } else {
                        // No card to use
                        this.emit('messageError', {
                            msg: 'No card to use.'
                        });
                    }
                } else {
                    // Wrong turn
                    this.emit('messageError', {
                        msg: 'This is not your turn.'
                    });
                }
            } else {
                console.log('[ERROR] SOMETHING WRONG.');
            }
        } else {
            // You must join a room first.
            this.emit('messageError', {
                msg: 'You must join a room first.'
            });
        }
    }
    // BECOME GHOST
    becomeGhost(player) {
        // IS PLAYER AVAILABLE
        var playerIndex = this.players.indexOf (player);
        if (playerIndex > -1) {
            this.players.splice (playerIndex, 1);
        }
        // IS GHOST AVAILABLE
        playerIndex = this.ghosts.indexOf (player);
        if (playerIndex == -1) {
            this.ghosts.push(player);
        } 
    }
    // RESET TURN 
    // RESET BONM INDEX
    resetPlayerTurn() {
        for (let i = 0; i < this.players.length; i++) {
            const player = this.players[i];
            player.player.turnIndex = i;
        }
        this.bomb.turnIndex = this.bomb.turnIndex % this.players.length;
    }
    // Shift 1 Card
    shiftCard(arr) {
        return arr.shift();
    }
    // Start game condition
    // 1. Full room.
    // 2. Room is free.
    canStartGame() {
        return this.players.length >= this.town.maximumMembers
                && this.town.status == 'FREE';
    }
    // If client contain in room.
    contain (player) {
        return this.players.indexOf (player) > -1;
    };
    // Get amount of players in room.
    length () {
        return this.players.length;
    };
    // Get room size
    size () {
        return this.town.maximumMembers;
    }
    // Is available room 
    // 1. Is maximum player < 6
    // 2. Is player not join a room
    // 3. Is all players ready.
    // 4. Join player have not room.
    isAvailable (player) {
        console.log ("Room is available: room length " 
        + this.players.length 
        + " is index " + this.players.indexOf (player));
        return this.players.length < this.town.maximumMembers 
                && this.players.indexOf (player) == -1
                && player.room == null;
    }
    // Random range
    randomIntRange (min, max) {
        return Math.floor(Math.random() * (max + 1 - min) + min);
    }
    // Shuffle array
    shuffleArray (a) {
        var j, x, i;
        for (i = a.length - 1; i > 0; i--) {
            j = Math.floor(Math.random() * (i + 1));
            x = a[i];
            a[i] = a[j];
            a[j] = x;
        }
        return a;
    }
    // Shuffle Array
    shuffleArrayMinMax(a, min, max) {
        var j, x, i;
        for (i = max - 1; i > min; i--) {
            j = Math.floor(Math.random() * (max - min) + min);
            x = a[i];
            a[i] = a[j];
            a[j] = x;
        }
        return a;
    }
};