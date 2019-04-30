using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayHUD : MonoBehaviour {
    [Header("Settings")]
    public float beginDelay = 0.5f;

    [Header("Display")]
    public GameObject readySetGoDisplayGO;

    [Header("Operator Display")]
    public GameObject opCurrentTabMultiplyGO;
    public GameObject opCurrentTabDivideGO;

    public GameObject opNextTabMultiplyGO;
    public GameObject opNextTabDivideGO;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeEnter;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeReadySetGo;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeChangeOp;

    [Header("Score Display")]
    public M8.UI.Texts.TextCounter scoreCounter;

    public M8.Animator.Animate scoreAnimator;
    [M8.Animator.TakeSelector(animatorField = "scoreAnimator")]
    public string scoreTakeUpdate;

    [Header("Combo Display")]
    public GameObject comboGO;
    public Text comboCountText;
    public string comboCountFormat = "x{0}";
    public Image comboTimeFill;

    public M8.Animator.Animate comboAnimator;
    [M8.Animator.TakeSelector(animatorField = "comboAnimator")]
    public string comboTakeEnter;
    [M8.Animator.TakeSelector(animatorField = "comboAnimator")]
    public string comboTakeUpdate;
    [M8.Animator.TakeSelector(animatorField = "comboAnimator")]
    public string comboTakeExit;
       
            
    [Header("Signal Listens")]
    public GameModeSignal signalListenGameMode;
    public M8.Signal signalListenPlayEnd;

    [Header("Signal Invoke")]
    public M8.Signal signalInvokePlayStart;

    private Coroutine mChangeOpRout;
    private Coroutine mComboDisplayRout;

    private int mCurComboCountDisplay = 0;

    private OperatorType mCurOpTypeDisplay = OperatorType.None;

    void OnDestroy() {
        if(PlayController.isInstantiated) {
            var playCtrl = PlayController.instance;
            playCtrl.roundBeginCallback -= OnRoundBegin;
            playCtrl.roundEndCallback -= OnRoundEnd;
        }

        signalListenGameMode.callback -= OnSignalGameMode;
        signalListenPlayEnd.callback -= OnSignalPlayEnd;
    }

    void Awake() {
        //initial display state
        if(scoreCounter) scoreCounter.SetCountImmediate(0);
        if(comboGO) comboGO.SetActive(false);

        ApplyOpCurrentDisplay();

        //hide stuff
        if(readySetGoDisplayGO) readySetGoDisplayGO.SetActive(false);

        if(animator) {
            if(!string.IsNullOrEmpty(takeEnter))
                animator.ResetTake(takeEnter);

            if(!string.IsNullOrEmpty(takeChangeOp))
                animator.ResetTake(takeChangeOp);
        }

        signalListenGameMode.callback += OnSignalGameMode;
        signalListenPlayEnd.callback += OnSignalPlayEnd;
    }

    void OnSignalGameMode(GameMode mode) {
        //hook up play callbacks
        PlayController.instance.roundBeginCallback += OnRoundBegin;
        PlayController.instance.roundEndCallback += OnRoundEnd;

        StartCoroutine(DoPlayBegin());
    }

    void OnSignalPlayEnd() {
        //change operation to none
        if(mCurOpTypeDisplay != OperatorType.None) {
            if(mChangeOpRout != null)
                StopCoroutine(mChangeOpRout);

            mChangeOpRout = StartCoroutine(DoOpChange(OperatorType.None));
        }

        //stop combo update
        if(mComboDisplayRout != null) {
            StopCoroutine(mComboDisplayRout);
            mComboDisplayRout = null;
        }
    }

    void OnRoundBegin() {
        var playerCtrl = PlayController.instance;

        //setup op
        if(mCurOpTypeDisplay != playerCtrl.curRoundOp) {
            if(mChangeOpRout != null)
                StopCoroutine(mChangeOpRout);

            mChangeOpRout = StartCoroutine(DoOpChange(playerCtrl.curRoundOp));
        }
    }

    void OnRoundEnd() {
        var playerCtrl = PlayController.instance;

        //update score
        if(scoreCounter) scoreCounter.count = playerCtrl.curScore;

        //do combo display if available
        if(playerCtrl.comboIsActive) {
            if(mComboDisplayRout == null)
                mComboDisplayRout = StartCoroutine(DoComboDisplay());
        }
    }

    IEnumerator DoPlayBegin() {
        //enter hud
        if(animator && !string.IsNullOrEmpty(takeEnter))
            yield return animator.PlayWait(takeEnter);

        //wait a bit
        yield return new WaitForSeconds(beginDelay);

        //ready set go
        if(readySetGoDisplayGO) readySetGoDisplayGO.SetActive(true);

        if(animator && !string.IsNullOrEmpty(takeReadySetGo))
            yield return animator.PlayWait(takeReadySetGo);

        if(readySetGoDisplayGO) readySetGoDisplayGO.SetActive(false);

        //play
        signalInvokePlayStart.Invoke();
    }

    IEnumerator DoOpChange(OperatorType opNext) {
        //wait for animator to finish
        if(animator) {
            while(animator.isPlaying)
                yield return null;

            //setup display for next
            ApplyOpNextDisplay(opNext);

            if(!string.IsNullOrEmpty(takeChangeOp)) {
                //animate
                yield return animator.PlayWait(takeChangeOp);

                //reset
                animator.ResetTake(takeChangeOp);
            }
        }

        //apply next to current
        mCurOpTypeDisplay = opNext;
        ApplyOpCurrentDisplay();
        ApplyOpNextDisplay(OperatorType.None);

        mChangeOpRout = null;
    }

    IEnumerator DoComboDisplay() {
        if(comboGO) comboGO.SetActive(true);

        var comboDuration = GameData.instance.comboDuration;

        var playCtrl = PlayController.instance;
        while(playCtrl.comboIsActive) {
            if(mCurComboCountDisplay != playCtrl.comboCount) {
                mCurComboCountDisplay = playCtrl.comboCount;

                if(comboCountText) comboCountText.text = string.Format(comboCountFormat, mCurComboCountDisplay);
            }

            if(comboTimeFill) comboTimeFill.fillAmount = 1.0f - Mathf.Clamp01(playCtrl.comboCurTime / comboDuration);

            yield return null;
        }

        //clear combo display
        if(comboGO) comboGO.SetActive(false);

        mCurComboCountDisplay = 0;

        mComboDisplayRout = null;
    }

    private void ApplyOpCurrentDisplay() {
        if(opCurrentTabMultiplyGO) opCurrentTabMultiplyGO.SetActive(mCurOpTypeDisplay == OperatorType.Multiply);
        if(opCurrentTabDivideGO) opCurrentTabDivideGO.SetActive(mCurOpTypeDisplay == OperatorType.Divide);
    }

    private void ApplyOpNextDisplay(OperatorType nextOp) {
        if(opNextTabMultiplyGO) opNextTabMultiplyGO.SetActive(nextOp == OperatorType.Multiply);
        if(opNextTabDivideGO) opNextTabDivideGO.SetActive(nextOp == OperatorType.Divide);
    }
}
