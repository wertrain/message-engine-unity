using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System;

public class MessageEngine
{
    /**
     * メッセージエンジンの各種パラメータ
     **/
    public struct Param
    {
        /** 1文字を表示する間隔（秒） */
        public float IntervalPrintChar { get; set; }
    }

    /** エンジンのパラメータ */
    private Param engineParam;
    /** 文字表示UI */
    private UnityEngine.UI.Text messageUI;
    /** 表示するメッセージ全文 */
    private string allMessage;
    /** 次のメッセージ開始インデックス */
    private int nextMessageIndex;
    /** 現在表示中のメッセージ */
    private string currentMessage;
    /** 現在表示中のメッセージのインデックス */
    private int currentIndex;
    /** 時刻をカウント */
    private float currentTime;

    private enum Sequence
    {
        Idle,           /** 待機中 */
        MakePage,       /** ページメッセージ作成中 */
        Progress,       /** 進行中 */
        WaitNextPage,   /** ページ送り待機中 */
        Complete,       /** 動作完了 */
    };
    /** エンジンのシーケンス */
    private Sequence sequence;

    private enum EngineEventType
    {
        Wait,           /** ページ送り */
    };
    private struct EngineEvent
    {
        public int Index { get; set; }
        public EngineEventType Type { get; set; }
    }
    private List<EngineEvent> eventList;

    public MessageEngine(UnityEngine.UI.Text uiText)
    {
        messageUI = uiText;
        engineParam = GetDefaultParam();
        allMessage = string.Empty;
        currentIndex = 0;
        nextMessageIndex = 0;
        currentMessage = string.Empty;
        sequence = Sequence.Idle;
        eventList = new List<EngineEvent>();
    }

    public Param GetDefaultParam()
    {
        Param param = new Param();
        param.IntervalPrintChar = 0.05f;
        return param;
    }

    public void SetParam(Param param)
    {
        engineParam = param;
    }

    public void SetMessage(string message)
    {
        allMessage = message;
    }

    public bool Start(string message)
    {
        if (sequence == Sequence.Idle || sequence == Sequence.Complete)
        {
            allMessage = message;

            if (allMessage.Length > 0)
            {
                sequence = Sequence.MakePage;
                return true;
            }
        }
        return false;
    }

    public bool NextPage()
    {
        switch (sequence)
        {
            case Sequence.Progress:
                messageUI.text = currentMessage;
                sequence = Sequence.WaitNextPage;
                return true;
            case Sequence.WaitNextPage:
                SequenceMakePage();
                return true;
        }
        return false;
    }

    public void Update()
    {
        switch(sequence)
        {
            case Sequence.MakePage:
                SequenceMakePage();
                break;
            case Sequence.Progress:
                SequenceProgress();
                break;
        }
    }

    private void SequenceMakePage()
    {
        string tmp = allMessage.Substring(nextMessageIndex);
        currentMessage = GetFormatedText(tmp);

        currentTime = Time.time;
        sequence = Sequence.Progress;
        currentIndex = 0;
        messageUI.text = string.Empty;
    }

    private void SequenceProgress()
    {
        // 指定された時間が経てば文字列の更新処理
        if (currentTime + engineParam.IntervalPrintChar < Time.time)
        {
            currentTime = Time.time;
            if (++currentIndex >= currentMessage.Length)
            {
                if (nextMessageIndex < allMessage.Length) sequence = Sequence.WaitNextPage;
                else sequence = Sequence.Complete;
            }
            else if (messageUI != null)
            {
                int findIndex = eventList.FindIndex(item => item.Index == currentIndex);
                if (findIndex > 0)
                {
                    EngineEvent engineEvent = eventList[findIndex];
                    switch (engineEvent.Type)
                    {
                        case EngineEventType.Wait:
                            break;
                    }
                    eventList.Remove(engineEvent);
                }
                messageUI.text = currentMessage.Substring(0, currentIndex);
            }
        }
    }

    // 以下は禁則処理の対応など
    // 参考 https://github.com/tsubaki/HyphenationJpn_uGUI/blob/master/Assets/HyphenationJpn.cs

