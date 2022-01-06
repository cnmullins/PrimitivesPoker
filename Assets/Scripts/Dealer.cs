/*
Author: Christian Mullins
Summary: Static dealer class to handle each hand.
*/
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Dealer : MonoBehaviour {
    public static Dealer instance;
    public static uint pot;
    public BasePlayer curDealer;
    public static uint communityBet;
    // delegates
    public List<Card> communityCards   { get { return _communityCards; } }
    public List<Card> curDeck          { get { return _curDeck; } }
    public List<BasePlayer> players    { get { return _players; } }
    public Blind bigBlindButton        { get { return _bigBlindButton; } }
    public Blind smallBlindButton      { get { return _smallBlindButton; } }
    public Blind dealerButton { get { return _dealerButton; } }

    private List<Card> _communityCards;
    private List<Card> _curDeck;
    private List<BasePlayer> _players;
    private Blind _smallBlindButton;
    private Blind _bigBlindButton;
    private Blind _dealerButton;

    private void Start() {
        instance = this;
        _curDeck = new List<Card>();
        _communityCards = new List<Card>();
    }

    public void InitializeValues() {
        _players = (from player in GameManager.instance.playerSpawnPos
                    where player.transform.childCount > 0
                    select player.GetChild(0).GetComponent<BasePlayer>()).ToList();

        // set buttons at their starting index
        foreach (var b in FindObjectsOfType<Blind>()) {
            b.transform.position = _players[0].buttonPos;
            if (b.name == "DealerButton") {
                _dealerButton = b;
                continue;
            }
            StartCoroutine(b.IterateAsync());
            if (b.name == "SmallBlindButton") {
                _smallBlindButton = b;
                continue; 
            }
            StartCoroutine(b.IterateAsync());
            _bigBlindButton = b;
        }
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
        ShuffleDeck(ref deck);
        deck.Sort((Card a, Card b) => {
            return UnityEngine.Random.Range(-1, 2);
        });
        _curDeck = deck;
    }

    /// <summary>
    /// Construct a hand from the current deck and remove cards from deck
    /// </summary>
    /// <returns>New hand.</returns>
    public Tuple<Card, Card> GetHand() {
        var hand = new Tuple<Card, Card>(_curDeck[0], _curDeck[1]);
        _curDeck.Remove(hand.Item1);
        _curDeck.Remove(hand.Item2);
        return hand;
    }

    /// <summary>
    /// Take all the current "in-hand" bets and moves them to the dealer for
    /// the community pot.
    /// </summary>
    public void GatherBetsToPot() {
        foreach (var p in players) {
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
        foreach (var p in players) {
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
        var findMax = _players.ToList<BasePlayer>();
        //_players.Max<BasePlayer>();
        foreach (var p in _players) {
            var currentHand = new List<Card>(p.GetHandAsList());
            currentHand.AddRange(_communityCards.ToList<Card>());
            //currentHand.Max<BasePlayer>(x => Card.ScoreHand(x));
            int temp = Card.ScoreHand(currentHand);
            if (temp < currentWinner) {
                currentWinner = temp;
            }
        }
        return winningIndex;
    }

    public void DisplayCommunityCards(int numToDisplay) {
        while (numToDisplay-- > 0) {
            //display front then remove
            _communityCards.RemoveAt(0);
        }
    }

    private void ShuffleDeck(ref List<Card> deck) {
        int deckSize = deck.Count;
        for (int i = 0; i < deckSize; ++i) {
            Card tempCard = deck[i];
            int randIndex = UnityEngine.Random.Range(0, deck.Count);
            deck[i] = deck[randIndex];
            deck[randIndex] = tempCard;
        }
    }
}