using System;
using System.Collections.Generic;

namespace FFXIVGambler
{
    internal class Hand
    {
        private List<string> cards;
        private int total = 0;
        private int aces = 0;
        private int numCards = 0;
        Boolean isBlackjack = false;
        public Hand(string card)
        {
            this.cards = new List<string>();
            this.cards.Add(card);
            this.total = parseCard(card);
            this.numCards = 1;
            this.isBlackjack = false;
        }
        public Hand()
        {
            this.cards = new List<string>();
            this.total = 0;
            this.aces = 0;
            this.numCards = 0;
            this.isBlackjack = false;
        }
        public int getTotal()
        {
            return this.total;
        }
        public string showHand()
        {
            string hand = "";
            foreach (string card in this.cards)
            {
                hand = card + "  " + hand;
            }
            return hand;
        }
        public List<string> getCards()
        {
            return this.cards;
        }
        private int parseCard(string card)
        {
            int cardValue = 0;
            if(card != null)
            {
                if(card.Contains("K") || card.Contains("Q") || card.Contains("J"))
                {
                    return 10;
                }
                if(card.Contains("A"))
                {
                    this.aces++;
                    return 11;
                }
                cardValue = int.Parse(card.Substring(0,card.Length-1));
            }
            return cardValue;
        }

        internal void addCard(string card)
        {
            int newCard = parseCard(card);
            this.total = this.total + newCard;
            this.numCards = this.numCards + 1;
            if(this.total > 21)
                if(this.aces!=0)
                {
                    //downgrade an ace
                    this.aces = this.aces - 1;
                    this.total = this.total - 10;
                }
            if(this.total == 21 && this.numCards == 2)
            {
                this.isBlackjack = true;
            }
            this.cards.Add(card);
        }

        internal void resetHand()
        {
            this.cards = new List<string>();
            this.total = 0;
            this.aces = 0;
            this.numCards = 0;
            this.isBlackjack = false;
        }

        public Boolean checkBlackjack()
        {
            return this.isBlackjack;
        }

        public int getNumCards()
        {
            return this.numCards;
        }
    }
}