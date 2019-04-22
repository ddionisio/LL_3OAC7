﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "gameData", menuName = "Game/GameData")]
public class GameData : M8.SingletonScriptableObject<GameData> {
    [Header("Play Settings")]
    public int blobSpawnCount = 5;
    public int correctPoints = 100;
    public int perfectPoints = 1000;
    public float comboDuration = 2.0f;

    protected override void OnInstanceInit() {

    }
}
