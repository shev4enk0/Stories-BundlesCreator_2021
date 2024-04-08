using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using PlasticGui;

public enum BookID
{
    Paradise_1,
    Kondotieri_1,
    Shism_1,
    Shism_2,
    Pirates_1,
    Pirates_NY_1,
    Paradise_2,
    Lis_1,
    Western_1,
    Royale_1,
    Superhero_1,
    Steampunk_1,
    Bandits_1,
    Ghosts_1,
    Pirates_2,
    Epic_1,
    Ninja_1,
    Elfsr_1,
    Ghosts_NY,
    Breadwinners_1,
    Stuck_Souls_1,
    Dates_Mateo,
    Dates_Jackie,
    NewGenres
}

public class CreateAssetBundles
{
    private const string AssetbundlesIos = "/AssetBundles/iOS/";
    private const string AssetbundlesAndroid = "/AssetBundles/Android/";
    private const string AssetbundlesWebgl = "/AssetBundles/WebGL/";
#if UNITY_EDITOR_OSX
    static string atSymbol = @"/";
#else
    static string atSymbol = @"\";
    private static string _bookNameToLower;
    private static string _absolutePath;
    private static string _rootAssetPath;
    private static string _metaPath;
    private static BookVersionInfo _bookVersionInfo;
    private static bool _isNewGenresBundle;
    private static string[] _dirs;
    private static string _absolutePathToABFolder;
    private static string _rootPathOriginal;
    private static string _binVersionPath;
    private static string _bookID;
#endif

    // [MenuItem("Assets/Build AssetBundles")]


    public static void BuildMobilesABForAllFoldersInTheBook(string bId)
    {
        BuildABForAllFolders(bId, BuildTarget.Android);
        BuildABForAllFolders(bId, BuildTarget.iOS);
    }

    /*public static void BuildAB()
    {
        var chosenBuildTarget = AssetBundlesWindow.ChosenBuildTarget;

        var isAllFolders = AssetBundlesWindow.FoldersForBundles == Folders.All;
      
        if (AssetBundlesWindow.IsAllBook)
        {
            foreach (var el in AssetBundlesWindow.BookIDs)
            {
                if (chosenBuildTarget == BuildTarget.NoTarget)
                {
                    if (isAllFolders)
                    {
                        BuildABForAllFolders(el, BuildTarget.Android);
                        BuildABForAllFolders(el, BuildTarget.iOS);
                    }
                    else
                    {
                        BuildFolderAB(el,BuildTarget.Android,AssetBundlesWindow.FoldersForBundles.ToString());
                        BuildFolderAB(el,BuildTarget.iOS,AssetBundlesWindow.FoldersForBundles.ToString());
                    }
                }
                else
                {
                    if (isAllFolders)
                    {
                        BuildABForAllFolders(el, chosenBuildTarget);
                    }
                    else
                    {
                        BuildFolderAB(el,chosenBuildTarget,AssetBundlesWindow.FoldersForBundles.ToString());
                    }
                }
                    
            }
        }
        else
        {
            if (chosenBuildTarget == BuildTarget.NoTarget)
            {
                if (isAllFolders)
                {
                    BuildABForAllFolders(AssetBundlesWindow.ChosenBookID, BuildTarget.Android);
                    BuildABForAllFolders(AssetBundlesWindow.ChosenBookID, BuildTarget.iOS);
                }
                else
                {
                    BuildFolderAB(AssetBundlesWindow.ChosenBookID,
                                  BuildTarget.Android,
                                  AssetBundlesWindow.FoldersForBundles.ToString());
                    BuildFolderAB(AssetBundlesWindow.ChosenBookID,
                                  BuildTarget.iOS,
                                  AssetBundlesWindow.FoldersForBundles.ToString());
                }
            }
            else
            {
                if (isAllFolders)
                {
                    BuildABForAllFolders(AssetBundlesWindow.ChosenBookID, chosenBuildTarget);
                }
                else
                {
                    BuildFolderAB(AssetBundlesWindow.ChosenBookID,
                                  chosenBuildTarget,
                                  AssetBundlesWindow.FoldersForBundles.ToString());
                }
            }
        }
    }*/
    
