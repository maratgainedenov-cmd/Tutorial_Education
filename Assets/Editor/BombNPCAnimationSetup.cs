#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class BombNPCAnimationSetup
{
    const string SpritePath    = "Assets/Sprits/bomb_character (2).aseprite";
    const string OutputPath    = "Assets/Animations/BombNPC";
    const string PrefabPath    = "Assets/Resources/BombNpc.prefab";
    const string VisualChild   = "Visual";

    const int WalkStart = 0;  const int WalkEnd = 9;
    const int JumpStart = 10; const int JumpEnd = 10;
    const int FuseStart = 10; const int FuseEnd = 10;
    const int ExplodeStart = 11; const int ExplodeEnd = 12;
    const int WalkFPS = 10, JumpFPS = 8, FuseFPS = 8, ExplodeFPS = 8;

    [MenuItem("Tools/Setup BombNPC Animations")]
    static void Setup()
    {
        if (!File.Exists(SpritePath))
        {
            Debug.LogError($"[BombNPCAnim] Файл не найден: {SpritePath}");
            return;
        }

        Directory.CreateDirectory(OutputPath);
        AssetDatabase.Refresh();

        var allSprites = AssetDatabase.LoadAllAssetsAtPath(SpritePath)
            .OfType<Sprite>()
            .OrderBy(s => {
                var p = s.name.Split('_');
                return int.TryParse(p[p.Length - 1], out int n) ? n : 0;
            })
            .ToArray();

        if (allSprites.Length == 0)
        {
            Debug.LogError("[BombNPCAnim] Спрайты не найдены.");
            return;
        }
        Debug.Log($"[BombNPCAnim] Спрайтов: {allSprites.Length}");

        var walk    = GetOrUpdateClip("Walk",    allSprites, WalkStart,    WalkEnd,    WalkFPS,    true);
        var jump    = GetOrUpdateClip("Jump",    allSprites, JumpStart,    JumpEnd,    JumpFPS,    true);
        var fuse    = GetOrUpdateClip("Fuse",    allSprites, FuseStart,    FuseEnd,    FuseFPS,    true);
        var explode = GetOrUpdateClip("Explode", allSprites, ExplodeStart, ExplodeEnd, ExplodeFPS, false);

        var controller = RebuildController(walk, jump, fuse, explode);

        AssignControllerToPrefab(controller);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[BombNPCAnim] Готово!");
    }

    static AnimationClip GetOrUpdateClip(string name, Sprite[] all, int from, int to, int fps, bool loop)
    {
        to = Mathf.Min(to, all.Length - 1);
        if (from > to) return null;

        string path = $"{OutputPath}/{name}.anim";
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip == null)
        {
            clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, path);
        }

        clip.ClearCurves();
        clip.frameRate = fps;

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        var sprites = all.Skip(from).Take(to - from + 1).ToArray();
        var keys = new ObjectReferenceKeyframe[sprites.Length + 1];
        for (int i = 0; i < sprites.Length; i++)
            keys[i] = new ObjectReferenceKeyframe { time = i / (float)fps, value = sprites[i] };
        keys[sprites.Length] = new ObjectReferenceKeyframe
            { time = sprites.Length / (float)fps, value = sprites[sprites.Length - 1] };

        // Путь "" — Animator находится на том же объекте что и SpriteRenderer (Visual)
        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        EditorUtility.SetDirty(clip);
        return clip;
    }

    static AnimatorController RebuildController(AnimationClip walk, AnimationClip jump,
                                                 AnimationClip fuse, AnimationClip explode)
    {
        string path = $"{OutputPath}/BombNPCAnimator.controller";
        var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(path)
                ?? AnimatorController.CreateAnimatorControllerAtPath(path);

        while (ctrl.parameters.Length > 0) ctrl.RemoveParameter(0);

        var sm = ctrl.layers[0].stateMachine;
        foreach (var s in sm.states.ToArray())       sm.RemoveState(s.state);
        foreach (var t in sm.anyStateTransitions.ToArray()) sm.RemoveAnyStateTransition(t);

        ctrl.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("IsFuse",     AnimatorControllerParameterType.Bool);
        ctrl.AddParameter("Explode",    AnimatorControllerParameterType.Trigger);

        var walkSt    = walk    != null ? sm.AddState("Walk",    new Vector3(200,   0)) : null;
        var jumpSt    = jump    != null ? sm.AddState("Jump",    new Vector3(400,   0)) : null;
        var fuseSt    = fuse    != null ? sm.AddState("Fuse",    new Vector3(200, 130)) : null;
        var explodeSt = explode != null ? sm.AddState("Explode", new Vector3(400, 130)) : null;

        if (walkSt    != null) walkSt.motion    = walk;
        if (jumpSt    != null) jumpSt.motion    = jump;
        if (fuseSt    != null) fuseSt.motion    = fuse;
        if (explodeSt != null) explodeSt.motion = explode;
        if (walkSt    != null) sm.defaultState  = walkSt;

        if (walkSt != null && jumpSt != null)
        {
            Transition(walkSt, jumpSt, AnimatorConditionMode.IfNot, "IsGrounded");
            Transition(jumpSt, walkSt, AnimatorConditionMode.If,    "IsGrounded");
        }
        if (fuseSt != null)
        {
            var t = sm.AddAnyStateTransition(fuseSt);
            t.AddCondition(AnimatorConditionMode.If, 0, "IsFuse");
            t.hasExitTime = false; t.duration = 0; t.canTransitionToSelf = false;
            if (walkSt != null)
                Transition(fuseSt, walkSt, AnimatorConditionMode.IfNot, "IsFuse");
        }
        if (explodeSt != null)
        {
            var t = sm.AddAnyStateTransition(explodeSt);
            t.AddCondition(AnimatorConditionMode.If, 0, "Explode");
            t.hasExitTime = false; t.duration = 0; t.canTransitionToSelf = false;
        }

        EditorUtility.SetDirty(ctrl);
        return ctrl;
    }

    // Прописывает контроллер во все prefab-ы у которых есть компонент BombNPC
    static void AssignControllerToPrefab(AnimatorController controller)
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        int count = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null) continue;

            var bombNpc = go.GetComponent<BombNPC>();
            if (bombNpc == null) continue;

            // Поле _animatorController на BombNPC (через SerializedObject чтобы не ломать prefab)
            var so = new SerializedObject(bombNpc);
            var prop = so.FindProperty("_animatorController");
            if (prop != null)
            {
                prop.objectReferenceValue = controller;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            // Поле Controller на Animator (Visual child)
            var visual = go.transform.Find(VisualChild);
            if (visual != null)
            {
                var animator = visual.GetComponent<Animator>();
                if (animator != null)
                {
                    var aso = new SerializedObject(animator);
                    var cprop = aso.FindProperty("m_Controller");
                    if (cprop != null)
                    {
                        cprop.objectReferenceValue = controller;
                        aso.ApplyModifiedPropertiesWithoutUndo();
                    }
                }
            }

            EditorUtility.SetDirty(go);
            PrefabUtility.SavePrefabAsset(go);
            count++;
            Debug.Log($"[BombNPCAnim] Контроллер прописан: {path}");
        }
        Debug.Log($"[BombNPCAnim] Обновлено префабов: {count}");
    }

    static void Transition(AnimatorState from, AnimatorState to,
                            AnimatorConditionMode mode, string param)
    {
        var t = from.AddTransition(to);
        t.AddCondition(mode, 0, param);
        t.hasExitTime = false;
        t.duration    = 0;
    }
}
#endif
