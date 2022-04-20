/*
Author: Christian Mullins
Summary: Player child of BasePlayer that deals with AI 
    functionality in the game.
*/
using System;
using System.Threading.Tasks;
using Random = UnityEngine.Random;
using UnityEngine;


public enum Difficulty {
    Easy, Normal, Hard
}

public class BotPlayer : BasePlayer {

    public Difficulty difficulty = Difficulty.Normal;
    [Range(0, 5)]
    public int waitTime = 2;

    public bool isBluffing { get; private set; } // disable for easy

    private int _confidenceVal; //range from 0-10
    protected override void Awake() {
        base.Awake();
        isBluffing = false;
    }

    public override void Fold() {
        base.Fold();
        isBluffing = false;
    }

    public PlayerAction GetBotDecision() {
        var action = PlayerAction.Check;
        int curScore = Card.ScoreHand(Dealer.communityCards.ToArray(), hand);
        //action w/ no community cards
        if (Dealer.communityCards.Count == 0) {
            //if scored for a pair or a face high card
            if (curScore > 150 || Card.SequenceCheck(hand)) { //check for pair or better
                if (Dealer.communityBet <= Dealer.instance.bigBlindButton.buyIn)
                    action = PlayerAction.Call;
                else
                    action = (Random.Range(0, 4) <= (int)difficulty) ? PlayerAction.Call : PlayerAction.Fold;   
            }
            return action;
        }
        /*
            -Assess situation, get confidence
            -Assess balance and bet
            -Assess relative card score
        */



        if (Dealer.communityBet > 0) {
            //check if low confidence level
        }

        return action;
    }
}
