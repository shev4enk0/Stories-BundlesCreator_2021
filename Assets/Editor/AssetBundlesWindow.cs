using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public enum Folders
{
	All,
	Preview,
	BaseResources,
}



public class AssetBundlesWindow : EditorWindow
{
	public static BuildTarget ChosenBuildTarget = BuildTarget.NoTarget;
	public static Folders FoldersForBundles;
	public static bool IsAllBook;

    private readonly string _basePath = Path.Combine("Assets", "Bundles");

	public static  readonly List<string> BookIDs = new List<string>();
	private static int _index;

    private bool _bundlesGenerated;

	private BookVersionInfo _bookVersionInfo;

	private string _login = "";
	private string _pass = "";


    public static string ChosenBookID => BookIDs[_index];

    private bool allGenerated = true;

	[MenuItem("Бандлы/Окно бандлов")]
	private static void ShowWindow()
	{
		var window = GetWindow<AssetBundlesWindow>();
		window.titleContent = new GUIContent("Бандлы");
		window.Show();
	}

	private void OnGUI()
	{
		var boldStyle = new GUIStyle
		{
			normal = new GUIStyleState
			{
				textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black
			},

			fontSize = 18,
			fontStyle = FontStyle.Bold,
			padding = new RectOffset(5, 5, 5, 5)
		};
		
		BuildIdList();

		if (BookIDs.Count == 0)
		{
			GUILayout.Label("Загрузите банд в папку Bundles", boldStyle);
			return;
		}

		GUILayout.Label("Выбери бандл", boldStyle);

		_index = EditorGUILayout.Popup(_index, BookIDs.ToArray(), GUILayout.MaxWidth(200));
		IsAllBook = GUI.Toggle(new Rect(250, 33, 100, 20), IsAllBook, "Все истории?");

		GUILayout.Label(ChosenBookID, EditorStyles.boldLabel);

		GetVersion(ChosenBookID);

        if (!_bundlesGenerated)
        {
            var style = new GUIStyle
            {
				normal = new GUIStyleState
				{
					textColor = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 0.8f) : Color.black
				},

                richText = true,
                padding = new RectOffset(5, 5, 2, 2)
            };

            GUILayout.Label(string.Format("BinVersion: <b>{0}</b>", _bookVersionInfo.BinVersion), style);
            GUILayout.Label(string.Format("BaseResourcesVersion: <b>{0}</b>", _bookVersionInfo.BaseResourcesVersion), style);
            GUILayout.Label(string.Format("PreviewVersion: <b>{0}</b>", _bookVersionInfo.PreviewVersion), style);
            
            ChosenBuildTarget = (BuildTarget) EditorGUILayout.EnumPopup("Build Target", ChosenBuildTarget);
            FoldersForBundles = (Folders) EditorGUILayout.EnumPopup("Folders for Bundle", FoldersForBundles);

            /*if (GUILayout.Button("Set import params"))
            {
	            SetImportParams(ChosenBookID);
	            EditorUtility.DisplayDialog("Параметры проставленны", "Не забудьте сделать реимпорт" , "OK"); 
            }*/

            /*if (GUILayout.Button("Reimport"))
            {
	            Reimport(ChosenBookID);
	            EditorUtility.DisplayDialog("Reimport", "Reimport" , "OK");
            }

            if (GUILayout.Button("Build all asset bundles"))
            {
	            BuildABForAllFolders(ChosenBookID);
	            EditorUtility.DisplayDialog("Ура","Ассет-бандлы успешно сгенерированы" , "Ok");
            }*/

            GUILayout.Space(10);
            
            if (GUILayout.Button("Создать Ассет Бандлы"))
            {
	            /*if (FoldersForBundles == Folders.All)
	            {
		            SetImportParams(ChosenBookID);
		            Reimport(ChosenBookID);
	            }*/

	            CreateAssetBundles.BuildAB();
	            _bundlesGenerated = true;
	            Debug.Log("Готово. Бандл собран");
            }

            /*if (GUILayout.Button("Создать Ассет Бандл"))
            {
                SetImportParams(ChosenBookID);
                Reimport(ChosenBookID);
                BuildAllAssetBundles(ChosenBookID);            
            }
            if (GUILayout.Button("Создать Ассет Бандл iOS + Android"))
            {
	            SetImportParams(ChosenBookID);
	            Reimport(ChosenBookID);
	            BuildAllAssetBundles(ChosenBookID);            
            }*/
            /*if (GUILayout.Button("Full job ALL"))
            {
	            foreach (var el in BookIDs)
	            {
		            SetImportParams(el);
		            Reimport(el);
		            BuildABForAllFolders(el);
	            }
            }*/
            if (GUILayout.Button("Generate Cards"))
            {
                BuildCardsBundle();
            }
            
            /*if (GUILayout.Button("Создать Превью Ассет Бандл"))
            {
	            SetImportParams(ChosenBookID);
	            Reimport(ChosenBookID);
	            BuildABForTheFolderInTheBook(ChosenBookID, "Preview");            
            }
            
            if (GUILayout.Button("Создать BaseResources Бандл"))
            {
	            SetImportParams(ChosenBookID);
	            Reimport(ChosenBookID);
	            BuildABForTheFolderInTheBook(ChosenBookID, "BaseResources");            
            }*/
        } 
        else
        {
            GUILayout.Label("Ассет-бандлы успешно сгенерированы!");

          if (GUILayout.Button("Заново"))
          {
            _bundlesGenerated = false;
          }
        }
	}

	private void SetImportParams(string bId)
	{
		SetSecondaryTextureSize(bId);

		var bannersPath = _basePath.CombinePathsWith(bId, "Preview", "Banners");
		SetupTextures(bannersPath, bId.ToLower() + "_preview");

		SetAudio(bId);

		Debug.Log("Параметры проставленны. Не забудьте сделать реимпорт");
	}


	private void Reimport(string bId)
	{
		var pathRsc = _basePath.CombinePathsWith(bId);
		AssetDatabase.ImportAsset(pathRsc, ImportAssetOptions.ImportRecursive);
		
		Debug.Log("Реимпорт сделан");
	}


	private void BuildAllAssetBundles(string bId)
	{
		CreateAssetBundles.BuildMobilesABForAllFoldersInTheBook(bId);
		_bundlesGenerated = true;
		Debug.Log("Готово. Бандл собран");
	}

	void BuildABForTheFolderInTheBook(string bookID, string folderNames)
	{
		CreateAssetBundles.BuildMobileAB(bookID, folderNames);
		_bundlesGenerated = true;
		Debug.Log($"Готово. {folderNames} Bundle собран");
	}

  private void BuildCardsBundle(){
    	CreateAssetBundles.BuildCardsAssetBundles(ChosenBuildTarget);
		// _bundlesGenerated = true;
    Debug.Log("Карты сгенерированы");
  }

	private void GetVersion(string bId)
	{
        var rootPath = Application.dataPath.CombinePathsWith("Bundles", bId);
        var rootAssetPath = "Assets".CombinePathsWith("Bundles", bId);

        var metaPath = rootPath.CombinePathsWith("Bin", "Meta.json");

        var meta = new AJLinkerMeta();

		var reader = new StreamReader(metaPath);
		meta = JsonUtility.FromJson<AJLinkerMeta>(reader.ReadToEnd());

		_bookVersionInfo = meta.Version;
	}

	private void BuildIdList()
	{
		BookIDs.Clear();

		var dir = new DirectoryInfo(_basePath);
		var subDirs = dir.GetDirectories();

		foreach (var subDir in subDirs)
			BookIDs.Add(subDir.Name);
	}

	private void SetSecondaryTextureSize(string bId)
	{
		var path = Path.Combine(_basePath, bId);

		var dir = new DirectoryInfo(path);
		var subDirs = dir.GetDirectories();

		foreach (var subDir in subDirs)
		{
			if (!subDir.Name.StartsWith("Chapter")) continue;
			var chapterPath = path.CombinePathsWith(subDir.Name, "Resources");
			var chapterDir = new DirectoryInfo(chapterPath);
			var files = chapterDir.GetFiles("*.png");

			foreach (var file in files)
			{
				if (!file.Name.StartsWith("Sec_")) continue;
				var filePath = Path.Combine(chapterPath, file.Name);
				SetupTexture(file, filePath);
			}
		}
	}

	private void SetupTextures(string path, string spritePackingTag)
	{
		var dir = new DirectoryInfo(path);
		var files = dir.GetFilesRecursive("*.png");

		foreach (var file in files)
		{
			var splitedFilePath = file.FullName.Split(new string[] { path }, StringSplitOptions.None)[1];
			var filePath = path + splitedFilePath;

			SetupTexture(file, filePath, spritePackingTag);
		}
	}

	private void SetupTexture(FileInfo file, string filePath, string spritePackingTag = "")
	{
		var textureImporter = TextureImporter.GetAtPath(filePath) as TextureImporter;
		textureImporter.maxTextureSize = 1024;
		textureImporter.spritePackingTag = spritePackingTag;

		textureImporter.SaveAndReimport();
		// AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
	}

	private void SetAudio(string bId)
	{
		var path = Path.Combine(_basePath, bId);

		var dir = new DirectoryInfo(path);
		var audioFiles = dir.GetFilesRecursive("*.mp3");

		foreach (var audioFile in audioFiles)
		{
			var splitedFilePath = audioFile.FullName.Split(new string[] { path }, StringSplitOptions.None)[1];
			var filePath = path + splitedFilePath;

			var audioImporter = AudioImporter.GetAtPath(filePath) as AudioImporter;

			var sampleSettings = audioImporter.defaultSampleSettings;
			// sampleSettings.loadType = AudioClipLoadType.Streaming;
			sampleSettings.loadType = AudioClipLoadType.DecompressOnLoad;
			sampleSettings.quality = 0.1f; // ranging from 0 (0%) to 1 (100%)

			audioImporter.defaultSampleSettings = sampleSettings;

			audioImporter.SaveAndReimport();
		}
	}
}