using LoLExt;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Lesson5Controller : GameModeController<Lesson5Controller> {
	[Header("Display")]
	public GameObject hudInstructGO;
	public Button exitButton;

	[Header("Animations")]
	public AnimatorEnterExit multiply9Anim;
	public AnimatorEnterExit multiply10Anim;

	[Header("Dialogs")]
	public ModalDialogFlow introDialog;
	public ModalDialogFlowIncremental multiply9Dialog;
	public ModalDialogFlowIncremental multiply10Dialog;
	public ModalDialogFlow endDialog;

	[Header("Audio")]
	[M8.MusicPlaylist]
	public string music;

	[Header("Next")]
	public M8.SceneAssetPath sceneNext;

	protected override void OnInstanceInit() {
		base.OnInstanceInit();

		exitButton.gameObject.SetActive(false);

		multiply9Anim.gameObject.SetActive(false);
		multiply10Anim.gameObject.SetActive(false);

		exitButton.onClick.AddListener(OnExit);
	}

	protected override IEnumerator Start() {
		yield return base.Start();

		var lolMgr = LoLManager.instance;

		while(!lolMgr.isReady)
			yield return null;

		if(!string.IsNullOrEmpty(music))
			M8.MusicPlaylist.instance.Play(music, true, false);

		yield return introDialog.Play();

		//multiply 9
		multiply9Anim.gameObject.SetActive(true);
		yield return multiply9Anim.PlayEnterWait();

		yield return multiply9Dialog.Play();

		yield return multiply9Anim.PlayExitWait();
		multiply9Anim.gameObject.SetActive(false);

		//multiply 10
		multiply10Anim.gameObject.SetActive(true);
		yield return multiply10Anim.PlayEnterWait();

		yield return multiply10Dialog.Play();

		yield return multiply10Anim.PlayExitWait();
		multiply10Anim.gameObject.SetActive(false);

		yield return endDialog.Play();

		hudInstructGO.SetActive(true);

		exitButton.gameObject.SetActive(true);
	}

	void OnExit() {
		var lolMgr = LoLManager.instance;

		lolMgr.ApplyProgress(lolMgr.curProgress + 1);

		sceneNext.Load();
	}
}
