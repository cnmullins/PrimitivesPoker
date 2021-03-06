/*
Author: Christian Mullins
Summary: PlayerIterator child that includes functionality for the player
    blinds.
*/

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blind : MonoBehaviour {
    public PlayerIterator iterator;
    [Range(0.0f, 1.5f)]
    public float baseVal;
    [Range(0.0f, 1.5f)]
    public float incrementorVal;
    public uint buyIn { get { return _buyIn; } }
    // inheritted getters
        // public int getIndex
        // public bool mustPlayerCall

    private uint _buyIn = 15;
    private float _baseVal = 1f;
    private float _incrementorVal = 5f;

    private IEnumerator Start() {
        var menu = GameObject.Find("MainMenuCanvas");
        // wait for game to start to obtain player values
        yield return new WaitUntil(() => {
            return GameManager.arePlayersInScene;
        });
        // get players
        iterator = new PlayerIterator(
                            (from seat in GameManager.instance.playerSpawnPos
                            where seat.childCount > 0
                            select seat.GetChild(0).GetComponent<BasePlayer>()).ToList());
    }

    public void IncrementBlind() {
        
    }

    /// <summary>
    /// Inheritted
    /// </summary>
    public void Iterate() {
        iterator.Iterate();
        _baseVal += _incrementorVal;
        //_buyIn =  (uint)(_baseVal * 20f);
        StartCoroutine("_MoveToNext");
    }

    public IEnumerator IterateAsync() { // stuck here???
        yield return new WaitUntil(() => {
            return GameManager.arePlayersInScene;
        });
        iterator.Iterate();
        StartCoroutine("_MoveToNext");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private IEnumerator _MoveToNext() {
        Vector3 targetPos, movePos;
        do {
            targetPos = iterator.currentPlayer.Value.buttonPos;
            movePos = (targetPos - transform.position).normalized;
            transform.Translate(movePos * 6.0f * Time.fixedDeltaTime, Space.World);
            yield return new WaitForFixedUpdate();
        } while (!_ApproximatePosition(targetPos, transform.position));
        yield return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pointA"></param>
    /// <param name="pointB"></param>
    /// <returns></returns>
    private bool _ApproximatePosition(Vector3 pointA, Vector3 pointB) {
        var clampVal = 0.25f;
        if (Mathf.Abs(pointA.x - pointB.x) < clampVal)
            if (Mathf.Abs(pointA.y - pointB.y) < clampVal)
                if (Mathf.Abs(pointA.z - pointB.z) < clampVal)
                    return true;
        return false;
    }
}