/*
Author: Christian Mullins
Summary: Child of PlayerIterator that will handle most player interaction 
    between data.
*/
using System.Collections.Generic;
using UnityEngine.UI;

public class PlayerFocuser : PlayerIterator {
    public bool isRoundOver { get { return _isRoundOver; } }
    //inheritted vals
        //currentPlayer
        //_playerRotation
        //_index

    private bool _isRoundOver;
    private Slider _betSlider;
    private float _decisionTime;

    public PlayerFocuser() {
        _decisionTime = GameManager.playerDecisionTime;
        base.Start();
    }

    public PlayerFocuser(List<BasePlayer> players) {
        _decisionTime = GameManager.playerDecisionTime;
        base.Start();
        _playerRotation = new LinkedList<BasePlayer>(players);
    }
}
