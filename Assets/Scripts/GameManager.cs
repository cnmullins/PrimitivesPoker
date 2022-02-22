/*
Author: Christian Mullins
Summary: Singleton instance that manipulates the entire flow of the game.
*/
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
    //public static PlayerFocuser gameIterator;
    public static bool arePlayersInScene { get {
        return instance.playerSpawnPos.Any<Transform>(s => s.childCount > 0);
    } }
    public static bool isGameOver { get {
        return Dealer.playersLL.Count() <= 1;
    } }
    private List<PlayerAction> _actions;
    private int _numOfPlayers;

    private BasePlayer _currentPlayer;

    #region UI_Variables
    [Header("UI Variables")]
    public GameObject actionGroup;
    public GameObject balancesGroup;
    public TMP_Text betText;
    [SerializeField]
    private Button _checkButton;
    [SerializeField]
    private Button _callButton;
    [SerializeField]
    private TMP_Text _potText;
    [SerializeField]
    private Slider _betSlider;
    [SerializeField]
    private TMP_Text[] _balanceTexts;
    #endregion

    private IEnumerator Start() {
        Application.targetFrameRate = 60;
        instance = this;
        yield return new WaitUntil(() => {
            return arePlayersInScene;
        });
        //Handle ALL logic here
        do {
            yield return StartCoroutine(_StartNewHand());
            //bool cardFlipReady = false;
            do { //create new rotation with dealer button in mind & assign cards
                var curRotation = Dealer.playersLL;
                var curPlayNode = curRotation.First;
                while (curRotation.First.Value != Dealer.curDealer) {
                    if (!curRotation.First.Value.isActiveAndEnabled) {
                        curRotation.RemoveFirst();
                        continue;
                    }
                    curRotation.AddLast(curRotation.First);
                    curRotation.RemoveFirst();
                }
                foreach (var player in curRotation) {
                    player.hand = Dealer.instance.GetHand();
                    //player.Debug_PrintHand();
                }
                
                do { // start player turns
                    curPlayNode.Value.ToggleBalanceHighlight(true);
                    //cycle through players until all active players have paid current bet or folded
                    //yield return wait for player input
                    if (curPlayNode.Value.isHuman) {
                        yield return new WaitUntil(delegate {
                            print("Awaiting: " + curPlayNode.Value.transform.parent.name + " -> " + curPlayNode.Value.handAction);
                            return curPlayNode.Value.handAction != PlayerAction.NoAction
                            || curPlayNode.Value.handAction == PlayerAction.Fold;
                        });
                    }
                    else {
                        var bot = (BotPlayer)curPlayNode.Value;
                        print("Implement bot action");
                        //bot.GenerateAction();
                    }
                    switch (curPlayNode.Value.handAction) {
                        case PlayerAction.Check:
                            break;
                        case PlayerAction.Bet:
                            Dealer.communityBet = curPlayNode.Value.currentBet;
                            break;
                        case PlayerAction.Call:
                            curPlayNode.Value.currentBet = Dealer.communityBet;
                            break;
                        case PlayerAction.Fold:
                            break;
                    }
                    curPlayNode.Value.ToggleBalanceHighlight(false);
                    curPlayNode = curPlayNode.Next;
                //iterate until new card flip (check for uniform community bet)
                } while (Dealer.playersLL.First(p => p.currentBet != Dealer.communityBet));
                //check for river (count com cards)
                /*
                if (Dealer.communityCards.Count <= 4)
                    Dealer.
                */

                //while not river
            } while (Dealer.communityCards.Count < 5);

            _EndCurrentHand();

        } while (!isGameOver);
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
            increment++;
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
    private IEnumerator _StartNewHand() {
        Dealer.instance.GetNewDeck();
        yield return new WaitUntil(delegate() {
            return Dealer.curDeck.Count == 52;
        });
        foreach (var p in Dealer.playersLL) {
            p.currentBet = 0;
            p.hand = Dealer.instance.GetHand();
        }
        // increment all buttons
        //gameIterator.Iterate();

        //Dealer shift
        yield return StartCoroutine(Dealer.instance.dealerButton.IterateAsync());
        yield return StartCoroutine(Dealer.instance.bigBlindButton.IterateAsync());
        yield return StartCoroutine(Dealer.instance.smallBlindButton.IterateAsync());
        // start UI
        
    }

    private void _EndCurrentHand() {
        // dealer distributes pot to winner(s)

        Dealer.pot = 0;
        Dealer.communityBet = 0;
        // dealer destroys hands

        

        // dealer restores deck
        
    }

    private void _UpdatePlayerActions() {
        var playIt = Dealer.playersLL.First;
        /*
        for (int i = 0; i < _actions.Count; ++i) {
            _actions[i] = players[i].handAction;
        }*/
        int i = 0;
        do {
            _actions[i] = playIt.Value.handAction;
            playIt = playIt.Next;
        } while (playIt != null);
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
        foreach (var p in Dealer.playersLL) {
            if (p.handAction == PlayerAction.NoAction) 
                return false;
        }
        return true;
    }

        /* PLAYER ACTION HANDLING THROUGH UI */
#region PlayerActionUI    
    public void OnClick_Check() {

    }

    public void OnClick_Fold() {

    }

    public void OnClick_Call() {
        //ADD ACTIONS HERE

        //adjust player balance UI
        _balanceTexts[_currentPlayer.seatIndex].text = "$" + _currentPlayer.balance;
        //adjust pot
        int newBet = Mathf.RoundToInt(_betSlider.value * (float)_currentPlayer.balance);
        _potText.text = "$" + newBet;
    }

    public void OnClick_Bet() {
        _currentPlayer.Bet(_betSlider.value);
        //adjust player balance UI
        _balanceTexts[_currentPlayer.seatIndex].text = "$" + _currentPlayer.balance;
        //adjust pot
        int newBet = Mathf.RoundToInt(_betSlider.value * (float)_currentPlayer.balance);
        _potText.text = "$" + newBet;
    }

    public void OnSlide_Bet() {
        betText.text = "$" + 
            (uint)(_betSlider.value * (float)_currentPlayer.balance);
    }

    private void _ToggleCallButton(bool showing) {
        _callButton.enabled  = showing;
        _checkButton.enabled = !showing;
        // adjust bet slider to only allow +callValue
    }

#endregion
    /* END: PLAYER ACTION HANDLING THROUGH UI */

}
