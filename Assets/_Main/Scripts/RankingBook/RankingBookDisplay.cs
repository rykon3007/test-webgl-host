using UnityEngine;
using TMPro;

public class RankingBookDisplay : MonoBehaviour
{
    public string year;
    public string rank;

    public Renderer firstViewPictureRenderer;
    public Renderer secondViewPictureRenderer;
    public TextMeshPro rankText;
    public TextMeshPro isbnText;
    public TextMeshPro titleText;
    public TextMeshPro authorText;
    public TextMeshPro publisherText;
    public TextMeshPro genreText;


    public void SetRankingBook(RankingBook data, Texture2D texture)
    {
        titleText.text = data.title;
        rankText.text = data.rank;
        firstViewPictureRenderer.material.mainTexture = texture;
        secondViewPictureRenderer.material.mainTexture = texture;
        isbnText.text = data.isbn;
        authorText.text = data.author;
        publisherText.text = data.publisher;
        genreText.text = data.genre;
    }
}