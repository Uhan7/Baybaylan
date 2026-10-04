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
    private const string LeftSidePrefabPath = "Assets/Prefabs/Side UI/Left Side Container Variant.prefab";
    private const string PreviousCharacterControllerPath = "Assets/Animations/Button.controller";

    [MenuItem("Tools/Baybaylan/Set Up Word Submission Animations")]
    public static void CreatePlaceholderAssets()
    {
        EnsureFolderExists();

        AnimationClip attackClip = GetOrCreateClip(AttackClipPath, "Attack Placeholder");
        AnimationClip healClip = GetOrCreateClip(HealClipPath, "Heal Placeholder");

        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;

            AnimatorState idleState = stateMachine.AddState("Idle");
            AnimatorState attackState = stateMachine.AddState("Attack");
            AnimatorState healState = stateMachine.AddState("Heal");

            attackState.motion = attackClip;
            healState.motion = healClip;
            stateMachine.defaultState = idleState;
        }

        ConfigureLeftSidePrefab(controller);

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
            Transform portraitHolder = prefabRoot.transform.Find("Portrait Holder");
            if (portraitHolder == null)
            {
                Debug.LogWarning($"Could not find Portrait Holder in {LeftSidePrefabPath}.");
                return;
            }

            RestoreCharacterImageController(portraitHolder);

            Animator animator = portraitHolder.GetComponent<Animator>();
            if (animator == null) animator = portraitHolder.gameObject.AddComponent<Animator>();

            string currentControllerPath = AssetDatabase.GetAssetPath(animator.runtimeAnimatorController);
            if (animator.runtimeAnimatorController == null ||
                currentControllerPath == PreviousCharacterControllerPath ||
                currentControllerPath == ControllerPath)
            {
                animator.runtimeAnimatorController = controller;
            }
            else
            {
                Debug.LogWarning(
                    $"Kept the custom Animator Controller already assigned to {portraitHolder.name}. " +
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

    private static void RestoreCharacterImageController(Transform portraitHolder)
    {
        Transform characterImage = portraitHolder.Find("Character Image");
        Animator characterAnimator = characterImage != null
            ? characterImage.GetComponent<Animator>()
            : null;

        if (characterAnimator == null ||
            AssetDatabase.GetAssetPath(characterAnimator.runtimeAnimatorController) != ControllerPath)
        {
            return;
        }

        characterAnimator.runtimeAnimatorController =
            AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(PreviousCharacterControllerPath);
    }
}
#endif
