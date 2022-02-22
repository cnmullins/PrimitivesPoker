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
    public Card[] hand  { 
        get { return _hand; }
        set { _hand = value;
            if (isHuman)
                _uiObserver.SetHandUI(_hand);
    } }
    public int seatIndex        { get; protected set; }
    public Vector3 buttonPos    { get; protected set; }

    public bool isHuman => GetType().Equals(typeof(HumanPlayer));

    protected PlayerObserver _uiObserver;
    protected const float BUTTON_DISTANCE = 2f;
    private Card[] _hand;

    // must be Awake() for instantiation + initialization
    protected virtual void Awake() {
        char seatNum = transform.parent.name.Split('_')[1][0];
        _uiObserver = GameObject.FindGameObjectWithTag("SeatBalances").transform.
            Find("Seat" + seatNum + "_StatusUI").GetComponent<PlayerObserver>();
        _hand = new Card[2];
        string parentStr = transform.parent.name;
        seatIndex = (parentStr[parentStr.Length - 1] - '0') - 1;
        balance = GameManager.instance.startBalance;
        currentBet = 0;
        handAction = PlayerAction.NoAction;

        // calculate buttonPos
        Vector3 tempPos = Camera.main.transform.position;
        tempPos.y = transform.position.y + 1f;
        tempPos = transform.position - tempPos;
        buttonPos = transform.position 
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
        if (sliderVal == 1f) {
            currentBet += balance;
            balance = 0;
            handAction = PlayerAction.AllIn;
        }

        _uiObserver.UpdateBalance(balance);
    }

    public virtual void Call() {
        handAction = PlayerAction.Call;
        uint callVal = Dealer.communityBet - currentBet;
        currentBet += callVal;
        balance -= callVal;
        // handle if the player is going all-in
        /*
        FIX ME
        if (balance <= callVal) {
            balance = 0;
            handAction = PlayerAction.AllIn;
        }
        */
        _uiObserver.UpdateBalance(balance);
    }

    public virtual void Fold() {
        handAction = PlayerAction.Fold;
        // remove from the rotation (and release any bet to the community pot?)
        Dealer.communityBet += currentBet;
        currentBet = 0;
        // TODO: later implementation for the player
        //      Ask if the player would like to reveal their cards

    }

    /// <summary>
    /// Transfer to a List for ease of adding to another list for scoring.
    /// </summary>
    /// <returns></returns>
    public List<Card> GetHandAsList() {
        return new List<Card>(new Card[2] { hand[0], hand[1] });
    }

    public void SetupBalanceText(TMP_Text text) {
        if (text.TryGetComponent<PlayerObserver>(out var pO)) {
            pO.UpdateBalance(balance);
            _uiObserver = pO;
        }
    }

    //toggled in GameManager Start() function
    public void ToggleBalanceHighlight(in bool active) {
        _uiObserver.balanceText.color = (active) ? Color.yellow : Color.white;
    }

    public void Debug_PrintHand() {
        print(name + ": " + hand[0].ToString() + ", " + hand[1]);
    }
}

