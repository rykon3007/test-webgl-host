using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SummarizedRankBookController : MonoBehaviour
{
    public SummarizedRankBookDisplay[] bookDisplays;
    public string csvUrl;

    List<SummarizedRankBook> allBooks;

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
                b.rank == display.rank);

            if (match != null)
            {
                display.SetSummarizedRankBook(match);
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

        allBooks = new List<SummarizedRankBook>();
        string[] lines = uwr.downloadHandler.text.Split('\n');
        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var fields = line.Split(',');

            SummarizedRankBook book = new SummarizedRankBook
            {
                rank = fields[0].Trim(),
                title = fields[1].Trim()
            };
            allBooks.Add(book);
        }
    }    
}
