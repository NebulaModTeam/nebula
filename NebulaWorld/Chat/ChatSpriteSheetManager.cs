#region

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore;

#endregion

namespace NebulaWorld.Chat;

public static class ChatSpriteSheetManager
{
    private static uint[] signalSpriteIndex;
    private static TMP_SpriteAsset iconsSpriteAsset;

    public static void Create(IconSet set)
    {
        signalSpriteIndex = new uint[60000];

        iconsSpriteAsset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
        iconsSpriteAsset.version = "1.1.0";
        iconsSpriteAsset.hashCode = TMP_TextUtilities.GetSimpleHashCode(iconsSpriteAsset.name);
        iconsSpriteAsset.spriteSheet = set.texture;

        var spriteGlyphTable = new List<TMP_SpriteGlyph>();
        var spriteCharacterTable = new List<TMP_SpriteCharacter>();

        PopulateSpriteTables(set, ref spriteCharacterTable, ref spriteGlyphTable);

        iconsSpriteAsset.spriteCharacterTable = spriteCharacterTable;
        iconsSpriteAsset.spriteGlyphTable = spriteGlyphTable;

        // Add new default material for sprite asset.
        AddDefaultMaterial(iconsSpriteAsset);
        TMP_Settings.defaultSpriteAsset.fallbackSpriteAssets = [iconsSpriteAsset];
    }

    private static void PopulateSpriteTables(IconSet iconSet, ref List<TMP_SpriteCharacter> spriteCharacterTable,
        ref List<TMP_SpriteGlyph> spriteGlyphTable)
    {
        uint lastSpriteIndex = 0;

        foreach (var item in LDB.items.dataArray)
        {
            if (item.ID <= 0 || item.ID >= iconSet.itemIconIndex.Length)
            {
                continue;
            }

            var spriteIndex = iconSet.itemIconIndex[item.ID];
            var spriteName = item.ID.ToString();


            AddSprite(ref spriteCharacterTable, ref spriteGlyphTable, spriteIndex, lastSpriteIndex, spriteName);
            signalSpriteIndex[item.ID] = lastSpriteIndex;
            lastSpriteIndex++;
        }

        foreach (var recipe in LDB.recipes.dataArray)
        {
            if (recipe.ID <= 0 || recipe.ID >= iconSet.recipeIconIndex.Length)
            {
                continue;
            }
            if (!recipe.hasIcon)
            {
                continue;
            }

            var spriteIndex = iconSet.recipeIconIndex[recipe.ID];
            var spriteName = (recipe.ID + 20000).ToString();


            AddSprite(ref spriteCharacterTable, ref spriteGlyphTable, spriteIndex, lastSpriteIndex, spriteName);
            signalSpriteIndex[recipe.ID + 20000] = lastSpriteIndex;
            lastSpriteIndex++;
        }

        foreach (var tech in LDB.techs.dataArray)
        {
            if (tech.ID <= 0 || tech.ID >= iconSet.techIconIndex.Length)
            {
                continue;
            }
            if (!tech.Published)
            {
                continue;
            }

            var spriteIndex = iconSet.techIconIndex[tech.ID];
            var spriteName = (tech.ID + 40000).ToString();

            AddSprite(ref spriteCharacterTable, ref spriteGlyphTable, spriteIndex, lastSpriteIndex, spriteName);
            signalSpriteIndex[tech.ID + 40000] = lastSpriteIndex;
            lastSpriteIndex++;
        }

        foreach (var signal in LDB.signals.dataArray)
        {
            if (signal.ID <= 0 || signal.ID >= iconSet.signalIconIndex.Length)
            {
                continue;
            }

            var spriteIndex = iconSet.signalIconIndex[signal.ID];
            var spriteName = signal.ID.ToString();

            AddSprite(ref spriteCharacterTable, ref spriteGlyphTable, spriteIndex, lastSpriteIndex, spriteName);
            signalSpriteIndex[signal.ID] = lastSpriteIndex;
            lastSpriteIndex++;
        }
    }

    private static void AddSprite(ref List<TMP_SpriteCharacter> spriteCharacterTable,
        ref List<TMP_SpriteGlyph> spriteGlyphTable, uint spriteIndex, uint i, string spriteName)
    {
        var x = (int)(spriteIndex % 25U);
        var y = (int)(spriteIndex / 25U);
        var rect = new Rect(x * 80, y * 80, 80, 80);

        var spriteGlyph = new TMP_SpriteGlyph
        {
            index = i,
            metrics = new GlyphMetrics(rect.width, rect.height, 0, 70, rect.width),
            glyphRect = new GlyphRect(rect),
            scale = 1.0f
        };

        spriteGlyphTable.Add(spriteGlyph);


        var spriteCharacter = new TMP_SpriteCharacter(0, spriteGlyph) { name = spriteName, scale = 1.0f };

        spriteCharacterTable.Add(spriteCharacter);
    }

    private static void AddDefaultMaterial(TMP_SpriteAsset spriteAsset)
    {
        var shader = AssetLoader.AssetBundle.LoadAsset<Shader>("Assets/Resources/TextMeshPro/Shaders/TMP_Sprite.shader");
        var material = new Material(shader);
        material.SetTexture(ShaderUtilities.ID_MainTex, spriteAsset.spriteSheet);

        spriteAsset.material = material;
        material.hideFlags = HideFlags.HideInHierarchy;
    }
}
