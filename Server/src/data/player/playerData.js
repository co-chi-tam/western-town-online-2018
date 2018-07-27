
module.exports = class PlayerData {

    constructor(data) {
        this.turnIndex  = data.turnIndex || -1;
        this.playerName = data.playerName || '';
        this.playerIcon = data.playerIcon || Math.floor(Math.random() * 12);
        this.totalGold  = data.totalGold || 0;
        this.gold       = data.gold || 0;
        this.active     = true;
        this.status     = 'ALIVE'; // ALIVE || HIDDEN || DEATH
        this.cardList   = data.cardList || []; 
        this.drawCardCount = 1;
    }
    
    getInfo() {
        return {
            turnIndex:  this.turnIndex,
            playerName: this.playerName,
            playerIcon: this.playerIcon,
            totalGold:  this.totalGold,
            gold:       this.gold,
            active:     this.active,
            status:     this.status,
            cardList:   this.cardList
        }
    }

}