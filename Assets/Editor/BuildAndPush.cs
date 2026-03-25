using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

// ── Окно с changelog ──────────────────────────────────────────────────────────

public class ChangelogWindow : EditorWindow
{
    private string _text = "";
    private Action<string> _onConfirm;

    public static void Show(string title, Action<string> onConfirm)
    {
        var w = GetWindow<ChangelogWindow>(true, title, true);
        w._onConfirm = onConfirm;
        w._text = "";
        w.minSize = new Vector2(440, 190);
        w.maxSize = new Vector2(440, 190);
    }

    void OnGUI()
    {
        GUILayout.Label("Что изменилось в этой версии?", EditorStyles.boldLabel);
        _text = EditorGUILayout.TextArea(_text, GUILayout.Height(100));
        GUILayout.Space(6);
        EditorGUILayout.HelpBox("После билда сделай git push — хук сам запушит на itch.io", MessageType.Info);
        GUILayout.Space(4);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Build")) { _onConfirm?.Invoke(_text); Close(); }
        if (GUILayout.Button("Отмена")) Close();
        GUILayout.EndHorizontal();
    }
}

// ── Основной класс ────────────────────────────────────────────────────────────

public static class BuildAndPush
{
    const string BUTLER   = @"C:\Users\Администратор\Documents\Butler\butler.exe";
    const string GAME     = "ypikaeigames/i-vs-blocks";
    const string WIN_EXE  = "Build/Windows/i-vs-blocks.exe";
    const string WEBGL    = "Build/WebGL";
    const string VER_FILE = "Build/version.txt";
    const string LOG_FILE = "Build/CHANGELOG.md";

    // ── Меню ─────────────────────────────────────────────────────────────────

    [MenuItem("Tools/Build/Windows + WebGL")]
    static void AllPlatforms() => OpenDialog(true, true);

    [MenuItem("Tools/Build/Windows only")]
    static void WindowsOnly() => OpenDialog(true, false);

    [MenuItem("Tools/Build/WebGL only")]
    static void WebGLOnly() => OpenDialog(false, true);

    // ── Диалог ───────────────────────────────────────────────────────────────

    static void OpenDialog(bool win, bool web)
    {
        ChangelogWindow.Show("Build", changelog =>
        {
            string version = BumpVersion();
            Debug.Log($"[Build] Версия: {version}");

            if (win && !DoBuild(true,  version)) return;
            if (web && !DoBuild(false, version)) return;

            AppendChangelog(changelog, version);
            Debug.Log("[Build] Готово! Теперь git push → хук сам запушит на itch.io");
        });
    }

    // ── Билд ─────────────────────────────────────────────────────────────────

    static bool DoBuild(bool windows, string version)
    {
        PlayerSettings.bundleVersion = version;
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes            = GetScenes(),
            locationPathName  = windows ? WIN_EXE : WEBGL,
            target            = windows ? BuildTarget.StandaloneWindows64 : BuildTarget.WebGL,
            options           = BuildOptions.None
        });
        bool ok = report.summary.result == BuildResult.Succeeded;
        string name = windows ? "Windows" : "WebGL";
        if (ok) Debug.Log($"[Build] {name} OK — v{version}");
        else    Debug.LogError($"[Build] {name} FAILED");
        return ok;
    }

    // ── Версия ────────────────────────────────────────────────────────────────

    static string BumpVersion()
    {
        Directory.CreateDirectory("Build");
        int n = 0;
        if (File.Exists(VER_FILE)) int.TryParse(File.ReadAllText(VER_FILE).Trim(), out n);
        n++;
        File.WriteAllText(VER_FILE, n.ToString());
        string v = $"0.{n}";
        PlayerSettings.bundleVersion = v;
        return v;
    }

    // ── Changelog ─────────────────────────────────────────────────────────────

    static void AppendChangelog(string text, string version)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        Directory.CreateDirectory("Build");
        File.AppendAllText(LOG_FILE,
            $"\n## {version} — {DateTime.Now:yyyy-MM-dd}\n{text.Trim()}\n");
        Debug.Log("[Build] Changelog обновлён.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static string[] GetScenes()
    {
        var list = new List<string>();
        foreach (var s in EditorBuildSettings.scenes)
            if (s.enabled) list.Add(s.path);
        return list.ToArray();
    }

}
