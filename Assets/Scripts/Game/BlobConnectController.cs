﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manage links between blobs with connects
/// </summary>
public class BlobConnectController : MonoBehaviour {
    public class Group {
        public BlobConnect connectOp;
        public BlobConnect connectEq;

        public Blob blobOpLeft;
        public Blob blobOpRight;

        public Blob blobEq;

        public bool isEmpty {
            get {
                return !(blobOpLeft || blobOpRight || blobEq);
            }
        }

        public bool isOpFilled {
            get {
                return blobOpLeft && blobOpRight;
            }
        }

        public bool isComplete {
            get {
                return isOpFilled && blobEq;
            }
        }

        public bool IsBlobOp(Blob blob) {
            return blobOpLeft == blob || blobOpRight == blob;
        }

        public bool IsBlobOp(GameObject blobGO) {
            if(blobOpLeft && blobOpLeft.gameObject == blobGO)
                return true;

            if(blobOpRight && blobOpRight.gameObject == blobGO)
                return true;

            return false;
        }

        public bool IsBlobInGroup(Blob blob) {
            return blobOpLeft == blob || blobOpRight == blob || blobEq == blob;
        }

        public bool IsBlobInGroup(GameObject blobGO) {
            if(blobOpLeft && blobOpLeft.gameObject == blobGO)
                return true;

            if(blobOpRight && blobOpRight.gameObject == blobGO)
                return true;

            if(blobEq && blobEq.gameObject == blobGO)
                return true;

            return false;
        }

        public void Clear() {
            if(connectOp) {
                connectOp.Release();
                connectOp = null;
            }

            if(connectEq) {
                connectEq.Release();
                connectEq = null;
            }

            blobOpLeft = null;
            blobOpRight = null;
            blobEq = null;
        }

        public void SetOp(Blob left, Blob right, BlobConnect connect) {
            Clear(); //fail-safe

            blobOpLeft = left;
            blobOpRight = right;

            connectOp = connect;
        }

        public void SetEq(Blob eq, BlobConnect connect) {
            if(connectEq)
                connectEq.Release();

            blobEq = eq;

            connectEq = connect;
        }

        /// <summary>
        /// Remove given blob from this group if it matches, returns true if able to match and clean-up.
        /// </summary>
        public bool RemoveBlob(Blob blob) {
            if(blobOpLeft == blob || blobOpRight == blob) {
                Clear();
                return true;
            }
            else if(blobEq == blob) {
                if(connectEq) {
                    connectEq.Release();
                    connectEq = null;
                }

                blobEq = null;

                return true;
            }

            return false;
        }
    }

    [Header("Connect Template")]
    public string poolGroup = "connect";
    public GameObject connectTemplate;
    public int capacity = 5;

    [Header("Data")]
    public int groupCapacity = 3;
    

    [Header("Signal Listens")]
    public SignalBlob signalListenBlobDragBegin;
    public SignalBlob signalListenBlobDragEnd;
    public SignalBlob signalListenBlobDespawn;

    /// <summary>
    /// Current operator type when connecting two unlinked blob
    /// </summary>
    public OperatorType curOp { get { return mCurOp; } set { mCurOp = value; } }

    private M8.PoolController mPool;

    private OperatorType mCurOp = OperatorType.Multiply;

    private BlobConnect mCurConnectDragging; //when dragging a blob around.

    private Blob mCurBlobDragging;
    private Group mCurGroupDragging; //which group is involved while dragging

    private M8.GenericParams mConnectSpawnParms = new M8.GenericParams();

    private M8.CacheList<Group> mGroupActives;
    private M8.CacheList<Group> mGroupCache;

    public void ReleaseDragging() {
        if(mCurConnectDragging) {
            mCurConnectDragging.Release();
            mCurConnectDragging = null;
        }

        mCurBlobDragging = null;
        mCurGroupDragging = null;
    }

    void OnDestroy() {
        signalListenBlobDragBegin.callback -= OnBlobDragBegin;
        signalListenBlobDragEnd.callback -= OnBlobDragEnd;
        signalListenBlobDespawn.callback -= OnBlobDespawn;
    }

    void Awake() {
        mPool = M8.PoolController.CreatePool(poolGroup);
        mPool.AddType(connectTemplate, capacity, capacity);

        //setup group
        mGroupActives = new M8.CacheList<Group>(groupCapacity);
        mGroupCache = new M8.CacheList<Group>(groupCapacity);

        //fill up cache
        for(int i = 0; i < groupCapacity; i++)
            mGroupCache.Add(new Group());

        signalListenBlobDragBegin.callback += OnBlobDragBegin;
        signalListenBlobDragEnd.callback += OnBlobDragEnd;
        signalListenBlobDespawn.callback += OnBlobDespawn;
    }

