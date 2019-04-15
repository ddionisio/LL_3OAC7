using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[System.Serializable]
public class JellySpriteEvent : UnityEvent<JellySprite, int, BaseEventData> {
}