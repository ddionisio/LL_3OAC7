using LoLExt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroController : GameModeController<IntroController> {
	[Header("Displays")]
	public GameObject blobsGO;
	public GameObject dangerGO;
	public GameObject foundGO;
	public GameObject operatorsGO;
	public GameObject reticleGO;
	public GameObject scanningGO;

	[Header("Animations")]
	public M8.Animator.Animate animScan;
	[M8.Animator.TakeSelector(animatorField = "animScan")]
	public int animScanTakePlay;

	[Header("Dialogs")]
	public ModalDialogFlowIncremental introDialog;
	public ModalDialogFlow introOPDialog;

	[Header("SFX")]
	[M8.SoundPlaylist]
	public string sfxScan;
	[M8.SoundPlaylist]
	public string sfxScanEnd;
	[M8.SoundPlaylist]
	public string sfxCompBeep;

	[Header("Next")]
	public M8.SceneAssetPath sceneNext;

	protected override void OnInstanceInit() {
		base.OnInstanceInit();

		blobsGO.SetActive(false);
		dangerGO.SetActive(false);
		foundGO.SetActive(false);
		operatorsGO.SetActive(false);
		reticleGO.SetActive(false);
		scanningGO.SetActive(false);
	}

	protected override IEnumerator Start() {
		yield return base.Start();

		var lolMgr = LoLManager.instance;

		while(!lolMgr.isReady)
			yield return null;

		//scanning
		scanningGO.SetActive(true);
		reticleGO.SetActive(true);

		if(!string.IsNullOrEmpty(sfxScan))
			M8.SoundPlaylist.instance.Play(sfxScan, false);

		if(animScanTakePlay != -1)
			yield return animScan.PlayWait(animScanTakePlay);

		//scanning end
		scanningGO.SetActive(false);

		if(!string.IsNullOrEmpty(sfxScanEnd))
			M8.SoundPlaylist.instance.Play(sfxScanEnd, false);

		//found
		foundGO.SetActive(true);
		blobsGO.SetActive(true);

		yield return new WaitForSeconds(2.5f);

		foundGO.SetActive(false);

		//danger
		dangerGO.SetActive(true);

		if(!string.IsNullOrEmpty(sfxCompBeep))
			M8.SoundPlaylist.instance.Play(sfxCompBeep, false);

		//intro
		yield return introDialog.Play();

		//intro op
		operatorsGO.SetActive(true);

		yield return new WaitForSeconds(1f);

		yield return introOPDialog.Play();

		sceneNext.Load();
	}
}
