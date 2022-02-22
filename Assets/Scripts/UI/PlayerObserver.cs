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
        for (int i = 0; i < 2; ++i) {
            //print(name + "_" + i + ": " + hand[i].value + ", " + hand[i].suit);
            _handImages[i].color = Card.DEFAULT_COLOR;
            _handImages[i].texture = Card.cardAssets[(int)hand[i].suit][(int)hand[i].value];
        }
    }

    [System.Obsolete]
    public void InitializeHand(in bool show) {
        if (show) {
            var suit1Mat = GameManager.cardSuitMats[(int)_player.hand[0].suit];
            var suit2Mat = GameManager.cardSuitMats[(int)_player.hand[1].suit];
            // calculate changes
            float aceHighVal1 = (_player.hand[0].value == Value.Ace) ? 1 : (int)_player.hand[0].value + 1;
            float aceHighVal2 = (_player.hand[1].value == Value.Ace) ? 1 : (int)_player.hand[1].value + 1;
            float xCoord1 = (float)(aceHighVal1) / 13f;
            float xCoord2 = (float)(aceHighVal2) / 13f;
            suit1Mat.SetTextureOffset(_player.hand[0].ToString(), new Vector2(xCoord1, 0f));
            suit2Mat.SetTextureOffset(_player.hand[1].ToString(), new Vector2(xCoord2, 0f));
        } else {
            print("hand is hidden");
        }
    }

    public void DiscardHand() {

    }

    public void UpdateBalance(in uint balance) {
        balanceText.text = "$" + balance;
    }
}
