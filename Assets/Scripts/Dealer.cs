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
    public static uint pot;
    public static BasePlayer curDealer;
    public static uint communityBet;
    public GameObject comCardsParent;
    // delegates
    public static List<Card> communityCards   { get; private set; }
    public static List<Card> curDeck          { get; private set; }
    public static LinkedList<BasePlayer> playersLL    { get; private set; }
    public Blind bigBlindButton               { get; private set; }
    public Blind smallBlindButton             { get; private set; }
    public Blind dealerButton                 { get; private set; }

    private List<RawImage> _comCardsUI; //communityCardsUI

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
        // set up the pot
        pot = 0;
        
    }

    /// <summary>
    /// Loop through all suites and values to create a new deck, shuffle,
    /// then assign to self.
    /// </summary>
    /// <returns>New shuffled deck.</returns>
    public void GetNewDeck() {
        var deck = new List<Card>();
        // loop through all suites and values
        for (int s = 0; s < 4; ++s) {
            for (int v = 0; v < 13; ++v) {
                deck.Add(new Card((Suit)s, (Value)v));
            }
        }
        // shuffle
        _ShuffleDeck(ref deck);
        deck.Sort((Card a, Card b) => {
            return UnityEngine.Random.Range(-1, 2);
        });
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

    /// <summary>
    /// Take all the current "in-hand" bets and moves them to the dealer for
    /// the community pot.
    /// </summary>
    public void GatherBetsToPot() {
        foreach (var p in playersLL) {
            pot += p.currentBet;
            p.currentBet = 0;
        }
    }

    /// <summary>
    /// Check every player's action state and only return true if 
    /// everyone has gone.
    /// </summary>
    /// <returns>Is the round over or not.</returns>
    public bool IsRoundOver() {
        foreach (var p in playersLL) {
            if (p.handAction == PlayerAction.NoAction)
                return false;
        }
        return true;
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
        foreach (var p in playersLL) {
            var currentHand = new List<Card>(p.GetHandAsList());
            currentHand.AddRange(communityCards.ToList<Card>());
            //currentHand.Max<BasePlayer>(x => Card.ScoreHand(x));
            int temp = Card.ScoreHand(currentHand);
            if (temp < currentWinner) {
                currentWinner = temp;
            }
        }
        return winningIndex;
    }

    public void Flop(in int count=1) {
        if (count + communityCards.Count > 5) {
            Debug.LogError("Illegal Mechanic: ComCards count surpases 5.");
            return;
        }
        for (int i = 0; i < count; ++i) {
            communityCards.Add(curDeck.First());
            //TODO: update _comCardsUI
            int newImageIndex = _comCardsUI.FindIndex(c => c.material == null);
            _comCardsUI[newImageIndex] = Card.GetCardImage(curDeck.First());
            curDeck.RemoveAt(0);
        }
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