    void Update() {
        //we are dragging
        if(mCurConnectDragging) {
            if(mCurBlobDragging) {
                //setup op
                OperatorType dragOp = mCurOp;

                if(mCurGroupDragging != null) {
                    if(mCurGroupDragging.IsBlobOp(mCurBlobDragging))
                        dragOp = OperatorType.Equal;
                }

                //setup link position
                Vector2 connectPtStart, connectPtEnd;

                connectPtEnd = mCurBlobDragging.dragPoint;

                //check if dragging inside
                var dragJellySprRef = mCurBlobDragging.dragPointerJellySpriteRefPt;
                if(dragJellySprRef && dragJellySprRef.ParentJellySprite == mCurBlobDragging.gameObject) {
                    //start set the same as end.
                    connectPtStart = connectPtEnd;
                }
                else {
                    //grab edge of the blob relative to end.
                    int refInd;
                    mCurBlobDragging.GetEdge(connectPtEnd, out connectPtStart, out refInd);

                    //check if we are over another blob
                    if(mCurBlobDragging.dragPointerJellySpriteRefPt) {
                        //check if it is in a group and we are setting it up as an equal connect
                        var blobGO = mCurBlobDragging.dragPointerJellySpriteRefPt.ParentJellySprite;
                        var toGrp = GetGroup(blobGO);
                        if(toGrp != null && toGrp != mCurGroupDragging) {
                            if(toGrp.IsBlobOp(blobGO))
                                dragOp = OperatorType.Equal;
                        }
                    }
                }

                mCurConnectDragging.op = dragOp;
                mCurConnectDragging.UpdateConnecting(connectPtStart, connectPtEnd);
            }
            else //blob being dragged on released?
                ReleaseDragging();
        }
    }

    void OnBlobDragBegin(Blob blob) {
        if(!mCurConnectDragging) {
            mConnectSpawnParms.Clear();
            //params?

            mCurConnectDragging = mPool.Spawn<BlobConnect>(connectTemplate.name, "", null, mConnectSpawnParms);
        }

        var toOp = mCurOp;

        //determine what the operator is based on blob's current connect state

        mCurConnectDragging.op = toOp;
        mCurConnectDragging.state = BlobConnect.State.Connecting;

        mCurBlobDragging = blob;

        //determine if this is in a group
        mCurGroupDragging = GetGroup(blob);
        if(mCurGroupDragging != null) {
            //highlight entire group
        }
    }

    void OnBlobDragEnd(Blob blob) {
        //determine if we can connect to a new blob
        var blobRefPt = blob.dragPointerJellySpriteRefPt;
        if(blobRefPt != null && blobRefPt.ParentJellySprite != blob.gameObject) {
            var endBlob = blobRefPt.ParentJellySprite.GetComponent<Blob>();

            //determine op
            var toOp = mCurOp;
            if(mCurGroupDragging != null) {
                if(mCurGroupDragging.IsBlobOp(mCurBlobDragging)) {
                    //cancel if dragging to the same group as ops
                    if(mCurGroupDragging.IsBlobOp(endBlob))
                        toOp = OperatorType.None;
                    else
                        toOp = OperatorType.Equal;
                }
            }

            //update link groups
            if(toOp != OperatorType.None) {
                //remove endBlob from its group
                var endGroup = GetGroup(endBlob);
                if(endGroup != null && endGroup != mCurGroupDragging) {
                    //if we are dragging to apply equal op, then remove it from end group and move to drag group
                    if(toOp == OperatorType.Equal) {
                        RemoveBlobFromGroup(endGroup, endBlob);

                        mCurGroupDragging.SetEq(endBlob, mCurConnectDragging);
                    }
                    else {
                        //if dragging to one of the operands of end group, then move blob to this group
                        if(endGroup.IsBlobOp(endBlob)) {
                            toOp = OperatorType.Equal;

                            //move blob to end group as the equal
                            endGroup.SetEq(blob, mCurConnectDragging);

                            //remove from dragging group
                            if(mCurGroupDragging != null)
                                RemoveBlobFromGroup(mCurGroupDragging, blob);
                        }
                        else {
                            //remove blobs from its group, and create new group together
                            if(mCurGroupDragging != null)
                                RemoveBlobFromGroup(mCurGroupDragging, blob);

                            RemoveBlobFromGroup(endGroup, endBlob);

                            var newGrp = NewGroup();
                            newGrp.SetOp(blob, endBlob, mCurConnectDragging);
                        }
                    }
                }
                else if(mCurGroupDragging != null) {
                    if(toOp == OperatorType.Equal) {
                        //refresh equal
                        mCurGroupDragging.SetEq(endBlob, mCurConnectDragging);
                    }
                    else //re-establish group
                        mCurGroupDragging.SetOp(blob, endBlob, mCurConnectDragging);
                }
                else {
                    //create new group
                    var newGrp = NewGroup();
                    newGrp.SetOp(blob, endBlob, mCurConnectDragging);
                }

                //setup link
                mCurConnectDragging.op = toOp;
                mCurConnectDragging.ApplyLink(blob, endBlob);
            }
            else //cancel
                mCurConnectDragging.Release();

            mCurConnectDragging = null;
            mCurBlobDragging = null;
            mCurGroupDragging = null;
        }
        else
            ReleaseDragging();
    }

    void OnBlobDespawn(Blob blob) {
        //check which connects need to be purged.
    }

    private void RemoveBlobFromGroup(Group grp, Blob blob) {
        grp.RemoveBlob(blob);
        if(grp.isEmpty) {
            mGroupActives.Remove(grp);
            mGroupCache.Add(grp);
        }
    }

    private Group NewGroup() {
        var newGrp = mGroupCache.Remove();
        if(newGrp != null)
            mGroupActives.Add(newGrp);
        return newGrp;
    }

    private Group GetGroup(Blob blob) {
        Group grp = null;

        for(int i = 0; i < mGroupActives.Count; i++) {
            if(mGroupActives[i].IsBlobInGroup(blob)) {
                grp = mGroupActives[i];
                break;
            }
        }

        return grp;
    }

    private Group GetGroup(GameObject blobGO) {
        Group grp = null;

        for(int i = 0; i < mGroupActives.Count; i++) {
            if(mGroupActives[i].IsBlobInGroup(blobGO)) {
                grp = mGroupActives[i];
                break;
            }
        }

        return grp;
    }
}
