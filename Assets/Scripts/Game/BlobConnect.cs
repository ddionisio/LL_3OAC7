﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Connection between blobs with an operator or equal
/// </summary>
public class BlobConnect : MonoBehaviour, M8.IPoolSpawn, M8.IPoolDespawn {
    public enum State {
        None,
        Connecting, //dragging to connect to another blob
        Connected,
        Releasing, //hold, then break out, and release
        Dragging, //drag around
    }

    [Header("Body Display")]
    public GameObject bodyConnectedGO;
    public GameObject bodyConnectingGO;
    public Transform connectingRoot; //follow mouse pointer

    [Header("Operator Display")]
    public GameObject[] operatorMultiplyGO;
    public GameObject[] operatorDivideGO;
    public GameObject[] operatorEqualGO;

    [Header("Link Display")]
    public SpriteRenderer linkBeginSpriteRender; //ensure it is in 9-slice render
    public SpriteRenderer linkEndSpriteRender; //ensure it is in 9-slice render
    public float linkConnectOfs = 0f; //length offset connecting towards blob

    [Header("Physics")]
    public Rigidbody2D body;
    public Collider2D coll;
    public float springDampingRatio = 0f;
    public float springDistance = 2.5f;
    public float springFrequency = 1f;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeConnectingEnter;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeConnectingExit;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeConnectedEnter;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeConnectedExit;

    public State state {
        get { return mState; }
        set {
            if(mState != value) {
                var prevState = mState;
                mState = value;
                ApplyCurState(prevState);
            }
        }
    }

    public OperatorType op {
        get { return mOp; }
        set {
            if(mOp != value) {
                mOp = value;
                ApplyCurOperator();
            }
        }
    }

    public M8.PoolDataController poolData { get; private set; }

    public Blob blobLinkStart { get; private set; }
    public Blob blobLinkEnd { get; private set; }

    private SpringJoint2D mLinkBeginSpring;
    private SpringJoint2D mLinkEndSpring;
        
    private State mState = State.None;
    private OperatorType mOp = OperatorType.None;
        
    private Coroutine mRout;

    public bool IsConnectedTo(Blob blob) {
        return blob == blobLinkStart || blob == blobLinkEnd;
    }

    /// <summary>
    /// Set state to connected and link both blobs
    /// </summary>
    public void ApplyLink(Blob blobStart, Blob blobEnd) {
        SpringRelease();
                
        //grab mid point
        Vector2 startPt = blobStart.jellySprite.CentralPoint.Body2D.position;
        Vector2 EndPt = blobEnd.jellySprite.CentralPoint.Body2D.position;
        Vector2 midPt = Vector2.Lerp(startPt, EndPt, 0.5f);

        //set ourself in mid point
        transform.position = midPt;

        //connect start
        Vector2 startEdgePt;
        int startEdgeRefPtInd;
        blobStart.GetEdge(midPt, out startEdgePt, out startEdgeRefPtInd);

        //just use center pt of ref
        startEdgePt = blobStart.jellySprite.ReferencePoints[startEdgeRefPtInd].Body2D.position;

        mLinkBeginSpring = GenerateSpringJoint(blobStart.jellySprite.ReferencePoints[startEdgeRefPtInd], startEdgePt);

        //connect end
        Vector2 endEdgePt;
        int endEdgeRefPtInd;
        blobEnd.GetEdge(midPt, out endEdgePt, out endEdgeRefPtInd);

        //just use center pt of ref
        endEdgePt = blobEnd.jellySprite.ReferencePoints[endEdgeRefPtInd].Body2D.position;

        mLinkEndSpring = GenerateSpringJoint(blobEnd.jellySprite.ReferencePoints[endEdgeRefPtInd], endEdgePt);

        state = State.Connected;
    }

