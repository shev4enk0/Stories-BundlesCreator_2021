using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
[CreateAssetMenu(fileName = "CardsCollection", menuName = "ScriptableObjects/CardsCollection", order = 1)]
public class CardsCollection : ScriptableObject
{
    public List<CardData> CharactersList;
    
    public Dictionary<string, CardData> GetDictionary()
    {
        Dictionary<string, CardData> _dict = new Dictionary<string, CardData>();
            foreach (var el in CharactersList) { _dict.TryAdd(el.Key, el); }

        return _dict;
    }
    
    // чтобы обновить CharactersList раскоментировать контекстный метод LoadSpritesIntoCharacterList
    //  при создании бандла Карт для WEB опять закоментировать иначе  бъет ошибку .


    /*
    #region LoadSpritesIntoCharacterList

    [ContextMenu("LoadData")]
    public void LoadSpritesIntoCharacterList()
    {
        string fullPath = Path.Combine(Application.dataPath, "Cards/Bundles/Sprites");

        // Get all files in directory
        string[] files = Directory.GetFiles(fullPath, "*.png");

        // If no files were found, return
        if (files.Length == 0)
        {
            Debug.Log("No files found at " + fullPath);
            return;
        }
        CharactersList = new List<CardData>(); 

        foreach (string file in files)
        {
            // Unity considers paths in the format "Assets/..."
            string unityPath = "Assets" + file.Substring(Application.dataPath.Length);

            // Load sprite from the path
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(unityPath);
            if (sprite == null)
            {
                Debug.Log("No sprite found at " + unityPath);
                continue;
            }

            // Extract filename
            string fileName = Path.GetFileNameWithoutExtension(file);

            var key = fileName.Replace("_preview", "");

            // Determine if the sprite is a preview sprite
            bool isPreview = fileName.ToLower().Contains("_preview");
            // Try to find existing CardData with the same key
            CardData existingCardData = CharactersList.Find(cardData => cardData.Key == key);
            if (existingCardData == null)
            {
                existingCardData = new CardData {Key = fileName};

                CharactersList.Add(existingCardData);
            }

            // Assign the sprite to the appropriate field
            if (isPreview) { existingCardData.PreviewTexture = sprite; }
            else { existingCardData.SpriteTexture = sprite; }
        }
    }

    #endregion
    */
    
}



