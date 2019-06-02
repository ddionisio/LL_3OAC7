﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "gameData", menuName = "Game/GameData")]
public class GameData : M8.SingletonScriptableObject<GameData> {
    public const string levelScoreHeader = "levelScore_";

    [System.Serializable]
    public struct RankData {
        public string grade; //SS, S, A, B, C, D
        public float scale;
    }

    [Header("Rank Settings")]
    public RankData[] ranks; //highest to lowest

    [Header("Play Settings")]
    public float hintDelay = 15f;
    public int hintErrorCount = 5;
    public int blobSpawnCount = 5;
    public int correctPoints = 100;
    public int correctDecayPoints = 25; //if hint was shown
    public int perfectPoints = 1000;
    public float comboDuration = 2.0f;
    public float timeParPerRound = 6f; //in seconds
    public float timeParPerRoundRanking; //in seconds, used for ranking, make sure it's less than timeParPerRound
    public int timeBonus = 50; //per second based on (timePar - time)

    public int bonusRoundScore = 1500;
    public float bonusRoundDuration = 60f;

    public void ScoreApply(int level, int score) {
        M8.SceneState.instance.global.SetValue(levelScoreHeader + level.ToString(), score, true);
    }

    public int ScoreGet(int level) {
        return M8.SceneState.instance.global.GetValue(levelScoreHeader + level.ToString());
    }

    public int GetRankIndex(int roundCount, int score) {
        int maxScore = 0;

        for(int i = 0; i < roundCount; i++)
            maxScore += (i + 1) * correctPoints;

        maxScore += perfectPoints;

        maxScore += bonusRoundScore;

        float timePar = timeParPerRound * roundCount;
        float rankTime = timeParPerRoundRanking * roundCount;

        maxScore += Mathf.RoundToInt((timePar - rankTime) * timeBonus);

        float scoreScale = (float)score / maxScore;

        for(int i = 0; i < ranks.Length; i++) {
            var rank = ranks[i];
            if(scoreScale >= rank.scale)
                return i;
        }

        return ranks.Length - 1;
    }

    protected override void OnInstanceInit() {

    }
}
