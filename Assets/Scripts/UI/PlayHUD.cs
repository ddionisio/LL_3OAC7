﻿using System.Collections;
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

    public GameObject equationOpGO;
    public GameObject equationEqGO;
        
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

    [Header("Equation Pop Up")]
    public Text correctEqPopText;
    public float correctEqPopShowDelay = 1.5f;
    public M8.Animator.Animate correctEqPopAnimator; //also treat it as the root display
    [M8.Animator.TakeSelector(animatorField = "correctEqPopAnimator")]
    public string correctEqPopTakeEnter;
    [M8.Animator.TakeSelector(animatorField = "correctEqPopAnimator")]
    public string correctEqPopTakeUpdate;
    [M8.Animator.TakeSelector(animatorField = "correctEqPopAnimator")]
    public string correctEqPopTakeExit;

    public Text incorrectEqPopText;
    public float incorrectEqPopShowDelay = 1.5f;
    public M8.Animator.Animate incorrectEqPopAnimator; //also treat it as the root display
    [M8.Animator.TakeSelector(animatorField = "incorrectEqPopAnimator")]
    public string incorrectEqPopTakeEnter;
    [M8.Animator.TakeSelector(animatorField = "incorrectEqPopAnimator")]
    public string incorrectEqPopTakeUpdate;
    [M8.Animator.TakeSelector(animatorField = "incorrectEqPopAnimator")]
    public string incorrectEqPopTakeExit;

    [Header("Signal Listens")]
    public GameModeSignal signalListenGameMode;
    public M8.Signal signalListenPlayEnd;

    [Header("Signal Invoke")]
    public M8.Signal signalInvokePlayStart;

    private Coroutine mChangeOpRout;
    private Coroutine mComboDisplayRout;
    private Coroutine mEquationRout;

    private BlobConnectController.Group mEquationGrp = null;

    private int mCurComboCountDisplay = 0;

    private OperatorType mCurOpTypeDisplay = OperatorType.None;

    private struct PopUpData {
        public int op1;
        public OperatorType op;
        public int op2;
        public int answer;

        public string GetString(bool isEqual) {
            var strBuff = new System.Text.StringBuilder();

            strBuff.Append(op1).Append(' ');

            switch(op) {
                case OperatorType.Multiply:
                    strBuff.Append("x ");
                    break;
                case OperatorType.Divide:
                    strBuff.Append("÷ ");
                    break;
            }

            strBuff.Append(op2).Append(' ');

            if(isEqual)
                strBuff.Append("= ");
            else
                strBuff.Append("≠ ");

            strBuff.Append(answer);

            return strBuff.ToString();
        }
    }

    private Queue<PopUpData> mCorrectPopUpQueue = new Queue<PopUpData>();
    private Coroutine mCorrectPopUpRout;
    private Coroutine mIncorrectPopUpRout;

    void OnDestroy() {
        if(PlayController.isInstantiated) {
            var playCtrl = PlayController.instance;
            playCtrl.roundBeginCallback -= OnRoundBegin;
            playCtrl.roundEndCallback -= OnRoundEnd;
            playCtrl.groupEvalCallback -= OnGroupEvaluated;

            if(playCtrl.connectControl)
                playCtrl.connectControl.groupAddedCallback -= OnGroupAdded;
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

        if(equationOpGO) equationOpGO.SetActive(false);
        if(equationEqGO) equationEqGO.SetActive(false);

        if(correctEqPopAnimator) correctEqPopAnimator.gameObject.SetActive(false);
        if(incorrectEqPopAnimator) incorrectEqPopAnimator.gameObject.SetActive(false);

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
        PlayController.instance.groupEvalCallback += OnGroupEvaluated;
        PlayController.instance.connectControl.groupAddedCallback += OnGroupAdded;

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

    void OnGroupEvaluated(Operation op, int answer, bool isCorrect) {
        var dat = new PopUpData { op1=op.operand1, op2=op.operand2, op=op.op, answer=answer };

        if(isCorrect) {
            mCorrectPopUpQueue.Enqueue(dat);

            if(mCorrectPopUpRout == null)
                mCorrectPopUpRout = StartCoroutine(DoCorrectPopUp());
        }
        else {
            if(incorrectEqPopText) incorrectEqPopText.text = dat.GetString(false);

            if(mIncorrectPopUpRout != null)
                StopCoroutine(mIncorrectPopUpRout);

            mIncorrectPopUpRout = StartCoroutine(DoIncorrectPopUp());
        }
    }

    void OnGroupAdded(BlobConnectController.Group grp) {
        mEquationGrp = grp;
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

        //TODO: assumes simple equation: num op num = answer
        while(playCtrl) {
            bool isOpActive = false;
            bool isEqActive = false;

            //grab blob that is dragging, and one that is highlighted
            Blob blobDragging = playCtrl.connectControl.curBlobDragging;
            Blob blobHighlight = null;

            var blobs = playCtrl.blobSpawner.blobActives;
            for(int i = 0; i < blobs.Count; i++) {
                var blob = blobs[i];
                if(blob.isHighlighted) {
                    if(blob != blobDragging)
                        blobHighlight = blob;
                }
            }
            
            //check for group
            if(mEquationGrp == null || !playCtrl.connectControl.IsGroupActive(mEquationGrp)) {
                if(playCtrl.connectControl.curGroupDragging != null)
                    mEquationGrp = playCtrl.connectControl.curGroupDragging;
                else if(playCtrl.connectControl.activeGroup != null)
                    mEquationGrp = playCtrl.connectControl.activeGroup;
                else
                    mEquationGrp = null;
            }

            if(mEquationGrp != null) {
                //change grp?
                if(playCtrl.connectControl.curGroupDragging != null)
                    mEquationGrp = playCtrl.connectControl.curGroupDragging;
                else if(blobHighlight) {
                    //check if highlighted blob is in a group
                    var otherGrp = playCtrl.connectControl.GetGroup(blobHighlight);
                    if(otherGrp != null)
                        mEquationGrp = otherGrp;
                }

                var grp = mEquationGrp;

                if(equationOp1Anim) equationOp1Anim.Stop();
                if(equationOp2Anim) equationOp2Anim.Stop();

                isOpActive = true;
                                
                string op1Text = grp.blobOpLeft ? grp.blobOpLeft.number.ToString() : "";
                string op2Text = grp.blobOpRight ? grp.blobOpRight.number.ToString() : "";
                                
                //check answer blob
                if(grp.blobEq) {
                    if(equationAnsAnim) equationAnsAnim.Stop();

                    if(equationAnsText) equationAnsText.text = grp.blobEq.number.ToString();
                }
                else {
                    if(equationAnsAnim) equationAnsAnim.Play(0);

                    //update answer text
                    if(equationAnsText) {
                        Blob blobSelect = null;

                        if(playCtrl.connectControl.curGroupDragging == grp) {
                            //select highlighted blob if it's not in our group
                            if(blobHighlight && !grp.IsBlobInGroup(blobHighlight))
                                blobSelect = blobHighlight;

                            isEqActive = true;
                        }
                        else if(playCtrl.connectControl.curGroupDragging == null) {
                            //grab dragging blob and ensure it is highlighting one of the blobs in our group
                            if(playCtrl.connectControl.curBlobDragging) {
                                if(grp.blobOpLeft && grp.blobOpLeft.isHighlighted) {
                                    //need to swap operand texts
                                    var _txt = op1Text;
                                    op1Text = op2Text;
                                    op2Text = _txt;
                                }

                                blobSelect = playCtrl.connectControl.curBlobDragging;

                                isEqActive = (grp.blobOpLeft && grp.blobOpLeft.isHighlighted) || (grp.blobOpRight && grp.blobOpRight.isHighlighted);
                            }
                        }

                        if(blobSelect)
                            equationAnsText.text = blobSelect.number.ToString();
                        else
                            equationAnsText.text = "";
                    }
                }

                if(equationOp1Text) equationOp1Text.text = op1Text;
                if(equationOp2Text) equationOp2Text.text = op2Text;
            }
            else {
                if(equationOp1Anim) equationOp1Anim.Play(0);
                if(equationOp2Anim) equationOp2Anim.Play(0);
                if(equationAnsAnim) equationAnsAnim.Stop();
                                
                if(blobDragging) {
                    if(equationOp1Text) equationOp1Text.text = blobDragging.number.ToString();

                    isOpActive = true;

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

            if(equationOpGO) equationOpGO.SetActive(isOpActive);
            if(equationEqGO) equationEqGO.SetActive(isEqActive);

            yield return wait;
        }

        mEquationRout = null;
        SetEquationUpdateActive(false);
    }

    IEnumerator DoCorrectPopUp() {
        var wait = new WaitForSeconds(correctEqPopShowDelay);

        while(mCorrectPopUpQueue.Count > 0) {
            var dat = mCorrectPopUpQueue.Dequeue();

            //apply display
            if(correctEqPopText)
                correctEqPopText.text = dat.GetString(true);

            //show/update
            if(correctEqPopAnimator) {
                if(!correctEqPopAnimator.gameObject.activeSelf) {
                    correctEqPopAnimator.gameObject.SetActive(true);

                    if(!string.IsNullOrEmpty(correctEqPopTakeEnter))
                        yield return correctEqPopAnimator.PlayWait(correctEqPopTakeEnter);
                }

                if(!string.IsNullOrEmpty(correctEqPopTakeUpdate))
                    yield return correctEqPopAnimator.PlayWait(correctEqPopTakeUpdate);
            }

            yield return wait;

            //hide
            if(mCorrectPopUpQueue.Count == 0) {
                if(correctEqPopAnimator) {
                    if(!string.IsNullOrEmpty(correctEqPopTakeExit))
                        yield return correctEqPopAnimator.PlayWait(correctEqPopTakeExit);

                    correctEqPopAnimator.gameObject.SetActive(false);
                }
            }
        }

        mCorrectPopUpRout = null;
    }

    IEnumerator DoIncorrectPopUp() {
        var wait = new WaitForSeconds(incorrectEqPopShowDelay);

        //show/update
        if(incorrectEqPopAnimator) {
            if(!incorrectEqPopAnimator.gameObject.activeSelf) {
                incorrectEqPopAnimator.gameObject.SetActive(true);

                if(!string.IsNullOrEmpty(incorrectEqPopTakeEnter))
                    yield return incorrectEqPopAnimator.PlayWait(incorrectEqPopTakeEnter);
            }

            if(!string.IsNullOrEmpty(incorrectEqPopTakeUpdate))
                yield return incorrectEqPopAnimator.PlayWait(incorrectEqPopTakeUpdate);
        }

        yield return wait;

        //hide
        if(incorrectEqPopAnimator) {
            if(!string.IsNullOrEmpty(incorrectEqPopTakeExit))
                yield return incorrectEqPopAnimator.PlayWait(incorrectEqPopTakeExit);

            incorrectEqPopAnimator.gameObject.SetActive(false);
        }

        mIncorrectPopUpRout = null;
    }

    private void ApplyCurrentComboCountDisplay() {
        mCurComboCountDisplay = PlayController.instance.comboCount;

        if(comboCountText) comboCountText.text = string.Format(comboCountFormat, mCurComboCountDisplay + 1);
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

            if(equationOpGO) equationOpGO.SetActive(false);
            if(equationEqGO) equationEqGO.SetActive(false);

            mEquationGrp = null;
        }
    }
}
