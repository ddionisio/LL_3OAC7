using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlobSpawner : MonoBehaviour {
    public struct SpawnInfo {
        public int number;
    }

    [Header("Template")]
    public string poolGroup = "blobs";
    public GameObject template;

    [Header("Spawn")]
    public Transform spawnPointsRoot; //grab child for spawn point
    public bool spawnPointsShuffle = true;
    public LayerMask spawnPointCheckMask; //ensure spot is fine to spawn
    public float spawnPointCheckRadius;
    public float spawnDelay = 0.3f;

    public int spawnQueueCount { get { return mSpawnQueue.Count; } }

    public Queue<SpawnInfo> spawnQueue { get { return mSpawnQueue; } }

    public M8.CacheList<Blob> blobActives { get { return mBlobActives; } }

    private M8.PoolController mPool;

    private Vector2[] mSpawnPts;
    private int mCurSpawnPtInd;

    private Queue<SpawnInfo> mSpawnQueue = new Queue<SpawnInfo>();
    private Coroutine mSpawnRout;

    private M8.GenericParams mSpawnParms = new M8.GenericParams();

    private M8.CacheList<Blob> mBlobActives;

	private M8.CacheList<int> mBlobCheck;

	private System.Text.StringBuilder mBlobNameCache = new System.Text.StringBuilder();

    public bool IsSolvable(int number, OperatorType op) {
        if(mBlobCheck == null)
            mBlobCheck = new M8.CacheList<int>(mBlobActives.Capacity);
        else
            mBlobCheck.Clear();

        for(int i = 0; i < mBlobActives.Count; i++) {
            var blob = mBlobActives[i];
            if(!blob)
                continue;

            if(blob.state == Blob.State.Normal) {
                mBlobCheck.Add(blob.number);
            }
        }

		for(int i = 0; i < mBlobCheck.Count; i++) {
			int numCheck = mBlobCheck[i];
			for(int j = 0; j < mBlobCheck.Count; j++) {
				if(j == i)
					continue;

                switch(op) {
                    case OperatorType.Multiply:
                        if(numCheck * number == mBlobCheck[j])
                            return true;
                        break;

                    case OperatorType.Divide:
                        if(numCheck / number == mBlobCheck[j])
                            return true;
                        break;
                }
			}
		}

		return false;
    }

    public int GetBlobActiveLowestNumber() {
        int num = -1;

		for(int i = 0; i < mBlobActives.Count; i++) {
			var blob = mBlobActives[i];
			if(!blob)
				continue;

			if(blob.state == Blob.State.Normal) {
                if(blob.number < num || num == -1)
                    num = blob.number;
            }
		}

        return num == -1 ? 1 : num;
	}

    public bool CheckAnyBlobActiveState(params Blob.State[] states) {
        for(int i = 0; i < mBlobActives.Count; i++) {
            var blob = mBlobActives[i];
            if(blob) {
                for(int j = 0; j < states.Length; j++) {
                    if(blob.state == states[j])
                        return true;
                }
            }
        }

        return false;
    }

    public int GetBlobStateCount(params Blob.State[] states) {
        int count = 0;

        for(int i = 0; i < mBlobActives.Count; i++) {
            var blob = mBlobActives[i];
            if(blob) {
                for(int j = 0; j < states.Length; j++) {
                    if(blob.state == states[j]) {
                        count++;
                        break;
                    }
                }
            }
        }

        return count;
    }

    public void DespawnAllNormalBlobs() {
        if(mBlobActives == null)
            return;

        for(int i = 0; i < mBlobActives.Count; i++) {
            var blob = mBlobActives[i];
            if(!blob)
                continue;

            if(blob.state != Blob.State.Despawning || blob.state != Blob.State.Correct || blob.state != Blob.State.None)
                blob.state = Blob.State.Despawning;
        }
    }

    public void SpawnStop() {
        if(mSpawnRout != null) {
            StopCoroutine(mSpawnRout);
            mSpawnRout = null;
        }

        mSpawnQueue.Clear();
    }

    public void Spawn(int number) {
        mSpawnQueue.Enqueue(new SpawnInfo { number = number });
        if(mSpawnRout == null)
            mSpawnRout = StartCoroutine(DoSpawnQueue());
    }

    public void RemoveFromActive(Blob blob) {
        if(mBlobActives.Remove(blob)) {
            blob.poolData.despawnCallback -= OnBlobRelease;
        }
    }

    void OnDisable() {
        SpawnStop();
    }

    void Start() {

        int blobCapacity = GameData.instance.blobSpawnCapacity;

        mBlobActives = new M8.CacheList<Blob>(blobCapacity);

        //setup pool
        mPool = M8.PoolController.GetPool(poolGroup);
        if(!mPool) {
			mPool = M8.PoolController.CreatePool(poolGroup);
            mPool.gameObject.DontDestroyOnLoad();
        }

        mPool.AddType(template, blobCapacity, blobCapacity);

		//generate spawn points
		mSpawnPts = new Vector2[spawnPointsRoot.childCount];
        for(int i = 0; i < spawnPointsRoot.childCount; i++)
            mSpawnPts[i] = spawnPointsRoot.GetChild(i).position;

        if(spawnPointsShuffle)
            M8.ArrayUtil.Shuffle(mSpawnPts);

        mCurSpawnPtInd = 0;
    }

    IEnumerator DoSpawnQueue() {
        var wait = new WaitForSeconds(spawnDelay);

        while(mSpawnQueue.Count > 0) {
            while(mBlobActives.IsFull) //wait for blobs to release
                yield return null;

            yield return wait;

            var spawnInfo = mSpawnQueue.Dequeue();

            //find valid spawn point
            Vector2 spawnPt = Vector2.zero;

            while(true) {
                var pt = mSpawnPts[mCurSpawnPtInd];
                mCurSpawnPtInd++;
                if(mCurSpawnPtInd == mSpawnPts.Length)
                    mCurSpawnPtInd = 0;

                //check if valid
                var coll = Physics2D.OverlapCircle(pt, spawnPointCheckRadius, spawnPointCheckMask);
                if(!coll) {
                    spawnPt = pt;
                    break;
                }

                //invalid, check next
                yield return null;
            }

            //spawn
            mSpawnParms[JellySpriteSpawnController.parmPosition] = spawnPt;
            mSpawnParms[Blob.parmNumber] = spawnInfo.number;

            mBlobNameCache.Clear();
            mBlobNameCache.Append("blob ");
            mBlobNameCache.Append(spawnInfo.number);

            var blob = mPool.Spawn<Blob>(template.name, mBlobNameCache.ToString(), null, mSpawnParms);

            blob.poolData.despawnCallback += OnBlobRelease;

            mBlobActives.Add(blob);
        }

        mSpawnRout = null;
    }

    void OnBlobRelease(M8.PoolDataController pdc) {
        pdc.despawnCallback -= OnBlobRelease;

        for(int i = 0; i < mBlobActives.Count; i++) {
            var blob = mBlobActives[i];
            if(blob && blob.poolData == pdc) {
                mBlobActives.RemoveAt(i);
                break;
            }
        }
    }
}
