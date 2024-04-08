using System;
using UnityEngine;
using System.IO;
using UnityEditor;

public class AssetBundles : MonoBehaviour
{
    private static readonly string BasePath = Path.Combine("Assets", "Bundles");

    private static bool _bundlesGenerated;

    private static BookVersionInfo _bookVersionInfo;

    private static string _bookId = "";

    private static void BuildBundle()
    {
        GetBookId();
        SetImportParams();
        Reimport();
        BuildAllAssetBundles();
    }
    
    private static void GetBookId()
    {
        Console.WriteLine("GetBookId");

        try
        {
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg == "-storyName")
                {
                    _bookId = args[i + 1];
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        Console.WriteLine("BookId: " + _bookId);
    }

    private static void SetImportParams()
    {
        Console.WriteLine("Выставляем параметры");

        try
        {
            SetSecondaryTextureSize();

            var bannersPath = BasePath.CombinePathsWith(_bookId, "Preview", "Banners");
            SetupTextures(bannersPath, _bookId.ToLower() + "_preview");

            SetAudio();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        Debug.Log("Параметры проставленны. Не забудьте сделать реимпорт");
        Console.WriteLine("Параметры проставленны");
    }

    private static void Reimport()
    {
        Console.WriteLine("Начинаем реимпорт");

        try
        {
            var pathRsc = BasePath.CombinePathsWith(_bookId);
            AssetDatabase.ImportAsset(pathRsc, ImportAssetOptions.ImportRecursive);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        Console.WriteLine("Реимпорт сделан");
        Debug.Log("Реимпорт сделан");
    }

    private static void BuildAllAssetBundles()
    {
        Console.WriteLine("BuildABForAllFolders");

        try
        {
            CreateAssetBundles.BuildABForAllFolders(_bookId, AssetBundlesWindow.ChosenBuildTarget);
            _bundlesGenerated = true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        Console.WriteLine("Готово. Бандл собран");
        Debug.Log("Готово. Бандл собран");
    }

     private static void BuildCardsAssetBundles()
    {
        Console.WriteLine("BuildCardsAssetBundles");

        try
        {
            CreateAssetBundles.BuildCardsAssetBundles(AssetBundlesWindow.ChosenBuildTarget);
            _bundlesGenerated = true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        Console.WriteLine("Готово. Бандл собран");
        Debug.Log("Готово. Бандл собран");
    }

    private static void GetVersion()
    {
        var rootPath = Application.dataPath.CombinePathsWith("Bundles", _bookId);
        var rootAssetPath = "Assets".CombinePathsWith("Bundles", _bookId);

        var metaPath = rootPath.CombinePathsWith("Bin", "Meta.json");

        var meta = new AJLinkerMeta();

        var reader = new StreamReader(metaPath);
        meta = JsonUtility.FromJson<AJLinkerMeta>(reader.ReadToEnd());

        _bookVersionInfo = meta.Version;
    }
    
    private static void SetSecondaryTextureSize()
    {
        var path = Path.Combine(BasePath, _bookId);

        var dir = new DirectoryInfo(path);
        var subDirs = dir.GetDirectories();

        foreach (var subDir in subDirs)
        {
            if (subDir.Name.StartsWith("Chapter"))
            {
                var chapterPath = path.CombinePathsWith(subDir.Name, "Resources");
                var chapterDir = new DirectoryInfo(chapterPath);
                var files = chapterDir.GetFiles("*.png");

                foreach (var file in files)
                {
                    if (file.Name.StartsWith("Sec_"))
                    {
                        var filePath = Path.Combine(chapterPath, file.Name);
                        SetupTexture(file, filePath);
                    }
                }
            }
        }
    }

    private static void SetupTextures(string path, string spritePackingTag)
    {
        var dir = new DirectoryInfo(path);
        var files = dir.GetFilesRecursive("*.png");

        foreach (var file in files)
        {
            var splitedFilePath = file.FullName.Split(new string[] {path}, StringSplitOptions.None)[1];
            var filePath = path + splitedFilePath;

            SetupTexture(file, filePath, spritePackingTag);
        }
    }

    private static void SetupTexture(FileInfo file, string filePath, string spritePackingTag = "")
    {
        var textureImporter = TextureImporter.GetAtPath(filePath) as TextureImporter;
        textureImporter.maxTextureSize = 1024;
        textureImporter.spritePackingTag = spritePackingTag;

        textureImporter.SaveAndReimport();
        // AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
    }

    private static void SetAudio()
    {
        var path = Path.Combine(BasePath, _bookId);

        var dir = new DirectoryInfo(path);
        var audioFiles = dir.GetFilesRecursive("*.mp3");

        foreach (var audioFile in audioFiles)
        {
            var splitedFilePath = audioFile.FullName.Split(new string[] {path}, StringSplitOptions.None)[1];
            var filePath = path + splitedFilePath;

            var audioImporter = AssetImporter.GetAtPath(filePath) as AudioImporter;

            var sampleSettings = audioImporter.defaultSampleSettings;
            // sampleSettings.loadType = AudioClipLoadType.Streaming;
            sampleSettings.loadType = AudioClipLoadType.DecompressOnLoad;
            sampleSettings.quality = 0.1f; // ranging from 0 (0%) to 1 (100%)

            audioImporter.defaultSampleSettings = sampleSettings;

            audioImporter.SaveAndReimport();
        }
    }
}