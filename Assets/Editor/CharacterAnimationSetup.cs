#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public static class CharacterAnimationSetup
{
    const string SpritesBase = "Assets/Sprits/2D-Pixel-Art-Character-Template";
    const string OutputPath  = "Assets/Animations/Character";

    struct ClipConfig
    {
        public string folder, file;
        public int cellSize, fps;
        public bool loop;
    }

    [MenuItem("Tools/Set Character Sprites PPU = 20")]
    static void SetPPU()
    {
        var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { SpritesBase });
        int count = 0;
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.EndsWith(".png")) continue;

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            importer.spritePixelsPerUnit = 20;
            importer.SaveAndReimport();
            count++;
        }
        AssetDatabase.Refresh();
        Debug.Log($"[CharAnim] PPU=20 применён к {count} текстурам.");
    }

    [MenuItem("Tools/Setup Character Animations")]
    static void Setup()
    {
        Directory.CreateDirectory(OutputPath);
        AssetDatabase.Refresh();

        var configs = new ClipConfig[]
        {
            new ClipConfig { folder = "Idle",  file = "Player Idle 48x48.png",     cellSize = 48, fps = 8,  loop = true  },
            new ClipConfig { folder = "Run",   file = "player run 48x48.png",      cellSize = 48, fps = 12, loop = true  },
            new ClipConfig { folder = "Jump",  file = "player new jump 48x48.png", cellSize = 48, fps = 10, loop = false },
            new ClipConfig { folder = "Land",  file = "player land 48x48.png",     cellSize = 48, fps = 12, loop = false },
            new ClipConfig { folder = "Push",      file = "player push 48x48.png",         cellSize = 48, fps = 10, loop = true  },
            new ClipConfig { folder = "Death",     file = "Player Death 64x64.png",        cellSize = 48, fps = 8,  loop = false },
            new ClipConfig { folder = "Wall Slide",file = "player wall slide 48x48.png",   cellSize = 48, fps = 8,  loop = true  },
            new ClipConfig { folder = "Air Spin",  file = "player air spin 48x48.png",     cellSize = 48, fps = 10, loop = false },
        };

        // Шаг 1: нарезаем все спрайтшиты
        foreach (var cfg in configs)
        {
            string path = $"{SpritesBase}/{cfg.folder}/{cfg.file}";
            if (!File.Exists(path)) { Debug.LogWarning($"[CharAnim] Не найден: {path}"); continue; }
            SliceSpriteSheet(path, cfg.cellSize);
        }

        // Шаг 2: обновляем базу данных чтобы подгрузились нарезанные спрайты
        AssetDatabase.Refresh();

        // Шаг 3: создаём клипы
        var clips = new Dictionary<string, AnimationClip>();
        foreach (var cfg in configs)
        {
            string path = $"{SpritesBase}/{cfg.folder}/{cfg.file}";
            if (!File.Exists(path)) continue;

            var clip = CreateClip(path, cfg.fps, cfg.loop, cfg.folder);
            if (clip != null) clips[cfg.folder] = clip;
        }

        // Шаг 4: создаём Animator Controller
        CreateAnimatorController(clips);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CharAnim] Готово! Смотри Assets/Animations/Character/");
    }

    // ── Нарезка через новый API ──────────────────────────────────────────

    static void SliceSpriteSheet(string assetPath, int cellSize)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) return;

        importer.textureType         = TextureImporterType.Sprite;
        importer.spriteImportMode    = SpriteImportMode.Multiple;
        importer.filterMode          = FilterMode.Point;
        importer.textureCompression  = TextureImporterCompression.Uncompressed;
        importer.spritePixelsPerUnit = cellSize;
        importer.maxTextureSize      = 2048;
        importer.npotScale           = TextureImporterNPOTScale.None; // не масштабировать текстуру
        importer.SaveAndReimport();

        // Загружаем текстуру ПОСЛЕ reimport чтобы получить реальный размер
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        if (tex == null) { Debug.LogWarning($"[CharAnim] Не удалось загрузить: {assetPath}"); return; }
        Debug.Log($"[CharAnim] {Path.GetFileName(assetPath)}: {tex.width}x{tex.height}, cellSize={cellSize}");

        int cols     = Mathf.Max(1, tex.width  / cellSize);
        int rows     = Mathf.Max(1, tex.height / cellSize);
        string bName = Path.GetFileNameWithoutExtension(assetPath);

        // Новый API нарезки
        var factory = new SpriteDataProviderFactories();
        factory.Init();
        var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
        dataProvider.InitSpriteEditorDataProvider();

        var rects = new SpriteRect[cols * rows];
        int idx = 0;
        for (int row = rows - 1; row >= 0; row--)
        {
            for (int col = 0; col < cols; col++)
            {
                rects[idx] = new SpriteRect
                {
                    name      = $"{bName}_{idx}",
                    rect      = new Rect(col * cellSize, row * cellSize, cellSize, cellSize),
                    pivot     = new Vector2(0.5f, 0f),
                    alignment = SpriteAlignment.Custom,
                    spriteID  = GUID.Generate()
                };
                idx++;
            }
        }

        dataProvider.SetSpriteRects(rects);
        dataProvider.Apply();
        (dataProvider.targetObject as AssetImporter)?.SaveAndReimport();
    }

    // ── Создание AnimationClip ───────────────────────────────────────────

    static AnimationClip CreateClip(string spritePath, int fps, bool loop, string clipName)
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath(spritePath)
            .OfType<Sprite>()
            .OrderBy(s =>
            {
                var parts = s.name.Split('_');
                return int.TryParse(parts[parts.Length - 1], out int n) ? n : 0;
            })
            .ToArray();

        if (sprites.Length == 0)
        {
            Debug.LogWarning($"[CharAnim] Нет спрайтов в {spritePath}");
            return null;
        }

        var clip = new AnimationClip { frameRate = fps };

        if (loop)
        {
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
        }

        var keyframes = new ObjectReferenceKeyframe[sprites.Length + 1];
        for (int i = 0; i < sprites.Length; i++)
            keyframes[i] = new ObjectReferenceKeyframe { time = i / (float)fps, value = sprites[i] };
        keyframes[sprites.Length] = new ObjectReferenceKeyframe
            { time = sprites.Length / (float)fps, value = sprites[sprites.Length - 1] };

        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        string clipPath = $"{OutputPath}/{clipName}.anim";
        AssetDatabase.CreateAsset(clip, clipPath);
        return clip;
    }

    // ── Animator Controller ──────────────────────────────────────────────

    static void CreateAnimatorController(Dictionary<string, AnimationClip> clips)
    {
        string path       = $"{OutputPath}/CharacterAnimator.controller";
        var    controller = AnimatorController.CreateAnimatorControllerAtPath(path);

        controller.AddParameter("Speed",          AnimatorControllerParameterType.Float);
        controller.AddParameter("IsGrounded",     AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsPushing",      AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsDead",         AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsWallSliding",  AnimatorControllerParameterType.Bool);

        var sm     = controller.layers[0].stateMachine;
        var states = new Dictionary<string, AnimatorState>();

        foreach (var kvp in clips)
        {
            var state  = sm.AddState(kvp.Key);
            state.motion = kvp.Value;
            states[kvp.Key] = state;
        }

        if (states.ContainsKey("Idle")) sm.defaultState = states["Idle"];

        Tr(states, "Idle", "Run",  t => t.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed"));
        Tr(states, "Run",  "Idle", t => t.AddCondition(AnimatorConditionMode.Less,    0.1f, "Speed"));

        foreach (string s in new[] { "Idle", "Run" })
            Tr(states, s, "Jump", t => t.AddCondition(AnimatorConditionMode.IfNot, 0, "IsGrounded"));

        if (states.ContainsKey("Land"))
        {
            Tr(states, "Jump", "Land", t => t.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded"));
            if (states.ContainsKey("Idle"))
            {
                var t2 = states["Land"].AddTransition(states["Idle"]);
                t2.hasExitTime = true; t2.exitTime = 1f; t2.duration = 0;
            }
        }
        else
        {
            Tr(states, "Jump", "Idle", t => t.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded"));
        }

        if (states.ContainsKey("Push"))
        {
            var t = sm.AddAnyStateTransition(states["Push"]);
            t.AddCondition(AnimatorConditionMode.If, 0, "IsPushing");
            t.hasExitTime = false; t.duration = 0; t.canTransitionToSelf = false;
            Tr(states, "Push", "Idle", t2 => t2.AddCondition(AnimatorConditionMode.IfNot, 0, "IsPushing"));
        }

        if (states.ContainsKey("Death"))
        {
            var t = sm.AddAnyStateTransition(states["Death"]);
            t.AddCondition(AnimatorConditionMode.If, 0, "IsDead");
            t.hasExitTime = false; t.duration = 0; t.canTransitionToSelf = false;
        }

        // WallSlide — из Jump и из воздуха
        if (states.ContainsKey("Wall Slide"))
        {
            var t = sm.AddAnyStateTransition(states["Wall Slide"]);
            t.AddCondition(AnimatorConditionMode.If, 0, "IsWallSliding");
            t.hasExitTime = false; t.duration = 0; t.canTransitionToSelf = false;

            // Wall Slide → WallJump (Air Spin)
            if (states.ContainsKey("Air Spin"))
            {
                Tr(states, "Wall Slide", "Air Spin", t2 => {
                    t2.AddCondition(AnimatorConditionMode.IfNot, 0, "IsWallSliding");
                    t2.AddCondition(AnimatorConditionMode.IfNot, 0, "IsGrounded");
                });
            }

            // Wall Slide → Idle (приземлился)
            Tr(states, "Wall Slide", "Idle", t2 => {
                t2.AddCondition(AnimatorConditionMode.IfNot, 0, "IsWallSliding");
                t2.AddCondition(AnimatorConditionMode.If,    0, "IsGrounded");
            });
        }

        // Air Spin → Jump/Idle
        if (states.ContainsKey("Air Spin"))
        {
            Tr(states, "Air Spin", "Idle", t2 =>
                t2.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded"));
        }

        EditorUtility.SetDirty(controller);
    }

    static void Tr(Dictionary<string, AnimatorState> states, string from, string to,
        System.Action<AnimatorStateTransition> setup)
    {
        if (!states.ContainsKey(from) || !states.ContainsKey(to)) return;
        var t = states[from].AddTransition(states[to]);
        t.hasExitTime = false;
        t.duration    = 0;
        setup(t);
    }
}
#endif
