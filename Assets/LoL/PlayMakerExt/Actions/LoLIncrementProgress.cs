using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.LoL {
    [ActionCategory("Legends of Learning")]
    public class LoLIncrementProgress : FsmStateAction {
        public override void OnEnter() {
            if(LoLManager.isInstantiated)
                LoLManager.instance.ApplyProgress(LoLManager.instance.curProgress + 1);

            Finish();
        }
    }
}