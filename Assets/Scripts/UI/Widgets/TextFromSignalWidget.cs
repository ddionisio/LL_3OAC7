using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextFromSignalWidget : MonoBehaviour {
    public Text target;

    public SignalString signalText;

    void OnDisable() {
        signalText.callback -= OnSignalText;
    }

    void OnEnable() {
        if(!target)
            target = GetComponent<Text>();

        target.text = "";

        signalText.callback += OnSignalText;
    }

    void OnSignalText(string s) {
        target.text = s;
    }
}
