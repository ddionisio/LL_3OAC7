using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayTutorialController : MonoBehaviour {
    public Operation op;

    [Header("Controls")]
    public BlobConnectController connectControl;
    public BlobSpawner blobSpawner;
    public GameObject activeGO;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeBegin;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeEnd;

    [Header("Signal Listens")]
    public M8.Signal signalListenStart;
    public M8.Signal signalListenEnd;

    [Header("Signal Invokes")]
    public M8.Signal signalInvokeCorrect;

    void OnDestroy() {
        if(connectControl)
            connectControl.evaluateCallback += OnGroupEval;

        signalListenStart.callback -= OnSignalStart;
        signalListenEnd.callback -= OnSignalEnd;
    }

    void Awake() {
        if(activeGO) activeGO.SetActive(false);

        connectControl.evaluateCallback += OnGroupEval;

        signalListenStart.callback += OnSignalStart;
        signalListenEnd.callback += OnSignalEnd;
    }

    void OnSignalStart() {
        StopAllCoroutines();
        StartCoroutine(DoPlay());
    }

    void OnSignalEnd() {
        StopAllCoroutines();
        StartCoroutine(DoEnd());
    }

    IEnumerator DoPlay() {
        if(activeGO) activeGO.SetActive(true);

        if(animator && !string.IsNullOrEmpty(takeBegin))
            yield return animator.PlayWait(takeBegin);

        //spawn operation
        blobSpawner.Spawn(0, op.operand1);
        blobSpawner.Spawn(1 % blobSpawner.templateGroups.Length, op.operand2);
        blobSpawner.Spawn(2 % blobSpawner.templateGroups.Length, op.equal);
    }

    IEnumerator DoEnd() {
        if(animator && !string.IsNullOrEmpty(takeEnd))
            yield return animator.PlayWait(takeEnd);

        if(activeGO) activeGO.SetActive(false);
    }

    void OnGroupEval(BlobConnectController.Group grp) {
        //check if correct
        float op1, op2, eq;
        grp.GetNumbers(out op1, out op2, out eq);

        bool isCorrect = false;

        switch(grp.connectOp.op) {
            case OperatorType.Multiply:
                isCorrect = op1 * op2 == eq;
                break;
            case OperatorType.Divide:
                isCorrect = op1 / op2 == eq;
                break;
        }

        Blob blobLeft = grp.blobOpLeft, blobRight = grp.blobOpRight, blobEq = grp.blobEq;
        BlobConnect connectOp = grp.connectOp, connectEq = grp.connectEq;

        if(isCorrect) {
            //do sparkly thing for blobs
            blobLeft.state = Blob.State.Correct;
            blobRight.state = Blob.State.Correct;
            blobEq.state = Blob.State.Correct;

            //clean out op
            connectOp.state = BlobConnect.State.Correct;
            connectEq.state = BlobConnect.State.Correct;
        }
        else {
            //do error thing for blobs
            blobLeft.state = Blob.State.Error;
            blobRight.state = Blob.State.Error;
            blobEq.state = Blob.State.Error;

            //clean out op
            connectOp.state = BlobConnect.State.Error;
            connectEq.state = BlobConnect.State.Error;
        }

        connectControl.ClearGroup(grp);

        if(isCorrect && signalInvokeCorrect)
            signalInvokeCorrect.Invoke();
    }
}
