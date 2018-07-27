
module.exports = class BombData {

    constructor(data) {
        // TURN INDEX
        this.turnIndex      = data.turnIndex || 0;
        // BOMB INDEX
        this.bombIndex      = data.bombIndex || 0;
        // TIMER
        this.currentTimer   = data.currentTimer || 0;
        this.maxTimer       = data.maxTimer || 10;
    }

    reset() {
        this.currentTimer   = 0;
    }

    isExplosion() {
        return this.currentTimer >= this.maxTimer;
    }

}