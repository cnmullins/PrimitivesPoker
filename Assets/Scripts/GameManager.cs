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
        _balanceTexts = balancesGroup.GetComponentsInChildren<TMP_Text>();
        yield return new WaitUntil(() => {
            return arePlayersInScene;
        });
        //initialize UI
        actionGroup.SetActive(true);
        potText.text = "$0";
        _betText.text = "$" + ((float)Dealer.playersLL.First.Value.balance * 0.025f).ToString("F0");
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
            foreach (var player in curRotation) {
                player.SetHand(Dealer.instance.GetHand());
            }
            do { //start new hand here
                
                do { // start player turns
                    _currentPlayer = curPlayNode.Value;
                    curPlayNode.Value.ToggleBalanceHighlight(true);
                    //cycle through players until all active players have paid current bet or folded
                    //yield return wait for player input

                    print("caught before"); // caught below
                    if (curPlayNode.Value.isHuman) {
                        print("start await");
                        yield return new WaitUntil(delegate {
                            return curPlayNode.Value.handAction != PlayerAction.NoAction;
                        });
                        print("end await");
                    }
                    else {
                        //invalid cast exception
                        var bot = (BotPlayer)curPlayNode.Value;
                        print("Implement bot action");
                        //yield return bot.GenerateAction(curPlayNode.Value);
                    }
                    print("caught after"); //caught above

                    switch (curPlayNode.Value.handAction) {
                        case PlayerAction.Check: break;
                        case PlayerAction.Bet:
                            Dealer.pot += curPlayNode.Value.currentBet;
                            _balanceTexts[curPlayNode.Value.seatIndex].text = "$" + curPlayNode.Value.balance;
                            break;
                        case PlayerAction.Call:
                            curPlayNode.Value.currentBet = Dealer.communityBet;
                            _balanceTexts[curPlayNode.Value.seatIndex].text = "$" + curPlayNode.Value.balance;
                            break;
                        case PlayerAction.Fold:
                            curRotation.Remove(curPlayNode);
                            break;
                    }
                    curPlayNode.Value.ToggleBalanceHighlight(false);
                    curPlayNode = curPlayNode.Next ?? curRotation.First;
                } while (!HaveAllPlayersGone());

                print("Ooops 1"); //reached twice before "crashing"
                _ToggleCallButton(false);
                //check for river (count com cards)
                //if (Dealer.communityBet > 0)
                  //  Dealer.instance.GatherBetsToPot();

                if (Dealer.communityCards.Count == 0)
                    Dealer.instance.Flop(3);
                else
                    Dealer.instance.Flop();
                
                foreach (var p in curRotation)
                    p.handAction = PlayerAction.NoAction;

                print("oops 2"); // this is reached twice before "crashing"
            } while (Dealer.communityCards.Count < 5);

            print("Hand has finished");
            curRotation = Dealer.playersLL;
            curPlayNode = curRotation.Find(Dealer.curDealer) ?? curRotation.First;
            _EndCurrentHand();
            
            //declare winner, remove any players that have a 0 balance at the table
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
        var playingIndexs = new List<int>();
        for (int i = startAt; i < playerSpawnPos.Length; i += increment) {
            if (numOfPlayers-- < 1) break;
            playingIndexs.Add(i);
            SeatNewPlayerAt(playerSpawnPos[i], true);
        }
        //hide unused UI
        for (int i = 0; i < _balanceTexts.Length; ++i) {
            if (!playingIndexs.Exists(pI => pI == i))
                _balanceTexts[i].gameObject.SetActive(false);
            else
                _balanceTexts[i].text = "$" + startBalance;
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
        //print("I'm getting stuck here start");
        // increment all buttons
        //yield return StartCoroutine(Dealer.instance.dealerButton.IterateAsync());
        //yield return StartCoroutine(Dealer.instance.bigBlindButton.IterateAsync());
        //yield return StartCoroutine(Dealer.instance.smallBlindButton.IterateAsync());
        // start UI
        //print("I'm getting stuck here end");
        
    }

    private void _EndCurrentHand() {
        // dealer distributes pot to winner(s)

        Dealer.pot = 0;
        // dealer destroys hands
        Dealer.instance.ClearCommunityCard();

        // dealer restores deck
        
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
        //TODO ADJUST DEALER.POT
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
        //print("Call button toggled");
        //kill image and button
        //_callButton.enabled  = showing;
        //_checkButton.enabled = !showing;
        _callButton.gameObject.SetActive(showing);
        _checkButton.gameObject.SetActive(!showing);
        // adjust bet slider to only allow +callValue
        _callButton.GetComponentInChildren<TMP_Text>().text = (!showing && callVal == -1) ? "Call" : "Call:\n$" + callVal;
    }

#endregion
    /* END: PLAYER ACTION HANDLING THROUGH UI */

}
