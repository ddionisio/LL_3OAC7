﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "gameData", menuName = "Game/GameData")]
public class GameData : M8.SingletonScriptableObject<GameData> {
    public const string levelScoreHeader = "levelScore_";

    public const int clickCategoryBackground = 1;
	public const int clickCategoryBlob = 2;
	public const int clickCategoryUI = 3;

	[System.Serializable]
    public struct RankData {
        public string grade; //SS, S, A, B, C, D
        public Sprite icon;
        public float scale;
    }

    [Header("Rank Settings")]
    public RankData[] ranks; //highest to lowest
    public int rankIndexRetry; //threshold for retry    

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

    public int maxRetryCount = 2;

    [Header("Signals")]
    public SignalInteger signalClickCategory;

    public int retryCounter { get; set; }

    public void ScoreApply(int level, int score) {
        LoLManager.instance.userData.SetInt(levelScoreHeader + level.ToString(), score);
    }

    public int ScoreGet(int level) {
        return LoLManager.instance.userData.GetInt(levelScoreHeader + level.ToString());
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

    public void ClickCategory(int category) {
        if(signalClickCategory) signalClickCategory.Invoke(category);

	}

    protected override void OnInstanceInit() {

    }
}
