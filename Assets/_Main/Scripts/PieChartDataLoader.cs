using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using ChartAndGraph;
using System.Linq;

public class PieChartDataLoader : MonoBehaviour
{
    public PieChart pieChart;
    public string csvUrl;

    void Start()
    {
        StartCoroutine(LoadData());
    }

    IEnumerator LoadData()
    {
        var uwr = UnityWebRequest.Get(csvUrl);
        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("CSV load failed: " + uwr.error);
            yield break;
        }

        pieChart.DataSource.Clear();

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            Debug.LogError("Shader not found!");
            yield break;
        }

        var lines = uwr.downloadHandler.text.Split('\n')
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToArray();

        for (int i = 1; i < lines.Length; i++)
        {
            var fields = lines[i].Split(',');
            if (fields.Length < 2) continue;

            string category = fields[0].Trim();
            if (!float.TryParse(fields[1].Trim(), out float amount)) continue;

            var mat = new Material(shader) { color = Random.ColorHSV() };
            pieChart.DataSource.AddCategory(category, mat);
            pieChart.DataSource.SetValue(category, amount);
        }
    }
}
