using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

// ── Окно с changelog ──────────────────────────────────────────────────────────

public class ChangelogWindow : EditorWindow
{
    private string _text = "";
    private bool _doGit = true;
    private Action<string, bool> _onConfirm;

    public static void Show(string title, Action<string, bool> onConfirm)
    {
        var w = GetWindow<ChangelogWindow>(true, title, true);
        w._onConfirm = onConfirm;
        w._text = "";
        w.minSize = new Vector2(440, 210);
        w.maxSize = new Vector2(440, 210);
    }

    void OnGUI()
    {
        GUILayout.Label("Что изменилось в этой версии?", EditorStyles.boldLabel);
        _text = EditorGUILayout.TextArea(_text, GUILayout.Height(100));
        GUILayout.Space(6);
        _doGit = GUILayout.Toggle(_doGit, "Git commit");
        GUILayout.Space(6);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Build & Push")) { _onConfirm?.Invoke(_text, _doGit); Close(); }
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

    [MenuItem("Tools/Build & Push/Windows + WebGL")]
    static void AllPlatforms() => OpenDialog(true, true);

    [MenuItem("Tools/Build & Push/Windows only")]
    static void WindowsOnly() => OpenDialog(true, false);

    [MenuItem("Tools/Build & Push/WebGL only")]
    static void WebGLOnly() => OpenDialog(false, true);

    [MenuItem("Tools/Build & Push/Push without rebuild")]
    static void PushOnly()
    {
        string winDir  = AbsDir(WIN_EXE);
        string webDir  = Path.GetFullPath(WEBGL);
        bool   hasWin  = Directory.Exists(winDir)  && Directory.GetFiles(winDir).Length > 0;
        bool   hasWeb  = Directory.Exists(webDir)  && Directory.GetFiles(webDir).Length > 0;

        if (!hasWin && !hasWeb)
        {
            Debug.LogError("[Build] Нет готовых билдов. Сначала сделай Windows only или Windows + WebGL.");
            return;
        }

        if (hasWin) RunButler($"push \"{winDir}\" {GAME}:windows");
        if (hasWeb) RunButler($"push \"{webDir}\" {GAME}:html5");
    }

    // ── Диалог ───────────────────────────────────────────────────────────────

    static void OpenDialog(bool win, bool web)
    {
        ChangelogWindow.Show("Build & Push — itch.io", (changelog, doGit) =>
        {
            string version = BumpVersion();
            Debug.Log($"[Build] Версия: {version}");

            if (doGit) GitCommit(changelog, version);

            if (win && !DoBuild(true,  version)) return;
            if (web && !DoBuild(false, version)) return;

            if (win) RunButler($"push \"{AbsDir(WIN_EXE)}\" {GAME}:windows --userversion {version}");
            if (web) RunButler($"push \"{Path.GetFullPath(WEBGL)}\" {GAME}:html5 --userversion {version}");

            AppendChangelog(changelog, version);
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

    // ── Butler с прогресс-баром ───────────────────────────────────────────────

    static void RunButler(string args)
    {
        EditorUtility.DisplayProgressBar("Pushing to itch.io", "Загружаем...", 0.1f);
        try
        {
            var output = new StringBuilder();
            float progress = 0.1f;

            var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName               = BUTLER,
                    Arguments              = args,
                    UseShellExecute        = false,
                    CreateNoWindow         = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true
                }
            };

            p.OutputDataReceived += (_, e) =>
            {
                if (e.Data == null) return;
                output.AppendLine(e.Data);
                var m = Regex.Match(e.Data, @"(\d+(?:\.\d+)?)\s*%");
                if (m.Success && float.TryParse(m.Groups[1].Value,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out float pct))
                    progress = Mathf.Clamp01(pct / 100f);
            };
            p.ErrorDataReceived += (_, e) => { if (e.Data != null) output.AppendLine(e.Data); };

            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            while (!p.WaitForExit(400))
                EditorUtility.DisplayProgressBar("Pushing to itch.io", "Загружаем...", progress);

            EditorUtility.ClearProgressBar();

            if (p.ExitCode == 0)
                Debug.Log("[Butler] Push OK\n" + output);
            else
                Debug.LogError("[Butler] Push FAILED\n" + output);
        }
        catch (Exception ex)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError("[Butler] Ошибка: " + ex.Message);
        }
    }

    // ── Git ───────────────────────────────────────────────────────────────────

    static void GitCommit(string changelog, string version)
    {
        Git("add -A");
        string msg = string.IsNullOrWhiteSpace(changelog)
            ? $"Build {version}"
            : $"Build {version}: {changelog.Split('\n')[0].Trim()}";
        Git($"commit -m \"{msg}\"");
    }

    static void Git(string args)
    {
        var p = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName               = "git",
                Arguments              = args,
                UseShellExecute        = false,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                CreateNoWindow         = true,
                WorkingDirectory       = Directory.GetCurrentDirectory()
            }
        };
        p.Start();
        string o = p.StandardOutput.ReadToEnd();
        string e = p.StandardError.ReadToEnd();
        p.WaitForExit();
        if (!string.IsNullOrEmpty(o)) Debug.Log($"[git] {args}: {o.Trim()}");
        if (!string.IsNullOrEmpty(e) && p.ExitCode != 0) Debug.LogWarning($"[git] {args}: {e.Trim()}");
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

    static string AbsDir(string exePath) =>
        Path.GetFullPath(Path.GetDirectoryName(exePath));
}
