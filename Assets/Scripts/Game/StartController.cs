using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartController : GameModeController<StartController> {
	[Header("Displays")]
	public GameObject loadingGO;
	public GameObject readyGO;
	public GameObject continueGO;

	[Header("Scenes")]
	public M8.SceneAssetPath introScene;
	public M8.SceneAssetPath[] progressScenes;
	public M8.SceneAssetPath endScene;

	[Header("Audio")]
	[M8.MusicPlaylist]
	public string musicRef;

	public void ContinueClick() {
		var lolMgr = LoLManager.instance;

		var progressSceneInd = lolMgr.curProgress;

		if(progressSceneInd >= 0 && progressSceneInd < progressScenes.Length)
			progressScenes[progressSceneInd].Load();
		else
			endScene.Load();
	}

	public void NewClick() {
		var lolMgr = LoLManager.instance;

		if(lolMgr.curProgress > 0) {
			if(lolMgr.userData)
				lolMgr.userData.Delete();

			lolMgr.ApplyProgress(0, 0);
		}

		introScene.Load();
	}

	protected override void OnInstanceInit() {
		base.OnInstanceInit();

		loadingGO.SetActive(true);

		readyGO.SetActive(false);
		continueGO.SetActive(false);
	}

	protected override IEnumerator Start() {
		yield return base.Start();

		var lolMgr = LoLManager.instance;

		while(!lolMgr.isReady)
			yield return null;

		loadingGO.SetActive(false);

		readyGO.SetActive(true);

		if(lolMgr.curProgress > 0)
			continueGO.SetActive(true);

		if(!string.IsNullOrEmpty(musicRef))
			M8.MusicPlaylist.instance.Play(musicRef, true, true);
	}
}
