using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlobSpawner : MonoBehaviour {
    [System.Serializable]
    public class TemplateGroup {
        public string name;
        public GameObject[] templates;

        public GameObject template {
            get { return templates.Length > 0 ? templates[Random.Range(0, templates.Length)] : null; }
        }
    }

    public struct SpawnInfo {
        public string templateName;
        public int number;
    }

    [Header("Template")]
    public string poolGroup = "blobs";
    public int poolCapacity = 5;
    public TemplateGroup[] templateGroups;

    [Header("Spawn")]
    public Transform spawnPointsRoot; //grab child for spawn point
    public LayerMask spawnPointCheckMask; //ensure spot is fine to spawn
    public float spawnPointCheckRadius;
    public float spawnDelay = 0.3f;

    private M8.PoolController mPool;

    private Vector2[] mSpawnPts;
    private int mCurSpawnPtInd;

    private Queue<SpawnInfo> mSpawnQueue = new Queue<SpawnInfo>();
    private Coroutine mSpawnRout;

    private M8.GenericParams mSpawnParms = new M8.GenericParams();

    public void Spawn(string templateName, int number) {
        mSpawnQueue.Enqueue(new SpawnInfo { templateName = templateName, number = number });
        if(mSpawnRout == null)
            mSpawnRout = StartCoroutine(DoSpawnQueue());
    }

    void OnDisable() {
        mSpawnQueue.Clear();
        mSpawnRout = null;
    }

    void Awake() {
        //setup pool
        mPool = M8.PoolController.CreatePool(poolGroup);
        for(int i = 0; i < templateGroups.Length; i++) {
            var grp = templateGroups[i];
            for(int j = 0; j < grp.templates.Length; j++)
                mPool.AddType(grp.templates[j], poolCapacity, poolCapacity);
        }

        //generate spawn points
        mSpawnPts = new Vector2[spawnPointsRoot.childCount];
        for(int i = 0; i < spawnPointsRoot.childCount; i++)
            mSpawnPts[i] = spawnPointsRoot.GetChild(i).position;

        ShuffleSpawnPoints();
    }

    IEnumerator DoSpawnQueue() {
        var wait = new WaitForSeconds(spawnDelay);

        while(mSpawnQueue.Count > 0) {
            yield return wait;

            var spawnInfo = mSpawnQueue.Dequeue();

            //grab template
            string templateName = null;
            for(int i = 0; i < templateGroups.Length; i++) {
                if(templateGroups[i].name == spawnInfo.templateName) {
                    var template = templateGroups[i].template;
                    if(template)
                        templateName = template.name;

                    break;
                }
            }

            if(string.IsNullOrEmpty(templateName)) {
                Debug.LogWarning("No template for: " + spawnInfo.templateName);
                continue;
            }

            //find valid spawn point
            Vector2 spawnPt = Vector2.zero;

            while(true) {
                if(mCurSpawnPtInd == mSpawnPts.Length)
                    ShuffleSpawnPoints();

                var pt = mSpawnPts[mCurSpawnPtInd];
                mCurSpawnPtInd++;

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

            mPool.Spawn(templateName, null, mSpawnParms);
        }

        mSpawnRout = null;
    }

    private void ShuffleSpawnPoints() {
        M8.ArrayUtil.Shuffle(mSpawnPts);
        mCurSpawnPtInd = 0;
    }
}
