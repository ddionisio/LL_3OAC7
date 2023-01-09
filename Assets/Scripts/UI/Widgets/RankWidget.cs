using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankWidget : MonoBehaviour {
    [Header("Display")]
    public Text rankText;
    public GameObject[] rankPlatings; //highest to lowest

    public void Apply(int rankIndex) {
        var rank = GameData.instance.ranks[rankIndex];

        //setup plating
        if(rankPlatings.Length > 0) {
            var rankPlatingIndex = Mathf.Clamp(rankIndex, 0, rankPlatings.Length - 1);
            for(int i = 0; i < rankPlatings.Length; i++) {
                if(rankPlatings[i])
                    rankPlatings[i].SetActive(i == rankPlatingIndex);
            }
        }

        //setup rank text
        if(rankText)
            rankText.text = rank.grade;
    }
}
