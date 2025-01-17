using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class EndController : GameModeController<EndController> {
    public M8.TextMeshPro.TextMeshProCounter[] levelCounters;
    public M8.TextMeshPro.TextMeshProCounter totalCounter;

    public TMP_Text[] levelRankingTexts;
	public Image[] levelRankingIcons;
	public RankWidget rankingWidget;

	[Header("Displays")]
	public GameObject congratsTitleGO;
	public GameObject congratsDescGO;
	public GameObject thanksGO;
	public GameObject summaryGO;

	[Header("Animations")]
	public AnimatorEnterExit blackholeAnim;
	public AnimatorEnterExit ringsAnim;
	public AnimatorEnterExit blobsAnim;

	[Header("Audio")]
	[M8.MusicPlaylist]
	public string music;

    [M8.SoundPlaylist]
    public string sfxMagic;

	protected override void OnInstanceInit() {
        base.OnInstanceInit();

        //setup each level's score
        for(int i = 0; i < levelCounters.Length; i++) {
            int score = GameData.instance.ScoreGet(i);
            levelCounters[i].SetCountImmediate(score);

            if(score > 0) {
                //assume all have 10 rounds
                var rankInd = GameData.instance.GetRankIndex(score);
                var rank = GameData.instance.ranks[rankInd];

                if(levelRankingTexts[i])
                    levelRankingTexts[i].text = rank.grade;

                if(levelRankingIcons[i])
                    levelRankingIcons[i].sprite = rank.icon;
            }
            else {
                if(levelRankingTexts[i])
                    levelRankingTexts[i].text = "";

				if(levelRankingIcons[i])
					levelRankingIcons[i].sprite = GameData.instance.ranks[GameData.instance.ranks.Length - 1].icon;
			}
        }

        int totalScore = LoLManager.instance.curScore;
        totalCounter.SetCountImmediate(totalScore);

        //apply average ranking        
        var avgScore = Mathf.RoundToInt((float)totalScore / levelCounters.Length);
        int rankIndex = GameData.instance.GetRankIndex(avgScore);
        rankingWidget.Apply(rankIndex);

		congratsTitleGO.SetActive(false);
		congratsDescGO.SetActive(false);
		thanksGO.SetActive(false);

		summaryGO.SetActive(false);

		blackholeAnim.gameObject.SetActive(false);
		ringsAnim.gameObject.SetActive(false);
		blobsAnim.gameObject.SetActive(false);
	}

	protected override IEnumerator Start() {
		yield return base.Start();

		var lolMgr = LoLManager.instance;

		while(!lolMgr.isReady)
			yield return null;

		if(!string.IsNullOrEmpty(music))
			M8.MusicPlaylist.instance.Play(music, false, false);

        var wait = new WaitForSeconds(0.3f);

        //enter
		blackholeAnim.gameObject.SetActive(true);
        yield return blackholeAnim.PlayEnterWait();

        yield return wait;

		ringsAnim.gameObject.SetActive(true);
		yield return ringsAnim.PlayEnterWait();

        if(!string.IsNullOrEmpty(sfxMagic))
            M8.SoundPlaylist.instance.Play(sfxMagic, false);

		yield return wait;

		blobsAnim.gameObject.SetActive(true);
		yield return blobsAnim.PlayEnterWait();


        yield return new WaitForSeconds(3f);

		//exit
		yield return blobsAnim.PlayExitWait();
		blobsAnim.gameObject.SetActive(false);

		yield return ringsAnim.PlayExitWait();
		ringsAnim.gameObject.SetActive(false);

		yield return blackholeAnim.PlayExitWait();
		blackholeAnim.gameObject.SetActive(false);

		//congrats
		congratsTitleGO.SetActive(true);
		yield return wait;
		congratsDescGO.SetActive(true);
		yield return wait;
		thanksGO.SetActive(true);

		//summary
		summaryGO.SetActive(true);

		yield return new WaitForSeconds(12f);

		lolMgr.Complete();
	}
}
