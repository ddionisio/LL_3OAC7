using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using M8;

namespace HutongGames.PlayMaker.Actions.LoL {
    [ActionCategory("Legends of Learning")]
    public class SignalGameModeChangedWait : FsmStateAction {
        [ObjectType(typeof(GameModeSignal))]
        [RequiredField]
        public FsmObject signal;

        public FsmEvent waitEndEvent;

        private GameModeSignal mSignal;

        public override void OnEnter() {
            mSignal = (GameModeSignal)signal.Value;
            mSignal.callback += OnSignal;
        }

        public override void OnExit() {
            if(mSignal)
                mSignal.callback -= OnSignal;
        }

        void OnSignal(GameMode parm) {
            Fsm.Event(waitEndEvent);
            Finish();
        }
    }
}