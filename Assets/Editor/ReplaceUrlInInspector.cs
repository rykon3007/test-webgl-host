using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ReplaceUrlAll : EditorWindow
{
    // 例：置換前/後（必要に応じて差し替え）
    private string beforeBase = "";
    private string afterBase  = "";

    // 対象（Inspector＝YAML資産 / Scripts＝.cs）
    private bool includeInspector = true;
    private bool includeScripts   = true;

    // 検索ルート
    private string searchRoot = "Assets";
    private bool dryRun = true;

    // YAML 対象拡張子
    private static readonly string[] InspectorExts = {
        ".prefab",".unity",".asset",".mat",".anim",".controller",".overridecontroller"
    };
    // スクリプト対象拡張子
    private static readonly string[] ScriptExts = { ".cs" };

    private int hitInspector, hitScripts, changedInspector, changedScripts;

    [MenuItem("Tools/URL一括置換（Inspector + Scripts）")]
    public static void Open() => GetWindow<ReplaceUrlAll>("URL一括置換");

    private void OnGUI()
    {
        EditorGUILayout.LabelField("指定したベースURLを、Inspectorに保存された値とC#スクリプト内で一括置換します。", EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space();

        searchRoot       = EditorGUILayout.TextField("検索ルート", searchRoot);
        beforeBase       = EditorGUILayout.TextField("置換前ベースURL", beforeBase);
        afterBase        = EditorGUILayout.TextField("置換後ベースURL", afterBase);
        includeInspector = EditorGUILayout.Toggle("Inspector(YAML)を対象にする", includeInspector);
        includeScripts   = EditorGUILayout.Toggle("C# Scripts(.cs)を対象にする", includeScripts);
        dryRun           = EditorGUILayout.Toggle(new GUIContent("ドライラン（書き込みしない）"), dryRun);

        EditorGUILayout.HelpBox("入力ルール: 1) http(s):// で始まる  2) 末尾の / は自動で削除されます", MessageType.Info);

        EditorGUILayout.Space();
        if (GUILayout.Button(dryRun ? "ドライラン実行" : "置換を実行"))
        {
            // --- 入力バリデーション & 正規化 ---
            if (!includeInspector && !includeScripts)
            {
                EditorUtility.DisplayDialog("エラー", "対象を1つ以上選択してください（Inspector / Scripts）。", "OK");
                return;
            }
            if (!TryNormalizeBaseUrl(ref beforeBase, "置換前ベースURL", out var beforeErr))
            {
                EditorUtility.DisplayDialog("入力エラー", beforeErr, "OK");
                return;
            }
            if (!TryNormalizeBaseUrl(ref afterBase, "置換後ベースURL", out var afterErr))
            {
                EditorUtility.DisplayDialog("入力エラー", afterErr, "OK");
                return;
            }
            if (beforeBase == afterBase)
            {
                EditorUtility.DisplayDialog("入力エラー", "置換前と置換後のベースURLが同じです。別の値を入力してください。", "OK");
                return;
            }

            if (!dryRun && !EditorUtility.DisplayDialog(
                    "確認",
                    $"以下を置換します：\n\n{beforeBase}\n→ {afterBase}\n\n対象: {(includeInspector?"Inspector ":"")}{(includeScripts?"Scripts":"")}\n\n実行してよろしいですか？",
                    "実行", "キャンセル"))
            {
                return;
            }

            RunReplace(); // 実行
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"[結果] Inspector: ヒット {hitInspector}, 変更 {changedInspector}");
        EditorGUILayout.LabelField($"[結果] Scripts  : ヒット {hitScripts}, 変更 {changedScripts}");
        EditorGUILayout.HelpBox(
            "注意: YAML置換を使う場合は Edit > Project Settings > Editor > Asset Serialization を Force Text に。実行前に必ずGitコミットを。", 
            MessageType.Info);
    }

    // 入力を検証しつつ正規化（先頭http/https必須、末尾/除去、周辺空白除去）
    private static bool TryNormalizeBaseUrl(ref string url, string label, out string error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(url))
        {
            error = $"{label} を入力してください。";
            return false;
        }

        var s = url.Trim();

        if (!(s.StartsWith("http://") || s.StartsWith("https://")))
        {
            error = $"{label} は http:// または https:// で始めてください。";
            return false;
        }

        // 末尾の / を削除（複数連続も除去）
        while (s.EndsWith("/")) s = s.Substring(0, s.Length - 1);

        url = s; // 正規化結果をUIにも反映
        return true;
    }

    private static bool ShouldSkip(string path)
    {
        // Library/ProjectSettings/UserSettings/Logs などは対象外（FindAssetsで基本出てこないが念のため）
        return path.EndsWith(".meta")
            || path.StartsWith("Library/")
            || path.StartsWith("ProjectSettings/")
            || path.StartsWith("UserSettings/")
            || path.StartsWith("Logs/")
            || Directory.Exists(path);
    }

    private void RunReplace()
    {
        hitInspector = hitScripts = changedInspector = changedScripts = 0;

        var guids = AssetDatabase.FindAssets(string.Empty, new[] { searchRoot });
        try
        {
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                EditorUtility.DisplayProgressBar("URL一括置換", path, (float)i / Mathf.Max(1, guids.Length));
                if (ShouldSkip(path)) continue;

                string ext = Path.GetExtension(path).ToLowerInvariant();
                bool isInspectorTarget = includeInspector && InspectorExts.Contains(ext);
                bool isScriptTarget    = includeScripts   && ScriptExts.Contains(ext);
                if (!isInspectorTarget && !isScriptTarget) continue;

                string text;
                try { text = File.ReadAllText(path, Encoding.UTF8); }
                catch { continue; }

                if (string.IsNullOrEmpty(text) || !text.Contains(beforeBase)) continue;

                string replaced = text.Replace(beforeBase, afterBase);
                bool changed = replaced != text;

                if (isInspectorTarget)
                {
                    hitInspector++;
                    if (!dryRun && changed)
                    {
                        File.WriteAllText(path, replaced, new UTF8Encoding(false));
                        changedInspector++;
                        Debug.Log($"[Inspector置換] {path}");
                    }
                }
                else if (isScriptTarget)
                {
                    hitScripts++;
                    if (!dryRun && changed)
                    {
                        File.WriteAllText(path, replaced, new UTF8Encoding(false));
                        changedScripts++;
                        Debug.Log($"[Scripts置換] {path}");
                    }
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
            var msg = dryRun
                ? $"ドライラン完了：Inspector ヒット {hitInspector} / Scripts ヒット {hitScripts}"
                : $"置換完了：Inspector 変更 {changedInspector} / Scripts 変更 {changedScripts}";
            EditorUtility.DisplayDialog("完了", msg, "OK");
            Debug.Log($"[URL置換] {msg}");
        }
    }

    // 選択フォルダ右クリックから実行したい場合の簡易版
    [MenuItem("Assets/URL一括置換（選択フォルダ内）", true)]
    private static bool ValidateContext() => Selection.assetGUIDs?.Length > 0;

    [MenuItem("Assets/URL一括置換（選択フォルダ内）")]
    private static void ContextRun()
    {
        var wnd = GetWindow<ReplaceUrlAll>("URL一括置換");
        // 最初の選択フォルダに検索ルートを寄せる
        var guid = Selection.assetGUIDs.FirstOrDefault();
        var path = AssetDatabase.GUIDToAssetPath(guid);
        if (AssetDatabase.IsValidFolder(path)) wnd.searchRoot = path;
        wnd.Show();
    }
}
