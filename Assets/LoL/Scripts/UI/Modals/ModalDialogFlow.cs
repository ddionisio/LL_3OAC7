//#define DIALOG_DISABLE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoLExt {
    [System.Serializable]
    public class ModalDialogFlow {
        public Sprite portrait;

        [M8.Localize]
        public string[] dialogTextRefs;

        private bool mIsNext;

        public IEnumerator Play() {
            yield return Play(ModalDialog.modalNameGeneric, portrait, null, false);
        }

        public IEnumerator PlayPaused() {
            yield return Play(ModalDialog.modalNameGeneric, portrait, null, true);
        }

        public IEnumerator Play(Sprite portrait) {
            yield return Play(ModalDialog.modalNameGeneric, portrait, null, false);
        }

        public IEnumerator PlayPaused(Sprite portrait) {
            yield return Play(ModalDialog.modalNameGeneric, portrait, null, true);
        }

        public IEnumerator Play(string modal, Sprite portrait, string nameTextRef, bool isPause) {
#if DIALOG_DISABLE
            if(mIsNext) { }
            yield return null;
#else
            if(isPause)
                M8.SceneManager.instance.Pause();

            for(int i = 0; i < dialogTextRefs.Length; i++) {
                string textRef = dialogTextRefs[i];
                if(string.IsNullOrEmpty(textRef))
                    continue;

                mIsNext = false;

                if(portrait)
                    ModalDialog.OpenApplyPortrait(modal, portrait, nameTextRef, textRef, OnDialogNext);
                else
                    ModalDialog.Open(modal, nameTextRef, textRef, OnDialogNext);

                while(!mIsNext)
                    yield return null;
            }

            if(M8.ModalManager.main.IsInStack(modal))
                M8.ModalManager.main.CloseUpTo(modal, true);

            //wait for dialog to close
            while(M8.ModalManager.main.isBusy || M8.ModalManager.main.IsInStack(modal))
                yield return null;

            if(isPause)
                M8.SceneManager.instance.Resume();
#endif
        }

        void OnDialogNext() {
            mIsNext = true;
        }
    }

    [System.Serializable]
    public class ModalDialogFlowIncremental {
        public string modal;
        public Sprite portrait;

        public string prefix;
        public int startIndex;
        public int count; //set to 0 for infinite

        private System.Text.StringBuilder mSB;
        private bool mIsNext;
                
        public IEnumerator Play() {
            yield return Play(GetModal(), portrait, null, false);
        }

        public IEnumerator PlayPaused() {
            yield return Play(GetModal(), portrait, null, true);
        }

        public IEnumerator Play(Sprite portrait) {
            yield return Play(GetModal(), portrait, null, false);
        }

        public IEnumerator PlayPaused(Sprite portrait) {
            yield return Play(GetModal(), portrait, null, true);
        }

        public IEnumerator Play(string aModal, Sprite portrait, string nameTextRef, bool isPause) {
#if DIALOG_DISABLE
			if(mIsNext) { }
			yield return null;
#else
            if(mSB == null)
                mSB = new System.Text.StringBuilder(prefix.Length + 2);

            int ind = 0;

            if(isPause)
                M8.SceneManager.instance.Pause();
            
            while(true) {
                mSB.Clear();
                mSB.Append(prefix);
                mSB.Append(ind + startIndex);

                string textRef = mSB.ToString();
                if(M8.Localize.Contains(textRef)) {
                    mIsNext = false;

                    if(portrait)
                        ModalDialog.OpenApplyPortrait(aModal, portrait, nameTextRef, textRef, OnDialogNext);
                    else
                        ModalDialog.Open(aModal, nameTextRef, textRef, OnDialogNext);

                    while(!mIsNext)
                        yield return null;

                    ind++;
                    if(count > 0 && ind == count)
                        break;
                }
                else
                    break;
            }

            if(M8.ModalManager.main.IsInStack(aModal))
                M8.ModalManager.main.CloseUpTo(aModal, true);

            //wait for dialog to close
            while(M8.ModalManager.main.isBusy || M8.ModalManager.main.IsInStack(aModal))
                yield return null;

            if(isPause)
                M8.SceneManager.instance.Resume();
#endif
        }

        void OnDialogNext() {
            mIsNext = true;
        }

		private string GetModal() { return !string.IsNullOrEmpty(modal) ? modal : ModalDialog.modalNameGeneric; }
	}
}