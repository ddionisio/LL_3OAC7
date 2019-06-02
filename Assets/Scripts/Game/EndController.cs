using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndController : GameModeController<EndController> {
    public M8.UI.Texts.TextCounter[] levelCounters;
    public M8.UI.Texts.TextCounter totalCounter;

    public Text[] levelRankingTexts;
    public RankWidget rankingWidget;

    protected override void OnInstanceInit() {
        base.OnInstanceInit();

        //setup each level's score
        for(int i = 0; i < levelCounters.Length; i++) {
            int score = GameData.instance.ScoreGet(i);
            levelCounters[i].SetCountImmediate(score);

            if(score > 0) {
                //assume all have 10 rounds
                var rankInd = GameData.instance.GetRankIndex(10, score);
                levelRankingTexts[i].text = GameData.instance.ranks[rankInd].grade;
            }
            else
                levelRankingTexts[i].text = "";
        }

        int totalScore = LoLManager.instance.curScore;
        totalCounter.SetCountImmediate(totalScore);

        //apply average ranking
        var avgScore = Mathf.RoundToInt((float)totalScore / levelCounters.Length);
        rankingWidget.Apply(10, avgScore);
    }
}
