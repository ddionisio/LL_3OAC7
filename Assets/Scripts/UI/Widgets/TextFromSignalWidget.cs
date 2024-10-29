using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class TextFromSignalWidget : MonoBehaviour {
    public TMP_Text target;

    public M8.SignalString signalText;

    void OnDisable() {
        signalText.callback -= OnSignalText;
    }

    void OnEnable() {
        if(!target)
            target = GetComponent<TMP_Text>();

        target.text = "";

        signalText.callback += OnSignalText;
    }

    void OnSignalText(string s) {
        target.text = s;
    }
}
