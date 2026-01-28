#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class AIUIImporterPostprocessor : AssetPostprocessor
{
    private const string TargetFolder = "Assets/AI_Generated/";
    private const float DefaultPPU = 100f;

    private void OnPreprocessTexture()
    {
        if (string.IsNullOrEmpty(assetPath)) return;
        if (!assetPath.StartsWith(TargetFolder, StringComparison.OrdinalIgnoreCase)) return;

        var ti = assetImporter as TextureImporter;
        if (ti == null) return;

        // Texture Type -> Sprite
        ti.textureType = TextureImporterType.Sprite;

        // Sprite Mode -> Single
        ti.spriteImportMode = SpriteImportMode.Single;

        // PPU
        ti.spritePixelsPerUnit = DefaultPPU;

        // NPOT
        ti.npotScale = TextureImporterNPOTScale.None;

        // Mipmap OFF
        ti.mipmapEnabled = false;

        // Alpha as transparency
        ti.alphaIsTransparency = true;

        // UI textures usually Clamp
        ti.wrapMode = TextureWrapMode.Clamp;

        // UI default filter
        ti.filterMode = FilterMode.Bilinear;

        // Sprite Mesh Type: FullRect (good for UI/9-slice)
        SetSpriteMeshTypeFullRect(ti);

    }

    private static void SetSpriteMeshTypeFullRect(TextureImporter ti)
    {
        // Prefer TextureImporterSettings if available in this Unity version.
        var settings = new TextureImporterSettings();
        ti.ReadTextureSettings(settings);

        var meshTypeProp = typeof(TextureImporterSettings).GetProperty(
            "spriteMeshType",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
        );

        if (meshTypeProp != null && meshTypeProp.CanWrite && meshTypeProp.PropertyType.IsEnum)
        {
            object fullRect = Enum.Parse(meshTypeProp.PropertyType, "FullRect");
            object current = meshTypeProp.GetValue(settings);

            if (!Equals(current, fullRect))
            {
                meshTypeProp.SetValue(settings, fullRect);
                ti.SetTextureSettings(settings);
            }

            return;
        }

        // Fallback to serialized property for older/newer versions.
        var so = new SerializedObject(ti);
        var prop = so.FindProperty("m_SpriteMeshType");
        if (prop == null) return;

        const int fullRectValue = 1;
        if (prop.intValue == fullRectValue) return;

        prop.intValue = fullRectValue;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
#endif
