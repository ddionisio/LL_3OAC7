using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "gameData", menuName = "Game/GameData")]
public class GameData : M8.SingletonScriptableObject<GameData> {
    public const string levelScoreHeader = "levelScore_";

    [Header("Play Settings")]
    public float hintDelay = 15f;
    public int blobSpawnCount = 5;
    public int correctPoints = 100;
    public int correctDecayPoints = 25; //if hint was shown
    public int perfectPoints = 1000;
    public float comboDuration = 2.0f;
    public float timeParPerRound = 6f; //in seconds
    public int timeBonus = 50; //per second based on (timePar - time)

    public void ScoreApply(int level, int score) {
        M8.SceneState.instance.global.SetValue(levelScoreHeader + level.ToString(), score, true);
    }

    public int ScoreGet(int level) {
        return M8.SceneState.instance.global.GetValue(levelScoreHeader + level.ToString());
    }

    protected override void OnInstanceInit() {

    }
}
