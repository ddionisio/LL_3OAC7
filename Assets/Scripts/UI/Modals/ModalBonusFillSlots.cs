using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalBonusFillSlots : M8.ModalController, M8.IModalPush, M8.IModalPop, M8.IModalActive {
    public const string parmDataIndexMin = "datIndMin";
    public const string parmDataIndexMax = "datIndMax";

    [System.Serializable]
    public class Data {
        public int[] numbers;
        public int[] slotNumbers;
    }

    [Header("Data")]
    public Data[] data;

    [Header("Template")]
    public SlotItemWidget templateItem;
    public SlotWidget templateItemSlot;
    public Transform templateCacheHolder;

    [Header("Slot Data")]
    public Text[] numberTexts;
    public SlotWidget[] fillSlots;
    public Transform itemSlotRoot;

    [Header("Drag Data")]
    public Transform dragAreaRoot;

    [Header("Instruct Data")]
    public DragToGuideWidget instructDragGuide;
    public Transform instructDragStart;
    public Transform instructDragEnd;

    [Header("Time Data")]
    public RectTransform timeWidget;
    public M8.UI.Graphics.ColorGroup timeColorGroup;
    public Color[] timeColors;
    public float timeNearExpirePulse = 1.0f;
    public float timeNearExpireScale = 0.75f;

    [Header("Bonus Score Data")]
    public Text bonusScoreText;
    [M8.Localize]
    public string bonusScoreTextFormatRef;

    [Header("Bottom Data")]
    public GameObject incorrectGO;
    public float incorrectDelay = 1f;
    public GameObject timeExpireGO;
    public GameObject bonusScoreGO;
    public GameObject finishGO;

    [Header("Sounds")]
    [M8.SoundPlaylist]
    public string soundCorrect;
    [M8.SoundPlaylist]
    public string soundIncorrect;

    [Header("Signal Invoke")]
    public SignalInteger signalInvokeBonusScore;
        
    private const int slotCapacity = 4;

    private M8.CacheList<SlotWidget> mItemSlotActives = new M8.CacheList<SlotWidget>(slotCapacity);
    private M8.CacheList<SlotWidget> mItemSlotCache = new M8.CacheList<SlotWidget>(slotCapacity);

    private M8.CacheList<SlotItemWidget> mItemActives = new M8.CacheList<SlotItemWidget>(slotCapacity);
    private M8.CacheList<SlotItemWidget> mItemCache = new M8.CacheList<SlotItemWidget>(slotCapacity);

    private float mTimeDefaultWidth;

    private Coroutine mRout;
    private Coroutine mIncorrectRout;

    private bool mIsCorrect;

    public void Next() {
        if(mIsCorrect) {
            if(signalInvokeBonusScore)
                signalInvokeBonusScore.Invoke(GameData.instance.bonusRoundScore);
        }

        Close();
    }

    void M8.IModalActive.SetActive(bool aActive) {
        if(aActive) {
            instructDragGuide.Show(false, instructDragStart.position, instructDragEnd.position);

            //start
            mRout = StartCoroutine(DoActive());
        }
        else {
            instructDragGuide.Hide();

            StopRouts();
        }
    }

    void M8.IModalPop.Pop() {
        ClearSlots();

        StopRouts();
    }

    void M8.IModalPush.Push(M8.GenericParams parms) {
        //grab data
        int datIndex = 0;

        if(parms != null) {
            int minInd = 0, maxInd = 0;

            if(parms.ContainsKey(parmDataIndexMin))
                minInd = parms.GetValue<int>(parmDataIndexMin);
            if(parms.ContainsKey(parmDataIndexMax))
                maxInd = parms.GetValue<int>(parmDataIndexMax);

            if(minInd == maxInd)
                datIndex = minInd;
            else
                datIndex = Random.Range(minInd, maxInd + 1);
        }

        datIndex = Mathf.Clamp(datIndex, 0, data.Length - 1);

        var dat = data[datIndex];

        ClearSlots();

        //fill up itemSlots based on the amount to fill
        for(int i = 0; i < fillSlots.Length; i++) {
            SlotWidget slot;
            if(mItemSlotCache.Count > 0) {
                slot = mItemSlotCache.RemoveLast();
                slot.transform.SetParent(itemSlotRoot);
            }
            else
                slot = Instantiate(templateItemSlot, itemSlotRoot);

            mItemSlotActives.Add(slot);
        }

        mItemSlotActives.Shuffle();

        //fill slot items
        for(int i = 0; i < fillSlots.Length; i++) {
            var slot = mItemSlotActives[i];
            var fillSlot = fillSlots[i];
            var number = dat.slotNumbers[i];

            SlotItemWidget item;
            if(mItemCache.Count > 0)
                item = mItemCache.RemoveLast();
            else
                item = Instantiate(templateItem);

            item.Init(number, slot, fillSlot, dragAreaRoot);

            mItemActives.Add(item);
        }

        //setup fixed numbers
        for(int i = 0; i < numberTexts.Length; i++) {
            var number = dat.numbers[i];
            numberTexts[i].text = number.ToString();
        }

        //setup bonus score display
        var textFormat = M8.Localize.Get(bonusScoreTextFormatRef);
        bonusScoreText.text = string.Format(textFormat, GameData.instance.bonusRoundScore);

        //init bottom display
        incorrectGO.SetActive(false);
        timeExpireGO.SetActive(false);
        bonusScoreGO.SetActive(false);
        finishGO.SetActive(false);
    }

    void Awake() {
        mTimeDefaultWidth = timeWidget.sizeDelta.x;
    }

    IEnumerator DoActive() {
        float curTime = 0f;
        float duration = GameData.instance.bonusRoundDuration;
                
        mIsCorrect = false;

        while(curTime < duration) {
            yield return null;

            //check slots
            int filledSlotCount = 0;
            int correctSlotCount = 0;
            int moveCount = 0;
            for(int i = 0; i < mItemActives.Count; i++) {
                var item = mItemActives[i];

                if(item.isMoving)
                    moveCount++;

                if(item.isSlotCorrect) {
                    correctSlotCount++;
                    filledSlotCount++;
                }
                else if(!mItemSlotActives.Exists(item.slotCurrent))
                    filledSlotCount++;
            }

            //all slots correctly filled?
            if(correctSlotCount == fillSlots.Length) {
                mIsCorrect = true;
                break;
            }
            //wait for item slots to finish moving
            else if(moveCount > 0)
                continue;
            //incorrectly filled?
            else if(filledSlotCount == fillSlots.Length) {
                //return all items
                for(int i = 0; i < mItemActives.Count; i++) {
                    var item = mItemActives[i];
                    item.RevertSlotToOrigin();
                }

                if(mIncorrectRout != null)
                    StopCoroutine(mIncorrectRout);
                mIncorrectRout = StartCoroutine(DoIncorrect());

                continue;
            }

            curTime += Time.deltaTime;

            float t = Mathf.Clamp01(curTime / duration);

            //update time display
            var timeSize = timeWidget.sizeDelta;
            timeSize.x = mTimeDefaultWidth * (1f - t);
            timeWidget.sizeDelta = timeSize;

            var timeColor = M8.ColorUtil.Lerp(timeColors, t);

            if(t >= timeNearExpireScale) {
                var a = Mathf.Sin(Mathf.PI * curTime * timeNearExpirePulse);
                a *= a;
                timeColor.a = a;
            }

            timeColorGroup.ApplyColor(timeColor);
            //
        }
                
        mRout = null;

        //cancel incorrect
        if(mIncorrectRout != null) {
            StopCoroutine(mIncorrectRout);
            mIncorrectRout = null;
        }

        incorrectGO.SetActive(false);

        LockItemInputs();

        //all correct?
        if(mIsCorrect) {
            if(!string.IsNullOrEmpty(soundCorrect))
                M8.SoundPlaylist.instance.Play(soundCorrect, false);

            //bonus achieved
            bonusScoreGO.SetActive(true);
        }
        else {
            if(!string.IsNullOrEmpty(soundIncorrect))
                M8.SoundPlaylist.instance.Play(soundIncorrect, false);

            //time expired
            timeExpireGO.SetActive(true);
        }

        finishGO.SetActive(true);
    }

    IEnumerator DoIncorrect() {
        if(!string.IsNullOrEmpty(soundIncorrect))
            M8.SoundPlaylist.instance.Play(soundIncorrect, false);

        incorrectGO.SetActive(true);

        yield return new WaitForSeconds(incorrectDelay);

        incorrectGO.SetActive(false);

        mIncorrectRout = null;
    }

    private void LockItemInputs() {
        for(int i = 0; i < mItemActives.Count; i++) {
            var item = mItemActives[i];
            item.inputLocked = true;
        }
    }

    private void ClearSlots() {
        //slots
        for(int i = 0; i < mItemSlotActives.Count; i++) {
            var slot = mItemSlotActives[i];
            if(slot) {
                slot.transform.SetParent(templateCacheHolder);
                mItemSlotCache.Add(slot);
            }
        }

        mItemSlotActives.Clear();

        //slot items
        for(int i = 0; i < mItemActives.Count; i++) {
            var itm = mItemActives[i];
            if(itm) {
                itm.Deinit(templateCacheHolder);
                mItemCache.Add(itm);
            }
        }

        mItemActives.Clear();
    }

    private void StopRouts() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }

        if(mIncorrectRout != null) {
            StopCoroutine(mIncorrectRout);
            mIncorrectRout = null;
        }
    }
}
