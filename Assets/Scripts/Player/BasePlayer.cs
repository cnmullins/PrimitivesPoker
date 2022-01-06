/*
Author: Christian Mullins
Summary: Parent class for all player classes.
*/
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum PlayerAction {
    Check, Bet, Call, Fold, AllIn, NoAction
}

public class BasePlayer : MonoBehaviour {
    public uint balance;
    public uint currentBet;
    public PlayerAction handAction;
    public Tuple<Card, Card> hand;
    public string balanceTextStr { 
        get { return (_balanceText == null) 
                            ? "$   " : _balanceText.text; }
        set { _balanceText.text = value; } 
    }
    public int seatIndex        { get { return _seatIndex; } }
    public Vector3 buttonPos    { get { return _buttonPos; } }
    protected TMP_Text _balanceText;
    protected Vector3 _buttonPos;
    protected int _seatIndex = -1;
    
    protected const float BUTTON_DISTANCE = 2f;

    // must be Awake() for instantiation + initialization
    protected virtual void Awake() {
        string parentStr = transform.parent.name;
        _seatIndex = (parentStr[parentStr.Length - 1] -  '0') - 1;
        balance = GameManager.instance.startBalance;
        currentBet = 0;
        handAction = PlayerAction.NoAction;

        // calculate buttonPos
        Vector3 tempPos = Camera.main.transform.position;
        tempPos.y = transform.position.y + 1f;
        tempPos = transform.position - tempPos;
        _buttonPos = transform.position 
                   - Vector3.ClampMagnitude(tempPos, BUTTON_DISTANCE);
    }

    /* PLAYER ACTIONS */
    public virtual void Check() {
        handAction = PlayerAction.Check;
    }

    public virtual void Bet(float sliderVal) {
        handAction = PlayerAction.Bet;
        uint betVal = (uint)(sliderVal * (float)balance);
        currentBet = betVal;
        balance -= betVal;

        // reset others hand actions
        if (Dealer.communityBet > 0)
            Dealer.communityBet = betVal;
        // handle if the player is going all-in
        if (balance < 1) {
            balance = 0;
            handAction = PlayerAction.AllIn;
        }
    }

    public virtual void Call() {
        handAction = PlayerAction.Call;
        uint callVal = Dealer.communityBet - currentBet;
        currentBet += callVal;
        balance -= callVal;
        // handle if the player is going all-in
        if (balance < 1) {
            balance = 0;
            handAction = PlayerAction.AllIn;
        }
    }

    public virtual void Fold() {
        handAction = PlayerAction.Fold;
        // remove from the rotation (and release any bet to the community pot?)

        // later implementation for the player
        //      Ask if the player would like to reveal their cards
    }

    /// <summary>
    /// Transfer to a List for ease of adding to another list for scoring.
    /// </summary>
    /// <returns></returns>
    public List<Card> GetHandAsList() {
        var asList = new List<Card>();
        asList.Add(hand.Item1);
        asList.Add(hand.Item2);
        return asList;
    }

    public void SetupBalanceText(TMP_Text text) {
        _balanceText = text;
        text.text = "$" + balance;
    }

    public void SetupBalanceTextColor() {
        if (_balanceText == null) return;
        if (this == GameManager.gameIterator.currentPlayer.Value)
            _balanceText.color = Color.yellow;
        else
            _balanceText.color = Color.white;
    }
}

