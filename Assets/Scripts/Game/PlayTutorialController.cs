using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayTutorialController : MonoBehaviour {
    public Operation op;
    [M8.TagSelector]
    public string dragIndicatorTag;

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
    public M8.Signal signalListenDragShow;

    [Header("Signal Invokes")]
    public M8.Signal signalInvokeCorrect;

    private Coroutine mRout;
    private Coroutine mDragIndicatorRout;

    private DragToGuideWidget mDragGuide;

    public void HideDrag() {
        if(mDragIndicatorRout != null) {
            StopCoroutine(mDragIndicatorRout);
            mDragIndicatorRout = null;
        }

        if(mDragGuide)
            mDragGuide.Hide();
    }
        
    void OnDestroy() {
        if(connectControl)
            connectControl.evaluateCallback += OnGroupEval;

        signalListenStart.callback -= OnSignalStart;
        signalListenEnd.callback -= OnSignalEnd;
        signalListenDragShow.callback -= OnSignalDragShow;
    }

    void OnDisable() {
        mRout = null;

        if(mDragIndicatorRout != null) {
            if(mDragGuide)
                mDragGuide.Hide();

            mDragIndicatorRout = null;
        }
    }

    void Awake() {
        if(activeGO) activeGO.SetActive(false);

        connectControl.evaluateCallback += OnGroupEval;

        signalListenStart.callback += OnSignalStart;
        signalListenEnd.callback += OnSignalEnd;
        signalListenDragShow.callback += OnSignalDragShow;
    }

    void OnSignalStart() {
        if(mRout != null)
            StopCoroutine(mRout);

        mRout = StartCoroutine(DoPlay());
    }

    void OnSignalEnd() {
        if(mRout != null)
            StopCoroutine(mRout);

        mRout = StartCoroutine(DoEnd());
    }

    void OnSignalDragShow() {
        if(mDragIndicatorRout != null)
            StopCoroutine(mDragIndicatorRout);

        mDragIndicatorRout = StartCoroutine(DoDragIndicator());
    }

    IEnumerator DoPlay() {
        if(activeGO) activeGO.SetActive(true);

        if(animator && !string.IsNullOrEmpty(takeBegin))
            yield return animator.PlayWait(takeBegin);

        //spawn operation
        blobSpawner.Spawn(0, op.operand1);
        blobSpawner.Spawn(1 % blobSpawner.templateGroups.Length, op.operand2);
        blobSpawner.Spawn(2 % blobSpawner.templateGroups.Length, op.equal);

        mRout = null;
    }

    IEnumerator DoEnd() {
        if(animator && !string.IsNullOrEmpty(takeEnd))
            yield return animator.PlayWait(takeEnd);

        if(activeGO) activeGO.SetActive(false);

        mRout = null;
    }

    IEnumerator DoDragIndicator() {
        //grab guide
        if(!mDragGuide) {
            var go = GameObject.FindGameObjectWithTag(dragIndicatorTag);
            mDragGuide = go.GetComponent<DragToGuideWidget>();
        }

        //grab camera
        var cam = Camera.main;

        //wait for blobs to spawn
        while(blobSpawner.spawnQueueCount > 0 && blobSpawner.CheckAnyBlobActiveState(Blob.State.Spawning))
            yield return null;

        var blobOp1 = blobSpawner.blobActives[0];
        var blobOp2 = blobSpawner.blobActives[1];
        var blobEq = blobSpawner.blobActives[2];

        while(!(blobOp1.isConnected && blobOp2.isConnected && blobEq.isConnected)) {
            //wait for blob 1 and 2 to be connected
            if(!(blobOp1.isConnected && blobOp2.isConnected)) {
                var blobOp1Pos = blobOp1.jellySprite.CentralPoint.Body2D.position;
                var blobOp2Pos = blobOp2.jellySprite.CentralPoint.Body2D.position;

                Vector2 blobOp1UIPos = cam.WorldToScreenPoint(blobOp1Pos);
                Vector2 blobOp2UIPos = cam.WorldToScreenPoint(blobOp2Pos);

                if(mDragGuide.isActive)
                    mDragGuide.UpdatePositions(blobOp1UIPos, blobOp2UIPos);
                else
                    mDragGuide.Show(false, blobOp1UIPos, blobOp2UIPos);
            }
            //wait for blob 3 to be connected
            else if(!blobEq.isConnected) {
                var blobOp2Pos = blobOp2.jellySprite.CentralPoint.Body2D.position;
                var blobEqPos = blobEq.jellySprite.CentralPoint.Body2D.position;

                Vector2 blobOp2UIPos = cam.WorldToScreenPoint(blobOp2Pos);
                Vector2 blobEqUIPos = cam.WorldToScreenPoint(blobEqPos);

                if(mDragGuide.isActive)
                    mDragGuide.UpdatePositions(blobOp2UIPos, blobEqUIPos);
                else
                    mDragGuide.Show(false, blobOp2UIPos, blobEqUIPos);
            }

            yield return null;
        }

        mDragGuide.Hide();

        mDragIndicatorRout = null;
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

            if(mDragIndicatorRout != null)
                HideDrag();
        }
        else {
            //do error thing for blobs
            blobLeft.state = Blob.State.Error;
            blobRight.state = Blob.State.Error;
            blobEq.state = Blob.State.Error;

            //clean out op
            connectOp.state = BlobConnect.State.Error;
            connectEq.state = BlobConnect.State.Error;

            //restart drag show
            if(mDragIndicatorRout != null)
                OnSignalDragShow();
        }

        connectControl.ClearGroup(grp);

        if(isCorrect && signalInvokeCorrect)
            signalInvokeCorrect.Invoke();
    }
}