    private SpringJoint2D GenerateSpringJoint(JellySprite.ReferencePoint refPt, Vector2 refPtPos) {
        var joint = gameObject.AddComponent<SpringJoint2D>();
        joint.enableCollision = true;
        joint.anchor = Vector2.zero;
        joint.dampingRatio = springDampingRatio;
        joint.frequency = springFrequency;
        joint.autoConfigureDistance = false;
        joint.distance = springDistance;
        joint.connectedBody = refPt.Body2D;
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = refPt.transform.worldToLocalMatrix.MultiplyPoint3x4(refPtPos);
        return joint;
    }

    /// <summary>
    /// Call during Connecting state
    /// </summary>
    public void UpdateConnecting(Vector2 srcPoint, Vector2 destPoint) {
        var dpos = srcPoint - destPoint;
        var lenSqr = dpos.sqrMagnitude;
        if(lenSqr > 0f) {
            var len = Mathf.Sqrt(lenSqr);
            var halfLen = len * 0.5f;
            var dirToSrc = dpos / len;
            var midPt = destPoint + dirToSrc * halfLen;

            //apply link displays
            if(linkBeginSpriteRender) {
                linkBeginSpriteRender.gameObject.SetActive(true);
                linkBeginSpriteRender.transform.up = dirToSrc;

                var s = linkBeginSpriteRender.size;
                s.y = halfLen;
                linkBeginSpriteRender.size = s;
            }

            if(linkEndSpriteRender) {
                linkEndSpriteRender.gameObject.SetActive(true);
                linkEndSpriteRender.transform.up = -dirToSrc;

                var s = linkEndSpriteRender.size;
                s.y = halfLen;
                linkEndSpriteRender.size = s;
            }

            //set self to midway
            transform.position = midPt;
        }
        else {
            if(linkBeginSpriteRender) linkBeginSpriteRender.gameObject.SetActive(false);
            if(linkEndSpriteRender) linkBeginSpriteRender.gameObject.SetActive(false);

            //set self to destPoint
            transform.position = destPoint;
        }

        if(connectingRoot) connectingRoot.position = destPoint;
    }

    public void Release() {
        if(poolData)
            poolData.Release();
    }

    void M8.IPoolDespawn.OnDespawned() {
        StopRout();

        SpringRelease();

        //this will reset display to nothing
        ApplyCurState(State.None);
    }

    void M8.IPoolSpawn.OnSpawned(M8.GenericParams parms) {
        if(!poolData) poolData = GetComponent<M8.PoolDataController>();

        var toState = State.None;
        var toOp = OperatorType.None;

        mState = toState;
        ApplyCurState(State.None);

        mOp = toOp;
        ApplyCurOperator();
    }

    void Awake() {
        ApplyCurState(State.None);

        mOp = OperatorType.None;
        ApplyCurOperator();
    }

    void Update() {
        switch(mState) {
            case State.Connected:
                ApplyLinkTelemetry(linkBeginSpriteRender, mLinkBeginSpring);
                ApplyLinkTelemetry(linkEndSpriteRender, mLinkEndSpring);
                break;
        }
    }

    private void ApplyLinkTelemetry(SpriteRenderer spriteRenderer, SpringJoint2D joint) {
        if(!spriteRenderer)
            return;
        
        Vector2 connectStartPos = transform.position;

        if(joint) {
            //orient towards connected
            Vector2 connectEndPos = joint.connectedBody.transform.TransformPoint(joint.connectedAnchor);
            Vector2 dpos = connectEndPos - connectStartPos;
            float len = dpos.magnitude;

            if(len > 0f) {
                spriteRenderer.gameObject.SetActive(true);

                spriteRenderer.transform.up = dpos / len;

                var s = spriteRenderer.size;
                s.y = len + linkConnectOfs;
                spriteRenderer.size = s;
            }
            else { //fail-safe
                spriteRenderer.gameObject.SetActive(false);
            }
        }
        else {
            spriteRenderer.gameObject.SetActive(false);
        }
    }

