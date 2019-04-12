using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.LoL {
    [ActionCategory("Legends of Learning")]
    public class LoLSetProgress : FsmStateAction {
        public FsmInt progress;

        public override void Reset() {
            progress = null;
        }

        public override void OnEnter() {
            if(LoLManager.isInstantiated && !progress.IsNone)
                LoLManager.instance.ApplyProgress(progress.Value);

            Finish();
        }
    }
}