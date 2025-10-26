using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RankingBookController : MonoBehaviour
{
    public RankingBookDisplay[] bookDisplays;
    public string csvUrl;

    List<RankingBook> allBooks;

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
                b.year == display.year &&
                b.rank == display.rank);

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

        allBooks = new List<RankingBook>();
        string[] lines = uwr.downloadHandler.text.Split('\n');
        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var fields = line.Split(',');

            RankingBook book = new RankingBook
            {
                rank = fields[0].Trim(),
                isbn = fields[1].Trim(),
                title = fields[2].Trim(),
                author = fields[3].Trim(),
                publisher = fields[4].Trim(),
                genre = fields[5].Trim(),
                year = fields[6].Trim()
            };
            allBooks.Add(book);
        }
    }

    IEnumerator LoadImage(string url, RankingBookDisplay display, RankingBook book)
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
                Debug.LogWarning($"Failed to load fallback image: {fallbackUrl}");
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

        display.SetRankingBook(book, texture);
    }
}
