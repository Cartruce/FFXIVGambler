using ImGuiNET;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace FFXIVGambler
{
    internal class CardDeck
    {
        //private static List<string> newDeck = new List<string>{ "A♥", "2♥", "3♥", "4♥", "5♥", "6♥", "7♥", "8♥", "9♥", "10♥", "J♥", "Q♥", "K♥", "A♦", "2♦", "3♦", "4♦", "5♦", "6♦", "7♦", "8♦", "9♦", "J♦", "Q♦", "K♦", "A♣", "2♣", "3♣", "4♣", "5♣", "6♣", "7♣", "8♣", "9♣", "10♣", "J♣", "Q♣", "K♣", "A♠", "2♠", "3♠", "4♠", "5♠", "6♠", "7♠", "8♠", "9♠", "10♠", "J♠", "Q♠", "K♠" };
        private static string[] newDeck = new string[]{ "A♥", "2♥", "3♥", "4♥", "5♥", "6♥", "7♥", "8♥", "9♥", "10♥", "J♥", "Q♥", "K♥", "A♦", "2♦", "3♦", "4♦", "5♦", "6♦", "7♦", "8♦", "9♦", "J♦", "Q♦", "K♦", "A♣", "2♣", "3♣", "4♣", "5♣", "6♣", "7♣", "8♣", "9♣", "10♣", "J♣", "Q♣", "K♣", "A♠", "2♠", "3♠", "4♠", "5♠", "6♠", "7♠", "8♠", "9♠", "10♠", "J♠", "Q♠", "K♠" };
        private Stack<string> deck = new Stack<string>();
        public CardDeck()
        {
            //create new card deck
            deck = reshuffleDeck();
        }

        public Stack<string> reshuffleDeck()
        {
            //reinstance deck from 'newDeck' and shuffle the deck
            deck.Clear();
            //create new shuffled array
            string[] shuffledDeck = newDeck;
            Shuffle(shuffledDeck);
            foreach (var item in shuffledDeck)
                deck.Push(item);
            return deck;
        }

        public void Shuffle(string[] array)
        {
            Random rand = new Random();
            int n = array.Length;
            for (int i = 0; i < (n - 1); i++)
            {
                // Use Next on random instance with an argument.
                // ... The argument is an exclusive bound.
                //     So we will not go past the end of the array.
                int r = i + rand.Next(n - i);
                string t = array[r];
                array[r] = array[i];
                array[i] = t;
            }
        }


        public string drawCard()
        {
            if (deck.Count != 0)
            {
                return deck.Pop();
            }
            else
            {
                return "Empty Deck :( ";
            }
        }
    }
}