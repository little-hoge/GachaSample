using NCMB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnClick : MonoBehaviour {
    // UIの制御
    private UIController uiCntrler;

    //---------------------------------------------------------------------------------------------
    // アプリ起動時に呼ばれる関メソッド(初期化)
    //---------------------------------------------------------------------------------------------
    void Start() {
        uiCntrler = GameObject.Find("Canvas").GetComponent<UIController>();

    }

    // 所持金追加
    public void AddMoney() {
        NCMBUser currUser = NCMBUser.CurrentUser;
        int addMoney = 1000;
        if (currUser != null) {
            currUser["money"] = System.Convert.ToInt32(currUser["money"]) + addMoney;
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
            // UI更新
            uiCntrler.UpdateMoneyPointText(addMoney);
        }
    }
}
