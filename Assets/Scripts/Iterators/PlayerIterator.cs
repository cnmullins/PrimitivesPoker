/*
Author: Christian Mullins
Summary: Base class for an iterate to move from player to player in game.
*/
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

// helper subclass for GameManager
public class PlayerIterator {
    [HideInInspector]
    public LinkedListNode<BasePlayer> currentPlayer;
    protected static LinkedList<BasePlayer> _playerRotation;
    protected int _index = 0;
    
    public int getIndex        { get { return _index; } }
    public bool mustPlayerCall { get {
        return (Dealer.communityBet >= currentPlayer.Value.currentBet);
    } }

    public PlayerIterator() {
        Dealer.instance.StartCoroutine(Start());
    }

    public PlayerIterator(List<BasePlayer> players) {
        _playerRotation = new LinkedList<BasePlayer>(players);
        currentPlayer = _playerRotation.First;
    }

    protected virtual IEnumerator Start() {
       var menu = GameObject.Find("MainMenuCanvas");
       // wait for game to start to obtain player values
        yield return new WaitUntil(() => {
            return GameManager.arePlayersInScene;
        });
        // get players
        _playerRotation = new LinkedList<BasePlayer>(
                            from seat in GameManager.instance.playerSpawnPos
                            where seat.childCount > 0
                            select seat.GetChild(0).GetComponent<BasePlayer>());
        currentPlayer   = _playerRotation.First;
        
    } 

    /// <summary>
    /// Adjust values of this variables class and record incrementation.
    /// </summary>
    public virtual void Iterate() {
        ++_index;
        if (_index > Dealer.playersLL.Count) _index = 0;
        // adjust class variables
        currentPlayer = currentPlayer?.Next ?? _playerRotation.First;
        //CHECK IF EVERYTHING UPDATES STILL
        //foreach (var p in _playerRotation) p.SetupBalanceTextColor();
    }
    
}
