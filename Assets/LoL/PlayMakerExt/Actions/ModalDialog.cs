using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using M8;

namespace HutongGames.PlayMaker.Actions.LoL {
    [ActionCategory("Legends of Learning")]
    public class DialogOpen : FsmStateAction {
        public FsmString modal;
        [ObjectType(typeof(Sprite))]
        public FsmObject portrait;
        public FsmBool usePortrait;

        public M8.FsmLocalize nameText;
        public M8.FsmLocalize[] dialogTexts;

        public FsmBool closeOnEnd;
        public FsmBool clearModals;

        private int mCurIndex;
        private bool mIsNext;

        public override void Reset() {
            modal = ModalDialog.modalNameGeneric;
            portrait = null;
            usePortrait = false;
            nameText = null;
            dialogTexts = null;
            closeOnEnd = true;
            clearModals = false;
        }

        public override void OnEnter() {
            if(dialogTexts.Length > 0) {
                mCurIndex = 0;
                OpenDialog();
            }
            else
                Finish();
        }

        public override void OnUpdate() {
            if(mIsNext) {
                if(mCurIndex < dialogTexts.Length)
                    OpenDialog();
                else {
                    mIsNext = false;

                    if(closeOnEnd.Value) {
                        if(ModalManager.main.IsInStack(modal.Value))
                            ModalManager.main.CloseUpTo(modal.Value, true);
                    }

                    Finish();
                }
            }
        }

        void OpenDialog() {

            string textRef = dialogTexts[mCurIndex].GetStringRef();
            if(!string.IsNullOrEmpty(textRef)) {
                if(clearModals.Value) {
                    var uiMgr = ModalManager.main;
                    if(uiMgr.GetTop() != modal.Value)
                        uiMgr.CloseAll();
                }

                if(usePortrait.Value)
                    ModalDialog.OpenApplyPortrait(modal.Value, portrait.Value as Sprite, nameText.GetStringRef(), textRef, OnDialogNext);
                else
                    ModalDialog.Open(modal.Value, nameText.GetStringRef(), textRef, OnDialogNext);

                mIsNext = false;
            }
            else {
                mCurIndex++;
                mIsNext = true;
            }
        }

        void OnDialogNext() {
            mCurIndex++;
            mIsNext = true;
        }
    }
}