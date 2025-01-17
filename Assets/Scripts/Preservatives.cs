using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Preservatives : MonoBehaviour {
	public Graphic _img;
	public CanvasGroup _canvasGroup;
	public RectTransformGroupSizeDeltaRange _rectTransformGroupSizeDeltaRange;
	public RectTransform _rectTransform;

	void Awake() {
		if(_img) {
			_img.color = Color.white;
		}

		if(_canvasGroup) {
			_canvasGroup.alpha = 1f;
			_canvasGroup.interactable = true;
			_canvasGroup.blocksRaycasts = true;
		}

		if(_rectTransformGroupSizeDeltaRange) {
			_rectTransformGroupSizeDeltaRange.range = 0f;
		}

		if(_rectTransform) {
			_rectTransform.anchoredPosition = Vector2.zero;
			_rectTransform.sizeDelta = Vector2.zero;
		}
	}
}
