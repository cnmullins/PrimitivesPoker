/*
Author: Christian Mullins
Summary: Player child of BasePlayer that deals with AI 
    functionality in the game.
*/
using System;
using System.Threading.Tasks;
using UnityEngine;


public enum Difficulty {
    Easy, Normal, Hard
}

public class BotPlayer : BasePlayer {

    public Difficulty difficulty = Difficulty.Normal;
    [Range(0, 5)]
    public int waitTime = 2;
/*
    // remove later if this is never used
    protected override void Start() {
        base.Start();
    }
*/
    // code for easy to medium
    private PlayerAction GetBotDecision() {
        var action = PlayerAction.NoAction;

        return action;
    }
}
