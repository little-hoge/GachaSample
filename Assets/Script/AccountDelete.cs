using NCMB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccountDelete : MonoBehaviour {
    private GachaButtonGenerator gachaButtonGenerator;

    //---------------------------------------------------------------------------------------------
    // アプリ起動時に呼ばれる関メソッド(初期化)
    //---------------------------------------------------------------------------------------------
    void Start() {
        gachaButtonGenerator = GameObject.Find("GachaButtonGenerator").GetComponent<GachaButtonGenerator>();
    }

    // ユーザーデータ削除
    public void userDelete() {

        // ガチャ中
        if (gachaButtonGenerator.IsRollingGachaAll()){
            UIController.DisplayLogs("ガチャ動作中で削除失敗");
            return;
        }

        // 情報削除
        NCMBUser.CurrentUser.DeleteAsync((NCMBException e) => {

            if (e != null) {
                UIController.DisplayLogs("ユーザー情報削除失敗:" + e.ErrorMessage);
                return;
            }
            else {
                UIController.DisplayLogs("ユーザー情報削除成功");
            }

        });

        // 端末から情報削除
        PlayerPrefs.DeleteKey("_user_name_");

        // ログアウト
        NCMBUser.LogOutAsync((NCMBException e) => {
            if (e != null) {
                UIController.DisplayLogs("ログアウトに失敗: " + e.ErrorMessage);
                return;
            }

            else {
                UIController.DisplayLogs("ログアウトに成功");
            }

        });

        // 再ログイン
        StartCoroutine(DelayCoroutine());

    }

    // 数秒後実行
    private IEnumerator DelayCoroutine() {
        yield return new WaitForSeconds(3);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Login");
    }
}
