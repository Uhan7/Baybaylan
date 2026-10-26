using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class LevelSelectionTools
{
    [MenuItem("Baybaylan/Level Selection/Copy Open Scene Into Level Config")]
    private static void CopyOpenScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        GameManager manager = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<GameManager>(true)).FirstOrDefault();
        LevelConfig config = Selection.activeObject as LevelConfig;
        if (config == null && manager != null) config = manager.config;
        if (config == null)
        {
            config = AssetDatabase.FindAssets("t:LevelConfig")
                .Select(guid => AssetDatabase.LoadAssetAtPath<LevelConfig>(AssetDatabase.GUIDToAssetPath(guid)))
                .FirstOrDefault(candidate => candidate.sceneName == scene.name);
        }
        if (config == null || scene.name == "Level Selection" || string.IsNullOrEmpty(scene.path))
        {
            Debug.LogWarning("Open a saved level scene and select its Level Config asset first.");
            return;
        }
        CopyScenePreview(scene, config);
        AssetDatabase.SaveAssetIfDirty(config);
        Selection.activeObject = config;
        EditorGUIUtility.PingObject(config);
        Debug.Log($"Copied {scene.name} into {config.name}. Review the character display names before saving.", config);
    }

    [MenuItem("Baybaylan/Level Selection/Copy Open Scene Into Level Config", true)]
    private static bool CanCopyOpenScene() => !EditorApplication.isPlayingOrWillChangePlaymode;

    public static void CopyScenePreview(Scene scene, LevelConfig config)
    {
        Undo.RecordObject(config, "Copy Level Selection Preview");
        if (string.IsNullOrWhiteSpace(config.levelName) || config.levelName == config.sceneName)
            config.levelName = scene.name;
        config.sceneName = scene.name;
        Image[] images = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Image>(true)).ToArray();
        Image background = images.FirstOrDefault(image => image.name == "Background Normal");
        if (background != null)
        {
            config.selectionBackground = background.sprite;
            config.flipSelectionBackground = background.transform.localScale.x < 0f;
        }
        // Find the gameplay target by its component, so hierarchy ordering is irrelevant.
        StoryTargetAnimationPlayer target = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<StoryTargetAnimationPlayer>(true)).FirstOrDefault();
        Image targetImage = target != null
            ? target.GetComponentsInChildren<Image>(true).FirstOrDefault(image => image.name == "Character Image")
            : null;
        Image protagonist = images.FirstOrDefault(image => image.name == "Character Image" &&
            (target == null || !image.transform.IsChildOf(target.transform)));
        Sprite protagonistSprite = protagonist != null ? protagonist.sprite : null;
        Sprite targetSprite = targetImage != null ? targetImage.sprite : null;
        if (config.protagonistPortrait != protagonistSprite || string.IsNullOrWhiteSpace(config.protagonistName))
            config.protagonistName = SuggestedName(protagonistSprite);
        if (config.targetPortrait != targetSprite || string.IsNullOrWhiteSpace(config.targetName))
            config.targetName = SuggestedName(targetSprite);
        config.protagonistPortrait = protagonistSprite;
        config.targetPortrait = targetSprite;
        EditorUtility.SetDirty(config);
    }

    private static string SuggestedName(Sprite sprite)
    {
        if (sprite == null) return "";
        // Asset names are only a starting suggestion; display names remain editable.
        string name = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(sprite)).Replace('_', ' ');
        name = Regex.Replace(name, @"\b(main|sketch|dialogue|sprite|full)\b|[0-9]+", "", RegexOptions.IgnoreCase);
        name = Regex.Replace(name, @"\s+", " ").Trim();
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(name.ToLowerInvariant());
    }
}
