using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndController : GameModeController<EndController> {
    public M8.UI.Texts.TextCounter[] levelCounters;
    public M8.UI.Texts.TextCounter totalCounter;

    protected override void OnInstanceInit() {
        base.OnInstanceInit();

        //setup each level's score
        for(int i = 0; i < levelCounters.Length; i++) {
            int score = GameData.instance.ScoreGet(i);
            levelCounters[i].SetCountImmediate(score);
        }

        int totalScore = LoLManager.instance.curScore;
        totalCounter.SetCountImmediate(totalScore);
    }
}
