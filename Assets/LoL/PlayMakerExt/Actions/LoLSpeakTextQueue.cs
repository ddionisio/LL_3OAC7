using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.LoL {
    [ActionCategory("Legends of Learning")]
    [Tooltip("Queue up textRefs to be played based on group. Group will replace current queues if it is different. Ensure textRef is either from: Localize, StringRef")]
    public class LoLSpeakTextQueue : FsmStateAction {
        public FsmString group;

        [RequiredField]
        public FsmInt startIndex;

        [Tooltip("This is a reference from localization.")]
        public M8.FsmLocalize[] textRef;

        public override void Reset() {
            group = "default";
            startIndex = 0;
            textRef = new M8.FsmLocalize[] { new M8.FsmLocalize() };
        }

        public override void OnEnter() {
            if(LoLManager.isInstantiated) {
                for(int i = 0; i < textRef.Length; i++)
                    LoLManager.instance.SpeakTextQueue(textRef[i].stringRef.Value, group.Value, i + startIndex.Value);
            }

            Finish();
        }
    }
}