    private string GetFormatedText(string msg)
    {
        if (string.IsNullOrEmpty(msg))
        {
            return string.Empty;
        }

        RectTransform rectTransform = messageUI.rectTransform;
        float rectWidth = rectTransform.rect.width;
        float spaceCharacterWidth = GetSpaceWidth();

        // override
        messageUI.horizontalOverflow = HorizontalWrapMode.Overflow;

        // work
        StringBuilder lineBuilder = new StringBuilder();
        StringBuilder originalMessage = new StringBuilder();

        float lineWidth = 0;
        foreach (var originalLine in GetWordList(msg))
        {
            string newText = lineBuilder.ToString() + originalLine;
            if (rectTransform.rect.height <= GetTextHeight(newText))
                break;

            lineWidth += GetTextWidth(originalLine);

            if (originalLine.IndexOf('\n') > 0 || 
                originalLine == Environment.NewLine)
            {
                lineWidth = 0;
            }
            else
            {
                if (originalLine == " ")
                {
                    lineWidth += spaceCharacterWidth;
                }

                if (lineWidth > rectWidth)
                {
                    lineBuilder.Append(Environment.NewLine);
                    lineWidth = GetTextWidth(originalLine);
                    
                    if (rectTransform.rect.height <= GetTextHeight(lineBuilder.ToString()))
                        break;
                }
            }
            originalMessage.Append(originalLine);
            lineBuilder.Append(originalLine);
        }

        nextMessageIndex += originalMessage.Length;
        return lineBuilder.ToString();
    }

    private List<string> GetWordList(string tmpText)
    {
        List<string> words = new List<string>();
        StringBuilder line = new StringBuilder();
        char emptyChar = new char();

        for (int characterCount = 0; characterCount < tmpText.Length; characterCount++)
        {
            char currentCharacter = tmpText[characterCount];
            char nextCharacter = (characterCount < tmpText.Length - 1) ? tmpText[characterCount + 1] : emptyChar;
            char preCharacter = (characterCount > 0) ? preCharacter = tmpText[characterCount - 1] : emptyChar;

            line.Append(currentCharacter);

            if (((IsLatin(currentCharacter) && IsLatin(preCharacter)) && (IsLatin(currentCharacter) && !IsLatin(preCharacter))) ||
                (!IsLatin(currentCharacter) && CHECK_HYP_BACK(preCharacter)) ||
                (!IsLatin(nextCharacter) && !CHECK_HYP_FRONT(nextCharacter) && !CHECK_HYP_BACK(currentCharacter)) ||
                (characterCount == tmpText.Length - 1))
            {
                words.Add(line.ToString());
                line = new StringBuilder();
                continue;
            }
        }
        return words;
    }

    // static
    private readonly static string RITCH_TEXT_REPLACE =
        "(\\<color=.*\\>|</color>|" +
        "\\<size=.n\\>|</size>|" +
        "<b>|</b>|" +
        "<i>|</i>)";

    // 禁則処理 http://ja.wikipedia.org/wiki/%E7%A6%81%E5%89%87%E5%87%A6%E7%90%86
    // 行頭禁則文字
    private readonly static char[] HYP_FRONT =
        (",)]｝、。）〕〉》」』】〙〗〟’”｠»" +// 終わり括弧類 簡易版
         "ァィゥェォッャュョヮヵヶっぁぃぅぇぉっゃゅょゎ" +//行頭禁則和字 
         "‐゠–〜ー" +//ハイフン類
         "?!！？‼⁇⁈⁉" +//区切り約物
         "・:;" +//中点類
         "。.").ToCharArray();//句点類

    private readonly static char[] HYP_BACK =
         "(（[｛〔〈《「『【〘〖〝‘“｟«".ToCharArray();//始め括弧類

    private readonly static char[] HYP_LATIN =
        ("abcdefghijklmnopqrstuvwxyz" +
         "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
         "0123456789" +
         "<>=/().,").ToCharArray();

    private static bool CHECK_HYP_FRONT(char str)
    {
        return Array.Exists<char>(HYP_FRONT, item => item == str);
    }

    private static bool CHECK_HYP_BACK(char str)
    {
        return Array.Exists<char>(HYP_BACK, item => item == str);
    }

    private static bool IsLatin(char s)
    {
        return Array.Exists<char>(HYP_LATIN, item => item == s);
    }

    private float GetSpaceWidth()
    {
        float tmp0 = GetTextWidth("m m");
        float tmp1 = GetTextWidth("mm");
        return (tmp0 - tmp1);
    }

    private float GetLineHeight()
    {
        messageUI.text = "m";
        float height = messageUI.preferredWidth;
        messageUI.text = string.Empty;
        return height;
    }

    private float GetTextWidth(string message)
    {
        if (messageUI.supportRichText)
        {
            message = Regex.Replace(message, RITCH_TEXT_REPLACE, string.Empty);
        }
        messageUI.text = message;
        return messageUI.preferredWidth;
    }

    private float GetTextHeight(string message)
    {
        if (messageUI.supportRichText)
        {
            message = Regex.Replace(message, RITCH_TEXT_REPLACE, string.Empty);
        }
        messageUI.text = message;
        return messageUI.preferredHeight;
    }
}
