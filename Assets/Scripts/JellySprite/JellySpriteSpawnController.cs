using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JellySpriteSpawnController : MonoBehaviour, M8.IPoolSpawn, M8.IPoolDespawn {
    public const string parmPosition = "position";
    public const string parmRotation = "rotation";
    public const string parmSprite = "sprite";

    public UnityJellySprite jellySprite;

    private int mDefaultLayer;

    void M8.IPoolSpawn.OnSpawned(M8.GenericParams parms) {

    }

    void M8.IPoolDespawn.OnDespawned() {

    }

    void Awake() {
        
    }
}
