using NCMB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//---------------------------------------------------------------
// スクリプトからのJSON形式のレスポンスをパースして保持するための構造体
public struct ScriptResponse {
    // ポイントの変化量
    public int pointDiff;
    // お金の変化量
    public int moneyDiff;
    // 種類
    public string typeDiff;
}
//---------------------------------------------------------------

public class Gacha : MonoBehaviour {
    // 各フィールド
    [SerializeField]
    private string gachaObjectId = "";
    [SerializeField]
    private uint cost;
    [SerializeField]
    private List<string> rewards;

    //-----------------------------------------------------------
    // get
    public string GachaObjectId { get { return gachaObjectId; } }
    public uint Cost { get { return cost; } }

    public int CountRewards() { return rewards.Count; }
    public string GetReward(int i) { return rewards[i]; }

    //-----------------------------------------------------------
    // 初期化
    public void InitGachaButton(string id, uint cost, List<string> rewards, uint number) {
        // フィールド値
        this.gachaObjectId = id;
        this.cost = cost;
        this.rewards = rewards;

    }

    //-----------------------------------------------------------

    // スクリプトからのレスポンスを保持
    private ScriptResponse scriptResponse;

    // UIの制御
    private UIController uiCntrler;

    //---------------------------------------------------------------------------------------------
    // アプリ起動時に呼ばれる関メソッド(初期化)
    //---------------------------------------------------------------------------------------------
    void Start() {
        scriptResponse = new ScriptResponse();
        uiCntrler = GameObject.Find("Canvas").GetComponent<UIController>();

    }

    //---------------------------------------------------------------------------------------------
    // ガチャを回す処理を実行するメソッド
    //---------------------------------------------------------------------------------------------
    public void GachaDraw() {
        // ガチャを回す処理を実行する
        StartCoroutine(rollGachaCoroutine());

    }

    private bool isRollingGacha = false;
    public bool IsRollingGacha {
        get { return isRollingGacha; }
    }
    private IEnumerator rollGachaCoroutine() {

        // ガチャがすでに回っているのならば処理を終了する (コルーチンの唯一性も保証される)
        if (isRollingGacha) yield break;
        // ガチャを回している
        isRollingGacha = true;


        NCMBUser currUser = NCMBUser.CurrentUser;

        if (currUser != null) {
            string gachaId = transform.GetComponent<Gacha>().GachaObjectId;
            // NCMB スクリプトによりガチャ結果を計算する関数を実行する（処理終了までストップ）
            yield return StartCoroutine(executeGachaLogicScriptCoroutine(gachaId, currUser));
            // サーバ側のユーザ情報の更新
            yield return StartCoroutine(updateUserPointAndMoney(currUser));
            // 該当ユーザのログを取得
            Debug.Log(currUser.ObjectId);
        }
        else {
            // 出るはずはないエラーメッセージ
            Debug.Log("Failed to roll Gacha(No Current User)");
        }

        // ガチャ結果テキストを更新する
        string typestr = "取得タイプ：" + scriptResponse.typeDiff + "\n";
        string pointstr = string.Format("取得ポイント：" + scriptResponse.pointDiff);
        uiCntrler.UpdateResultText(typestr + pointstr);

        // ガチャ結果を表示する
        uiCntrler.EnableResultPopup(true);

        yield return new WaitForSeconds(3);

        // ガチャ結果を非表示にする
        uiCntrler.EnableResultPopup(false);

        // 各UIテキストの更新・表示
        uiCntrler.UpdateMoneyPointText(scriptResponse.moneyDiff, scriptResponse.pointDiff);

        // ガチャを回す処理終了
        isRollingGacha = false;
    }

    //---------------------------------------------------------------------------------------------
    // スクリプトでガチャロジックを実行するメソッド
    //---------------------------------------------------------------------------------------------	
    private IEnumerator executeGachaLogicScriptCoroutine(string gachaId, NCMBObject currUser) {
        // scriptResponse 初期化
        scriptResponse.pointDiff = 0;
        scriptResponse.moneyDiff = 0;

        // 実行するファイル名とリクエストのタイプでインスタンス生成
        NCMBScript gachaLogicScript = new NCMBScript("gachaTest.js", NCMBScript.MethodType.GET);
        // スクリプトに渡すクエリ（ガチャのIDとユーザID）を設定する
        Dictionary<string, object> query = new Dictionary<string, object>(){
                                                        {"gachaId", gachaId},
                                                        {"userId", currUser.ObjectId} };
        // スクリプトを実行する
        bool isCalculating = true;
        gachaLogicScript.ExecuteAsync(null, null, query, (byte[] result, NCMBException e) => {
            if (e != null) {
                // スクリプト実行失敗
                Debug.Log(e.ErrorCode + ":" + e.ErrorMessage);
            }
            else {
                // スクリプト実行成功(JSONパース（byte[] -> string -> ScriptResponse)
                string resultStr = System.Text.Encoding.ASCII.GetString(result);
                scriptResponse = JsonUtility.FromJson<ScriptResponse>(resultStr);

                // スクリプト実行失敗
                Debug.Log("Data:" + scriptResponse);
            }
            // スクリプト処理終了
            isCalculating = false;
        });
        // スクリプトでの処理が終了するまで待機する
        yield return new WaitWhile(() => { return isCalculating; });
    }

    //---------------------------------------------------------------------------------------------
    // サーバ側のユーザの情報（お金とポイント）を更新するメソッド
    //---------------------------------------------------------------------------------------------	
    private IEnumerator updateUserPointAndMoney(NCMBObject currUser) {
        currUser["money"] = System.Convert.ToInt32(currUser["money"]) + scriptResponse.moneyDiff;
        currUser["points"] = System.Convert.ToInt32(currUser["points"]) + scriptResponse.pointDiff;
        // 保存
        currUser.SaveAsync((NCMBException e) => {
            if (e != null) {
                // 保存失敗
                Debug.Log(e.ErrorCode + ": " + e.ErrorMessage);
            }
            else {
                // 保存成功
                Debug.Log("Succeeded to update the user data)");
            }
        });
        // 今回は特に同期処理にする必要なし
        yield return null;
    }

}

