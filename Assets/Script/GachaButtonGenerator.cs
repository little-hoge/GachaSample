using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NCMB;
using UnityEngine.UI;

// データストアからガチャを取得してボタンオブジェクトを生成するクラス
public class GachaButtonGenerator : MonoBehaviour {

    // データストア->ガチャクラス から取得したレコードの数（ガチャの数）
    private uint numOfGacha;
    // すべてのガチャのID
    private List<string> allGachaId;



    // ガチャ数の取得
    public uint NumOfGacha {
        get { return numOfGacha; }
    }
    // ガチャIDの取得
    public string GetGachaId(int i) {
        return allGachaId[i];
    }

    // Startメソッドが終了したかどうかのフラグ
    private bool isInitialized = false;
    // isInitialized の取得
    public bool IsInitialized {
        get { return isInitialized; }
    }

    // ガチャ動作中かどうか
    public bool IsRollingGachaAll() {
        bool result =false;

        for (int i = 0; i < gachaCubeWork.Count; i++) {
            if (gachaCubeWork[i].IsRollingGacha) {
                result = true;
                break;
            }

        }
        return result;
    }

    //---------------------------------------------------------------------------------------------
    // アプリ起動時に呼ばれるメソッド (データストアからガチャを取得)
    //---------------------------------------------------------------------------------------------
    IEnumerator Start() {
        // ガチャの数とガチャIDリストの初期化
        numOfGacha = 0;
        allGachaId = new List<string>();
        gachaCubeWork = new List<Gacha>();

        // データストアにアクセスして全ガチャレコードを取得
        bool isGettingGachaData = true;
        NCMBQuery<NCMBObject> getAll = new NCMBQuery<NCMBObject>("GachaTest");
        getAll.FindAsync((List<NCMBObject> allGacha, NCMBException e) => {
            if (e != null) {
                // データ取得失敗
                Debug.Log(e.ErrorCode + ":" + e.ErrorMessage);
            }
            else {
                //データ取得成功
                foreach (NCMBObject gacha in allGacha) {
                    createGachaButton(gacha, numOfGacha);

                    // ガチャの数を増やす
                    numOfGacha++;
                }
            }
            // データ取得処理終了
            isGettingGachaData = false;
        });
        // データ取得処理が終了するまで以下の行でストップ
        yield return new WaitWhile(() => { return isGettingGachaData; });

        // ガチャの読み込み終了
        isInitialized = true;
    }

    //---------------------------------------------------------------------------------------------
    // ガチャボタンオブジェクトを生成するメソッド
    //---------------------------------------------------------------------------------------------
    public GameObject gachaCubePrefab;
    [SerializeField] private List<Gacha> gachaCubeWork;

    private void createGachaButton(NCMBObject gacha, uint gachaNum) {
        // 生成元の親階層設定
        Transform characterParent = GameObject.Find("GachaButtonSpace").transform;
        // 生成
        GameObject gachaCube = (GameObject)Instantiate(
            gachaCubePrefab, Vector3.zero, transform.rotation, characterParent
        );
        // 追加
        gachaCubeWork.Add(gachaCube.GetComponent<Gacha>());

        // 生成されたオブジェクトに名前を設定
        gachaCube.name = "GachaButton" + gachaNum.ToString();

        // ガチャの各フィールドの取得
        uint cost = System.Convert.ToUInt32(gacha["cost"]);
        ArrayList rewards_arrayList = (ArrayList)gacha["rewards"];
        List<string> rewards = new List<string>();
        foreach (object reward in rewards_arrayList) {
            rewards.Add(System.Convert.ToString(reward));
        }

        // ボタン名設定
        gachaCube.GetComponentInChildren<Text>().text = "ガチャ" + gachaNum.ToString() + "\n";
        gachaCube.GetComponentInChildren<Text>().text += "必要資金：" + cost;

        // クリック時動作追加
        Gacha m_gacha = gachaCube.GetComponent<Gacha>();
        gachaCube.GetComponent<Button>().onClick.AddListener(m_gacha.GachaDraw);

        // 各プロパティをGachaクラスで管理する
        gachaCube.GetComponent<Gacha>().InitGachaButton(gacha.ObjectId, cost, rewards, gachaNum);

        // ガチャIDリストにIDを追加
        allGachaId.Add(gacha.ObjectId);
    }
}