    public static void BuildAB()
    {
        var chosenBuildTarget = AssetBundlesWindow.ChosenBuildTarget;
        var isAllFolders = AssetBundlesWindow.FoldersForBundles == Folders.All;
    
        var bookIdToBuild = AssetBundlesWindow.IsAllBook 
                                ? AssetBundlesWindow.BookIDs
                                : new List<string>{AssetBundlesWindow.ChosenBookID};

        foreach (var el in bookIdToBuild)
        {
            if (chosenBuildTarget == BuildTarget.NoTarget)
            {
                BuildABForTarget(el, BuildTarget.Android, isAllFolders);
                BuildABForTarget(el, BuildTarget.iOS, isAllFolders);
            }
            else
            {
                BuildABForTarget(el, chosenBuildTarget, isAllFolders);
            }
        }
    }

    private static void BuildABForTarget(string bookId, BuildTarget target, bool isAllFolders)
    {
        if (isAllFolders)
        {
            BuildABForAllFolders(bookId, target);
        }
        else
        {
            BuildFolderAB(bookId, target, AssetBundlesWindow.FoldersForBundles.ToString());
        }
    }


    public static void BuildMobileAB(string bookID, string folderNames)
    {
        BuildFolderAB(bookID, BuildTarget.Android, folderNames); 
        BuildFolderAB(bookID, BuildTarget.iOS, folderNames); 
        
    }

    private static void BuildFolderAB(string bookID, BuildTarget buildTarget, string folderNames)
    {
        SetAbsolutePathToABFolder(buildTarget, bookID);

        InitFields(bookID);

        SetBookVersionInfo();

        SetAssetBundleNameAndVariant(false, folderNames);

        CreateRootDirectory();

        BuildPipeline.BuildAssetBundles(_absolutePathToABFolder,
                                        BuildAssetBundleOptions.ChunkBasedCompression,
                                        buildTarget);
        SetAssetBundleNameEmpty();
    }

    public static void BuildABForAllFolders(string bookID, BuildTarget _target)
    {
        SetAbsolutePathToABFolder(_target, bookID);
        
        InitFields(bookID);
        
        SetBookVersionInfo();

        SetBinVersionPath();

        SetAssetBundleNameAndVariant(true,"Bin","Chapter");

        CreateRootDirectory();

        BuildPipeline.BuildAssetBundles(_absolutePathToABFolder,
                                        BuildAssetBundleOptions.ChunkBasedCompression,
                                        _target);
        SetAssetBundleNameEmpty();
       
        SetAssetBundleNameAndVariant(false, "Bin","Chapter");
        Directory.CreateDirectory(_binVersionPath);
        BuildPipeline.BuildAssetBundles(_binVersionPath, 
                                        BuildAssetBundleOptions.ChunkBasedCompression, 
                                        _target);

        SetAssetBundleNameEmpty();
    }

    private static void SetBinVersionPath()
    {
        _binVersionPath = _absolutePathToABFolder + "/v" + _bookVersionInfo.BinVersion;
    }

