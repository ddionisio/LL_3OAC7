using LoLExt;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Lesson4Controller : GameModeController<Lesson4Controller> {
	[Header("Display")]
	public GameObject hudInstructGO;
	public Button exitButton;

	[Header("Animations")]
	public AnimatorEnterExit multiply7Anim;
	public AnimatorEnterExit multiply8Anim;
	public AnimatorEnterExit distributiveAnim;

	[Header("Dialogs")]
	public ModalDialogFlow introDialog;
	public ModalDialogFlowIncremental multiply7Dialog;
	public ModalDialogFlowIncremental multiply8Dialog;
	public ModalDialogFlowIncremental distributiveDialog;
	public ModalDialogFlow endDialog;

	[Header("Audio")]
	[M8.MusicPlaylist]
	public string music;

	[Header("Next")]
	public M8.SceneAssetPath sceneNext;

	protected override void OnInstanceInit() {
		base.OnInstanceInit();

		exitButton.gameObject.SetActive(false);

		multiply7Anim.gameObject.SetActive(false);
		multiply8Anim.gameObject.SetActive(false);
		distributiveAnim.gameObject.SetActive(false);

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

		//multiply 7
		multiply7Anim.gameObject.SetActive(true);
		yield return multiply7Anim.PlayEnterWait();

		yield return multiply7Dialog.Play();

		yield return multiply7Anim.PlayExitWait();
		multiply7Anim.gameObject.SetActive(false);

		//multiply 8
		multiply8Anim.gameObject.SetActive(true);
		yield return multiply8Anim.PlayEnterWait();

		yield return multiply8Dialog.Play();

		yield return multiply8Anim.PlayExitWait();
		multiply8Anim.gameObject.SetActive(false);

		//distributive
		distributiveAnim.gameObject.SetActive(true);
		yield return distributiveAnim.PlayEnterWait();

		yield return distributiveDialog.Play();

		yield return distributiveAnim.PlayExitWait();
		distributiveAnim.gameObject.SetActive(false);

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
