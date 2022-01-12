// unset

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
            ItemProto[] items = LDB.items.dataArray;
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].ID > 0 && items[i].ID < iconSet.itemIconIndex.Length)
                {
                    uint spriteIndex = iconSet.itemIconIndex[items[i].ID];
                    string spriteName = items[i].Name.Translate(Language.enUS);
                    spriteName = spriteName.ToLower()
                        .Replace(' ', '-')
                        .Replace("mk.iv", "4")
                        .Replace("mk.iii", "3")
                        .Replace("mk.ii", "2")
                        .Replace("mk.i", "1");


                    AddSprite(ref spriteCharacterTable, ref spriteGlyphTable, spriteIndex, i, spriteName);
                    signalSpriteIndex[items[i].ID] = (uint)i;
                }
            }

            RecipeProto[] recipes = LDB.recipes.dataArray;
            for (int i = 0; i < recipes.Length; i++)
            {
                if (recipes[i].ID > 0 && recipes[i].ID < iconSet.recipeIconIndex.Length)
                {
                    uint spriteIndex = iconSet.recipeIconIndex[recipes[i].ID];
                    string spriteName = recipes[i].Name.Translate(Language.enUS);
                    spriteName = spriteName.ToLower()
                        .Replace(' ', '-')
                        .Replace("mk.iv", "4")
                        .Replace("mk.iii", "3")
                        .Replace("mk.ii", "2")
                        .Replace("mk.i", "1");


                    AddSprite(ref spriteCharacterTable, ref spriteGlyphTable, spriteIndex, i, spriteName);
                    signalSpriteIndex[recipes[i].ID + 20000] = (uint)i;
                }
            }

            TechProto[] technologies = LDB.techs.dataArray;
            for (int i = 0; i < technologies.Length; i++)
            {
                if (technologies[i].ID > 0 && technologies[i].ID < iconSet.techIconIndex.Length)
                {
                    uint spriteIndex = iconSet.techIconIndex[technologies[i].ID];
                    string spriteName = technologies[i].Name.Translate(Language.enUS);
                    spriteName = spriteName.ToLower().Replace(' ', '-');

                    AddSprite(ref spriteCharacterTable, ref spriteGlyphTable, spriteIndex, i, spriteName);
                    signalSpriteIndex[technologies[i].ID + 40000] = (uint)i;
                }
            }

            SignalProto[] signals = LDB.signals.dataArray;
            for (int i = 0; i < signals.Length; i++)
            {
                if (signals[i].ID > 0 && signals[i].ID < iconSet.signalIconIndex.Length)
                {
                    uint spriteIndex = iconSet.signalIconIndex[signals[i].ID];
                    string spriteName = signals[i].Name.Translate(Language.enUS);
                    spriteName = spriteName.ToLower().Replace(' ', '-');

                    AddSprite(ref spriteCharacterTable, ref spriteGlyphTable, spriteIndex, i, spriteName);
                    signalSpriteIndex[signals[i].ID] = (uint)i;
                }
            }
        }

        private static void AddSprite(ref List<TMP_SpriteCharacter> spriteCharacterTable, ref List<TMP_SpriteGlyph> spriteGlyphTable, uint spriteIndex, int i, string spriteName)
        {
            int x = (int) (spriteIndex % 25U);
            int y = (int) (spriteIndex / 25U);
            Rect rect = new Rect(x*80, y*80, 80, 80);

            TMP_SpriteGlyph spriteGlyph = new TMP_SpriteGlyph
            {
                index = (uint) i,
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
            InGameChatAssetLoader.ChatManager();
            Shader shader = InGameChatAssetLoader.assetBundle.LoadAsset<Shader>("Assets/Resources/TextMeshPro/Shaders/TMP_Sprite.shader");
            Material material = new Material(shader);
            material.SetTexture(ShaderUtilities.ID_MainTex, spriteAsset.spriteSheet);

            spriteAsset.material = material;
            material.hideFlags = HideFlags.HideInHierarchy;
        }
    }
}