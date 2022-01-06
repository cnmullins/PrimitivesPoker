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
    public GameObject actionGroup;
    public GameObject balancesGroup;
    public TMP_Text betValText;
    public Slider betSlider;
    public Button checkButton;
    public Button callButton;
    public TMP_Text betText;
    public Scrollbar decisionTimerBar;
    public float? time     { get { return _timer; } }
    public bool isTimeUp   { get { return _timer > _decisionTime; } }

    private float? _timer;
    private Slider _betSlider;
    private float _decisionTime;
    private TMP_Text[] _balanceTexts;

    private IEnumerator Start() {
        _timer   = null;
        _balanceTexts = balancesGroup.GetComponentsInChildren<TMP_Text>();
        yield return new WaitUntil(() => {
            return GameManager.arePlayersInScene;
        });

        actionGroup.SetActive(true);
        balancesGroup.SetActive(true);
        yield return new WaitForFixedUpdate();
        var tempIter   = GameManager.gameIterator;
        int startIndex = tempIter.getIndex;
        do {
            BasePlayer player = tempIter.currentPlayer?.Value;
            // only setup if not null
            player?.SetupBalanceText(_balanceTexts[player.seatIndex]);
            tempIter.Iterate();
        } while (tempIter.getIndex != startIndex);
        // hide any unused Balance Texts
        var seats = GameManager.instance.playerSpawnPos;
        for (int i = 0; i < seats.Length; ++i) {
            if (seats[i].childCount == 0) {
                _balanceTexts[i].text = string.Empty;
            }
        }
        yield return null;
    }
    // set PlayerActionUI through

    /* PLAYER ACTION HANDLING THROUGH UI */
#region PlayerActionUI    
    public void OnClick_Check() {
        GameManager.gameIterator.currentPlayer.Value.Check();
        GameManager.gameIterator.Iterate();
        MoveToNextPlayer(GameManager.gameIterator.getIndex);
    }

    public void OnClick_Fold() {
        GameManager.gameIterator.currentPlayer.Value.Fold();
        GameManager.gameIterator.Iterate();
        MoveToNextPlayer(GameManager.gameIterator.getIndex);
    }

    public void OnClick_Call() {
        GameManager.gameIterator.currentPlayer.Value.Call();
        GameManager.gameIterator.Iterate();
        MoveToNextPlayer(GameManager.gameIterator.getIndex);
    }

    public void OnClick_Bet() {
        GameManager.gameIterator.currentPlayer.Value.Bet(betSlider.value);
        GameManager.gameIterator.Iterate();
        MoveToNextPlayer(GameManager.gameIterator.getIndex);
    }

    public void OnSlide_Bet() {
        betValText.text = "$" + 
            (uint)(betSlider.value * (float)GameManager.gameIterator
                                            .currentPlayer.Value.balance);
    }

    private void _ToggleCallButton(bool showing) {
        callButton.enabled  = showing;
        checkButton.enabled = !showing;
        // adjust bet slider to only allow +callValue
    }

#endregion
    /* END: PLAYER ACTION HANDLING THROUGH UI */

    private void MoveToNextPlayer(int playerIndex) {
        //GameManager.gameIterator.Iterate();
        if (!Dealer.instance.IsRoundOver()) {
            
            return;
        }
        // reset UI values
        var moveTo = Camera.main.WorldToScreenPoint(
                    GameManager.gameIterator.currentPlayer.Value.transform.position);
        moveTo.y = actionGroup.transform.position.y;
        actionGroup.transform.position = moveTo;
        // move timer UI (will be incorporated later)
    }

    /// <summary>
    /// Turn the timer on or off based on the passing argument.
    /// </summary>
    /// <param name="setting">Turning on or off.</param>
    private void SetTimer(bool setting) {
        _timer = (setting) ? new float?(0f) : null;
    }
}