    private void ApplyCurState(State prevState) {
        StopRout();

        switch(mState) {
            case State.Connected:
                SetPhysicsActive(true);

                if(body) body.bodyType = RigidbodyType2D.Dynamic;

                if(bodyConnectedGO) bodyConnectedGO.SetActive(true);
                if(connectingRoot) connectingRoot.gameObject.SetActive(false);

                switch(prevState) {
                    case State.Connecting:
                        mRout = StartCoroutine(DoAnimations(HideConnectingDisplay, takeConnectingExit, takeConnectedEnter));
                        break;
                    case State.Dragging:
                        break;
                    default:
                        mRout = StartCoroutine(DoAnimations(null, takeConnectedEnter));
                        break;
                }
                break;

            case State.Connecting:
                SetPhysicsActive(false);

                SpringRelease();

                if(bodyConnectingGO) bodyConnectingGO.SetActive(true);
                if(connectingRoot) connectingRoot.gameObject.SetActive(true);

                switch(prevState) {
                    case State.Connected:
                    case State.Dragging:
                        mRout = StartCoroutine(DoAnimations(HideConnectDisplay, takeConnectedExit, takeConnectingEnter));
                        break;
                    default:
                        mRout = StartCoroutine(DoAnimations(null, takeConnectingEnter));
                        break;
                }
                break;

            case State.Releasing:
                SetPhysicsActive(false);

                SpringRelease();

                switch(prevState) {
                    case State.Connected:
                    case State.Dragging:
                        mRout = StartCoroutine(DoAnimations(Release, takeConnectedExit));
                        break;
                    case State.Connecting:
                        mRout = StartCoroutine(DoAnimations(Release, takeConnectingExit));
                        break;
                    default:
                        Release();
                        break;
                }
                break;

            case State.None:
                SetPhysicsActive(false);

                SpringRelease();

                HideConnectingDisplay();
                HideConnectDisplay();
                break;
        }
    }

    private void SetPhysicsActive(bool aActive) {
        if(body) body.simulated = aActive;
        if(coll) coll.enabled = aActive;
    }

    private void ApplyCurOperator() {
        for(int i = 0; i < operatorMultiplyGO.Length; i++) {
            if(operatorMultiplyGO[i])
                operatorMultiplyGO[i].SetActive(mOp == OperatorType.Multiply);
        }

        for(int i = 0; i < operatorDivideGO.Length; i++) {
            if(operatorDivideGO[i])
                operatorDivideGO[i].SetActive(mOp == OperatorType.Divide);
        }

        for(int i = 0; i < operatorEqualGO.Length; i++) {
            if(operatorEqualGO[i])
                operatorEqualGO[i].SetActive(mOp == OperatorType.Equal);
        }
    }

    private void SpringRelease() {
        if(mLinkBeginSpring) { Destroy(mLinkBeginSpring); mLinkBeginSpring = null; }
        if(mLinkEndSpring) { Destroy(mLinkEndSpring); mLinkEndSpring = null; }

        if(linkBeginSpriteRender) linkBeginSpriteRender.gameObject.SetActive(false);
        if(linkEndSpriteRender) linkEndSpriteRender.gameObject.SetActive(false);

        blobLinkStart = null;
        blobLinkEnd = null;
    }

    private void StopRout() {
        if(mRout != null) {
            StopCoroutine(mRout);
            mRout = null;
        }
    }
        
    private void HideConnectingDisplay() {
        if(bodyConnectingGO) bodyConnectingGO.SetActive(false);
        if(connectingRoot) connectingRoot.gameObject.SetActive(false);
    }

    private void HideConnectDisplay() {
        if(bodyConnectedGO) bodyConnectedGO.SetActive(false);
    }

    IEnumerator DoAnimations(System.Action postCall, params string[] takes) {
        for(int i = 0; i < takes.Length; i++) {
            if(animator && !string.IsNullOrEmpty(takes[i]))
                yield return animator.PlayWait(takes[i]);
        }

        mRout = null;

        if(postCall != null)
            postCall();
    }
}
