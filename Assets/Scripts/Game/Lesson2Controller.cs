using LoLExt;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Lesson2Controller : GameModeController<Lesson2Controller> {
	[Header("Display")]
	public GameObject hudInstructGO;
	public Button exitButton;

	[Header("Animations")]
	public AnimatorEnterExit multiply3Anim;
	public AnimatorEnterExit multiply4Anim;
	public AnimatorEnterExit divisionAnim;
	public AnimatorEnterExit tutorialOpDisplayAnim;

	[Header("Audio")]
	[M8.MusicPlaylist]
	public string music;

	[Header("Dialogs")]
	public ModalDialogFlow introDialog;
	public ModalDialogFlowIncremental multiply3Dialog;
	public ModalDialogFlowIncremental multiply4Dialog;
	public ModalDialogFlowIncremental divisionDialog;
	public ModalDialogFlow tutorialDialog;
	public ModalDialogFlow tutorialEndDialog;

	[Header("Signals")]
	public M8.Signal signalPlayStart;
	public M8.Signal signalShowDrag;
	public M8.Signal signalCorrect;
	public M8.Signal signalPlayEnd;

	[Header("Next")]
	public M8.SceneAssetPath sceneNext;

	protected override void OnInstanceDeinit() {
		base.OnInstanceDeinit();

		signalCorrect.callback -= OnSignalCorrect;
	}

	protected override void OnInstanceInit() {
		base.OnInstanceInit();

		exitButton.gameObject.SetActive(false);

		multiply3Anim.gameObject.SetActive(false);
		multiply4Anim.gameObject.SetActive(false);
		divisionAnim.gameObject.SetActive(false);
		tutorialOpDisplayAnim.gameObject.SetActive(false);

		signalCorrect.callback += OnSignalCorrect;

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

		//multiply 3
		multiply3Anim.gameObject.SetActive(true);
		yield return multiply3Anim.PlayEnterWait();

		yield return multiply3Dialog.Play();

		yield return multiply3Anim.PlayExitWait();
		multiply3Anim.gameObject.SetActive(false);

		//multiply 4
		multiply4Anim.gameObject.SetActive(true);
		yield return multiply4Anim.PlayEnterWait();

		yield return multiply4Dialog.Play();

		yield return multiply4Anim.PlayExitWait();
		multiply4Anim.gameObject.SetActive(false);

		//division
		divisionAnim.gameObject.SetActive(true);
		yield return divisionAnim.PlayEnterWait();

		yield return divisionDialog.Play();

		yield return divisionAnim.PlayExitWait();
		divisionAnim.gameObject.SetActive(false);

		//tutorial
		tutorialOpDisplayAnim.gameObject.SetActive(true);
		yield return tutorialOpDisplayAnim.PlayEnterWait();

		signalPlayStart.Invoke();

		yield return tutorialDialog.Play();

		signalShowDrag.Invoke();
	}

	IEnumerator DoTutorialFinish() {
		yield return tutorialEndDialog.Play();

		yield return tutorialOpDisplayAnim.PlayExitWait();
		tutorialOpDisplayAnim.gameObject.SetActive(false);

		signalPlayEnd.Invoke();

		hudInstructGO.SetActive(true);

		exitButton.gameObject.SetActive(true);
	}

	void OnSignalCorrect() {
		StartCoroutine(DoTutorialFinish());
	}

	void OnExit() {
		var lolMgr = LoLManager.instance;

		lolMgr.ApplyProgress(lolMgr.curProgress + 1);

		sceneNext.Load();
	}
}
