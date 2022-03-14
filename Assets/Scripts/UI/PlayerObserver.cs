/*
    Author: Christian Mullins
    Summary: This class connects players to their card visuals.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerObserver : MonoBehaviour {
    public TMP_Text balanceText { get; private set; }
    private RawImage[] _handImages;
    private BasePlayer _player;
    private static Texture[] _cardAssets = new Texture[4];

    private void Start() {
        //initialize values
        balanceText = GetComponent<TMP_Text>();
        _handImages = GetComponentsInChildren<RawImage>();
        //Card must be initialized from here
        foreach (var h in _handImages) 
            h.color = Card.BACK_COLOR;
        //assign _player?
    }

    public void SetHandUI(in Card[] hand) {
        foreach (var h in _handImages)
            h.enabled = (hand != null);
        if (hand == null) return;
        for (int i = 0; i < 2; ++i) {
            //print(name + "_" + i + ": " + hand[i].value + ", " + hand[i].suit);
            _handImages[i].color = Card.DEFAULT_COLOR;
            _handImages[i].texture = Card.cardAssets[(int)hand[i].suit][(int)hand[i].value];
        }
    }

    public void EnableHand(in bool active) {
        foreach (var h in _handImages)
            h.enabled = active;
    }

    public void UpdateBalance(in uint balance) {
        balanceText.text = "$" + balance;
    }
}
