using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.LoL {
    [ActionCategory("Legends of Learning")]
    [Tooltip("Set current score. Make sure to call either: (ApplyCurrentProgress, IncrementProgress, SetProgress) to push it to LoL service.")]
    public class LoLSetScore : FsmStateAction {
        public FsmInt score;

        public override void Reset() {
            score = null;
        }

        public override void OnEnter() {
            if(LoLManager.isInstantiated && !score.IsNone)
                LoLManager.instance.curScore = score.Value;

            Finish();
        }
    }
}