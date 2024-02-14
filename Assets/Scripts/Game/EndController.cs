using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndController : GameModeController<EndController> {
    public M8.UI.Texts.TextCounter[] levelCounters;
    public M8.UI.Texts.TextCounter totalCounter;

    public Text[] levelRankingTexts;
	public Image[] levelRankingIcons;
	public RankWidget rankingWidget;

    protected override void OnInstanceInit() {
        base.OnInstanceInit();

        //setup each level's score
        for(int i = 0; i < levelCounters.Length; i++) {
            int score = GameData.instance.ScoreGet(i);
            levelCounters[i].SetCountImmediate(score);

            if(score > 0) {
                //assume all have 10 rounds
                var rankInd = GameData.instance.GetRankIndex(score);
                var rank = GameData.instance.ranks[rankInd];

                if(levelRankingTexts[i])
                    levelRankingTexts[i].text = rank.grade;

                if(levelRankingIcons[i])
                    levelRankingIcons[i].sprite = rank.icon;
            }
            else {
                if(levelRankingTexts[i])
                    levelRankingTexts[i].text = "";

				if(levelRankingIcons[i])
					levelRankingIcons[i].sprite = GameData.instance.ranks[GameData.instance.ranks.Length - 1].icon;
			}
        }

        int totalScore = LoLManager.instance.curScore;
        totalCounter.SetCountImmediate(totalScore);

        //apply average ranking        
        var avgScore = Mathf.RoundToInt((float)totalScore / levelCounters.Length);
        int rankIndex = GameData.instance.GetRankIndex(avgScore);
        rankingWidget.Apply(rankIndex);
    }
}
