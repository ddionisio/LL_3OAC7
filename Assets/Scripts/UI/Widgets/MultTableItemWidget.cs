using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MultTableItemWidget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public RectTransform columnHighlight;
    public RectTransform rowHighlight;
    public float columnOfsScale = -1f; //size relative to deltaSize of this RectTransform
    public float rowOfsScale = -1f; //size relative to deltaSize of this RectTransform

    public string enterText;

    public SignalString signalInvokeTextEnter;

    void OnApplicationFocus(bool aActive) {
        if(!aActive)
            SetHighlightActive(false);
    }

    void OnEnable() {
        SetHighlightActive(false);
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
        SetHighlightActive(true);

        //apply position
        var rt = transform as RectTransform;
        var parentRT = transform.parent as RectTransform;
        var parentRTRect = parentRT.rect;
        parentRTRect.position += (Vector2)parentRT.position;

        //column
        if(columnHighlight) {
            Vector2 pos = columnHighlight.position;

            //assume proper transform setup for pivot = (0.5, 0), top stretch
            var colDeltaSize = columnHighlight.sizeDelta;

            var height = Mathf.Abs(parentRTRect.max.y - pos.y) + rt.sizeDelta.y * columnOfsScale;

            colDeltaSize.y = height;

            columnHighlight.sizeDelta = colDeltaSize;
        }

        //row
        if(rowHighlight) {
            Vector2 pos = rowHighlight.position;

            //assume proper transform setup for pivot = (1, 0.5), left stretch
            var rowDeltaSize = rowHighlight.sizeDelta;

            var width = Mathf.Abs(parentRTRect.min.x - pos.x) + rt.sizeDelta.x * rowOfsScale;

            rowDeltaSize.x = width;

            rowHighlight.sizeDelta = rowDeltaSize;
        }

        if(signalInvokeTextEnter)
            signalInvokeTextEnter.Invoke(enterText);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
        SetHighlightActive(false);
    }

    private void SetHighlightActive(bool aActive) {
        if(columnHighlight) columnHighlight.gameObject.SetActive(aActive);
        if(rowHighlight) rowHighlight.gameObject.SetActive(aActive);
    }
}
