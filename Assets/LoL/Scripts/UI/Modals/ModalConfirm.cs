using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalConfirm : M8.ModalController, M8.IModalPush, M8.IModalPop, M8.IModalActive {
    public const string parmTitleTextRef = "confirmTitleTxtRef";
    public const string parmDescTextRef = "confirmDescTxtRef";
    public const string parmCallback = "confirmCB";

    [Header("UI")]
    public Text titleText;
    public Text descText;
    
    private System.Action<bool> mCallback;

    private string mDescTextRef;
    private bool mIsDescSpoken;

    public void Confirm() {
        if(mCallback != null)
            mCallback(true);

        Close();
    }

    public void Cancel() {
        if(mCallback != null)
            mCallback(false);

        Close();
    }

    void M8.IModalActive.SetActive(bool aActive) {
        if(aActive) {
            if(!mIsDescSpoken) {
                mIsDescSpoken = true;

                if(!string.IsNullOrEmpty(mDescTextRef))
                    LoLManager.instance.SpeakText(mDescTextRef);
            }
        }
    }

    void M8.IModalPop.Pop() {
        mCallback = null;
    }

    void M8.IModalPush.Push(M8.GenericParams parms) {
        mDescTextRef = null;
        mIsDescSpoken = false;
        mCallback = null;

        if(parms != null) {
            if(parms.ContainsKey(parmTitleTextRef)) {
                if(titleText) titleText.text = M8.Localize.Get(parms.GetValue<string>(parmTitleTextRef));
            }

            if(parms.ContainsKey(parmDescTextRef)) {
                mDescTextRef = parms.GetValue<string>(parmDescTextRef);
                if(descText) descText.text = M8.Localize.Get(mDescTextRef);
            }

            if(parms.ContainsKey(parmCallback))
                mCallback = parms.GetValue<System.Action<bool>>(parmCallback);
        }
    }
}
