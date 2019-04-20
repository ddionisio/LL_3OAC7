using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayController : GameModeController<PlayController> {
    [System.Serializable]
    public struct OperationInfo {
        public int operand1;
        public OperatorType op;
        public int operand2;
        public int equal;
    }

    [System.Serializable]
    public class OperationInfoGroup {
        public OperationInfo[] infos;
    }

    [Header("Info")]
    public OperationInfoGroup[] opGroups;

    [Header("Controls")]
    public BlobConnectController connectControl;
    public BlobSpawner blobSpawner;

    [Header("Signal Invokes")]
    public SignalOperatorType signalInvokeOpChange;

    private OperatorType[] mRoundOps;
    private int[] mNumbers;

    private bool mIsAnswerCorrectWait;
    private Coroutine mSpawnRout;

    protected override void OnInstanceDeinit() {
        if(connectControl)
            connectControl.evaluateCallback -= OnGroupEval;

        base.OnInstanceDeinit();
    }

    protected override void OnInstanceInit() {
        base.OnInstanceInit();

        //setup rounds
        var roundOpList = new List<OperatorType>();
        var numberList = new List<int>();

        for(int i = 0; i < opGroups.Length; i++) {
            var grp = opGroups[i];
            M8.ArrayUtil.Shuffle(grp.infos);

            for(int j = 0; j < grp.infos.Length; j++) {
                var inf = grp.infos[j];

                //setup round, fill numbers later
                roundOpList.Add(inf.op);

                numberList.Add(inf.equal);
                numberList.Add(inf.operand1);
                numberList.Add(inf.operand2);
            }
        }

        mRoundOps = roundOpList.ToArray();
        mNumbers = numberList.ToArray();

        connectControl.evaluateCallback += OnGroupEval;
    }

    protected override IEnumerator Start() {
        yield return base.Start();

        //animation intro

        StartCoroutine(DoRounds());

        mSpawnRout = StartCoroutine(DoBlobSpawn());
    }

    IEnumerator DoRounds() {
        for(int i = 0; i < mRoundOps.Length; i++) {
            var curOp = mRoundOps[i];

            connectControl.curOp = curOp;

            //signal operation change
            if(signalInvokeOpChange)
                signalInvokeOpChange.Invoke(curOp);

            //wait for correct answer
            mIsAnswerCorrectWait = true;
            while(mIsAnswerCorrectWait)
                yield return null;
        }
    }

    IEnumerator DoBlobSpawn() {
        int curBlobTemplateIndex = 0;
        int curNumberIndex = 0;

        while(curNumberIndex < mNumbers.Length) {
            var maxCount = GameData.instance.blobSpawnCount;

            //check if we have enough on the board
            while(blobSpawner.blobActiveCount + blobSpawner.spawnQueueCount < maxCount) {
                blobSpawner.Spawn(curBlobTemplateIndex, mNumbers[curNumberIndex]);

                curBlobTemplateIndex = (curBlobTemplateIndex + 1) % blobSpawner.templateGroups.Length;

                curNumberIndex++;
                if(curNumberIndex == mNumbers.Length)
                    break;
            }

            yield return null;
        }

        mSpawnRout = null;
    }

    void OnGroupEval(BlobConnectController.Group grp) {
        int op1, op2, eq;
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

        if(isCorrect) {
            //do sparkly thing for blobs

            //clean out op

            //add score
            //increment and refresh combo

            //go to next round
            mIsAnswerCorrectWait = false;
        }
        else {
            //do error thing for blobs

            //clean out op
            grp.connectOp.state = BlobConnect.State.Error;
            grp.connectOp = null;
            grp.connectEq.state = BlobConnect.State.Error;
            grp.connectEq = null;

            //reset combo
        }

        connectControl.ClearGroup(grp);
    }
}
