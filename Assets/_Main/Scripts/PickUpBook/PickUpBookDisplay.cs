using UnityEngine;
using TMPro;

public class PickUpBookDisplay : MonoBehaviour
{
    public string order;

    public Renderer firstViewPictureRenderer;
    public Renderer secondViewPictureRenderer;
    public TextMeshPro isbnText;
    public TextMeshPro titleText;
    public TextMeshPro authorText;
    public TextMeshPro publisherText;
    public TextMeshPro genreText;
    public TextMeshPro descriptionText;


    public void SetPickUpBook(PickUpBook data, Texture2D texture)
    {
        firstViewPictureRenderer.material.mainTexture = texture;
        secondViewPictureRenderer.material.mainTexture = texture;
        isbnText.text = data.isbn;
        titleText.text = data.title;
        authorText.text = data.author;
        publisherText.text = data.publisher;
        genreText.text = data.genre;
        descriptionText.text = data.description;
    }
}