using UnityEngine;
using System.Collections;

public class GameMain : MonoBehaviour {

    private MessageEngine messageEngine;

    // Use this for initialization
    void Start ()
    {
        UnityEngine.UI.Text uiText = GetComponentInChildren<UnityEngine.UI.Text>();
        messageEngine = new MessageEngine(uiText);
        messageEngine.SetMessage(
            "これはテストメッセージです。現在試作中のため、動作がおかしい場合があります。このエンジンでは指定行で勝手に改行されます。\n"
            + "メッセージで￥nを入力すれば、手動での改行も可能です。改善の余地がかなりあります。\n"
            + "リッチテキストには未対応です。試しに使ってみると、こんなことになります。<b>太字</b> や<color=yellow>色変え</color>！！！\n"
        );
    }
	
    // Update is called once per frame
    void Update ()
    {
        messageEngine.Update();
    }
}
