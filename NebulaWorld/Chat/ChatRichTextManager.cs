using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore;

namespace NebulaWorld.Chat
{
    public static class ChatRichTextManager
    {
        public static uint[] signalSpriteIndex;
        public static TMP_SpriteAsset iconsSpriteAsset;
        
        public static void Create(IconSet set)
        {
            signalSpriteIndex = new uint[60000];
            
            iconsSpriteAsset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
            iconsSpriteAsset.version = "1.1.0";
            iconsSpriteAsset.hashCode = TMP_TextUtilities.GetSimpleHashCode(iconsSpriteAsset.name);
            iconsSpriteAsset.spriteSheet = set.texture;

            List<TMP_SpriteGlyph> spriteGlyphTable = new List<TMP_SpriteGlyph>();
            List<TMP_SpriteCharacter> spriteCharacterTable = new List<TMP_SpriteCharacter>();

            PopulateSpriteTables(set, ref spriteCharacterTable, ref spriteGlyphTable);

            iconsSpriteAsset.spriteCharacterTable = spriteCharacterTable;
            iconsSpriteAsset.spriteGlyphTable = spriteGlyphTable;

            // Add new default material for sprite asset.
            AddDefaultMaterial(iconsSpriteAsset);
            TMP_Settings.defaultSpriteAsset.fallbackSpriteAssets = new List<TMP_SpriteAsset> {iconsSpriteAsset};
        }
        
        private static void PopulateSpriteTables(IconSet iconSet, ref List<TMP_SpriteCharacter> spriteCharacterTable,
            ref List<TMP_SpriteGlyph> spriteGlyphTable)
        {
            uint lastSpriteIndex = 0;
            
            foreach (ItemProto item in LDB.items.dataArray)
            {
                if (item.ID <= 0 || item.ID >= iconSet.itemIconIndex.Length) continue;

                uint spriteIndex = iconSet.itemIconIndex[item.ID];
                string spriteName = item.ID.ToString();


                AddSprite(ref spriteCharacterTable, ref spriteGlyphTable, spriteIndex, lastSpriteIndex, spriteName);
                signalSpriteIndex[item.ID] = lastSpriteIndex;
                lastSpriteIndex++;
            }

            foreach (RecipeProto recipe in LDB.recipes.dataArray)
            {
                if (recipe.ID <= 0 || recipe.ID >= iconSet.recipeIconIndex.Length) continue;
                if (!recipe.hasIcon) continue;
                
                uint spriteIndex = iconSet.recipeIconIndex[recipe.ID];
                string spriteName = (recipe.ID + 20000).ToString();


                AddSprite(ref spriteCharacterTable, ref spriteGlyphTable, spriteIndex, lastSpriteIndex, spriteName);
                signalSpriteIndex[recipe.ID + 20000] = lastSpriteIndex;
                lastSpriteIndex++;
            }

            foreach (TechProto tech in LDB.techs.dataArray)
            {
                if (tech.ID <= 0 || tech.ID >= iconSet.techIconIndex.Length) continue;
                if (!tech.Published) continue;

                uint spriteIndex = iconSet.techIconIndex[tech.ID];
                string spriteName = (tech.ID + 40000).ToString();

                AddSprite(ref spriteCharacterTable, ref spriteGlyphTable, spriteIndex, lastSpriteIndex, spriteName);
                signalSpriteIndex[tech.ID + 40000] = lastSpriteIndex;
                lastSpriteIndex++;
            }

            foreach (SignalProto signal in LDB.signals.dataArray)
            {
                if (signal.ID <= 0 || signal.ID >= iconSet.signalIconIndex.Length) continue;

                uint spriteIndex = iconSet.signalIconIndex[signal.ID];
                string spriteName = signal.ID.ToString();

                AddSprite(ref spriteCharacterTable, ref spriteGlyphTable, spriteIndex, lastSpriteIndex, spriteName);
                signalSpriteIndex[signal.ID] = lastSpriteIndex;
                lastSpriteIndex++;
            }
        }

        private static void AddSprite(ref List<TMP_SpriteCharacter> spriteCharacterTable, ref List<TMP_SpriteGlyph> spriteGlyphTable, uint spriteIndex, uint i, string spriteName)
        {
            int x = (int) (spriteIndex % 25U);
            int y = (int) (spriteIndex / 25U);
            Rect rect = new Rect(x*80, y*80, 80, 80);

            TMP_SpriteGlyph spriteGlyph = new TMP_SpriteGlyph
            {
                index = i,
                metrics = new GlyphMetrics(rect.width, rect.height, 0, 70, rect.width),
                glyphRect = new GlyphRect(rect),
                scale = 1.0f
            };

            spriteGlyphTable.Add(spriteGlyph);


            TMP_SpriteCharacter spriteCharacter = new TMP_SpriteCharacter(0, spriteGlyph)
            {
                name = spriteName,
                scale = 1.0f
            };

            spriteCharacterTable.Add(spriteCharacter);
        }
        
        private static void AddDefaultMaterial(TMP_SpriteAsset spriteAsset)
        {
            Shader shader = AssetLoader.AssetBundle.LoadAsset<Shader>("Assets/Resources/TextMeshPro/Shaders/TMP_Sprite.shader");
            Material material = new Material(shader);
            material.SetTexture(ShaderUtilities.ID_MainTex, spriteAsset.spriteSheet);

            spriteAsset.material = material;
            material.hideFlags = HideFlags.HideInHierarchy;
        }
    }
}