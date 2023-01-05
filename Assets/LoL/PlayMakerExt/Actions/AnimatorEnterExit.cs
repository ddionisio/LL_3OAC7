using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.LoL {
    [ActionCategory("Legends of Learning")]
    [Tooltip("Play AnimatorEnterExit's Enter and wait for it to finish.")]
    public class AnimatorEnter : ComponentAction<AnimatorEnterExit> {
        [RequiredField]
        [CheckForComponent(typeof(AnimatorEnterExit))]
        public FsmOwnerDefault gameObject;

        public override void Reset() {
            gameObject = null;
        }

        public override void OnEnter() {
            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            if(!UpdateCache(go)) {
                Finish();
                return;
            }

            cachedComponent.PlayEnter();
        }

        public override void OnUpdate() {
            if(!cachedComponent.isPlaying)
                Finish();
        }
    }

    [ActionCategory("Legends of Learning")]
    [Tooltip("Play AnimatorEnterExit's Exit and wait for it to finish.")]
    public class AnimatorExit : ComponentAction<AnimatorEnterExit> {
        [RequiredField]
        [CheckForComponent(typeof(AnimatorEnterExit))]
        public FsmOwnerDefault gameObject;

        public override void Reset() {
            gameObject = null;
        }

        public override void OnEnter() {
            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            if(!UpdateCache(go)) {
                Finish();
                return;
            }

            cachedComponent.PlayExit();
        }

        public override void OnUpdate() {
            if(!cachedComponent.isPlaying)
                Finish();
        }
    }
}