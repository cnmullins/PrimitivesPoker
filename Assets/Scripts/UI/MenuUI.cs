/*
Author: Christian Mullins
Summary: Handle the Main Menu UI and any other code necessary for 
*/

using UnityEngine;
using TMPro;

[RequireComponent(typeof(Canvas))]
public class MenuUI : MonoBehaviour {
    public TMP_Dropdown botNumSelector;

    public void StartGame() {
        int numOfPlayers = botNumSelector.value + 3;
        GameManager.instance.StartNewGame(numOfPlayers);
        gameObject.SetActive(false);
    }
}
