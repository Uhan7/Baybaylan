#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class PaghihigpitRevealPrefabSetup
{
    private const string RevealPrefabPath =
        "Assets/Prefabs/Game HUD/Paghihigpit Reveal Screen.prefab";
    private const string MainGamePrefabPath =
        "Assets/Prefabs/!Core/Main Game.prefab";
    private const string AlahasScreenPrefabPath =
        "Assets/Prefabs/~Temp/Alahas Screen.prefab";

    [InitializeOnLoadMethod]
    private static void BuildMissingPrefabAfterCompile()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(RevealPrefabPath) != null) return;
        EditorApplication.delayCall += BuildAndValidate;
    }

    [MenuItem("Baybaylan/Setup Paghihigpit Reveal")]
    public static void BuildAndValidate()
    {
        BuildRevealPrefab();
        WireMainGamePrefab();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Paghihigpit reveal prefab created and connected to Main Game.");
    }

    private static void BuildRevealPrefab()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(AlahasScreenPrefabPath);
        try
        {
            root.name = "Paghihigpit Reveal Screen";

            AlahasRewardScreen rewardScreen = root.GetComponent<AlahasRewardScreen>();
            if (rewardScreen != null) Object.DestroyImmediate(rewardScreen);
            CanvasGroupFader fader = root.GetComponent<CanvasGroupFader>();
            if (fader != null) Object.DestroyImmediate(fader);

            Canvas canvas = root.GetComponent<Canvas>();
            if (canvas == null) canvas = root.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 1100;
            if (root.GetComponent<GraphicRaycaster>() == null)
                root.AddComponent<GraphicRaycaster>();

            Transform artworkFrame = FindDescendant(root.transform, "Box Border");
            Transform artworkObject = FindDescendant(root.transform, "Alahas Image");
            Transform artworkBackground = FindDescendant(root.transform, "BG Image");
            Transform nameObject = FindDescendant(root.transform, "Alahas Name Text");
            Transform descriptionObject =
                FindDescendant(root.transform, "Alahas Description Text");
            Transform extraObject = FindDescendant(root.transform, "Alahas Flavor Text");

            if (artworkFrame == null || artworkObject == null || nameObject == null ||
                descriptionObject == null || extraObject == null)
                throw new MissingReferenceException(
                    "The Alahas Screen layout is missing an expected visual element.");

            // Paghihigpit art already owns its border and background. Keep the
            // Alahas screen's spacing without stacking a second frame behind it.
            Image frameImage = artworkFrame.GetComponent<Image>();
            if (frameImage != null) frameImage.enabled = false;
            if (artworkBackground != null) artworkBackground.gameObject.SetActive(false);

            artworkFrame.name = "Paghihigpit Artwork Holder";
            artworkObject.name = "Paghihigpit Artwork";
            nameObject.name = "Paghihigpit Name Text";
            descriptionObject.name = "Paghihigpit Description Text";
            extraObject.name = "Paghihigpit Flavor Text";

            Image artwork = artworkObject.GetComponent<Image>();
            Stretch(artwork.rectTransform);
            artwork.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            artwork.preserveAspect = true;
            artwork.raycastTarget = false;

            TextMeshProUGUI nameText = nameObject.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI description =
                descriptionObject.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI extra = extraObject.GetComponent<TextMeshProUGUI>();

            Button button = root.GetComponentInChildren<Button>(true);
            if (button == null)
                throw new MissingReferenceException("The Alahas Screen has no Next button.");
            button.name = "Next Paghihigpit Button";
            button.onClick = new Button.ButtonClickedEvent();
            SerializedObject serializedButton = new SerializedObject(button);
            SerializedProperty persistentCalls = serializedButton.FindProperty(
                "m_OnClick.m_PersistentCalls.m_Calls");
            if (persistentCalls != null) persistentCalls.ClearArray();
            serializedButton.ApplyModifiedPropertiesWithoutUndo();

            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (buttonText == null)
                throw new MissingReferenceException("The Next button has no label.");
            buttonText.text = "Susunod";

            PaghihigpitRevealController controller =
                root.AddComponent<PaghihigpitRevealController>();
            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("artwork").objectReferenceValue = artwork;
            serializedController.FindProperty("nameText").objectReferenceValue = nameText;
            serializedController.FindProperty("descriptionText").objectReferenceValue = description;
            serializedController.FindProperty("extraText").objectReferenceValue = extra;
            serializedController.FindProperty("progressText").objectReferenceValue = null;
            serializedController.FindProperty("proceedButton").objectReferenceValue = button;
            serializedController.FindProperty("proceedButtonText").objectReferenceValue = buttonText;
            serializedController.FindProperty("foregroundSortingOrder").intValue = 1100;
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, RevealPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void WireMainGamePrefab()
    {
        PaghihigpitRevealController revealPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>(RevealPrefabPath)
                ?.GetComponent<PaghihigpitRevealController>();
        if (revealPrefab == null)
            throw new MissingReferenceException("Could not load the Paghihigpit reveal prefab.");

        GameObject root = PrefabUtility.LoadPrefabContents(MainGamePrefabPath);
        try
        {
            TileSet tileSet = root.GetComponentInChildren<TileSet>(true);
            if (tileSet == null)
                throw new MissingReferenceException("Main Game prefab has no TileSet.");

            SerializedObject serializedTileSet = new SerializedObject(tileSet);
            serializedTileSet.FindProperty("paghihigpitRevealPrefab").objectReferenceValue =
                revealPrefab;
            serializedTileSet.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, MainGamePrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void Stretch(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private static Transform FindDescendant(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform match = FindDescendant(child, name);
            if (match != null) return match;
        }

        return null;
    }
}
#endif
