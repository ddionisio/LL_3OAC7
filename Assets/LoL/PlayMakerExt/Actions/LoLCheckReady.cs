using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.LoL {
    [ActionCategory("Legends of Learning")]
    public class LoLCheckReady : FsmStateAction {
        [UIHint(UIHint.Variable)]
        public FsmBool storeResult;

        public FsmEvent isTrue;
        public FsmEvent isFalse;

        public FsmBool everyFrame;

        public override void Reset() {
            isTrue = null;
            isFalse = null;
            storeResult = null;
            everyFrame = false;
        }

        // Code that runs on entering the state.
        public override void OnEnter() {
            Check();

            if(!everyFrame.Value)
                Finish();
        }

        public override void OnUpdate() {
            Check();
        }

        void Check() {            
            bool isReady = LoLManager.isInstantiated ? LoLManager.instance.isReady : true;

            if(!storeResult.IsNone)
                storeResult.Value = isReady;

            Fsm.Event(isReady ? isTrue : isFalse);
        }

        public override string ErrorCheck() {
            if(everyFrame.Value &&
                FsmEvent.IsNullOrEmpty(isTrue) &&
                FsmEvent.IsNullOrEmpty(isFalse))
                return "Action sends no events!";
            return "";
        }
    }
}