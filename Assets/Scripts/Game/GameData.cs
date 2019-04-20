using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "gameData", menuName = "Game/GameData")]
public class GameData : M8.SingletonScriptableObject<GameData> {
    [Header("Play Settings")]
    public int blobSpawnCount = 5;
    public float comboDelay = 2.0f;

    protected override void OnInstanceInit() {

    }
}
