using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class WordChecker : MonoBehaviour
{
    string url = "https://api.dictionaryapi.dev/api/v2/entries/en/";
    public IEnumerator Check(string word, Action<bool> callback = null)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url + word))
        {
            yield return request.SendWebRequest();

            Debug.Log(request.downloadHandler.text);
            bool b = false;
            if (request.result == UnityWebRequest.Result.Success)
                b = true;
            else
                Debug.LogError(request.error);

            callback?.Invoke(b);
        }
    }
}
