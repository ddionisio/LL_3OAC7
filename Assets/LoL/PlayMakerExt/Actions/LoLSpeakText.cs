using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.LoL {
    [ActionCategory("Legends of Learning")]
    [Tooltip("Immediately play speech. Ensure textRef is either from: Localize, StringRef")]
    public class LoLSpeakText : FsmStateAction {
        [Tooltip("This is a reference from localization.")]
        public M8.FsmLocalize textRef;

        public override void Reset() {
            textRef = new M8.FsmLocalize();
        }

        public override void OnEnter() {
            if(LoLManager.isInstantiated)
                LoLManager.instance.SpeakText(textRef.stringRef.Value);

            Finish();
        }
    }
}