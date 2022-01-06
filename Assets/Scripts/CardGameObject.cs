/*
Author: Christian Mullins
Summary: Decorator for Card object that incorporates displaying
*/
using UnityEngine;

public class CardGameObject : MonoBehaviour {
    public Suit suit         { get { return _myCard.suit; } }
    public Value value       { get { return _myCard.value; } }
    public Material mySprite { get { return _mySprite; } }

    private Card _myCard;
    private Material _mySprite;

    public CardGameObject(Card card) {
        _myCard = card;
        // set _mySprite using the appropriate
        var suitMat = GameManager.cardSuitMats[(int)_myCard.suit];
        // calculate changes
        float aceHighVal = (value == Value.Ace) ? 1 : (int)value + 1;
        float xCoord = (float)(aceHighVal) / 13f;
        suitMat.SetTextureOffset(_myCard.GetString(), new Vector2(xCoord, 0f));
        _mySprite = suitMat;
    }
}
