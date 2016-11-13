using UnityEngine;
using System.Collections.Generic;

public class MessageEngine
{
    /**
     * メッセージエンジンの各種パラメータ
     **/
    public struct Param
    {
        /** 最大行数 */
        public int TextLine { get; set; }
        /** 1行におけるテキストの長さ */
        public int TextLengthInLine { get; set; }
        /** 1ページにおけるテキストの長さ */
        public int TextLengthInPage { get; set; }
        /** 1文字を表示する間隔（秒） */
        public float IntervalPrintChar { get; set; }
    }

    /** エンジンのパラメータ */
    private Param engineParam;
    /** 文字表示UI */
    private UnityEngine.UI.Text messageUI;
    /** 表示するメッセージ全文 */
    private string allMessage;
    /** 現在表示中のメッセージの開始インデックス */
    private int messageStartIndex;
    /** 現在表示中のメッセージの終了インデックス */
    private int messageEndIndex;
    /** 現在表示中のメッセージ */
    private string currentMessage;
    /** 現在表示中のメッセージのインデックス */
    private int currentIndex;
    /** 時刻をカウント */
    private float currentTime;
    /** 0.5文字としてカウントする文字列 */
    private readonly static char[] halfChars =
        ("abcdefghijklmnopqrstuvwxyz" +
         "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
         "0123456789" +
         "<>=/()[].,").ToCharArray();

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
        messageStartIndex = 0;
        messageEndIndex = 0;
        currentIndex = 0;
        currentMessage = string.Empty;
        sequence = Sequence.Idle;
        eventList = new List<EngineEvent>();
    }

    public Param GetDefaultParam()
    {
        Param param = new Param();
        param.TextLine = 4;
        param.TextLengthInLine = 22;
        param.TextLengthInPage = param.TextLengthInLine * (param.TextLine + 1);
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
                messageStartIndex = messageEndIndex;
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
        currentMessage = allMessage.Substring(messageStartIndex, Mathf.Min(engineParam.TextLengthInPage, allMessage.Length - messageStartIndex));
        char[] currentMessageCharArray = currentMessage.ToCharArray();
        currentMessage = string.Empty;
        eventList.Clear();

        float charCount = 0;
        int lineCount = 0;
        bool exit = false;
        for (int i = 0; i < currentMessageCharArray.Length; ++i)
        {
            char c = currentMessageCharArray[i];
            currentMessage += c;
            // 文字ごとの処理
            // ここでタグなどの判定が必要
            switch (c)
            {
                case '\n':
                    charCount = 0;
                    if (++lineCount >= engineParam.TextLine)
                        exit = true;
                    break;
                case '<':
                    exit = true;
                    break;
                case '-':
                    EngineEvent engineEvent = new EngineEvent() { Index = i, Type = EngineEventType.Wait };
                    eventList.Add(engineEvent);
                    break;
            }
            // プロポーショナルフォントへの適当な対応
            charCount += (System.Array.Exists<char>(halfChars, item => item == c)) ? 0.5f : 1.0f;
            // 一行に表示する最大文字数に達した場合は改行を追加する
            if (charCount > engineParam.TextLengthInLine)
            {
                // 直前に追加した文字が改行だった場合は追加しない
                if (c != '\n') currentMessage += '\n';

                charCount = 0;
                if (++lineCount >= engineParam.TextLine)
                    exit = true;
            }
            // 今回のテキストの終端を保持しておく
            messageEndIndex = messageStartIndex + i + 1;
            if (exit) break;
        }
        currentTime = Time.time;
        sequence = Sequence.Progress;
        currentIndex = 0;
    }

    private void SequenceProgress()
    {
        // 指定された時間が経てば文字列の更新処理
        if (currentTime + engineParam.IntervalPrintChar < Time.time)
        {
            currentTime = Time.time;
            if (++currentIndex >= currentMessage.Length)
            {
                messageStartIndex = messageEndIndex;
                if (messageStartIndex < allMessage.Length) sequence = Sequence.WaitNextPage;
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
}
