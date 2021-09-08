// １モジュール １ファイル
module.exports = function(req, res)
{
    //--------------------------------------------------------------
    // 1. ガチャIDに一致するガチャのデータをデータストアから取得する
    //--------------------------------------------------------------

    // データストアに接続する準備
    var NCMB = require('ncmb');
    const APL_KEY = "";
    const CLI_KEY = "";
    var ncmb = new NCMB(APL_KEY, CLI_KEY);

    // リクエストのクエリ（ガチャID）を取得
    var gachaId = req.query.gachaId;
    if(gachaId == null){
        // ガチャIDが渡されていない
        res.status(400)
           .json({"message":"BadRequest (No gachaId)"})
    }

    // データストアの "GachaTest"クラスに接続し、
    //      objectIdがgachaIdに一致するものを検索して取得（ヒットするものは１つ）
    var gachaClass = ncmb.DataStore("GachaTest");
    gachaClass.equalTo("objectId", gachaId)
        .fetchAll()
        .then(function(results){
            // データストアから取得成功
            if(results.length == 0){
                // １つも見つからなかった
                res.status(404)
                    .json({"message":"NotFound (Confirm objectId)"});
            }

            //--------------------------------------------------------------
            // 2. ガチャロジック本体 (ガチャから得られる報酬５つのうち１つを決定)
            //--------------------------------------------------------------
            rewardNum = selectReward(results[0].probability);
            if(rewardNum == -1){
                res.status(500)
                    .json({"message":"Probabilities of rewards must be defined as Array(length=2)"});
            }
            // ガチャの結果
            var moneyDiff = -results[0].cost;
            var typeDiff = results[0].rewards[rewardNum];
            var pointDiff = results[0].point[rewardNum];

            //--------------------------------------------------------------
            // 3. ガチャの結果が得られたらそのログを保存する
            //--------------------------------------------------------------

            // リクエストのクエリ(userId)を取得
            var userId = req.query.userId;
            if(userId == null){
                // ユーザIDが渡されていない
                res.status(400)
                   .json({"message":"BadRequest (No userId)"})
            }

            // データストアの "GachaLogTest"クラスに接続
            var GachaLogClass = ncmb.DataStore("GachaLogTest");
            var gachaLogClass = new GachaLogClass();
            // ログ保存を実行
            gachaLogClass.set("moneyDiff", moneyDiff)   // お金の増減
                         .set("pointDiff", pointDiff)   // 取得ポイント
                         .set("typeDiff", typeDiff)     // 取得タイプ
                         .set("userId", userId)         // ユーザID
                         .save()
                         .then(function(gachaLogClass){
                             //--------------------------------------------------------------
                             // 4. JSON形式でガチャの結果とログ保存成功の旨を端末に返す
                             //--------------------------------------------------------------
                             res.status(200)
                                .json({"moneyDiff":moneyDiff,
                                      "pointDiff":pointDiff,
                                       "typeDiff":typeDiff});
                         })
                         .catch(function(err){
                             // ログ保存失敗
                             res.status(500).json({"message": "Failed to save log."});
                         });
        })
        .catch(function(err){
            // データストアにエラーあり
            res.status(500).json({error: 500});
        });
}

//--------------------------------------------------------------
// ガチャの報酬５つのうちから１つを選択する関数
//--------------------------------------------------------------
function selectReward(probabilities)
{
    // probabilities は Array か
    if ( !(Array.isArray(probabilities)) ) return -1;
    // probabilities の要素数は４か
    if ( probabilities.length != 4) return -1;

    const p0 = Number(probabilities[0]); // rewards[0]が選択される確率
    const p1 = Number(probabilities[1]); // rewards[1]が   〃
    const p2 = Number(probabilities[2]); // rewards[2]が   〃
    const p3 = Number(probabilities[3]); // rewards[3]が   〃
    const p4 = Number(probabilities[4]); // rewards[4]が   〃

    // randNum: [0.0, 1.0]
    var randNum = Math.random();
    if(randNum <= p0) return 0;
    else if(randNum <= p0+p1) return 1;
    else if(randNum <= p0+p1+p2) return 2;
    else if(randNum <= p0+p1+p2+p3) return 3;
    else return 4;
}
