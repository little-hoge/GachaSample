using NCMB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// UIの制御をするクラス
// 主にUIのテキストの変更を実行する
public class UIController : MonoBehaviour {

    /// <summary> テキスト </summary>
    [SerializeField] Text money, point;

    /// <summary> ポップアップ </summary>
    [SerializeField] GameObject resultpopup;


    void Start() {
        money = GameObject.Find("Header/Text").GetComponent<Text>();
        point = GameObject.Find("Header/Text_1").GetComponent<Text>();
        resultpopup = GameObject.Find("Popup");

        // 非表示
        resultpopup.SetActive(false);

        // UI にユーザのポイントとお金を表示する
        UpdateMoneyPointText(System.Convert.ToInt32(NCMBUser.CurrentUser["money"]),
            System.Convert.ToInt32(NCMBUser.CurrentUser["points"]));

    }

    //-----------------------------------------------------------
    // 所持金とポイントの更新

    public void UpdateMoneyPointText(int moneyDiff, int pointDiff = 0) {
        // 初回
        if (money.text == "所持金：") {
            money.text = string.Format("所持金：" + "{0:#,0}", moneyDiff);
            point.text = "ポイント：" + pointDiff;

        }
        else {
            // "所持金："分削除
            var moneystr = money.text.Substring(4, money.text.Length - 4);
            moneystr = string.Join("", moneystr.Split(','));
            money.text = string.Format("所持金：" + "{0:#,0}", int.Parse(moneystr) + moneyDiff);
            // "ポイント："分削除
            var pointstr = point.text.Substring(5, point.text.Length - 5);
            point.text = "ポイント：" + (int.Parse(pointstr) + pointDiff);
        }
    }

    // ガチャ結果更新
    public void UpdateResultText(string resultDiff) {
        resultpopup.GetComponentInChildren<Text>().text = resultDiff;
    }

    // データ削除時のログ
    public static void DisplayLogs(string str) {
        Text log = GameObject.Find("Body/Text").GetComponent<Text>();
        log.text = str + "\n"+ log.text;
    }

    // ガチャ結果表示非表示
    public void EnableResultPopup(bool enabled) {
        resultpopup.SetActive(enabled);
    }

}
