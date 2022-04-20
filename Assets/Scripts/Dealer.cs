/*
Author: Christian Mullins
Summary: Static dealer class to handle each hand.
*/
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dealer : MonoBehaviour {
    public static Dealer instance;
    public static uint pot => _pot;
    public static BasePlayer curDealer;
    public static uint communityBet { get {
        uint bet = 0;
        foreach (var p in playersLL)
            if (p.currentBet > bet)
                bet = p.currentBet;
        return bet;
    } }
    public GameObject comCardsParent;
    public static List<Card> communityCards   { get; private set; }
    public static List<Card> curDeck          { get; private set; }
    public static LinkedList<BasePlayer> playersLL    { get; private set; }
    public Blind bigBlindButton               { get; private set; }
    public Blind smallBlindButton             { get; private set; }
    public Blind dealerButton                 { get; private set; }

    private List<RawImage> _comCardsUI; //communityCardsUI
    private static uint _pot;

    private void Start() {
        instance = this;
        curDeck = new List<Card>();
        communityCards = new List<Card>();
        _comCardsUI = new List<RawImage>(comCardsParent.GetComponentsInChildren<RawImage>());
    }

    public void InitializeValues() {
        playersLL = new LinkedList<BasePlayer>
                    (from player in GameManager.instance.playerSpawnPos
                    where player.transform.childCount > 0
                    select player.GetChild(0).GetComponent<BasePlayer>());                
        // assign dealer
        curDealer = playersLL.First.Value;
        // set buttons at their starting index
        foreach (var b in FindObjectsOfType<Blind>()) {
            b.transform.position = playersLL.First.Value.buttonPos;
            if (b.name == "DealerButton") {
                dealerButton = b;
                continue;
            }
            StartCoroutine(b.IterateAsync());
            if (b.name == "SmallBlindButton") {
                smallBlindButton = b;
                continue; 
            }
            StartCoroutine(b.IterateAsync());
            bigBlindButton = b;
        }
        ClearPot();
    }

    /// <summary>
    /// Loop through all suites and values to create a new deck, shuffle,
    /// then assign to self.
    /// </summary>
    public void GetNewDeck() {
        var deck = new List<Card>();
        // loop through all suites and values
        for (int s = 0; s < 4; ++s) {
            for (int v = 0; v < 13; ++v) {
                deck.Add(new Card((Suit)s, (Value)v));
            }
        }
        
        // shuffle
        deck.Sort(0, deck.Count, new Card.ShuffleSorter());
        
        curDeck = deck;
    }

    /// <summary>
    /// Construct a hand from the current deck and remove cards from deck
    /// </summary>
    /// <returns>New hand.</returns>
    public Card[] GetHand() {
        var hand = new Card[2] { curDeck[0], curDeck[1] };
        curDeck.RemoveRange(0, 2);
        return hand;
    }

    public static uint AddToPot(in uint addition) {
        PotObserver.instance.IncrementFeedback((int)addition);
        _pot += addition;
        GameManager.instance.potText.text = "$" + _pot;
        return _pot;
    }

    public static void ClearPot() {
        PotObserver.instance.ClearFeedback();
        _pot = 0;
        GameManager.instance.potText.text = "$" + _pot;
    }

    /// <summary>
    /// Compare every possible hand per player and return the highest scoring
    /// player.
    /// </summary>
    /// <returns>Winner of the hand based on "_players" indexing.</returns>
    public int DeclareWinner() {
        int winningIndex = -1;
        int currentWinner = 0;
        //var findMax = playersLL.ToList<BasePlayer>();
        //playersLL.Max<BasePlayer>();
        string debugStr = "--PLAYER SCORES--\n";
        foreach (var p in playersLL) {
            int temp = Card.ScoreHand(communityCards.ToArray(), p.hand);
            debugStr += p.playerName + ": " + temp + '\n';
            if (temp < currentWinner) {
                currentWinner = temp;
            }
        }
        Debug.Log(debugStr);
        return winningIndex;
    }

    public BasePlayer[] GetMultipleWinners() {
        var winnersOut = new List<BasePlayer>();
        int curBest = -1;
        foreach (var p in playersLL) {
            int temp = Card.ScoreHand(communityCards.ToArray(), p.hand);
            if (temp > curBest) {
                curBest = temp;
                winnersOut.Clear();
            } else if (temp == curBest) {
                winnersOut.Add(p);
            }
        }
        return winnersOut.ToArray();
    }

    public void Flop(in int count=1) {
        if (count + communityCards.Count > 5) {
            Debug.LogError("Illegal Move: ComCards count surpases 5.");
            return;
        }

        for (int i = 0; i < count; ++i) {
            Card newCard = curDeck.First();
            curDeck.RemoveAt(0);
            communityCards.Add(newCard);
            int newCardIndex = _comCardsUI.FindIndex(image => image.texture == null);
            _comCardsUI[newCardIndex].texture = Card.cardAssets[(int)newCard.suit][(int)newCard.value];
        }
    }

    public void ClearCommunityCard() {
        communityCards.Clear();
        foreach (var image in _comCardsUI) {
            image.texture = null;
        }
        Debug.Log("Community cards cleared");
    }

    private void _ShuffleDeck(ref List<Card> deck) {
        int deckSize = deck.Count;
        for (int i = 0; i < deckSize; ++i) {
            Card tempCard = deck[i];
            int randIndex = UnityEngine.Random.Range(0, deck.Count);
            deck[i] = deck[randIndex];
            deck[randIndex] = tempCard;
        }
    }
}