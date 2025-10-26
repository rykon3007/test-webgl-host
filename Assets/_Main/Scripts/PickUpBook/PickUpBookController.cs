using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PickUpBookController : MonoBehaviour
{
    public PickUpBookDisplay[] bookDisplays;
    public string csvUrl;

    List<PickUpBook> allBooks;

    void Start()
    {
        StartCoroutine(Init());
    }

    IEnumerator Init()
    {
        yield return LoadCSV(csvUrl);

        foreach (var display in bookDisplays)
        {
            var match = allBooks.FirstOrDefault(b =>
                b.order == display.order);

            if (match != null)
            {
                string imageUrl = $"https://bookgarden.hmc.l.u-tokyo.ac.jp/StreamingAssets/image/{match.isbn}.jpg";
                yield return LoadImage(imageUrl, display, match);
            }
        }
    }

    IEnumerator LoadCSV(string url)
    {
        UnityWebRequest uwr = UnityWebRequest.Get(url);
        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("CSV load error: " + uwr.error);
            yield break;
        }

        allBooks = new List<PickUpBook>();
        string[] lines = uwr.downloadHandler.text.Split('\n');
        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var fields = line.Split(',');

            PickUpBook book = new PickUpBook
            {
                order = fields[0].Trim(),
                isbn = fields[1].Trim(),
                title = fields[2].Trim(),
                author = fields[3].Trim(),
                publisher = fields[4].Trim(),
                genre = fields[5].Trim(),
                description = fields[6].Trim()
            };
            allBooks.Add(book);
        }
    }

    IEnumerator LoadImage(string url, PickUpBookDisplay display, PickUpBook book)
    {
        UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url);
        yield return uwr.SendWebRequest();

        Texture2D texture = null;
        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"Failed to load image: {url}, trying fallback URL");
            // 固定のフォールバックURLを指定
            string fallbackUrl = "https://bookgarden.hmc.l.u-tokyo.ac.jp/StreamingAssets/image/404_not_found.jpg";
            UnityWebRequest fallbackUwr = UnityWebRequestTexture.GetTexture(fallbackUrl);
            yield return fallbackUwr.SendWebRequest();
            if (fallbackUwr.result != UnityWebRequest.Result.Success)
            {
                Debug.Log($"ImageURL:'{url}'");
                Debug.Log($"FallbackURL:'{fallbackUrl}'");
                Debug.LogWarning($"Failed to load fallback image: {fallbackUrl}");
                Debug.LogWarning($"Failed to load image: {url}");
                Debug.LogWarning($"Error Reason: {uwr.error}");
                yield break;
            }
            else
            {
                texture = DownloadHandlerTexture.GetContent(fallbackUwr);
            }
        }
        else
        {
            texture = DownloadHandlerTexture.GetContent(uwr);
        }

        display.SetPickUpBook(book, texture);
    }
}
