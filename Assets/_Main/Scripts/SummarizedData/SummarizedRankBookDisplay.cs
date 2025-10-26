using UnityEngine;
using TMPro;

public class SummarizedRankBookDisplay : MonoBehaviour
{
    public string rank;
    public TextMeshPro titleText;


    public void SetSummarizedRankBook(SummarizedRankBook data)
    {
        titleText.text = data.title;
    }
}