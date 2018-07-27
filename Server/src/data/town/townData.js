module.exports = class TownData {

    constructor(data) {
        this.townName       = data.townName || '';
        this.maximumMembers = data.maximumMembers || 6;
        this.townMembers    = data.townMembers || [];
        this.ghostMembers   = data.ghostMembers || [];
        this.townTime       = data.townTime || 0;
        this.status         = data.status || 'FREE'; // FREE || ACTIVE || PAUSE
    }

    // GET SHORT INFO
    getInfo() {
        var playerInfoes = [];
        for (let i = 0; i < this.townMembers.length; i++) {
            const player = this.townMembers[i];
            const playerData = player.player.getInfo();
            playerInfoes.push (playerData);
        }
        return {
            townName: this.townName,
            players: playerInfoes
        };
    }

}