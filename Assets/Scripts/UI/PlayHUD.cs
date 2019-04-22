using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayHUD : MonoBehaviour {
    [Header("Display")]
    public M8.UI.Texts.TextCounter scoreCounter;
    public GameObject comboGO;
    public Text comboCountText;
    public Image comboTimeFill;
    public Text opCurrentText;
    public Text opNextText;

    //for display purpose
    [M8.Localize]
    public string opMultiplyDisplayTextRef;
    [M8.Localize]
    public string opDivideDisplayTextRef;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeEnter;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeChangeOp;

    [Header("Signal Listens")]
    public GameModeSignal signalListenPlayStart;

    private Coroutine mChangeOpRout;

    void OnDestroy() {
        if(PlayController.isInstantiated) {
            var playCtrl = PlayController.instance;
            playCtrl.roundBeginCallback -= OnRoundBegin;
            playCtrl.roundEndCallback -= OnRoundEnd;
        }

        signalListenPlayStart.callback -= OnSignalPlayStart;
    }

    void Awake() {
        //initial state
        if(scoreCounter) scoreCounter.SetCountImmediate(0);
        if(comboGO) comboGO.SetActive(false);

        //hide stuff
        if(animator) {
            if(!string.IsNullOrEmpty(takeEnter))
                animator.ResetTake(takeEnter);

            if(!string.IsNullOrEmpty(takeChangeOp))
                animator.ResetTake(takeChangeOp);
        }

        signalListenPlayStart.callback += OnSignalPlayStart;
    }

    void OnSignalPlayStart(GameMode mode) {
        //hook up play callbacks
        PlayController.instance.roundBeginCallback += OnRoundBegin;
        PlayController.instance.roundEndCallback += OnRoundEnd;

        //enter
        if(animator && !string.IsNullOrEmpty(takeEnter))
            animator.Play(takeEnter);
    }

    void OnRoundBegin() {

    }

    void OnRoundEnd() {

    }

    IEnumerator DoOpChange(OperatorType opPrev, OperatorType opNext) {
        yield return null;

        mChangeOpRout = null;
    }
}
