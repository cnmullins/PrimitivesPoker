/*
Author: Christian Mullins
Summary: Class to construct cards based on enumerators.
*/
using System.Linq;
using System.Collections.Generic;
public enum Suit {
    Spade, Club, Heart, Diamond
}

/// <summary>
/// Int based enum ranking card value (Aces high)
/// </summary>
public enum Value {
    Two, Three, Four, Five, Six, Seven, 
    Eight, Nine, Ten, Jack, Queen, King, Ace
}

/// <summary>
/// Int base enum ranking score values
/// </summary>
public enum Score {
    Highcard = 1, Pair, TwoPairs, ThreeOfAKind, Straight, 
    Flush, FullHouse, FourOfAKind, StraightFlush, RoyalFlush
}

public class Card {
    public Suit suit   { get { return _suit;  } }
    public Value value { get { return _value; } }

    private Suit _suit;
    private Value _value;

    public Card(Suit s, Value v) {
        _suit = s;
        _value = v;
    }

    /// <summary>
    /// Properly format name of card.
    /// </summary>
    /// <returns>"'Value' of 'suit'"</returns>
    public string GetString() {
        string output = string.Empty;
        switch (value) {
            case Value.Two:     output += "Two";      break;
            case Value.Three:   output += "Three";    break;
            case Value.Four:    output += "Four";     break;
            case Value.Five:    output += "Five";     break;
            case Value.Six:     output += "Six";      break;
            case Value.Seven:   output += "Seven";    break;
            case Value.Eight:   output += "Eight";    break;
            case Value.Nine:    output += "Nine";     break;
            case Value.Ten:     output += "Ten";      break;
            case Value.Jack:    output += "Jack";     break;
            case Value.Queen:   output += "Queen";    break;
            case Value.King:    output += "King";     break;
            case Value.Ace:     output += "Ace";      break;
        }
        output += " of ";
        switch (suit) {
            case Suit. Spade:  output += "Spades";   break;
            case Suit.Club:    output += "Clubs";    break;
            case Suit.Heart:   output += "Hearts";   break;
            case Suit.Diamond: output += "Diamonds"; break;
        }
        return output;
    }

    /// <summary>
    /// Get score of the hand at the showdown.
    /// </summary>
    /// <param name="hand">Community and player hand.</param>
    /// <returns>Numerical evaluation of cards based on their score.</returns>
    public static int ScoreHand(List<Card> hand) {
        // initialize values for scoring
        var score = Score.Highcard; // start with lowest score
        var suitCount = new List<int>() { 0, 0, 0, 0 };
        var valueCount = new List<int>(13);
        /*
            TODO
                -Check if the default .Sort() for List<Card> will work.
        */
        hand.Sort(delegate(Card a, Card b) {
            if      (a.value < b.value) return -1;
            else if (a.value > b.value) return  1;
            return 0;
        });
        foreach (var card in hand) {
            ++suitCount[(int)card.suit];
            ++valueCount[(int)card.value];
        }
        int duplicates = suitCount.Max();
        bool isFlush = false;
        var highCard = (Value)valueCount.FindLastIndex(0, 13, v => v > 0);
        // evaluate values for scoring
        // check for sequence and track it
        int? seqTracker = null;
        int seq = 1;
        int greatestSeq = 1;
        for (int i = 0; i < hand.Count; ++i) {
            // catch if ace to two
            if (seqTracker == 12) seqTracker = -1;
            
            int curHandVal = (int)hand[i].value;
            if (seqTracker == curHandVal - 1) {
                seqTracker = curHandVal;
                ++seq;
            }
            else if (seqTracker == null) {
                seqTracker = curHandVal;
                if (seq > greatestSeq) greatestSeq = seq;
                seq = 1;
            }
            else seqTracker = null;
        }
        // check for suits
        if (suitCount.Max() > 4) isFlush = true;
        // evaluate as sequences first
        if (greatestSeq > 4) {
            if (isFlush) {
                List<Card> rFlushRef = new List<Card> {
                    new Card(Suit.Spade, Value.Ten),
                    new Card(Suit.Spade, Value.Jack),
                    new Card(Suit.Spade, Value.Queen),
                    new Card(Suit.Spade, Value.King),
                    new Card(Suit.Spade, Value.Ace)
                };
                rFlushRef.RemoveAll(card => hand.Contains(card));
                score = (rFlushRef.Count == 0) ? Score.RoyalFlush
                                               : Score.StraightFlush;
            }
            else
                score = Score.Straight;
        }
        else {
            // evaluate duplicates and suit count
            if (duplicates > 1) { // check for duplicate values
                switch (duplicates) {
                    case 2: score = Score.Pair;
                        break;
                    case 3: score = Score.ThreeOfAKind;
                        // check for the second max for full house
                        var temp = new List<int>(valueCount);
                        temp.RemoveAt(valueCount.IndexOf(suitCount.Max()));
                        if (temp.Max() > 1) score = Score.FullHouse;
                        break;
                    case 4: score = Score.FourOfAKind;
                        break;
                }
            }
        }
        return ((int)score * 10) + (int)highCard;
    }

}
