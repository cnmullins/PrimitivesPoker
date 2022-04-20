/*
Author: Christian Mullins
Summary: Class to construct cards based on enumerators.
*/
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public enum Suit {
    Spade, Club, Heart, Diamond
}

/// <summary>
/// Int based enum ranking card value (Aces high)
/// </summary>
public enum Value {
    Ace, Two, Three, Four, Five, Six, Seven, 
    Eight, Nine, Ten, Jack, Queen, King
}

/// <summary>
/// Int base enum ranking score values
/// </summary>
public enum Score {
    Highcard = 1, Pair, TwoPairs, ThreeOfAKind, Straight, 
    Flush, FullHouse, FourOfAKind, StraightFlush, RoyalFlush
}

public class Card {
    //TODO: create comparerrs to randomize and initialize arrays into the right location
    public class ValueSorter : IComparer<Texture> {
        int IComparer<Texture>.Compare(Texture t1, Texture t2) {
            int val1 = Convert.ToInt32(t1.ToString().Split('_')[0]);
            int val2 = Convert.ToInt32(t2.ToString().Split('_')[0]);
            if (val1 > val2)        return 1;
            else if (val1 < val2)   return -1;
            else                    return 0;
        }
    }
    public class ShuffleSorter : IComparer<Card> {
        int IComparer<Card>.Compare(Card c1, Card c2) {
            return (UnityEngine.Random.value > 0.5f) ? -1 : 1;
        }
    }
    public static Color BACK_COLOR => new Color(1f, 0.5f, 0.5f, 1.0f);
    public static Color DEFAULT_COLOR => new Color(1f, 1f, 1f, 1f); //white
    public Suit suit   { get; private set; }
    public Value value { get; private set; }

    public static Texture[][] cardAssets => _cardAssets;
    private static Texture[][] _cardAssets;
    
    static Card() {
        _cardAssets = new Texture[][] {
            Resources.LoadAll<Texture>("Spades"),
            Resources.LoadAll<Texture>("Clubs"),
            Resources.LoadAll<Texture>("Hearts"),
            Resources.LoadAll<Texture>("Diamonds"),
        };
        
        for (int x = 0; x < _cardAssets.Length; ++x) {
            var tempArr = new List<Texture>(_cardAssets[x]);
            tempArr.Sort(0, _cardAssets[x].Length, new ValueSorter());
            _cardAssets[x] = tempArr.ToArray();
            for (int y = 0; y < _cardAssets[x].Length; ++y) {
                //Debug.Log(x + ", " + y + ": " + _cardAssets[x][y]);
            }   
        }
    }
    

    public Card(Suit s, Value v) {
        suit = s;
        value = v;
    }

    /// <summary>
    /// Properly format name of card.
    /// </summary>
    /// <returns>"'Value' of 'suit'"</returns>
    public override string ToString() {
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
            case Suit.Spade:   output += "Spades";   break;
            case Suit.Club:    output += "Clubs";    break;
            case Suit.Heart:   output += "Hearts";   break;
            case Suit.Diamond: output += "Diamonds"; break;
        }
        return output;
    }

    /// <summary>
    /// Properly format name of card in a condensed form.
    /// </summary>
    /// <returns>"'Value' of 'suit'"</returns>
    public string ToShortString() {
        string output = string.Empty;
        if ((int)value < 10) {
            output = System.Convert.ToString((int)value + 1);
        }
        else {
            output += "JQKA"[(int)value - 10];
        }
        output += "♠♣♥♦"[(int)suit];
        return output;
    }

    public static bool SequenceCheck(in Card[] cards) {
        var cardsList = new List<Card>(cards);
        cardsList.Sort();
        int tempVal = (int)cardsList[0].value;
        for (int i = 0; i < cardsList.Count; ++i) {
            if (tempVal != (int)cardsList[i].value) {
                return false;
            }
            //check for overflow enum?
            tempVal++;
        }
        return true;
    }

    /// <summary>
    /// Get score of the hand at the showdown.
    /// </summary>
    /// <param name="hand">Community and player hand.</param>
    /// <returns>Numerical evaluation of cards based on their score.</returns>
    public static int ScoreHand(in Card[] comHand, in Card[] pHand) { // partition card list?
    /*
     * Algorithm:
     *  -Sort hand
     *  -Check for straights and flushes
     *
     */
        // initialize values for scoring
        List<Card> hand = new List<Card>(comHand);
        if (pHand == null) return -1;
        hand.AddRange(pHand);
        var score = Score.Highcard; // start with lowest score
        List<int> suitCount = Enumerable.Repeat(0, 4).ToList();
        List<int> valueCount = Enumerable.Repeat(0, 13).ToList();
        int maxSuitDuplicates, maxValueDuplicates;
        int seq, greatestSeq;
        int? seqTracker = null;
        /*
            TODO
                -Double check if the default .Sort() for List<Card> will work.
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
        maxSuitDuplicates = suitCount.Max();
        maxValueDuplicates = valueCount.Max();
        bool isFlush = false;
        Value highCard = (Value)Mathf.Max((int)pHand[0].value, (int)pHand[1].value);
        // evaluate values for scoring
        // check for sequence and track it
        seqTracker = null;
        seq = 1;
        greatestSeq = 1;
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
            if (maxValueDuplicates > 1) { // check for duplicate values
                switch (maxValueDuplicates) {
                    case 2: score = Score.Pair;
                        break;
                    case 3: score = Score.ThreeOfAKind;
                        // check for the second max for full house
                        var temp = valueCount;
                        //Debug.Log("bugbug?: " + valueCount.IndexOf(suitCount.Max()));
                        int index = valueCount.IndexOf(suitCount.Max());
                        if (temp.Max() > 2) {
                            Debug.Log("index of: " + index);
                            temp.RemoveAt(index);
                            score = Score.FullHouse;
                        }
                        break;
                    case 4: score = Score.FourOfAKind;
                        break;
                }
            }
        }
        return ((int)score * 100) + (int)highCard;
    }

}
