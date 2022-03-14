using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanPlayer : BasePlayer {

    public override void Bet (float sliderVal) {
        base.Bet(sliderVal);
    }

    public override void Call() {
        base.Call();
    }

    public override void Fold() {
        base.Fold();
    }
}