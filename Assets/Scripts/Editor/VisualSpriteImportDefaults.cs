using System;
using UnityEditor;

internal sealed class VisualSpriteImportDefaults : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith("Assets/Visuals/", StringComparison.Ordinal))
            return;

        var importer = (TextureImporter)assetImporter;
        if (importer.importSettingsMissing && importer.textureType == TextureImporterType.Sprite)
            importer.spriteImportMode = SpriteImportMode.Single;
    }
}
