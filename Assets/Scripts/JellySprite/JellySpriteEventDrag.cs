using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Jelly Sprite/Events/Drag")]
public class JellySpriteEventDrag : JellySpriteEventBase {
    [Header("Events")]
    public JellySpriteEvent beginDragEvent;
    public JellySpriteEvent dragEvent;
    public JellySpriteEvent endDragEvent;
    public JellySpriteEvent initializePotentialDragEvent;

    protected override void AddCallbacks(JellySpriteEventTrigger eventTrigger) {
        eventTrigger.beginDragCallback += beginDragEvent.Invoke;
        eventTrigger.dragCallback += dragEvent.Invoke;
        eventTrigger.endDragCallback += endDragEvent.Invoke;
        eventTrigger.initializePotentialDragCallback += initializePotentialDragEvent.Invoke;
    }

    protected override void RemoveCallbacks(JellySpriteEventTrigger eventTrigger) {
        eventTrigger.beginDragCallback -= beginDragEvent.Invoke;
        eventTrigger.dragCallback -= dragEvent.Invoke;
        eventTrigger.endDragCallback -= endDragEvent.Invoke;
        eventTrigger.initializePotentialDragCallback -= initializePotentialDragEvent.Invoke;
    }
}