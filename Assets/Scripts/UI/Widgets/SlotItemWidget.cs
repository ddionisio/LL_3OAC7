using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SlotItemWidget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
    [Header("Data")]
    public float moveDelay = 0.3f; //when moving to a slot

    [Header("Display")]
    public Text numberText;
    public GameObject highlightGO;
    public Transform dragRoot;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeDragBegin;

    [Header("Sound")]
    [M8.SoundPlaylist]
    public string soundPlace;

    public bool inputLocked {
        get { return mInputLocked || mMoveRout != null; }
        set {
            mInputLocked = value;

            if(mInputLocked) {
                DragInvalidate();

                if(highlightGO) highlightGO.SetActive(false);
            }
        }
    }

    public bool isHighlighted {
        get { return highlightGO ? highlightGO.activeSelf : false; }
    }

    public bool isDragging { get; private set; }

    public SlotWidget slotCurrent { get; private set; }
    public SlotWidget slotCorrect { get; private set; }

    public bool isMoving { get { return mMoveRout != null; } }

    public bool isSlotCorrect { get { return slotCurrent == slotCorrect; } }

    private bool mInputLocked;

    private SlotWidget mSlotOrigin;

    private Transform mDragAreaRoot;

    private Vector3 mDragDefaultLPos;

    private Coroutine mMoveRout;

    public void SetCurrentSlot(SlotWidget slot) {
        slotCurrent = slot;
        if(slotCurrent) {
            transform.SetParent(slotCurrent.transform, true);
            MoveToCurrentSlot();
        }
    }

    public void RevertSlotToOrigin() {
        slotCurrent = mSlotOrigin;
        transform.SetParent(slotCurrent.transform, true);
        MoveToCurrentSlot();
    }

    public void Init(int number, SlotWidget originSlot, SlotWidget correctSlot, Transform dragAreaRoot) {
        //setup refs
        slotCurrent = mSlotOrigin = originSlot;
        slotCorrect = correctSlot;
        mDragAreaRoot = dragAreaRoot;

        transform.SetParent(slotCurrent.transform);
        transform.localPosition = Vector3.zero;

        if(numberText) numberText.text = number.ToString();

        //reset states
        if(highlightGO) highlightGO.SetActive(false);

        mInputLocked = false;
        isDragging = false;
    }

    public void Deinit(Transform toParent) {
        slotCurrent = mSlotOrigin = slotCorrect = null;
        mDragAreaRoot = null;

        transform.SetParent(toParent);
    }

    void OnApplicationFocus(bool focus) {
        if(!focus)
            DragInvalidate();
    }

    void OnDisable() {
        mMoveRout = null;
    }

    void Awake() {
        mDragDefaultLPos = dragRoot.localPosition;
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
        if(inputLocked)
            return;

        if(highlightGO) highlightGO.SetActive(true);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
        if(highlightGO) highlightGO.SetActive(false);
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
        if(inputLocked)
            return;

        if(animator && !string.IsNullOrEmpty(takeDragBegin))
            animator.Play(takeDragBegin);

        if(highlightGO) highlightGO.SetActive(false);

        transform.SetParent(mDragAreaRoot);

        isDragging = true;

        DragUpdate(eventData);
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        if(!isDragging)
            return;

        DragUpdate(eventData);
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
        if(!isDragging)
            return;

        if(eventData.pointerCurrentRaycast.isValid) {
            var ptrGO = eventData.pointerCurrentRaycast.gameObject;

            //check for a slot item
            if(ptrGO != gameObject && ptrGO != slotCurrent.gameObject) {
                var slotItem = ptrGO.GetComponentInChildren<SlotItemWidget>();
                if(slotItem) {
                    //swap slot
                    var toSlot = slotItem.slotCurrent;

                    slotItem.SetCurrentSlot(slotCurrent);

                    slotCurrent = toSlot;

                    if(!string.IsNullOrEmpty(soundPlace))
                        M8.SoundPlaylist.instance.Play(soundPlace, false);
                }
                else {
                    //check for slot
                    var slot = ptrGO.GetComponent<SlotWidget>();
                    if(slot) {
                        //apply slot
                        slotCurrent = slot;

                        if(!string.IsNullOrEmpty(soundPlace))
                            M8.SoundPlaylist.instance.Play(soundPlace, false);
                    }
                }
            }
        }

        DragInvalidate();
    }

    IEnumerator DoMoveToCurrentSlot() {
        var easeFunc = DG.Tweening.Core.Easing.EaseManager.ToEaseFunction(DG.Tweening.Ease.OutSine);

        Vector2 startPos = transform.position;
        Vector2 endPos = slotCurrent.transform.position;

        var dpos = endPos - startPos;
        var dist = dpos.magnitude;
        if(dist > 0f) {
            var curTime = 0f;
            while(curTime < moveDelay) {
                yield return null;

                curTime += Time.deltaTime;

                var t = easeFunc(curTime, moveDelay, 0f, 0f);

                transform.position = Vector2.Lerp(startPos, endPos, t);
            }
        }

        mMoveRout = null;
    }

    private void DragUpdate(PointerEventData eventData) {
        dragRoot.position = eventData.position;

        bool isHighlight = false;

        //check if cast object is placeable
        if(eventData.pointerCurrentRaycast.isValid) {
            var ptrGO = eventData.pointerCurrentRaycast.gameObject;
            if(ptrGO.GetComponent<SlotWidget>() || ptrGO.GetComponent<SlotItemWidget>())
                isHighlight = true;
        }

        if(highlightGO) highlightGO.SetActive(isHighlight);
    }

    private void DragInvalidate() {
        transform.SetParent(slotCurrent.transform, true);
        transform.localPosition = Vector3.zero;
        dragRoot.localPosition = mDragDefaultLPos;

        isDragging = false;
    }

    private void MoveToCurrentSlot() {
        if(mMoveRout != null)
            StopCoroutine(mMoveRout);

        mMoveRout = StartCoroutine(DoMoveToCurrentSlot());
    }
}
