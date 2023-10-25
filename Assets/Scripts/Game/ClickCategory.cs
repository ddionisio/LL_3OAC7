using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickCategory : MonoBehaviour, IPointerClickHandler {
    public int category;

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
		GameData.instance.ClickCategory(category);
	}
}
