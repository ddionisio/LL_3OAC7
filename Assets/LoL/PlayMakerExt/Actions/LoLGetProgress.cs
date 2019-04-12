using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.LoL {
    [ActionCategory("Legends of Learning")]
    public class LoLGetProgress : FsmStateAction {
        [UIHint(UIHint.Variable)]
        [RequiredField]
        public FsmInt output;

        public FsmBool everyFrame;

        public override void Reset() {
            output = null;
            everyFrame = false;
        }

        public override void OnEnter() {
            DoGet();

            if(!everyFrame.Value)
                Finish();
        }

        public override void OnUpdate() {
            DoGet();
        }

        void DoGet() {
            if(!LoLManager.isInstantiated)
                return;

            output.Value = LoLManager.instance.curProgress;
        }
    }
}