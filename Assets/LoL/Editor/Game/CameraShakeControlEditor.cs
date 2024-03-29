﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CameraShakeControl))]
public class CameraShakeControlEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if(GUILayout.Button("Preview")) {
            ((CameraShakeControl)target).Shake();
        }
    }
}
