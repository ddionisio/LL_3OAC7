using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalVictory : M8.ModalController, M8.IModalPush, M8.IModalActive {
    public const string parmLevel = "level";
    public const string parmScore = "score";
    public const string parmTime = "time";
    public const string parmRoundsCount = "roundsCount";
    public const string parmMistakeCount = "mistake";
    public const string parmBonusRoundScore = "bonusRoundScore";
    public const string parmNextScene = "nextScene";

    [Header("Display")]
    public GameObject scoreGO;
    public M8.UI.Texts.TextCounter scoreCounterText;

    public GameObject bonusRoundScoreGO;
    public M8.UI.Texts.TextCounter bonusRoundScoreCounterText;

    public GameObject timeGO;
    public M8.UI.Texts.TextTime timeText;

    public GameObject timeBonusGO;
    public M8.UI.Texts.TextCounter timeBonusCounterText;

    public GameObject perfectBonusGO;
    public M8.UI.Texts.TextCounter perfectBonusCounterText;

    public GameObject separatorGO;

    public GameObject totalGO;
    public M8.UI.Texts.TextCounter totalCounterText;

    public GameObject rankingGO;
    public RankWidget rankWidget;

    [Header("Settings")]
    public float showDelay = 0.6f;

    [Header("Sfx")]
    [M8.SoundPlaylist]
    public string soundEnter;

    private int mScore;
    private int mBonusRoundScore;
    private float mTime;
    private int mRoundsCount;
    private int mTimeScore;
    private int mPerfectScore;
    private int mMistakeCount;
    private int mTotalScore;

    private int mLevelIndex = -1;

    private M8.SceneAssetPath mNextScene;

    public void Proceed() {
        var curProgress = LoLManager.instance.curProgress;
        var curScore = LoLManager.instance.curScore;
        LoLManager.instance.ApplyProgress(curProgress + 1, curScore + mTotalScore);

        if(mLevelIndex != -1)
            GameData.instance.ScoreApply(mLevelIndex, mTotalScore);

        mNextScene.Load();
    }

    void M8.IModalPush.Push(M8.GenericParams parms) {

        scoreGO.SetActive(false);
        bonusRoundScoreGO.SetActive(false);
        timeGO.SetActive(false);
        timeBonusGO.SetActive(false);
        perfectBonusGO.SetActive(false);
        separatorGO.SetActive(false);
        totalGO.SetActive(false);

        if(rankingGO) rankingGO.SetActive(false);

        mLevelIndex = -1;
        mScore = 0;
        mBonusRoundScore = 0;
        mTime = float.MaxValue;
        mTimeScore = 0;
        mRoundsCount = 0;
        mPerfectScore = 0;
        mMistakeCount = 0;

        if(parms != null) {
            if(parms.ContainsKey(parmLevel))
                mLevelIndex = parms.GetValue<int>(parmLevel);
            if(parms.ContainsKey(parmBonusRoundScore))
                mBonusRoundScore = parms.GetValue<int>(parmBonusRoundScore);
            if(parms.ContainsKey(parmScore))
                mScore = parms.GetValue<int>(parmScore);
            if(parms.ContainsKey(parmTime))
                mTime = parms.GetValue<float>(parmTime);
            if(parms.ContainsKey(parmRoundsCount))
                mRoundsCount = parms.GetValue<int>(parmRoundsCount);
            if(parms.ContainsKey(parmMistakeCount))
                mMistakeCount = parms.GetValue<int>(parmMistakeCount);
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

        mTotalScore = mScore + mBonusRoundScore + mTimeScore + mPerfectScore;

        if(rankWidget) rankWidget.Apply(mRoundsCount, mTotalScore);

        M8.SoundPlaylist.instance.Play(soundEnter, false);
    }

    void M8.IModalActive.SetActive(bool aActive) {
        if(aActive) {
            StartCoroutine(DoShow());
        }
    }

    IEnumerator DoShow() {
        var wait = new WaitForSeconds(showDelay);

        //score
        scoreGO.SetActive(true);
        scoreCounterText.SetCountImmediate(0);
        yield return wait;
        scoreCounterText.count = mScore;

        //bonus round score
        if(mBonusRoundScore > 0) {
            bonusRoundScoreGO.SetActive(true);
            bonusRoundScoreCounterText.SetCountImmediate(0);
            yield return wait;
            bonusRoundScoreCounterText.count = mBonusRoundScore;
        }

        //time
        timeGO.SetActive(true);
        timeText.time = mTime;
        yield return wait;

        //time bonus
        timeBonusGO.SetActive(true);
        timeBonusCounterText.SetCountImmediate(0);
        yield return wait;
        timeBonusCounterText.count = mTimeScore;

        if(mPerfectScore > 0) {
            perfectBonusGO.SetActive(true);
            perfectBonusCounterText.SetCountImmediate(0);
            yield return wait;
            perfectBonusCounterText.count = mPerfectScore;
        }

        separatorGO.SetActive(true);

        if(rankingGO) rankingGO.SetActive(true);

        //total
        totalGO.SetActive(true);
        totalCounterText.SetCountImmediate(0);
        yield return wait;
        totalCounterText.count = mTotalScore;
    }
}
