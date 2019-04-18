using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manage links between blobs with connects
/// </summary>
public class BlobConnectController : MonoBehaviour {
    [Header("Connect Template")]
    public string poolGroup = "connect";
    public GameObject connectTemplate;
    public int capacity = 5;

    //[Header("Data")]
    

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

    private M8.GenericParams mConnectSpawnParms = new M8.GenericParams();

    public void ReleaseDragging() {
        if(mCurConnectDragging) {
            mCurConnectDragging.Release();
            mCurConnectDragging = null;
        }

        mCurBlobDragging = null;
    }

    void OnDestroy() {
        signalListenBlobDragBegin.callback -= OnBlobDragBegin;
        signalListenBlobDragEnd.callback -= OnBlobDragEnd;
        signalListenBlobDespawn.callback -= OnBlobDespawn;
    }

    void Awake() {
        mPool = M8.PoolController.CreatePool(poolGroup);
        mPool.AddType(connectTemplate, capacity, capacity);

        signalListenBlobDragBegin.callback += OnBlobDragBegin;
        signalListenBlobDragEnd.callback += OnBlobDragEnd;
        signalListenBlobDespawn.callback += OnBlobDespawn;
    }

    void Update() {
        //we are dragging
        if(mCurConnectDragging) {
            if(mCurBlobDragging) {
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
                }

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
    }

    void OnBlobDragEnd(Blob blob) {
        //determine if we can connect to a new blob
        var blobRefPt = blob.dragPointerJellySpriteRefPt;
        if(blobRefPt != null && blobRefPt.ParentJellySprite != blob.gameObject) {

            //manage links, remove if we are reconnecting, etc.

            //update link groups
            Blob endBlob = blobRefPt.ParentJellySprite.GetComponent<Blob>();

            mCurConnectDragging.ApplyLink(blob, endBlob);

            mCurConnectDragging = null;
            mCurBlobDragging = null;
        }
        else
            ReleaseDragging();
    }

void OnBlobDespawn(Blob blob) {
//check which connects need to be purged.
}
}
