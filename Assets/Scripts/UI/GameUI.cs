/*
Author: Christian Mullins
Summary: Controller that maniupulates the In Game UI;
*/
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Canvas))]
public class GameUI : MonoBehaviour {
    public static GameUI instance; //singleton

    public GameObject actionGroup;
    public GameObject balancesGroup;
    public TMP_Text betValText;
    public TMP_Text potText;
    public Slider betSlider;
    public Button checkButton;
    public Button callButton;
    public TMP_Text betText;
    public Scrollbar decisionTimerBar; // NOTE: not yet implemented
    public float? timer    { get; private set; }
    public bool isTimeUp   { get { return timer > _decisionTime; } }
    public LinkedListNode<BasePlayer> iter { get; private set; }

    private Slider _betSlider;
    private float _decisionTime;
    private TMP_Text[] _balanceTexts;
    private int _curIterIndex => new List<BasePlayer>(Dealer.playersLL).IndexOf(iter.Value);

    private IEnumerator Start() {
        timer = null;
        instance = this;
        _balanceTexts = balancesGroup.GetComponentsInChildren<TMP_Text>();
        yield return new WaitUntil(() => {
            return GameManager.arePlayersInScene;
        });

        iter = Dealer.playersLL.First;
        actionGroup.SetActive(true);
        balancesGroup.SetActive(true);
        yield return new WaitForFixedUpdate();
        var tempIter   = Dealer.playersLL.First;
        //int startIndex = tempIter.getIndex;
        do {
            BasePlayer player = tempIter.Value;
            // only setup if not null
            player?.SetupBalanceText(_balanceTexts[player.seatIndex]);
            tempIter = tempIter.Next;//tempIter.Va.Iterate();
        } while (tempIter != null);
        // hide any unused Balance Texts
        var seats = GameManager.instance.playerSpawnPos;
        for (int i = 0; i < seats.Length; ++i) {
            if (seats[i].childCount == 0) {
                _balanceTexts[i].text = string.Empty;
            }
        }
        betValText.text = "$0";
        potText.text = "$0";
    }
    // set PlayerActionUI through

    /* PLAYER ACTION HANDLING THROUGH UI */
#region PlayerActionUI    
    public void OnClick_Check() {
        iter.Value.Check();
        iter = iter.Next;
        _MoveToCurrentPlayer();
    }

    public void OnClick_Fold() {
        iter.Value.Fold();
        iter = iter.Next;
        _MoveToCurrentPlayer();
    }

    public void OnClick_Call() {
        iter.Value.Call();
        iter = iter.Next;
        _MoveToCurrentPlayer();
        //adjust player balance UI
        _balanceTexts[_curIterIndex].text = "$" + iter.Value.balance;
        //adjust pot
        int newBet = Mathf.RoundToInt(_betSlider.value * (float)iter.Value.balance);
        potText.text = "$" + newBet;
    }

    public void OnClick_Bet() {
        iter.Value.Bet(betSlider.value);
        iter = iter.Next;
        _MoveToCurrentPlayer();
        //adjust player balance UI
        _balanceTexts[_curIterIndex].text = "$" + iter.Value.balance;
        //adjust pot
        int newBet = Mathf.RoundToInt(_betSlider.value * (float)iter.Value.balance);
        potText.text = "$" + newBet;
    }

    public void OnSlide_Bet() {
        betValText.text = "$" + 
            (uint)(betSlider.value * (float)iter.Value.balance);
    }

    private void _ToggleCallButton(bool showing) {
        callButton.enabled  = showing;
        checkButton.enabled = !showing;
        // adjust bet slider to only allow +callValue
    }

#endregion
    /* END: PLAYER ACTION HANDLING THROUGH UI */

    private void _MoveToCurrentPlayer() {
        //GameManager.gameIterator.Iterate();
        if (!Dealer.instance.IsRoundOver()) {
            
            return;
        }
        // reset UI values
        var moveTo = Camera.main.WorldToScreenPoint(
                    iter.Value.transform.position);
        moveTo.y = actionGroup.transform.position.y;
        actionGroup.transform.position = moveTo;
        // move timer UI (will be incorporated later)
    }

    /// <summary>
    /// Turn the timer on or off based on the passing argument.
    /// </summary>
    /// <param name="setting">Turning on or off.</param>
    private void _SetTimer(bool setting) {
        timer = (setting) ? new float?(0f) : null;
    }
}
