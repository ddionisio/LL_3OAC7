using LoLExt;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Lesson3Controller : GameModeController<Lesson3Controller> {
	[Header("Display")]
	public GameObject hudInstructGO;
	public Button exitButton;

	[Header("Animations")]
	public AnimatorEnterExit multiply5Anim;
	public AnimatorEnterExit multiply6Anim;
	public AnimatorEnterExit associativeAnim;

	[Header("Dialogs")]
	public ModalDialogFlow introDialog;
	public ModalDialogFlowIncremental multiply5Dialog;
	public ModalDialogFlowIncremental multiply6Dialog;
	public ModalDialogFlowIncremental associativeDialog;
	public ModalDialogFlow endDialog;

	[Header("Audio")]
	[M8.MusicPlaylist]
	public string music;

	[Header("Next")]
	public M8.SceneAssetPath sceneNext;

	protected override void OnInstanceInit() {
		base.OnInstanceInit();

		exitButton.gameObject.SetActive(false);

		multiply5Anim.gameObject.SetActive(false);
		multiply6Anim.gameObject.SetActive(false);
		associativeAnim.gameObject.SetActive(false);

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

		//multiply 5
		multiply5Anim.gameObject.SetActive(true);
		yield return multiply5Anim.PlayEnterWait();

		yield return multiply5Dialog.Play();

		yield return multiply5Anim.PlayExitWait();
		multiply5Anim.gameObject.SetActive(false);

		//multiply 4
		multiply6Anim.gameObject.SetActive(true);
		yield return multiply6Anim.PlayEnterWait();

		yield return multiply6Dialog.Play();

		yield return multiply6Anim.PlayExitWait();
		multiply6Anim.gameObject.SetActive(false);

		//division
		associativeAnim.gameObject.SetActive(true);
		yield return associativeAnim.PlayEnterWait();

		yield return associativeDialog.Play();

		yield return associativeAnim.PlayExitWait();
		associativeAnim.gameObject.SetActive(false);

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
