using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalRetry : M8.ModalController, M8.IModalPush, M8.IModalPop {
    public const string parmContinueCallback = "ccb";

    private System.Action mContinueCallback;

    public void Retry() {
        M8.SceneManager.instance.Reload();
    }

    public void Continue() {
        mContinueCallback?.Invoke();
    }

    void M8.IModalPush.Push(M8.GenericParams parms) {
        if(parms != null) {
            if(parms.ContainsKey(parmContinueCallback))
                mContinueCallback = parms[parmContinueCallback] as System.Action;
        }
    }

    void M8.IModalPop.Pop() {
        mContinueCallback = null;
    }
}
