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

    public Text[] opSymbolTexts;
    public GameObject opSymbolGO;
    public string opSymbolMultiply = "x";
    public string opSymbolDivide = "÷";

    [M8.Localize]
    public string opTextSpeakRefMultiply;
    [M8.Localize]
    public string opTextSpeakRefDivide;

    [Header("Equation Display")]
    public M8.Animator.Animate equationOp1Anim; //play take index 0 when highlight
    public Text equationOp1Text;

    public M8.Animator.Animate equationOp2Anim; //play take index 0 when highlight
    public Text equationOp2Text;

    public M8.Animator.Animate equationAnsAnim; //play take index 0 when highlight
    public Text equationAnsText;

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
    private Coroutine mEquationRout;

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
        if(opSymbolGO) opSymbolGO.SetActive(false);

        if(equationOp1Text) equationOp1Text.text = "";
        if(equationOp2Text) equationOp2Text.text = "";
        if(equationAnsText) equationAnsText.text = "";

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

        SetEquationUpdateActive(false);
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
        if(scoreCounter) {
            if(scoreCounter.count != playerCtrl.curScore) {
                scoreCounter.count = playerCtrl.curScore;

                if(scoreAnimator && !string.IsNullOrEmpty(scoreTakeUpdate))
                    scoreAnimator.Play(scoreTakeUpdate);
            }
        }

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

        SetEquationUpdateActive(true);
    }

    IEnumerator DoOpChange(OperatorType opNext) {
        if(opSymbolGO) opSymbolGO.gameObject.SetActive(false);

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

        //play symbol display
        if(opSymbolGO)
            opSymbolGO.SetActive(mCurOpTypeDisplay == OperatorType.Multiply || mCurOpTypeDisplay == OperatorType.Divide);

        //speak text
        switch(mCurOpTypeDisplay) {
            case OperatorType.Multiply:
                if(!string.IsNullOrEmpty(opTextSpeakRefMultiply))
                    LoLManager.instance.SpeakText(opTextSpeakRefMultiply);
                break;

            case OperatorType.Divide:
                if(!string.IsNullOrEmpty(opTextSpeakRefDivide))
                    LoLManager.instance.SpeakText(opTextSpeakRefDivide);
                break;
        }

        mChangeOpRout = null;
    }

    IEnumerator DoComboDisplay() {
        ApplyCurrentComboCountDisplay();

        if(comboGO) comboGO.SetActive(true);

        if(comboAnimator && !string.IsNullOrEmpty(comboTakeEnter))
            yield return comboAnimator.PlayWait(comboTakeEnter);

        var comboDuration = GameData.instance.comboDuration;

        var playCtrl = PlayController.instance;
        while(playCtrl.comboIsActive) {
            if(mCurComboCountDisplay != playCtrl.comboCount) {
                ApplyCurrentComboCountDisplay();

                if(comboAnimator && !string.IsNullOrEmpty(comboTakeUpdate))
                    comboAnimator.Play(comboTakeUpdate);
            }

            if(comboTimeFill) comboTimeFill.fillAmount = 1.0f - Mathf.Clamp01(playCtrl.comboCurTime / comboDuration);

            yield return null;
        }

        if(comboAnimator && !string.IsNullOrEmpty(comboTakeExit))
            yield return comboAnimator.PlayWait(comboTakeExit);

        //clear combo display
        if(comboGO) comboGO.SetActive(false);

        mCurComboCountDisplay = 0;

        mComboDisplayRout = null;
    }

    IEnumerator DoEquationUpdate() {
        var wait = new WaitForSeconds(0.1f);

        var playCtrl = PlayController.instance;

        while(playCtrl) {
            //TODO: assumes simple equation: num op num = answer
            //check for group
            var grp = playCtrl.connectControl.activeGroup;
            if(grp != null) {
                if(equationOp1Anim) equationOp1Anim.Stop();
                if(equationOp2Anim) equationOp2Anim.Stop();
                
                if(equationOp1Text) equationOp1Text.text = grp.blobOpLeft ? grp.blobOpLeft.number.ToString() : "";
                if(equationOp2Text) equationOp2Text.text = grp.blobOpRight ? grp.blobOpRight.number.ToString() : "";

                //check answer blob
                if(grp.blobEq) {
                    if(equationAnsAnim) equationAnsAnim.Stop();

                    if(equationAnsText) equationAnsText.text = grp.blobEq.number.ToString();
                }
                else {
                    if(equationAnsAnim) equationAnsAnim.Play(0);

                    //update answer text
                    if(equationAnsText) {
                        bool isBlobInGroupDragging = (grp.blobOpLeft && grp.blobOpLeft.isDragging) || (grp.blobOpRight && grp.blobOpRight.isDragging);
                                                
                        Blob blobSelect = null;

                        if(playCtrl.connectControl.curGroupDragging == grp || playCtrl.connectControl.curGroupDragging == null) {
                            //get active blob that is highlighted or dragging
                            var blobs = playCtrl.blobSpawner.blobActives;
                            for(int i = 0; i < blobs.Count; i++) {
                                var blob = blobs[i];
                                if(blob.isHighlighted || blob.isDragging) {
                                    //make sure it's not our blob in group
                                    if(grp.IsBlobInGroup(blob))
                                        continue;

                                    //make sure it's not in group
                                    if(isBlobInGroupDragging || playCtrl.connectControl.GetGroup(blob) == null) {
                                        blobSelect = blob;
                                        break;
                                    }
                                }
                            }
                        }

                        if(blobSelect)
                            equationAnsText.text = blobSelect.number.ToString();
                        else
                            equationAnsText.text = "";
                    }
                }
            }
            else {
                if(equationOp1Anim) equationOp1Anim.Play(0);
                if(equationOp2Anim) equationOp2Anim.Play(0);
                if(equationAnsAnim) equationAnsAnim.Stop();

                //grab blob that is dragging, and one that is highlighted
                Blob blobDragging = null, blobHighlight = null;

                var blobs = playCtrl.blobSpawner.blobActives;
                for(int i = 0; i < blobs.Count; i++) {
                    var blob = blobs[i];
                    if(blob.isDragging) {
                        if(blob != blobHighlight)
                            blobDragging = blob;
                    }
                    else if(blob.isHighlighted) {
                        if(blob != blobDragging)
                            blobHighlight = blob;
                    }
                }

                if(blobDragging) {
                    if(equationOp1Text) equationOp1Text.text = blobDragging.number.ToString();

                    if(blobHighlight) {
                        if(equationOp2Text) equationOp2Text.text = blobHighlight.number.ToString();
                    }
                    else {
                        if(equationOp2Text) equationOp2Text.text = "";
                    }
                }
                else if(blobHighlight) {
                    if(equationOp1Text) equationOp1Text.text = blobHighlight.number.ToString();
                    if(equationOp2Text) equationOp2Text.text = "";
                }
                else {
                    if(equationOp1Text) equationOp1Text.text = "";
                    if(equationOp2Text) equationOp2Text.text = "";
                }

                if(equationAnsText) equationAnsText.text = "";
            }

            yield return wait;
        }

        mEquationRout = null;
        SetEquationUpdateActive(false);
    }

    private void ApplyCurrentComboCountDisplay() {
        mCurComboCountDisplay = PlayController.instance.comboCount;

        if(comboCountText) comboCountText.text = string.Format(comboCountFormat, mCurComboCountDisplay);
    }

    private void ApplyOpCurrentDisplay() {
        if(opCurrentTabMultiplyGO) opCurrentTabMultiplyGO.SetActive(mCurOpTypeDisplay == OperatorType.Multiply);
        if(opCurrentTabDivideGO) opCurrentTabDivideGO.SetActive(mCurOpTypeDisplay == OperatorType.Divide);

        string opSymbolStr;
        switch(mCurOpTypeDisplay) {
            case OperatorType.Multiply:
                opSymbolStr = opSymbolMultiply;
                break;
            case OperatorType.Divide:
                opSymbolStr = opSymbolDivide;
                break;
            default:
                opSymbolStr = "";
                break;
        }

        for(int i = 0; i < opSymbolTexts.Length; i++) {
            if(opSymbolTexts[i])
                opSymbolTexts[i].text = opSymbolStr;
        }
    }

    private void ApplyOpNextDisplay(OperatorType nextOp) {
        if(opNextTabMultiplyGO) opNextTabMultiplyGO.SetActive(nextOp == OperatorType.Multiply);
        if(opNextTabDivideGO) opNextTabDivideGO.SetActive(nextOp == OperatorType.Divide);
    }

    private void SetEquationUpdateActive(bool isActive) {
        if(isActive) {
            if(mEquationRout != null) StopCoroutine(mEquationRout);
            mEquationRout = StartCoroutine(DoEquationUpdate());
        }
        else {
            if(mEquationRout != null) {
                StopCoroutine(mEquationRout);
                mEquationRout = null;
            }

            if(equationOp1Anim) equationOp1Anim.Stop();
            if(equationOp2Anim) equationOp2Anim.Stop();
            if(equationAnsAnim) equationAnsAnim.Stop();

            if(equationOp1Text) equationOp1Text.text = "";
            if(equationOp2Text) equationOp2Text.text = "";
            if(equationAnsText) equationAnsText.text = "";
        }
    }
}