    private static void SetAssetBundleNameAndVariant(bool toExclude, params string[] folderNames)
    {
        foreach (var el in _dirs)
        {
            var name = el.Replace(_absolutePath + @"\", "");
            var compare = name[..Math.Min(3, el.Length)];
            var any = folderNames.Any(x=>x.Contains(compare));
            switch (any)
            {
                case true when !toExclude:
                case false when toExclude:
                    SetAssetBundleNameAndVariant(name);
                    break;
            }
        }
    }

    private static void SetAssetBundleNameAndVariant(string folderName)
    {
        var path = _rootAssetPath + @"/" + folderName;
        var assetBundleName = $"{_bookID.ToLower()}_{folderName.ToLower()}_v{GetVersionValue(folderName)}";
        var assetImporter = AssetImporter.GetAtPath(path);
        assetImporter?.SetAssetBundleNameAndVariant(assetBundleName, "");
    }

    private static void SetAssetBundleNameEmpty()
    {
        foreach (var el in _dirs)
        {
            var _dir_name = el.Replace(_absolutePath + atSymbol, "");

            AssetImporter.GetAtPath(_rootAssetPath + @"/" + _dir_name)
                         .SetAssetBundleNameAndVariant("", "");
        }
    }

    private static void CreateRootDirectory()
    {
        if (Directory.Exists(_absolutePathToABFolder)) 
            Directory.Delete(_absolutePathToABFolder, true);

        Directory.CreateDirectory(_absolutePathToABFolder);
    }

    private static void SetAbsolutePathToABFolder(BuildTarget _target, string bookID)
    {
        switch (_target)
        {
            case BuildTarget.iOS:
                _absolutePathToABFolder =  Application.dataPath + AssetbundlesIos + bookID.ToLower() + "_ios";
                break;
               
            case BuildTarget.Android:
                _absolutePathToABFolder =  Application.dataPath + AssetbundlesAndroid + bookID.ToLower() + "_android";
                break;
              
            case BuildTarget.WebGL:
                _absolutePathToABFolder =  Application.dataPath + AssetbundlesWebgl + bookID.ToLower() + "";
                break;
                
        }
    }

    private static string GetVersionValue(string dirName)
    {
        var version = _bookVersionInfo.BinVersion;
        
        if(dirName.Contains("BaseResources"))
            version = _bookVersionInfo.BaseResourcesVersion;
       
        if(dirName.Contains("Preview"))
            version = _bookVersionInfo.PreviewVersion;

        return version;
    }

    private static void SetBookVersionInfo()
    {
        if (_isNewGenresBundle) return;
        var reader = new StreamReader(_metaPath);
        _bookVersionInfo = new BookVersionInfo();
        _bookVersionInfo = JsonUtility.FromJson<AJLinkerMeta>(reader.ReadToEnd()).Version;
    }

    private static void InitFields(string bookID)
    {
        _bookID = bookID;

        _absolutePath = Application.dataPath + @"/Bundles/" +bookID/* _bookNameToLower*/;
        
        _rootAssetPath = @"Assets/Bundles/" + bookID;

        _metaPath = _absolutePath + @"/Bin/Meta.json";

        _isNewGenresBundle = bookID == "Newgenres";

        _dirs = Directory.GetDirectories(_absolutePath);
    }


    public static void BuildCardsAssetBundles(BuildTarget _target){

        string outpathName = "Cards";
        string _root_asset_path = @"Assets/Cards/Bundles";

        string _root_ab_path = "";
         
        BuildAssetBundleOptions bundleOptions  = BuildAssetBundleOptions.None;
        if (_target == BuildTarget.iOS){
            _root_ab_path = Application.dataPath + AssetbundlesIos + outpathName + "_ios";
        }
        else if (_target == BuildTarget.Android){
            _root_ab_path = Application.dataPath + AssetbundlesAndroid + outpathName + "_android";
            
        }
        else if (_target == BuildTarget.WebGL){
          // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
          //Добавил для WEBGL
          bundleOptions = BuildAssetBundleOptions.ChunkBasedCompression;
          _root_ab_path = Application.dataPath + AssetbundlesWebgl + outpathName + "_webgl";
        }
      //  if (!Directory.Exists(_root_ab_path))
      //       Directory.CreateDirectory(_root_ab_path);
        //Вот это для WebGl добавил
        // BuildPipeline.BuildAssetBundles(_root_ab_path, bundleOptions, _target);
        //  BuildPipeline.BuildAssetBundles(_root_asset_path,bundleOptions, _target);
      
// The second bundle is all the asset files found recursively in the Meshes directory
    List<AssetBundleBuild> assetBundleDefinitionList = new();
        {
            AssetBundleBuild ab = new();
            ab.assetBundleName = "Cards";
            ab.assetNames = RecursiveGetAllAssetsInDirectory(_root_asset_path).ToArray();
            Debug.Log(ab.assetNames.Count());
            assetBundleDefinitionList.Add(ab);
        }
//  BuildAssetBundlesParameters buildInput = new()
//         {
//             outputPath = outputPath,
//             options = BuildAssetBundleOptions.AssetBundleStripUnityVersion,
//             bundleDefinitions = assetBundleDefinitionList.ToArray()
//         };
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(_root_ab_path, 
                                                                       assetBundleDefinitionList.ToArray(),
                                                                       bundleOptions, 
                                                                       _target);
        // Look at the results
        if (manifest != null)
        {
            foreach(var bundleName in manifest.GetAllAssetBundles())
            {
                string projectRelativePath = _root_ab_path + "/" + bundleName;
                Debug.Log($"Size of AssetBundle {projectRelativePath} is {new FileInfo(projectRelativePath).Length}");
            }
        }
        else
        {
            Debug.Log("Build failed, see Console and Editor log for details");
        }
    
     
       

       
    }

    static List<string> RecursiveGetAllAssetsInDirectory(string path)
    {
        List<string> assets = new();
        foreach (var f in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
        {
            if (Path.GetExtension(f) == ".meta" || Path.GetExtension(f) == ".cs" || Path.GetExtension(f) == ".unity") 
                continue;
            
            assets.Add(f);
            Debug.Log($"___{f}");
        }
        return assets;
    }
}