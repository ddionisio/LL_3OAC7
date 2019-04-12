using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.LoL {
    [ActionCategory("Legends of Learning")]
    [Tooltip("This will effectively end the game.")]
    public class LoLComplete : FsmStateAction {
        public override void OnEnter() {
            if(LoLManager.isInstantiated)
                LoLManager.instance.Complete();

            Finish();
        }
    }
}