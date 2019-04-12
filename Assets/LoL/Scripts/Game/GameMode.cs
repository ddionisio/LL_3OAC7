using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "gameMode", menuName = "Game/Mode")]
public class GameMode : ScriptableObject {
    public static GameMode currentMode { get; private set; }

    public static void ClearCurrent() {
        currentMode = null;
    }

    public void SetAsCurrent() {
        currentMode = this;
    }
}
