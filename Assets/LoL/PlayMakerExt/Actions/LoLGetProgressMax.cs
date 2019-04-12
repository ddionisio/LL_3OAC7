using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.LoL {
    [ActionCategory("Legends of Learning")]
    public class LoLGetProgressMax : FsmStateAction {
        [UIHint(UIHint.Variable)]
        [RequiredField]
        public FsmInt output;

        public override void Reset() {
            output = null;
        }

        public override void OnEnter() {
            if(LoLManager.isInstantiated) {
                output.Value = LoLManager.instance.progressMax;
            }

            Finish();
        }
    }
}