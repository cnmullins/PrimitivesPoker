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
#if UNITY_EDITOR
    public const bool DEBUG_MODE = false;
#endif
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
    public static bool arePlayersInScene { get {
        return instance.playerSpawnPos.Any<Transform>(s => s.childCount > 0);
    } }
    public static bool isGameOver { get {
        return Dealer.playersLL.Count <= 1;
    } }
    private int _numOfPlayers;

    private BasePlayer _currentPlayer;

    #region UI_Variables
    [Header("UI Variables")]
    public TMP_Text potText;
    public GameObject actionGroup;
    public GameObject balancesGroup;
    [SerializeField]
    public TMP_Text _betText;
    [SerializeField]
    private Button _checkButton;
    [SerializeField]
    private Button _callButton;
    [SerializeField]
    private Slider _betSlider;
    private TMP_Text[] _balanceTexts;
    #endregion

    private IEnumerator Start() {
        Application.targetFrameRate = 60;
        instance = this;
        _balanceTexts = (from text in balancesGroup.GetComponentsInChildren<TMP_Text>()
            where text.name.Contains("StatusUI")
            select text).ToArray();

        yield return new WaitUntil(() => {
            return arePlayersInScene;
        });
        //initialize UI
        actionGroup.SetActive(true);
        potText.text = "$0";
        _ToggleCallButton(false);
        //Handle ALL logic here
        do {
            yield return StartCoroutine(_StartNewHand());
            
            var curRotation = Dealer.playersLL;
            var curPlayNode = curRotation.First;
            while (curRotation.First.Value != Dealer.curDealer) {
                if (!curRotation.First.Value.isActiveAndEnabled) {
                    curRotation.RemoveFirst();
                    continue;
                }
                var tempNode = curRotation.First;
                curRotation.RemoveFirst();
                curRotation.AddLast(tempNode);
            }
#if UNITY_EDITOR
            bool debugMode = true;
            if (debugMode) {
                
            }
#endif            
            // deal cards
            foreach (var player in curRotation) {
                player.SetHand(Dealer.instance.GetHand());
            }
            
            do { //start new hand here
                
                do { // start player turns
                    _currentPlayer = curPlayNode.Value;
                    curPlayNode.Value.ToggleBalanceHighlight(true);
                    //cycle through players until all active players have paid current bet or folded
                    //yield return wait for player input

                    if (curPlayNode.Value.isHuman) {
                        yield return new WaitUntil(delegate {
                            
                            return curPlayNode.Value.handAction != PlayerAction.NoAction;
                        });
                    }
                    else {
                        //invalid cast exception
                        var bot = (BotPlayer)curPlayNode.Value;
                        //var bot = (BotPlayer)curPlayNode.Value;
                        print("Implement bot action");
                    }

                    switch (curPlayNode.Value.handAction) {
                        case PlayerAction.Check: break;
                        case PlayerAction.Bet:
                            Dealer.AddToPot(curPlayNode.Value.currentBet);
                            break;
                        case PlayerAction.Call:
                            Dealer.AddToPot(Dealer.communityBet - curPlayNode.Value.currentBet);
                            curPlayNode.Value.currentBet = Dealer.communityBet;
                            break;
                        case PlayerAction.Fold:
                            break;
                    }
                    curPlayNode.Value.ToggleBalanceHighlight(false);
                    curPlayNode = curPlayNode.Next ?? curRotation.First;
                } while (!HaveAllPlayersGone());

                _ToggleCallButton(false);
                //check for river (count com cards)
                if (Dealer.communityCards.Count == 0)
                    Dealer.instance.Flop(3);
                else
                    Dealer.instance.Flop();
                //check for remaining players if a lot of people have folded
                int foldCount = 1;
                foreach (var p in curRotation) {
                    if (p.handAction != PlayerAction.Fold) {
                        p.handAction = PlayerAction.NoAction;
                    }
                    else  {
                        if (foldCount >= curRotation.Count - 1) {
                            //call current player the winner
                            p.Award(Dealer.pot);
                            goto endHand;
                        }
                        foldCount++;
                    }   
                }
            } while (Dealer.communityCards.Count < 5);
            //declare winner
            int winID = Dealer.instance.DeclareWinner();
#if DEBUG || UNITY_EDITOR            
            DebugTools.ScoreToTextFile(Dealer.communityCards.ToArray(), curRotation.ToArray());
#endif
            if (winID == -1) {
                print("A TIE HAS OCCURED!!");
                var winners = Dealer.instance.GetMultipleWinners();
                foreach (var w in winners)
                    w.Award(Dealer.pot / (uint)winners.Length);
            }
            else {
                var winningPlayer = Dealer.playersLL.ToArray()[winID];
                print("WINNER IS: " + winningPlayer.playerName);
                winningPlayer.Award(Dealer.pot);
            }
            

            endHand : {
                foreach (var p in curRotation) {
                    p.handAction = PlayerAction.NoAction;
                }
                curRotation = Dealer.playersLL;
                curPlayNode = curRotation.Find(Dealer.curDealer) ?? curRotation.First;
                _EndCurrentHand();
            }
            
            //declare winner, remove any players that have a 0 balance at the table
        } while (!isGameOver);
    }

    /// <summary>
    /// Seat number of players in "randomized" positions.
    /// </summary>
    /// <param name="numOfPlayers">Number of players in the game.</param>
    public void StartNewGame(int numOfPlayers) {
        _numOfPlayers = numOfPlayers;
        bool[] seating = Enumerable.Repeat(false, 7).ToArray();
                
        for (int i = 0; i < numOfPlayers; ++i) {
            seating[i] = true;
            /*
            FIX LATER, it's causing infinite loop
            int randSeat = Random.Range(0, seating.Length);
            if (!seating[randSeat]) {
                seating[randSeat] = true;
            } else {
                int curIndex = i;
                while (true) {
                    if (curIndex >= seating.Length - 1) {
                        curIndex = 0;
                    } else if (seating[curIndex] || i < seating.Length - 1) {
                        ++curIndex;
                    } else {
                        seating[curIndex] = true;
                        break; // from while loop
                    }
                }
            }
            */
        }
        //TODO: shuffle?
        
        if (Random.value > 0.5f)
            seating.Reverse();
        for (int i = 0, pCounter = 1; i < seating.Length; ++i) {
            if (seating[i]) {
                var newP = SeatNewPlayerAt(playerSpawnPos[i], true, pCounter);
                //assign observers
                newP.name += (pCounter++).ToString();
                //Debug.Log("balanceTextObj_" + i + ": " + _balanceTexts[i]);
                newP.SetUIObserver(_balanceTexts[i].GetComponent<PlayerObserver>());
            } else {
                //deactivate unused
                _balanceTexts[i].gameObject.SetActive(false);
            }
        }
        Dealer.instance.InitializeValues();
    }

    /// <summary>
    /// Reset all values, get new deck, pass cards, and pass the buttons.
    /// </summary>
    private IEnumerator _StartNewHand() {
        Dealer.instance.GetNewDeck();
        yield return new WaitUntil(delegate() {
            //print("awaiting for full Deck");
            return Dealer.curDeck.Count == 52;
        });
        foreach (var p in Dealer.playersLL) {
            p.currentBet = 0;
            p.SetHand(Dealer.instance.GetHand());
        }
        //increment all buttons
        yield return StartCoroutine(Dealer.instance.dealerButton.IterateAsync());
        yield return StartCoroutine(Dealer.instance.bigBlindButton.IterateAsync());
        yield return StartCoroutine(Dealer.instance.smallBlindButton.IterateAsync());
        // start UI
        
    }

    private void _EndCurrentHand() {
        // dealer distributes pot to winner(s)

        Dealer.ClearPot();
        // dealer destroys hands
        Dealer.instance.ClearCommunityCard();

        // dealer restores deck
        
    }

    /// <summary>
    /// Instatiate the right gameObject in the appropriate position.
    /// </summary>
    /// <param name="spawnPos">Where to instantiate.</param>
    /// <param name="isHuman">Is human or bot player.</param>
    public BasePlayer SeatNewPlayerAt(Transform spawnPos, bool isHuman, int playerID=-1) {
        GameObject seatingPlayer = (isHuman) ? humanPrefab : botPrefab;
        GameObject newPlayer = Instantiate(seatingPlayer, spawnPos);
        BasePlayer playerObj = newPlayer.GetComponent<BasePlayer>();
        if (isHuman) {
            newPlayer.name = playerObj.SetHumanName(playerID);
        } else {
            newPlayer.name = playerObj.SetRandomizedName();
        }
        //Debug.Log("new player entered: " + playerObj.playerName);
        Vector3 lookPos = Camera.main.transform.position;
        //face dealer
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
        _currentPlayer.Check();
        _betSlider.value = 0f;
    }

    public void OnClick_Fold() {
        _currentPlayer.Fold();
        _betSlider.value = 0f;
    }

    public void OnClick_Call() {
        _currentPlayer.Call();

        //adjust player balance UI
        _balanceTexts[_currentPlayer.seatIndex].text = "$" + _currentPlayer.balance;
        //adjust pot
        _betSlider.value = 0f;
    }

    public void OnClick_Bet() {
        float sliderVal = Mathf.Clamp(_betSlider.value, 0.025f, 1f);
        _currentPlayer.Bet(sliderVal);
        //adjust player balance UI
        _balanceTexts[_currentPlayer.seatIndex].text = "$" + _currentPlayer.balance;
        //adjust pot
        int newBet = Mathf.RoundToInt(sliderVal * (float)_currentPlayer.balance);
        //TODO THE
        //get next player in the rotation
        var nextPlayer = Dealer.playersLL.Find(_currentPlayer).Next?.Value ?? Dealer.playersLL.First.Value;
        int callVal = Mathf.Min((int)Dealer.communityBet, (int)nextPlayer.balance);
        print("callVal: " + callVal);
        //TODO ADJUST DEALER.POT
        _ToggleCallButton(true, callVal); // force players to call instead of check
        _betSlider.value = 0f;

        foreach (var p in Dealer.playersLL) {
            if (p.handAction != PlayerAction.Fold && !p.Equals(_currentPlayer))
                p.handAction = PlayerAction.NoAction;
        }
    }

    public void OnSlide_BetCall() {
        //TODO:
            //ADJUST FOR CALL REQUIREMENTS
        float sliderVal = Mathf.Clamp(_betSlider.value, 0.025f, 1f);
        _betText.text = "$";
        if (!_callButton.gameObject.activeInHierarchy) { // bet
            //check if player will bet on top of another players bet
            if (Dealer.communityBet > 0) {
                _betText.text += (sliderVal * ((float)_currentPlayer.balance + Dealer.communityBet)).ToString("F0");
            } else {
                _betText.text += (sliderVal * (float)_currentPlayer.balance).ToString("F0");
            }
        } else { // call
        print("call slider");
            float clampedCallVal = Mathf.Clamp(sliderVal * (_currentPlayer.balance + Dealer.communityBet), 
                                               Dealer.communityBet, 
                                               _currentPlayer.balance);
            _betText.text += clampedCallVal.ToString("F0");
        }
    }

    private void _ToggleCallButton(bool showing, in int callVal=-1) {
        //kill image and button
        _callButton.gameObject.SetActive(showing);
        _checkButton.gameObject.SetActive(!showing);
        // adjust bet slider to only allow +callValue
        _callButton.GetComponentInChildren<TMP_Text>().text = (!showing && callVal == -1) ? "Call" : "Call:\n$" + callVal;
    }

#endregion
    /* END: PLAYER ACTION HANDLING THROUGH UI */
}
