/*
Author: Christian Mullins
Summary: Parent abstracts class for all player classes.
*/
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum PlayerAction {
    Check, Bet, Call, Fold, AllIn, NoAction
}

public abstract class BasePlayer : MonoBehaviour {
    public uint balance;
    public uint currentBet;
    public PlayerAction handAction;
    public Card[] hand          { get; private set; }
    public int seatIndex        { get; protected set; }
    public Vector3 buttonPos    { get; protected set; }

    public bool isHuman => GetType().Equals(typeof(HumanPlayer));

    protected PlayerObserver _uiObserver;
    protected const float BUTTON_DISTANCE = 2f;

    // must be Awake() for instantiation + initialization
    protected virtual void Awake() {
        char seatNum = transform.parent.name.Split('_')[1][0];
        _uiObserver = GameObject.FindGameObjectWithTag("SeatBalances").transform.
            Find("Seat" + seatNum + "_StatusUI").GetComponent<PlayerObserver>();
        hand = new Card[2];
        string parentStr = transform.parent.name;
        seatIndex = (parentStr[parentStr.Length - 1] - '0') - 1;
        balance = GameManager.instance.startBalance;
        currentBet = 0;
        handAction = PlayerAction.NoAction;

        // calculate buttonPos
        Vector3 tempPos = Camera.main.transform.position;
        tempPos.y = transform.position.y + 1f;
        tempPos = transform.position - tempPos;
        buttonPos = transform.position - Vector3.ClampMagnitude(tempPos, BUTTON_DISTANCE);
    }

    /* PLAYER ACTIONS */
    public void Check() {
        handAction = PlayerAction.Check;
    }

    public virtual void Bet(float sliderVal) {
        handAction = PlayerAction.Bet;
        uint betVal = (uint)(sliderVal * (float)balance);
        if (Dealer.communityBet > currentBet)
            betVal += (uint)Mathf.Clamp((betVal - Dealer.communityBet), 0, balance);
        currentBet = betVal;
        balance -= betVal;
        Dealer.pot += betVal;

        // handle if the player is going all-in
        if (sliderVal == 1f) {
            currentBet += balance;
            balance = 0;
            handAction = PlayerAction.AllIn;
        }
        _uiObserver.UpdateBalance(balance);
    }

    public virtual void Call() {
        handAction = PlayerAction.Call;
        uint callVal = (uint)Mathf.Min(Dealer.communityBet - currentBet, balance);
        balance -= callVal;
        Dealer.pot += callVal;
        currentBet += callVal;
        // handle if the player is going all-in
        if (balance <= callVal) { //FIX ME
            balance = 0;
            handAction = PlayerAction.AllIn;
        }
        _uiObserver.UpdateBalance(balance);
    }

    public virtual void Fold() {
        handAction = PlayerAction.Fold;
        currentBet = 0;
        _uiObserver.EnableHand(false);
        SetHand(null);
        // TODO: later implementation for the player
        //      Ask if the player would like to reveal their cards
    }

    public void SetHand(in Card[] newHand) {
        hand = newHand;
        _uiObserver.EnableHand(newHand != null);
        if (isHuman)
            _uiObserver.SetHandUI(newHand);
    }

    //toggled in GameManager Start() function
    public void ToggleBalanceHighlight(in bool active) {
        _uiObserver.balanceText.color = (active) ? Color.yellow : Color.white;
    }
}