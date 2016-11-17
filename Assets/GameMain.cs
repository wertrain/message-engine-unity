using UnityEngine;
using System.Collections;

public class GameMain : MonoBehaviour {

    private MessageEngine messageEngine;

    // Use this for initialization
    void Start ()
    {
        UnityEngine.UI.Text uiText = GetComponentInChildren<UnityEngine.UI.Text>();
        messageEngine = new MessageEngine(uiText);
        messageEngine.Start(
            " Unity（別名:Unity3D）とは、統合開発環境を内蔵し、複数のプラットホームに対応するゲームエンジンである。\n"
            + "ユニティ・テクノロジーズ（英語版）が開発した。日本法人はユニティテクノロジーズジャパン合同会社。ウェブプラグイン、デスクトッププラットフォーム、ゲーム機、携帯機器向けのコンピュータゲームを開発するために用いられ、100万人以上の開発者が利用している[3]。Unityは主にモバイルやブラウザゲーム製作に使用されるが、コンソールゲーム機およびPCにゲームをインストールすることもできる。このゲームエンジン自体はC言語/C++で書かれているが、スクリプト言語としてC#、UnityScript (JavaScript)、Booの3種類のプログラミング言語に対応している。2005年にOS Xに対応したゲーム開発ツールとして誕生してから、今日ではマルチプラットフォームに対応したゲームエンジンにまで成長した[3]。\n"
            + "2015年10月にリリースされたバージョン5.2.2現在、iOS、Android、Tizen、Android TV、Windows、Windows Phone 8、Windowsストアアプリ、OS X、Linux、ウェブブラウザ(WebGL、Unity Web Player)、PlayStation 3、PlayStation 4、PS Vita、Xbox 360、Xbox One、Wii UそしてVR/AR向けの開発に対応している[4]。また、2016年1月8日からは、パチンコ・パチスロを含む日本国内の遊技機およびアーケードゲーム機開発用ライセンス「Unity for 遊技機」の販売も開始された[5]。\n"
            + "グラフィックエンジンはDirect3D(Windows)、OpenGL(Mac, Windows, Linux)、OpenGL ES(Android, iOS)、プロプライエタリのAPIを使用。バンプマッピング、環境マッピング、視差マッピング、スクリーンスペースアンビエントオクルージョン（英語版）(SSAO)、シャドウマップ（英語版）を使ったダイナミック・シャドウ、テクスチャレンダリング（英語版）、フルスクリーンポストプロセッシングエフェクトに対応している[6]。"
            + "また、3ds Max、Maya、Softimage、Blender、modo、ZBrush、3D-Coat、Cinema 4D、Cheetah3D、Adobe Photoshop、Adobe Fireworks、Allegorithmic Substanceのアートアセットとファイル形式に対応しており、これらの資産をゲームプロジェクトに追加したり、Unityのグラフィカルユーザーインターフェースで管理することができる[7]。"
            + "ShaderLabの言語はシェーダーのために使用され、固定機能パイプラインとGLSLやCg/HLSLで書かれたシェーダープログラム両方の宣言型「プログラミング」に対応している[8][9]。シェーダーは複数のバリエーションや宣言されたフォールバック仕様を含むことができるため、Unityは現在使用しているビデオカードに最もよいバリエーションを検出したり、互換性が無い場合でも性能を出すために機能を犠牲にできる代替のシェーダーにフォールバックすることができる[10]。 また、NVIDIA（かつてはAgeia（英語版））のPhysX物理エンジンを内蔵サポートしており、Unity 3.0では任意メッシュおよびスキンメッシュでのリアルタイムクロスシミュレーション、シックレイキャスト、衝突レイヤーへの対応が追加された[11]。"
        );
    }
	
    // Update is called once per frame
    void Update ()
    {
        messageEngine.Update();

        if (Input.GetMouseButtonUp(0))
        {
            messageEngine.NextPage();
        }
    }
}
