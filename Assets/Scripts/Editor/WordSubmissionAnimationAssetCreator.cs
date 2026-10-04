#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class WordSubmissionAnimationAssetCreator
{
    private const string RootFolder = "Assets/Animations/Word Submission";
    private const string ControllerPath = RootFolder + "/Word Submission.controller";
    private const string AttackClipPath = RootFolder + "/Attack Placeholder.anim";
    private const string HealClipPath = RootFolder + "/Heal Placeholder.anim";
    private const string HitClipPath = RootFolder + "/Hit Placeholder.anim";
    private const string LeftSidePrefabPath = "Assets/Prefabs/Side UI/Left Side Container Variant.prefab";
    private const string RightSidePrefabPath = "Assets/Prefabs/Side UI/Right Side Container Variant.prefab";

    [MenuItem("Tools/Baybaylan/Set Up Word Submission Animations")]
    public static void CreatePlaceholderAssets()
    {
        EnsureFolderExists();

        AnimationClip attackClip = GetOrCreateClip(AttackClipPath, "Attack Placeholder");
        AnimationClip healClip = GetOrCreateClip(HealClipPath, "Heal Placeholder");
        AnimationClip hitClip = GetOrCreateHitClip();

        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        }

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        AnimatorState idleState = EnsureState(stateMachine, "Idle", null);
        EnsureState(stateMachine, "Attack", attackClip);
        EnsureState(stateMachine, "Heal", healClip);
        EnsureState(stateMachine, "Hit", hitClip);
        if (stateMachine.defaultState == null) stateMachine.defaultState = idleState;

        ConfigureLeftSidePrefab(controller);
        ConfigureRightSidePrefab(controller);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);

        Debug.Log($"Word submission animations are set up. Placeholder assets are in {RootFolder}.");
    }

    private static AnimationClip GetOrCreateClip(string path, string clipName)
    {
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip != null) return clip;

        clip = new AnimationClip
        {
            name = clipName,
            frameRate = 60f,
            wrapMode = WrapMode.Once
        };

        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    private static AnimationClip GetOrCreateHitClip()
    {
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(HitClipPath);
        if (clip != null) return clip;

        clip = GetOrCreateClip(HitClipPath, "Hit Placeholder");
        ApplyDefaultHitCurves(clip);
        return clip;
    }

    private static AnimatorState EnsureState(AnimatorStateMachine stateMachine, string stateName, Motion motion)
    {
        foreach (ChildAnimatorState childState in stateMachine.states)
        {
            if (childState.state.name != stateName) continue;
            if (childState.state.motion == null && motion != null) childState.state.motion = motion;
            return childState.state;
        }

        AnimatorState state = stateMachine.AddState(stateName);
        state.motion = motion;
        return state;
    }

    private static void EnsureFolderExists()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Animations"))
            AssetDatabase.CreateFolder("Assets", "Animations");

        if (!AssetDatabase.IsValidFolder(RootFolder))
            AssetDatabase.CreateFolder("Assets/Animations", "Word Submission");
    }

    private static void ConfigureLeftSidePrefab(AnimatorController controller)
    {
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(LeftSidePrefabPath);

        try
        {
            if (prefabRoot.transform.Find("Portrait Holder") == null)
            {
                Debug.LogWarning($"Could not find Portrait Holder in {LeftSidePrefabPath}.");
                return;
            }

            Animator animator = prefabRoot.GetComponent<Animator>();
            if (animator == null) animator = prefabRoot.AddComponent<Animator>();

            string currentControllerPath = AssetDatabase.GetAssetPath(animator.runtimeAnimatorController);
            if (animator.runtimeAnimatorController == null ||
                currentControllerPath == ControllerPath)
            {
                animator.runtimeAnimatorController = controller;
            }
            else
            {
                Debug.LogWarning(
                    $"Kept the custom Animator Controller already assigned to {prefabRoot.name}. " +
                    $"Add Attack and Heal states to that controller or assign {ControllerPath} manually.");
            }

            WordSubmissionAnimationPlayer player = prefabRoot.GetComponent<WordSubmissionAnimationPlayer>();
            if (player == null) player = prefabRoot.AddComponent<WordSubmissionAnimationPlayer>();

            SerializedObject serializedPlayer = new SerializedObject(player);
            SerializedProperty animatorProperty = serializedPlayer.FindProperty("targetAnimator");
            animatorProperty.objectReferenceValue = animator;
            serializedPlayer.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, LeftSidePrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }

    private static void ConfigureRightSidePrefab(AnimatorController controller)
    {
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(RightSidePrefabPath);

        try
        {
            if (prefabRoot.transform.Find("Portrait Holder") == null)
            {
                Debug.LogWarning($"Could not find Portrait Holder in {RightSidePrefabPath}.");
                return;
            }

            Animator animator = prefabRoot.GetComponent<Animator>();
            if (animator == null) animator = prefabRoot.AddComponent<Animator>();

            string currentControllerPath = AssetDatabase.GetAssetPath(animator.runtimeAnimatorController);
            if (animator.runtimeAnimatorController == null || currentControllerPath == ControllerPath)
            {
                animator.runtimeAnimatorController = controller;
            }
            else
            {
                Debug.LogWarning(
                    $"Kept the custom Animator Controller already assigned to {prefabRoot.name}. " +
                    $"Add a Hit state to that controller or assign {ControllerPath} manually.");
            }

            StoryTargetAnimationPlayer player = prefabRoot.GetComponent<StoryTargetAnimationPlayer>();
            if (player == null) player = prefabRoot.AddComponent<StoryTargetAnimationPlayer>();

            SerializedObject serializedPlayer = new SerializedObject(player);
            serializedPlayer.FindProperty("targetAnimator").objectReferenceValue = animator;
            serializedPlayer.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, RightSidePrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }

    private static void ApplyDefaultHitCurves(AnimationClip clip)
    {
        float[] times = { 0f, 2f / 60f, 5f / 60f, 9f / 60f, 14f / 60f };

        SetCurve(clip, times, "m_AnchoredPosition.x", -20f, 18f, 30f, -28f, -20f);
        SetCurve(clip, times, "m_AnchoredPosition.y", -20f, -12f, -16f, -20f, -20f);
        SetCurve(clip, times, "localEulerAnglesRaw.x", 0f, 0f, 0f, 0f, 0f);
        SetCurve(clip, times, "localEulerAnglesRaw.y", 0f, 0f, 0f, 0f, 0f);
        SetCurve(clip, times, "localEulerAnglesRaw.z", 0f, 5f, 4f, -2f, 0f);
        SetCurve(clip, times, "m_LocalScale.x", 1f, 0.96f, 0.94f, 1.02f, 1f);
        SetCurve(clip, times, "m_LocalScale.y", 1f, 1.04f, 1.06f, 0.98f, 1f);
        SetCurve(clip, times, "m_LocalScale.z", 1f, 1f, 1f, 1f, 1f);

        clip.frameRate = 60f;
        clip.wrapMode = WrapMode.Once;
        EditorUtility.SetDirty(clip);
    }

    private static void SetCurve(AnimationClip clip, float[] times, string propertyName, params float[] values)
    {
        Keyframe[] keys = new Keyframe[times.Length];
        for (int i = 0; i < keys.Length; i++) keys[i] = new Keyframe(times[i], values[i]);

        AnimationCurve curve = new AnimationCurve(keys);
        for (int i = 0; i < keys.Length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
            AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
        }

        EditorCurveBinding binding = EditorCurveBinding.FloatCurve(
            "Portrait Holder",
            typeof(RectTransform),
            propertyName);

        AnimationUtility.SetEditorCurve(clip, binding, curve);
    }
}
#endif
