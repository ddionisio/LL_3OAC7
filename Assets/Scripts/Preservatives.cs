using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Preservatives : MonoBehaviour {
	public Graphic _img;
	public CanvasGroup _canvasGroup;
	public RectTransformGroupSizeDeltaRange _rectTransformGroupSizeDeltaRange;
	public RectTransform _rectTransform;
	public Blob _blob;
	public SpriteRenderer _spriteRenderer;
	public M8.SpriteColorAlpha _spriteColorAlpha;
	public ParticleSystem _particleSystem;

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

		if(_blob) {
			_blob.colorAlpha = 1f;
		}

		if(_spriteRenderer) {
			_spriteRenderer.sprite = null;
			_spriteRenderer.color = Color.white;
		}

		if(_spriteColorAlpha) {
			_spriteColorAlpha.alpha = 1f;
		}

		if(_particleSystem) {
			_particleSystem.Play();
			_particleSystem.Stop();
		}
	}
}
