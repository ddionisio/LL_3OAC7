using LoLExt;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Lesson1Controller : GameModeController<Lesson1Controller> {
	[Header("Display")]
	public GameObject linkDisconnectTutorialGO;	
	public GameObject hudInstructGO;
	public Button exitButton;

	[Header("Animations")]
	public AnimatorEnterExit multiplyTwoAnim;
	public AnimatorEnterExit commutativeAnim;
	public AnimatorEnterExit tutorialOpDisplayAnim;

	[Header("Dialogs")]
	public ModalDialogFlow introDialog;
	public ModalDialogFlow multiply2Dialog;
	public ModalDialogFlowIncremental commutativeDialog;
	public ModalDialogFlow tutorialDialog;
	public ModalDialogFlow tutorialEndDialog;

	[Header("Audio")]
	[M8.MusicPlaylist]
	public string music;

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

		linkDisconnectTutorialGO.SetActive(false);
		exitButton.gameObject.SetActive(false);

		multiplyTwoAnim.gameObject.SetActive(false);
		commutativeAnim.gameObject.SetActive(false);
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

		//multiply 2
		multiplyTwoAnim.gameObject.SetActive(true);
		yield return multiplyTwoAnim.PlayEnterWait();

		yield return multiply2Dialog.Play();

		yield return multiplyTwoAnim.PlayExitWait();
		multiplyTwoAnim.gameObject.SetActive(false);

		//commutative
		commutativeAnim.gameObject.SetActive(true);
		yield return commutativeAnim.PlayEnterWait();

		yield return commutativeDialog.Play();

		yield return commutativeAnim.PlayExitWait();
		commutativeAnim.gameObject.SetActive(false);

		//tutorial
		tutorialOpDisplayAnim.gameObject.SetActive(true);
		yield return tutorialOpDisplayAnim.PlayEnterWait();

		signalPlayStart.Invoke();

		yield return tutorialDialog.Play();

		signalShowDrag.Invoke();

		linkDisconnectTutorialGO.SetActive(true);
	}

	IEnumerator DoTutorialFinish() {
		yield return tutorialEndDialog.Play();

		yield return tutorialOpDisplayAnim.PlayExitWait();
		tutorialOpDisplayAnim.gameObject.SetActive(false);

		linkDisconnectTutorialGO.SetActive(false);

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
