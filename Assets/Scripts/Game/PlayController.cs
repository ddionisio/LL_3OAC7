using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayController : GameModeController<PlayController> {    
    [System.Serializable]
    public class OperationGroup {
        public Operation[] infos;
    }

    [Header("Info")]
    public OperationGroup[] opGroups;

    [Header("Controls")]
    public BlobConnectController connectControl;
    public BlobSpawner blobSpawner;

    public int curRoundIndex { get; private set; }
    public int roundCount { get { return mRoundOps != null ? mRoundOps.Length : 0; } }
    public OperatorType curRoundOp { get { return mRoundOps != null && curRoundIndex >= 0 && curRoundIndex < mRoundOps.Length ? mRoundOps[curRoundIndex] : OperatorType.None; } }
    public int comboCount { get; private set; }
    public float comboCurTime { get; private set; }

    //callbacks
    public event System.Action roundUpdateCallback;
    public event System.Action roundCompleteCallback;

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

        //mix up spawn orders
        var spawnCount = GameData.instance.blobSpawnCount;
        var spawnNextCount = 3;

        M8.ArrayUtil.Shuffle(mNumbers, 0, spawnCount);

        for(int i = spawnCount; i < mNumbers.Length; i += spawnNextCount) {
            int shuffleCount = spawnNextCount;
            if(i + spawnNextCount - 1 >= mNumbers.Length)
                shuffleCount = mNumbers.Length - i;

            if(shuffleCount > 0)
                M8.ArrayUtil.Shuffle(mNumbers, i, spawnNextCount);
        }

        connectControl.evaluateCallback += OnGroupEval;
    }

    protected override IEnumerator Start() {
        yield return base.Start();

        //animation intro

        StartCoroutine(DoRounds());

        mSpawnRout = StartCoroutine(DoBlobSpawn());
    }

    IEnumerator DoRounds() {
        for(curRoundIndex = 0; curRoundIndex < mRoundOps.Length; curRoundIndex++) {
            connectControl.curOp = curRoundOp;

            //signal new round
            roundUpdateCallback?.Invoke();

            //wait for correct answer
            mIsAnswerCorrectWait = true;
            while(mIsAnswerCorrectWait)
                yield return null;

            //signal complete round
            roundCompleteCallback?.Invoke();
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

        if(isCorrect) {
            //do sparkly thing for blobs
            grp.blobOpLeft.state = Blob.State.Correct;
            grp.blobOpRight.state = Blob.State.Correct;
            grp.blobEq.state = Blob.State.Correct;

            //clean out op
            grp.connectOp.state = BlobConnect.State.Correct;
            grp.connectOp = null;
            grp.connectEq.state = BlobConnect.State.Correct;
            grp.connectEq = null;

            //add score
            //increment and refresh combo

            //go to next round
            mIsAnswerCorrectWait = false;
        }
        else {
            //do error thing for blobs
            grp.blobOpLeft.state = Blob.State.Error;
            grp.blobOpRight.state = Blob.State.Error;
            grp.blobEq.state = Blob.State.Error;

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
