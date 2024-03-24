using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text.RegularExpressions;

public class WordGenerator : MonoBehaviour
{
    string randomWordURL = "https://random-word-api.herokuapp.com/word?length=5";

    public IEnumerator GenerateRandomWord(Action<string> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(randomWordURL))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback?.Invoke(ParseWord(request.downloadHandler.text));
            }
            else
                Debug.LogError("Failed to get random word: " + request.error);
        }
    }
    static string ParseWord(string input)
    {
        // 정규 표현식을 사용하여 문자열에서 대괄호와 따옴표를 제거
        string cleanString = Regex.Replace(input, "[\"\\[\\]]", "");

        // 결과를 반환
        return cleanString.ToUpper();
    }
}
