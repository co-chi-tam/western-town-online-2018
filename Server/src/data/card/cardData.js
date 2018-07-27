
module.exports = class CardData {

    constructor(data) {
        this.cardName      = data.cardName || 'CARD_';
        this.cardIcon      = data.cardIcon || 'ICON_';
        this.cardDescription = data.cardDescription || 'DESC_';
        this.active         = true;
        this.cardValue      = data.cardValue || 0;
        this.goldValue      = data.goldValue || 0;
    }

}