using LoLExt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoLInteractableOnReady : MonoBehaviour {
    public Selectable target;

    private bool mIsReadyChecked;

	void OnEnable() {
        if(!mIsReadyChecked)
            StartCoroutine(DoCheck());
	}

    IEnumerator DoCheck() {
        while(!mIsReadyChecked) {
            yield return null;

            if(LoLManager.isInstantiated && LoLManager.instance.isReady) {
                if(target)
                    target.interactable = true;

				mIsReadyChecked = true;
			}
        }
    }
}
