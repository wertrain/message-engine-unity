using UnityEngine;
using System.Collections;

public class MessageEngine
{
    private UnityEngine.UI.Text messageUI;
    private string allMessage;
    private string currentMessage;
    private int messageStartIndex;
    private int messageEndIndex;

    private int maxTextLine;
    private int maxTextInLine;
    private int maxTextLengthInPage;

    private int currentIndex;
    private bool messageExit;

    public MessageEngine(UnityEngine.UI.Text uiText)
    {
        messageUI = uiText;
        messageStartIndex = 0;
        messageEndIndex = 0;
        maxTextLine = 4;
        maxTextInLine = 28;
        maxTextLengthInPage = maxTextInLine * maxTextLine;
        currentIndex = 0;
        currentMessage = string.Empty;
        messageExit = false;
    }

    public void SetMessage(string message)
    {
        allMessage = message;
    }

    public void Update()
    {
        if (messageUI == null) return;
        if (messageExit) return;

        if (currentMessage.Length == 0)
        {
            currentMessage = allMessage.Substring(messageStartIndex, Mathf.Min(maxTextLengthInPage, allMessage.Length - messageStartIndex));
            char[] currentMessageCharArray = currentMessage.ToCharArray();
            currentMessage = string.Empty;

            int lineCount = 0;
            bool exit = false;
            for (int i = 0; i < currentMessageCharArray.Length; ++i)
            {
                char c = currentMessageCharArray[i];
                currentMessage += c;
                if (++lineCount > maxTextInLine)
                {
                    currentMessage += '\n';
                    lineCount = 0;
                }
                switch (c)
                {
                    case '\n':
                        lineCount = 0;
                        if (++lineCount >= maxTextLine)
                            exit = true;
                        break;
                }
                messageEndIndex = messageStartIndex + i;
                if (exit)
                {
                    break;
                }
            }
        }

        if (++currentIndex >= currentMessage.Length)
        {
            currentMessage = string.Empty;
            messageStartIndex = messageEndIndex;
            if (messageStartIndex >= allMessage.Length)
                messageExit = true;
            currentIndex = 0;
        }
        else
        {
            messageUI.text = currentMessage.Substring(0, currentIndex);
        }
    }
}
