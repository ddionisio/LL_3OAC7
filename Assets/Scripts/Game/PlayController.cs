using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayController : GameModeController<PlayController> {
    [System.Serializable]
    public class OperationGroup {
        public Operation[] infos;
    }

    [Header("Settings")]
    public int levelIndex;
    public string modalVictory = "victory";    
    public M8.SceneAssetPath nextScene; //after victory

    [Header("Operations")]
    public OperationGroup[] opGroups;

    [Header("Controls")]
    public BlobConnectController connectControl;
    public BlobSpawner blobSpawner;

    [Header("Rounds")]
    public Transform roundsRoot; //grab SpriteColorFromPalette for each child
    public float roundCompleteBrightness = 0.3f;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeBegin;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeEnd;

    [Header("Music")]
    [M8.MusicPlaylist]
    public string playMusic;

    [Header("Signal Listen")]
    public M8.Signal signalListenPlayStart;

    [Header("Signal Invoke")]
    public M8.Signal signalInvokePlayEnd;

    public int curRoundIndex { get; private set; }
    public int roundCount { get { return mRoundOps != null ? mRoundOps.Length : 0; } }
    public OperatorType curRoundOp { get { return mRoundOps != null && curRoundIndex >= 0 && curRoundIndex < mRoundOps.Length ? mRoundOps[curRoundIndex] : OperatorType.None; } }
    public int comboCount { get; private set; }
    public float comboCurTime { get; private set; }
    public bool comboIsActive { get { return mComboRout != null; } }
    public int curScore { get; private set; }
    public int curNumberIndex { get; private set; }
    public int mistakeCount { get; private set; }

    public bool isHintShown { get; private set; }
    public float curPlayTime { get { return Time.time - mPlayLastTime; } }

    //callbacks
    public event System.Action roundBeginCallback;
    public event System.Action roundEndCallback;
    public event System.Action<Operation, int, bool> groupEvalCallback; //params: equation, answer, isCorrect

    private OperatorType[] mRoundOps;
    private int[] mNumbers;

    private bool mIsAnswerCorrectWait;

    private float mPlayLastTime;

    private int mMistakeRoundCount;

    private M8.SpriteColorFromPalette[] mSpriteColorFromPalettes;

    private Coroutine mSpawnRout;
    private Coroutine mComboRout;

    protected override void OnInstanceDeinit() {
        if(connectControl)
            connectControl.evaluateCallback -= OnGroupEval;

        signalListenPlayStart.callback -= OnSignalPlayBegin;

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

        //init rounds display
        int roundsDisplayCount = Mathf.Min(mRoundOps.Length, roundsRoot.childCount);
        mSpriteColorFromPalettes = new M8.SpriteColorFromPalette[roundsDisplayCount];
        for(int i = 0; i < roundsDisplayCount; i++) {
            mSpriteColorFromPalettes[i] = roundsRoot.GetChild(i).GetComponent<M8.SpriteColorFromPalette>();
        }

        for(int i = roundsDisplayCount; i < roundsRoot.childCount; i++) //deactivate the rest
            roundsRoot.GetChild(i).gameObject.SetActive(false);

        connectControl.evaluateCallback += OnGroupEval;

        signalListenPlayStart.callback += OnSignalPlayBegin;
    }

    protected override IEnumerator Start() {
        yield return base.Start();

        //music
        M8.MusicPlaylist.instance.Play(playMusic, true, false);

        //play enter if available
        if(animator && !string.IsNullOrEmpty(takeBegin))
            yield return animator.PlayWait(takeBegin);
    }

    void OnSignalPlayBegin() {
        //start spawning
        StartCoroutine(DoRounds());

        mSpawnRout = StartCoroutine(DoBlobSpawn());

        mPlayLastTime = Time.time;
    }

    private void ApplyHintActive() {
        for(int i = 0; i < blobSpawner.blobActives.Count - 2; i++) {
            var op1 = blobSpawner.blobActives[i];

            for(int j = i + 1; j < blobSpawner.blobActives.Count - 1; j++) {
                var op2 = blobSpawner.blobActives[j];

                switch(curRoundOp) {
                    case OperatorType.Multiply:
                        for(int k = j + 1; k < blobSpawner.blobActives.Count; k++) {
                            var op3 = blobSpawner.blobActives[k];
                            if(op1.number * op2.number == op3.number
                                || op1.number * op3.number == op2.number
                                || op2.number * op3.number == op1.number) {
                                op1.hintActive = true;
                                op2.hintActive = true;
                                op3.hintActive = true;
                                return;
                            }
                        }
                        break;

                    case OperatorType.Divide:
                        for(int k = j + 1; k < blobSpawner.blobActives.Count; k++) {
                            var op3 = blobSpawner.blobActives[k];
                            if(op1.number / op2.number == op3.number || op2.number / op1.number == op3.number
                                || op1.number / op3.number == op2.number || op3.number / op1.number == op2.number
                                || op2.number / op3.number == op1.number || op3.number / op2.number == op1.number) {
                                op1.hintActive = true;
                                op2.hintActive = true;
                                op3.hintActive = true;
                                return;
                            }
                        }
                        break;
                }
            }
        }
    }

    private void ClearHintActive() {
        for(int i = 0; i < blobSpawner.blobActives.Count; i++) {
            var blob = blobSpawner.blobActives[i];
            blob.hintActive = false;
        }
    }

    IEnumerator DoRounds() {
        var hintDelay = GameData.instance.hintDelay;

        isHintShown = false;

        for(curRoundIndex = 0; curRoundIndex < mRoundOps.Length; curRoundIndex++) {
            connectControl.curOp = curRoundOp;

            //signal new round
            roundBeginCallback?.Invoke();

            var roundBeginLastTime = Time.time;
            isHintShown = false;

            mMistakeRoundCount = 0;
                        
            //wait for correct answer
            mIsAnswerCorrectWait = true;
            while(mIsAnswerCorrectWait) {
                //check if we are in the last 3 and it is unsolvable
                if(curRoundIndex == mRoundOps.Length - 1 && blobSpawner.GetBlobStateCount(Blob.State.Normal) == 3 && blobSpawner.blobActives.Count == 3) {
                    var blob1 = blobSpawner.blobActives[0];
                    var blob2 = blobSpawner.blobActives[1];
                    var blob3 = blobSpawner.blobActives[2];

                    bool isValid = false;

                    switch(curRoundOp) {
                        case OperatorType.Multiply:
                            isValid = blob1.number * blob2.number == blob3.number
                                   || blob1.number * blob3.number == blob2.number
                                   || blob2.number * blob3.number == blob1.number;
                            break;
                        case OperatorType.Divide:
                            isValid = blob1.number / blob2.number == blob3.number 
                                   || blob2.number / blob1.number == blob3.number 
                                   || blob3.number / blob1.number == blob2.number 
                                   || blob1.number / blob3.number == blob2.number 
                                   || blob3.number / blob2.number == blob1.number 
                                   || blob2.number / blob3.number == blob1.number;
                            break;
                    }

                    if(!isValid) {
                        blob1.state = Blob.State.Correct;
                        blob2.state = Blob.State.Correct;
                        blob3.state = Blob.State.Correct;

                        //force end the round, give credits
                        curScore += GameData.instance.correctPoints * (comboCount + 1);
                        break;
                    }
                }
                //check if we want hint to activate
                else if(!isHintShown) {
                    if(mMistakeRoundCount >= GameData.instance.hintErrorCount || Time.time - roundBeginLastTime > hintDelay) {
                        //find matching operands and equal based on operation
                        ApplyHintActive();
                        isHintShown = true;
                    }
                }

                yield return null;
            }

            ClearHintActive();

            mSpriteColorFromPalettes[curRoundIndex].brightness = roundCompleteBrightness;

            //signal complete round
            roundEndCallback?.Invoke();
        }

        var playTotalTime = curPlayTime;

        //wait for blobs to clear out
        while(blobSpawner.blobActives.Count > 0)
            yield return null;

        //play finish
        signalInvokePlayEnd.Invoke();

        //play end animation if available
        if(animator && !string.IsNullOrEmpty(takeEnd))
            yield return animator.PlayWait(takeEnd);

        //show victory
        var parms = new M8.GenericParams();
        parms[ModalVictory.parmLevel] = levelIndex;
        parms[ModalVictory.parmScore] = curScore;
        parms[ModalVictory.parmTime] = playTotalTime;
        parms[ModalVictory.parmRoundsCount] = mRoundOps.Length;
        parms[ModalVictory.parmMistakeCount] = mistakeCount;
        parms[ModalVictory.parmNextScene] = nextScene;

        M8.ModalManager.main.Open(modalVictory, parms);
    }

    IEnumerator DoBlobSpawn() {
        curNumberIndex = 0;

        int curBlobTemplateIndex = 0;
        
        while(curNumberIndex < mNumbers.Length) {
            var maxBlobCount = GameData.instance.blobSpawnCount;

            //check if we have enough on the board
            while(blobSpawner.blobActives.Count + blobSpawner.spawnQueueCount < maxBlobCount) {
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

    IEnumerator DoComboUpdate() {
        var maxBlobCount = GameData.instance.blobSpawnCount;

        var comboMaxTime = GameData.instance.comboDuration;

        comboCount = 0;
        comboCurTime = 0f;

        while(comboCurTime < comboMaxTime) {
            //wait for blobs to be filled
            while(mSpawnRout != null && blobSpawner.blobActives.Count < maxBlobCount)
                yield return null;

            //wait for blob states to finish
            while(blobSpawner.CheckAnyBlobActiveState(Blob.State.Spawning, Blob.State.Despawning, Blob.State.Correct))
                yield return null;
                        
            yield return null;
            comboCurTime += Time.deltaTime;

            //time expire, decrement combo?
            if(comboCurTime >= comboMaxTime) {
                if(comboCount > 1) {
                    comboCount--;
                    comboCurTime = 0;
                }
            }
        }

        comboCount = 0;
        comboCurTime = 0f;
        
        mComboRout = null;
    }

    void OnGroupEval(BlobConnectController.Group grp) {
        float op1, op2, eq;
        grp.GetNumbers(out op1, out op2, out eq);

        var op = grp.connectOp.op;

        bool isCorrect = false;

        switch(op) {
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

            blobSpawner.RemoveFromActive(blobLeft);
            blobSpawner.RemoveFromActive(blobRight);
            blobSpawner.RemoveFromActive(blobEq);

            //clean out op
            connectOp.state = BlobConnect.State.Correct;
            connectEq.state = BlobConnect.State.Correct;
                        
            //increment and refresh combo            
            if(mComboRout == null)
                mComboRout = StartCoroutine(DoComboUpdate());

            comboCurTime = 0f;
            comboCount++;

            //add score
            if(isHintShown)
                curScore += GameData.instance.correctDecayPoints;
            else
                curScore += GameData.instance.correctPoints * comboCount;

            //go to next round
            mIsAnswerCorrectWait = false;
        }
        else {
            //do error thing for blobs
            blobLeft.state = Blob.State.Error;
            blobRight.state = Blob.State.Error;
            blobEq.state = Blob.State.Error;

            //clean out op
            connectOp.state = BlobConnect.State.Error;
            connectEq.state = BlobConnect.State.Error;

            //decrement combo count
            if(mComboRout != null) {
                if(comboCount > 0)
                    comboCount--;

                if(comboCount == 0) {
                    StopCoroutine(mComboRout);
                    mComboRout = null;
                }

                comboCurTime = 0f;
            }

            mistakeCount++;
            mMistakeRoundCount++;
        }

        connectControl.ClearGroup(grp);

        groupEvalCallback?.Invoke(new Operation { operand1=(int)op1, operand2= (int)op2, op=op }, (int)eq, isCorrect);
    }
}
