#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class BombNPCAnimationSetup
{
    const string AssetPath  = "Assets/Sprits/bomb_character (2).aseprite";
    const string OutputPath = "Assets/Animations/BombNPC";

    // Фреймы из aseprite (Frame_0 .. Frame_12)
    // 0-9  — ходьба (10 фреймов)
    // 10   — приземление / idle стоп
    // 11   — взрыв большой (51x51)
    // 12   — взрыв малый  (42x42)
    const int WalkStart    = 0;  const int WalkEnd    = 9;
    const int JumpStart    = 10; const int JumpEnd    = 10; // поза в воздухе
    const int FuseStart    = 10; const int FuseEnd    = 10; // стоит, фитиль горит
    const int ExplodeStart = 11; const int ExplodeEnd = 12;
    const int WalkFPS    = 10;
    const int JumpFPS    = 8;
    const int FuseFPS    = 8;
    const int ExplodeFPS = 8;

    [MenuItem("Tools/Setup BombNPC Animations")]
    static void Setup()
    {
        if (!File.Exists(AssetPath))
        {
            Debug.LogError($"[BombNPCAnim] Файл не найден: {AssetPath}");
            return;
        }

        Directory.CreateDirectory(OutputPath);
        AssetDatabase.Refresh();

        // Загружаем все спрайты из aseprite
        var allSprites = AssetDatabase.LoadAllAssetsAtPath(AssetPath)
            .OfType<Sprite>()
            .OrderBy(s =>
            {
                var parts = s.name.Split('_');
                return int.TryParse(parts[parts.Length - 1], out int n) ? n : 0;
            })
            .ToArray();

        if (allSprites.Length == 0)
        {
            Debug.LogError("[BombNPCAnim] Спрайты не найдены. Убедись что aseprite импортирован корректно.");
            return;
        }

        Debug.Log($"[BombNPCAnim] Найдено {allSprites.Length} спрайтов");

        var walkClip    = CreateClip("Walk",    allSprites, WalkStart,    WalkEnd,    WalkFPS,    loop: true);
        var jumpClip    = CreateClip("Jump",    allSprites, JumpStart,    JumpEnd,    JumpFPS,    loop: true);
        var fuseClip    = CreateClip("Fuse",    allSprites, FuseStart,    FuseEnd,    FuseFPS,    loop: true);
        var explodeClip = CreateClip("Explode", allSprites, ExplodeStart, ExplodeEnd, ExplodeFPS, loop: false);

        CreateAnimatorController(walkClip, jumpClip, fuseClip, explodeClip);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[BombNPCAnim] Готово! Assets/Animations/BombNPC/");
    }

    static AnimationClip CreateClip(string name, Sprite[] all, int from, int to, int fps, bool loop)
    {
        to = Mathf.Min(to, all.Length - 1);
        if (from > to) { Debug.LogWarning($"[BombNPCAnim] Пустой диапазон для {name}"); return null; }

        var sprites = all.Skip(from).Take(to - from + 1).ToArray();

        var clip = new AnimationClip { frameRate = fps };

        if (loop)
        {
            var s = AnimationUtility.GetAnimationClipSettings(clip);
            s.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, s);
        }

        var keyframes = new ObjectReferenceKeyframe[sprites.Length + 1];
        for (int i = 0; i < sprites.Length; i++)
            keyframes[i] = new ObjectReferenceKeyframe { time = i / (float)fps, value = sprites[i] };
        keyframes[sprites.Length] = new ObjectReferenceKeyframe
            { time = sprites.Length / (float)fps, value = sprites[sprites.Length - 1] };

        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        string path = $"{OutputPath}/{name}.anim";
        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    static void CreateAnimatorController(AnimationClip walk, AnimationClip jump, AnimationClip fuse, AnimationClip explode)
    {
        string path       = $"{OutputPath}/BombNPCAnimator.controller";
        var    controller = AnimatorController.CreateAnimatorControllerAtPath(path);

        controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsFuse",     AnimatorControllerParameterType.Bool);
        controller.AddParameter("Explode",    AnimatorControllerParameterType.Trigger);

        var sm = controller.layers[0].stateMachine;

        AnimatorState walkState    = walk    != null ? sm.AddState("Walk")    : null;
        AnimatorState jumpState    = jump    != null ? sm.AddState("Jump")    : null;
        AnimatorState fuseState    = fuse    != null ? sm.AddState("Fuse")    : null;
        AnimatorState explodeState = explode != null ? sm.AddState("Explode") : null;

        if (walkState    != null) walkState.motion    = walk;
        if (jumpState    != null) jumpState.motion    = jump;
        if (fuseState    != null) fuseState.motion    = fuse;
        if (explodeState != null) explodeState.motion = explode;

        if (walkState != null) sm.defaultState = walkState;

        // Walk ↔ Jump по IsGrounded
        if (walkState != null && jumpState != null)
        {
            var t1 = walkState.AddTransition(jumpState);
            t1.AddCondition(AnimatorConditionMode.IfNot, 0, "IsGrounded");
            t1.hasExitTime = false; t1.duration = 0;

            var t2 = jumpState.AddTransition(walkState);
            t2.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");
            t2.hasExitTime = false; t2.duration = 0;
        }

        // AnyState → Fuse (фитиль горит)
        if (fuseState != null)
        {
            var t = sm.AddAnyStateTransition(fuseState);
            t.AddCondition(AnimatorConditionMode.If, 0, "IsFuse");
            t.hasExitTime = false; t.duration = 0; t.canTransitionToSelf = false;

            // Fuse → Walk (если фитиль сброшен — на случай пуша)
            if (walkState != null)
            {
                var t2 = fuseState.AddTransition(walkState);
                t2.AddCondition(AnimatorConditionMode.IfNot, 0, "IsFuse");
                t2.hasExitTime = false; t2.duration = 0;
            }
        }

        // AnyState → Explode (по триггеру)
        if (explodeState != null)
        {
            var t = sm.AddAnyStateTransition(explodeState);
            t.AddCondition(AnimatorConditionMode.If, 0, "Explode");
            t.hasExitTime = false; t.duration = 0; t.canTransitionToSelf = false;
        }

        EditorUtility.SetDirty(controller);
    }
}
#endif
