using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerObserver : MonoBehaviour {
    public TMP_Text balanceText { get; private set; }
    private Image[] _handImages = new Image[2];
    private BasePlayer _player;

    private void Awake() {
        //initialize values
        balanceText = GetComponent<TMP_Text>();
        _handImages = GetComponentsInChildren<Image>();
        //assign _player
    }

    public void InitializeHand(in bool show) {
        if (show) {
            var suit1Mat = GameManager.cardSuitMats[(int)_player.hand[0].suit];
            var suit2Mat = GameManager.cardSuitMats[(int)_player.hand[1].suit];
            // calculate changes
            float aceHighVal1 = (_player.hand[0].value == Value.Ace) ? 1 : (int)_player.hand[0].value + 1;
            float aceHighVal2 = (_player.hand[1].value == Value.Ace) ? 1 : (int)_player.hand[1].value + 1;
            float xCoord1 = (float)(aceHighVal1) / 13f;
            float xCoord2 = (float)(aceHighVal2) / 13f;
            suit1Mat.SetTextureOffset(_player.hand[0].GetString(), new Vector2(xCoord1, 0f));
            suit2Mat.SetTextureOffset(_player.hand[1].GetString(), new Vector2(xCoord2, 0f));
        } else {

        }
    }

    public void DiscardHand() {

    }

    public void UpdateBalance(in uint balance) {
        balanceText.text = "$" + balance;
    }
}
