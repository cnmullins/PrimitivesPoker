/*
Author: Christian Mullins
Summary: Singleton instance that manipulates the entire flow of the game.
*/
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {
    public static GameManager instance;
    [Range(200, 500)]
    public uint startBalance;
    [Range(10f, 20f)]
    public static float playerDecisionTime;
    [Header("Prefab variables")]
    public GameObject humanPrefab;
    public GameObject botPrefab;
    public GameObject dealerButtonPrefab;
    public GameObject smallBlindButtonPrefab;
    public GameObject bigBlindButtonPrefab;
    public Transform[] playerSpawnPos;
    [Header("Card Suit Image")]
    public static Material[] cardSuitMats;
    public static PlayerFocuser gameIterator;
    public static bool arePlayersInScene { get {
        return instance.playerSpawnPos.Any<Transform>(s => s.childCount > 0);
    } }

    private List<PlayerAction> _actions;
    private int _numOfPlayers;
    
    private IEnumerator Start() {
        instance = this;
        // *** maybe try to assign this as 'new PlayerFocuser()'
        yield return new WaitUntil(() => {
            return arePlayersInScene;
        });
        gameIterator = new PlayerFocuser(new List<BasePlayer>(
                            from seat in GameManager.instance.playerSpawnPos
                            where seat.childCount > 0
                            select seat.GetChild(0).GetComponent<BasePlayer>()));
    }

    /// <summary>
    /// Seat number of players in "randomized" positions.
    /// </summary>
    /// <param name="numOfPlayers">Number of players in the game.</param>
    public void StartNewGame(int numOfPlayers) {
        _numOfPlayers = numOfPlayers;
        int increment = 1;
        int startAt = 0;
        // determine seat spacing
        if (numOfPlayers <= 3){     
            startAt = Random.Range(0, 3);
            increment += 1;
        }
        else if (numOfPlayers < 6) {
            startAt = Random.Range(0, 2);
        }
        for (int i = startAt; i < playerSpawnPos.Length; i += increment) {
            if (numOfPlayers-- < 1) break;
            SeatNewPlayerAt(playerSpawnPos[i], true);
        }
        Dealer.instance.InitializeValues();
        
        //_gameIterator
    }

    /// <summary>
    /// Reset all values, get new deck, pass cards, and pass the buttons.
    /// </summary>
    private void StartNewHand() {
        Dealer.instance.GetNewDeck();
        foreach (var p in Dealer.instance.players) {
            p.currentBet = 0;
            p.hand = Dealer.instance.GetHand();
        }
        // increment all buttons
        gameIterator.Iterate();
        Dealer.instance.dealerButton.Iterate();
        Dealer.instance.bigBlindButton.Iterate();
        Dealer.instance.smallBlindButton.Iterate();
        // start UI
        
    }

    private void EndCurrentHand() {
        // dealer distributes pot to winner(s)

        Dealer.pot = 0;
        // dealer destroys hands

        // dealer restores deck
    }

    private void UpdatePlayerActions() {
        var players = Dealer.instance.players;
        for (int i = 0; i < _actions.Count; ++i) {
            _actions[i] = players[i].handAction;
        }
    }

    /// <summary>
    /// Instatiate the right gameObject in the appropriate position.
    /// </summary>
    /// <param name="spawnPos">Where to instantiate.</param>
    /// <param name="isHuman">Is human or bot player.</param>
    public BasePlayer SeatNewPlayerAt(Transform spawnPos, bool isHuman) {
        GameObject seating = (isHuman) ? humanPrefab : botPrefab;
        var newPlayer = Instantiate(seating, spawnPos) as GameObject;
        var lookPos = Camera.main.transform.position;
        lookPos.y = spawnPos.position.y;
        newPlayer.transform.LookAt(lookPos, newPlayer.transform.up);
        return newPlayer.GetComponent<BasePlayer>();
    }

    /// <summary>
    /// Iterate through every player and see if everyone has gone.
    /// </summary>
    /// <returns>If all players are gone or not.</returns>
    public static bool HaveAllPlayersGone() {
        foreach (var p in Dealer.instance.players) {
            if (p.handAction == PlayerAction.NoAction) 
                return false;
        }
        return true;
    }
}
