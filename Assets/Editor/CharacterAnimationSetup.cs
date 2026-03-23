#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public class CharacterAnimSetupWindow : EditorWindow
{
    int _ppu = 18;

    [MenuItem("Tools/Setup Character Animations")]
    static void OpenWindow()
    {
        var w = GetWindow<CharacterAnimSetupWindow>("Character Anim Setup");
        w._ppu = EditorPrefs.GetInt("CharAnim_PPU", 18);
        w.minSize = new Vector2(260, 100);
    }

    void OnGUI()
    {
        GUILayout.Label("Настройки анимации персонажа", EditorStyles.boldLabel);
        _ppu = EditorGUILayout.IntField("PPU (размер спрайтов)", _ppu);
        EditorGUILayout.HelpBox("Меньше = крупнее. Обычно 16–32.", MessageType.Info);

        if (GUILayout.Button("Применить и пересоздать анимации"))
        {
            EditorPrefs.SetInt("CharAnim_PPU", _ppu);
            CharacterAnimationSetup.Run(_ppu);
            Close();
        }
    }
}

public static class CharacterAnimationSetup
{
    const string SpritesBase = "Assets/Sprits/2D-Pixel-Art-Character-Template";
    const string OutputPath  = "Assets/Animations/Character";

    struct ClipConfig
    {
        public string folder, file;
        public int cellSize, ppu, fps;
        public bool loop;
        public Vector2 pivot;
    }

    public static void Run(int targetPPU)
    {
        Directory.CreateDirectory(OutputPath);
        AssetDatabase.Refresh();

        var configs = new ClipConfig[]
        {
            new ClipConfig { folder = "Idle",       file = "Player Idle 48x48.png",           cellSize = 48, ppu = targetPPU, fps = 8,  loop = true,  pivot = new Vector2(0.5f, 0f)    },
            new ClipConfig { folder = "Run",        file = "player run 48x48.png",            cellSize = 48, ppu = targetPPU, fps = 12, loop = true,  pivot = new Vector2(0.5f, 0f)    },
            new ClipConfig { folder = "Jump",       file = "player new jump 48x48.png",       cellSize = 48, ppu = targetPPU, fps = 10, loop = false, pivot = new Vector2(0.5f, 0f)    },
            new ClipConfig { folder = "Land",       file = "player land 48x48.png",           cellSize = 48, ppu = targetPPU, fps = 12, loop = false, pivot = new Vector2(0.5f, 0f)    },
            new ClipConfig { folder = "Push",       file = "player push 48x48.png",           cellSize = 48, ppu = targetPPU, fps = 10, loop = true,  pivot = new Vector2(0.5f, 0f)    },
            new ClipConfig { folder = "Death",      file = "Player Death 64x64.png",          cellSize = 64, ppu = targetPPU, fps = 8,  loop = false, pivot = new Vector2(0.5f, 0f)    },
            new ClipConfig { folder = "Wall Slide", file = "player wall slide 48x48.png",     cellSize = 48, ppu = targetPPU, fps = 8,  loop = true,  pivot = new Vector2(0.5f, 0f)    },
            new ClipConfig { folder = "Air Spin",   file = "player air spin 48x48.png",       cellSize = 48, ppu = targetPPU, fps = 10, loop = false, pivot = new Vector2(0.5f, 0f)    },
            new ClipConfig { folder = "Punch",      file = "Player Punch 64x64.png",          cellSize = 64, ppu = targetPPU, fps = 10, loop = false, pivot = new Vector2(0.5f, 0.15f) },
        };

        // Шаг 1: нарезаем все спрайтшиты
        foreach (var cfg in configs)
        {
            string path = $"{SpritesBase}/{cfg.folder}/{cfg.file}";
            if (!File.Exists(path)) { Debug.LogWarning($"[CharAnim] Не найден: {path}"); continue; }
            if (cfg.file.EndsWith(".aseprite"))
                SetAsepritePPU(path, cfg.ppu);
            else
                SliceSpriteSheet(path, cfg.cellSize, cfg.ppu, cfg.pivot);
        }

        // Шаг 2: обновляем базу данных чтобы подгрузились нарезанные спрайты
        AssetDatabase.Refresh();

        // Шаг 3: создаём клипы
        var clips = new Dictionary<string, AnimationClip>();
        foreach (var cfg in configs)
        {
            string path = $"{SpritesBase}/{cfg.folder}/{cfg.file}";
            if (!File.Exists(path)) continue;

            string clipName = cfg.folder == "Punch" ? "Destroy" : cfg.folder;
            var clip = CreateClip(path, cfg.fps, cfg.loop, clipName);
            if (clip != null) clips[clipName] = clip;
        }

        // Шаг 4: создаём Animator Controller
        CreateAnimatorController(clips);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CharAnim] Готово! Смотри Assets/Animations/Character/");
    }

    // ── Нарезка через новый API ──────────────────────────────────────────

    static void SetAsepritePPU(string assetPath, int ppu)
    {
        // Для aseprite меняем только PPU через TextureImporter settings в meta
        var importer = AssetImporter.GetAtPath(assetPath);
        if (importer == null) return;
        var so = new UnityEditor.SerializedObject(importer);
        var ppuProp = so.FindProperty("textureImporterSettings.spritePixelsToUnits");
        if (ppuProp != null)
        {
            ppuProp.floatValue = ppu;
            so.ApplyModifiedPropertiesWithoutUndo();
            importer.SaveAndReimport();
            Debug.Log($"[CharAnim] Aseprite PPU={ppu}: {assetPath}");
        }
        else
        {
            Debug.LogWarning($"[CharAnim] Не удалось найти PPU поле для {assetPath}");
        }
    }

    static void SliceSpriteSheet(string assetPath, int cellSize, int ppu, Vector2 pivot)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) return;

        importer.textureType         = TextureImporterType.Sprite;
        importer.spriteImportMode    = SpriteImportMode.Multiple;
        importer.filterMode          = FilterMode.Point;
        importer.textureCompression  = TextureImporterCompression.Uncompressed;
        importer.spritePixelsPerUnit = ppu;
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
                    pivot     = pivot,
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
        controller.AddParameter("IsWallSliding",   AnimatorControllerParameterType.Bool);
        controller.AddParameter("Destroy",     AnimatorControllerParameterType.Trigger);
        controller.AddParameter("DestroyDown", AnimatorControllerParameterType.Trigger);

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

        // AnyState → Land для удара снизу (Trigger)
        if (states.ContainsKey("Land"))
        {
            var t = sm.AddAnyStateTransition(states["Land"]);
            t.AddCondition(AnimatorConditionMode.If, 0, "DestroyDown");
            t.hasExitTime = false; t.duration = 0; t.canTransitionToSelf = false;
        }

        // AnyState → Destroy для удара по бокам (Trigger)
        if (states.ContainsKey("Destroy"))
        {
            var t = sm.AddAnyStateTransition(states["Destroy"]);
            t.AddCondition(AnimatorConditionMode.If, 0, "Destroy");
            t.hasExitTime = false; t.duration = 0; t.canTransitionToSelf = false;
            var idleState = states.ContainsKey("Idle") ? states["Idle"] : sm.defaultState;
            var t2 = states["Destroy"].AddTransition(idleState);
            t2.hasExitTime = true; t2.exitTime = 1f; t2.duration = 0;
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
