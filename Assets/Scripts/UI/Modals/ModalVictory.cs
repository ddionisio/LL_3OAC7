using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalVictory : M8.ModalController, M8.IModalPush {
    public const string parmScore = "score";
    public const string parmTime = "time";
    public const string parmRoundsCount = "roundsCount";
    public const string parmMistakeCount = "mistake";
    public const string parmNextScene = "nextScene";

    private int mScore;
    private float mTime;
    private int mRoundsCount;
    private int mTimeScore;
    private int mPerfectScore;
    private int mMistakeCount;
    private int mTotalScore;

    private M8.SceneAssetPath mNextScene;

    public void Proceed() {
        var curProgress = LoLManager.instance.curProgress;
        var curScore = LoLManager.instance.curScore;
        LoLManager.instance.ApplyProgress(curProgress + 1, curScore + mTotalScore);

        mNextScene.Load();
    }

    void M8.IModalPush.Push(M8.GenericParams parms) {
        mScore = 0;
        mTime = float.MaxValue;
        mTimeScore = 0;
        mRoundsCount = 0;
        mPerfectScore = 0;
        mMistakeCount = 0;

        if(parms != null) {
            if(parms.ContainsKey(parmScore))
                mScore = parms.GetValue<int>(parmScore);
            if(parms.ContainsKey(parmTime))
                mTime = parms.GetValue<float>(parmTime);
            if(parms.ContainsKey(parmRoundsCount))
                mRoundsCount = parms.GetValue<int>(parmRoundsCount);
            if(parms.ContainsKey(parmMistakeCount))
                mRoundsCount = parms.GetValue<int>(parmMistakeCount);
            if(parms.ContainsKey(parmNextScene))
                mNextScene = parms.GetValue<M8.SceneAssetPath>(parmNextScene);
        }

        //apply scores
        if(mRoundsCount > 0) {
            float timePar = GameData.instance.timeParPerRound * mRoundsCount;
            if(mTime < timePar) {
                mTimeScore = Mathf.RoundToInt(GameData.instance.timeBonus * (timePar - mTime));
            }
        }

        if(mMistakeCount == 0)
            mPerfectScore = GameData.instance.perfectPoints;

        mTotalScore = mScore + mTimeScore + mPerfectScore;

        //update displays and which gameobjects to show up
    }
}
