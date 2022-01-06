using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField]
    private Image _timerUI;
    private bool _cancel;

    public bool isActive { get; private set; }

    private void Start() {
        _cancel = false;
        _timerUI.fillAmount = 1f;
    }

    public IEnumerator StartNewTimer(float time) {
        _cancel = false;
        isActive = true;
        float start = time;
        while (time > 0 && !_cancel) {
            yield return new WaitForEndOfFrame();
            time -= Time.deltaTime;
            _timerUI.fillAmount = time / start;
        }
        isActive = false;
    }

    public void CancelTimer() {
        _cancel = true;
    }
}
