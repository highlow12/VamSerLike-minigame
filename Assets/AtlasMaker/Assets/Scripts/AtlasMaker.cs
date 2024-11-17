#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

[InitializeOnLoad]
public class AtlasMaker : Editor
{
    //SET THE IMAGE SIZE 4096x4096
    const int imageSize = 4096;

    //SET THE NORMAL STRENGTH
    static float normalStrength = 1f;

    #region VARIABLES

    const string menuTitle = "Atlas Maker/";
    const string newAtlasString = menuTitle + "New Atlas";
    const string lastAtlasString = menuTitle + "Last Atlas";
    const string exportPngString = menuTitle + "Export PNG";
    const string openImageString = menuTitle + "Open Image";
    const string atlasNormalString = menuTitle + "Atlas Normal";

    static readonly string[] imageExtensions = { "*.jpg", "*.png" };
    static string GetLastPath
    {
        get
        {
            if (EditorPrefs.HasKey("AtlasMaker: Last Folder Path"))
            {
                return EditorPrefs.GetString("AtlasMaker: Last Folder Path");
            }
            else
            {
                return "";
            }
        }
    }

    static int cellSize = imageSize;

    static List<string> imageList = new List<string>();
    static List<Texture2D> texturesList = new List<Texture2D>();

    static bool exportPNG = false;
    static bool cancelled = false;
    static bool openImage = true;
    static bool createNormalMap = false;

    static Texture2D tmpTex = null;
    static Texture2D combinedTextures = null;

    static Color32[] tmpPixels = null;
    static Color32[] newColors = null;
    static byte[] tmpBytes = null;

    static float ratioX;
    static float ratioY;
    static float progress = 0;

    #endregion

    #region BEHAVIOUR

    static AtlasMaker()
    {
        exportPNG = EditorPrefs.GetBool(exportPngString, false);
        openImage = EditorPrefs.GetBool(openImageString, true);
        createNormalMap = EditorPrefs.GetBool(openImageString, false);

        EditorUtility.ClearProgressBar();
    }

    #region NEW ATLAS
    [MenuItem(newAtlasString, false, 0)]
    static void NewAtlas()
    {
        //Open folder panel to get main folder path
        string folderPath = EditorUtility.OpenFolderPanel("Select Textures Folder", GetLastPath, "");

        if (!string.IsNullOrEmpty(folderPath))
        {
            EditorPrefs.SetString("AtlasMaker: Last Folder Path", folderPath);
            Stitch(folderPath);
        }
    }
    #endregion

    #region LAST ATLAS
    [MenuItem(lastAtlasString, false, 0)]
    static void LastAtlas()
    {
        if (EditorPrefs.HasKey("AtlasMaker: Last Folder Path"))
        {
            var folderPath = EditorPrefs.GetString("AtlasMaker: Last Folder Path");

            if (!string.IsNullOrEmpty(folderPath))
            {
                Stitch(folderPath);
            }
        }
        else
        {
            NewAtlas();
        }
    }
    #endregion

    #region EXPORT PNG
    [MenuItem(exportPngString, false, 11)]
    private static void ToggleExportPNG()
    {
        exportPNG = !exportPNG;
        EditorPrefs.SetBool(exportPngString, exportPNG);
    }

    [MenuItem(exportPngString, true, 11)]
    private static bool ToggleExportPNGValidate()
    {
        Menu.SetChecked(exportPngString, exportPNG);
        return true;
    }
    #endregion

    #region ATLAS NORMAL MAP
    [MenuItem(atlasNormalString, false, 11)]
    private static void ToggleAtlasNormal()
    {
        createNormalMap = !createNormalMap;
        EditorPrefs.SetBool(atlasNormalString, createNormalMap);
    }

    [MenuItem(atlasNormalString, true, 11)]
    private static bool ToggleAtlasNormalValidate()
    {
        Menu.SetChecked(atlasNormalString, createNormalMap);
        return true;
    }
    #endregion

    #region OPEN IMAGE
    [MenuItem(openImageString, false, 11)]
    private static void ToggleOpeImageEditor()
    {
        openImage = !openImage;
        EditorPrefs.SetBool(openImageString, openImage);
    }

    [MenuItem(openImageString, true, 11)]
    private static bool ToggleOpeImageValidate()
    {
        Menu.SetChecked(openImageString, openImage);
        return true;
    }
    #endregion


    #endregion

    #region FUNCTION

    static void ReadTextures(ref string path)
    {
        #region CREATE IMAGE LIST
        //Clear old image list values
        imageList.Clear();

        //Get all images from folder path;
        for (int e = 0; e < imageExtensions.Length; e++)
        {
            //Get current extension images;
            var images = Directory.GetFiles(path, imageExtensions[e], SearchOption.AllDirectories);

            if (images != null && images.Length > 0)
            {
                //Add current extension image to imageList;
                for (int i = 0; i < images.Length; i++)
                {
                    imageList.Add(images[i]);
                }
            }
        }
        #endregion
        #region CREATE TEXTURE LIST
        if (imageList != null && imageList.Count > 0)
        {
            //Clear old texture list values
            texturesList.Clear();

            cellSize = GetCellSize(imageSize, imageList.Count);

            //Iterate image list
            for (int i = 0; i < imageList.Count; i++)
            {
                //Create new texture with image
                tmpTex = new Texture2D(cellSize / 2, cellSize / 2, TextureFormat.RGBA32, false);
                tmpBytes = File.ReadAllBytes(imageList[i]);
                tmpTex.LoadImage(tmpBytes);

                #region APPLY SCALING
                tmpPixels = tmpTex.GetPixels32();
                newColors = new Color32[cellSize * cellSize];

                ratioX = ((float)tmpTex.width) / cellSize;
                ratioY = ((float)tmpTex.height) / cellSize;

                for (var y = 0; y < cellSize; y++)
                {
                    var thisY = (int)(ratioY * y) * tmpTex.width;
                    var yw = y * cellSize;
                    for (var x = 0; x < cellSize; x++)
                    {
                        newColors[yw + x] = tmpPixels[(int)(thisY + ratioX * x)];
                    }
                }

                tmpTex.Reinitialize(cellSize, cellSize);
                tmpTex.SetPixels32(newColors);
                tmpTex.Apply();
                #endregion

                //Add to texture list
                texturesList.Add(tmpTex);
                progress++;

                if (EditorUtility.DisplayCancelableProgressBar("Atlas Maker: Reading Texture " + progress + "/" + imageList.Count, imageList[i], progress / imageList.Count))
                {
                    Debug.LogError("AtlasMaker: Cancelled...");
                    EditorUtility.ClearProgressBar();

                    cancelled = true;
                    break;
                }

            }
        }
        else
        {
            Debug.LogError("AtlasMaker: No images found at path...");
            EditorUtility.ClearProgressBar();

            cancelled = true;
        }
        #endregion
    }
    static int GetCellSize(float size, float squares)
    {
        size = size - 1;

        float px = Mathf.Ceil(Mathf.Sqrt(squares * size / size));
        float sx, sy;
        if (Mathf.Floor(px * size / size) * px < squares) //does not fit, y/(x/px)=px*y/x
            sx = size / Mathf.Ceil(px * size / size);
        else
            sx = size / px;

        float py = Mathf.Ceil(Mathf.Sqrt(squares * size / size));

        if (Mathf.Floor(py * size / size) * py < squares) //does not fit
            sy = size / Mathf.Ceil(size * py / size);
        else
            sy = size / py;

        return (int)Mathf.Max(sx, sy);
    }
    static void Stitch(string path)
    {
        ReadTextures(ref path);

        //Create stiched texture
        if (!cancelled && texturesList != null && texturesList.Count > 0)
        {
            progress = 0;

            combinedTextures = new Texture2D(imageSize, imageSize, TextureFormat.RGBA32, false);

            int x = 0, y = 0, max = imageSize / cellSize;

            for (int i = 0; i < texturesList.Count; i++)
            {
                tmpPixels = texturesList[i].GetPixels32();

                if (i > 0)
                {
                    if (i % max == 0) { y++; x = 0; } else { x++; }
                }

                combinedTextures.SetPixels32(x * cellSize, y * cellSize, cellSize, cellSize, tmpPixels);

                progress++;

                EditorUtility.DisplayProgressBar("Atlas Maker: Stitching..." + progress + "/" + texturesList.Count, imageList[i], progress / texturesList.Count);
            }

            combinedTextures.Apply();

            string fileName = Path.GetFileName(path);
            string fullPath = "";
            if (exportPNG)
            {
                EditorUtility.DisplayProgressBar("Atlas Maker: Success!...", "Stitched " + texturesList.Count + " images into " + imageSize + "x" + imageSize + " PNG", 1);
                fullPath = path + "/../" + fileName + "Atlas.png";
                File.WriteAllBytes(fullPath, combinedTextures.EncodeToPNG());
                if (openImage) { Application.OpenURL(fullPath); }  
                else { EditorUtility.RevealInFinder(fullPath); }

                EditorUtility.ClearProgressBar();
            }
            else
            {
                EditorUtility.DisplayProgressBar("Atlas Maker: Success!...", "Stitched " + texturesList.Count + " images into " + imageSize + "x" + imageSize + " JPG", 1);
                fullPath = path + "/../" + fileName + "Atlas.jpg";
                File.WriteAllBytes(fullPath, combinedTextures.EncodeToJPG());
                if (openImage) { Application.OpenURL(fullPath); }
                else { EditorUtility.RevealInFinder(fullPath); }

                EditorUtility.ClearProgressBar();
            }

            if (createNormalMap)
            {
                Texture2D normalMap = new Texture2D(imageSize, imageSize, TextureFormat.ARGB32, true);
                progress = 0;

                normalStrength = Mathf.Clamp(normalStrength, 0.0F, 10.0F);
                float xLeft, xRight, yUp, yDown, yDelta, xDelta;

               
                for (int by = 0; by < normalMap.height; by++)
                {
                    for (int bx = 0; bx < normalMap.width; bx++)
                    {
                        xLeft = combinedTextures.GetPixel(bx - 1, by).grayscale * normalStrength;
                        xRight = combinedTextures.GetPixel(bx + 1, by).grayscale * normalStrength;
                        yUp = combinedTextures.GetPixel(bx, by - 1).grayscale * normalStrength;
                        yDown = combinedTextures.GetPixel(bx, by + 1).grayscale * normalStrength;
                        xDelta = ((xLeft - xRight) + 1) * 0.5f;
                        yDelta = ((yUp - yDown) + 1) * 0.5f;
                        normalMap.SetPixel(bx, by, new Color(xDelta, yDelta, 1.0f, yDelta));
                    }

                    progress++;
                    if (EditorUtility.DisplayCancelableProgressBar("Atlas Maker: Creating Normal Map... " + Mathf.RoundToInt((progress / normalMap.height) * 100) + "%", "", progress / normalMap.height))
                    {
                        Debug.LogError("AtlasMaker: Cancelled...");
                        EditorUtility.ClearProgressBar();

                        cancelled = true;
                        break;
                    }
                }
                normalMap.Apply();
                EditorUtility.ClearProgressBar();

                if (exportPNG)
                {
                    fullPath = path + "/../" + fileName + "AtlasNormalMap.png";
                    File.WriteAllBytes(fullPath, normalMap.EncodeToPNG());
                    if (openImage) { Application.OpenURL(fullPath); }
                    else { EditorUtility.RevealInFinder(fullPath); }

                    EditorUtility.ClearProgressBar();
                }
                else
                {         
                    fullPath = path + "/../" + fileName + "AtlasNormalMap.jpg";
                    File.WriteAllBytes(fullPath, normalMap.EncodeToJPG());
                    if (openImage) { Application.OpenURL(fullPath); }
                    else { EditorUtility.RevealInFinder(fullPath); }

                    EditorUtility.ClearProgressBar();
                }

            }
        }

        progress = 0;
        imageList.Clear();
        texturesList.Clear();
        tmpTex = null;
        combinedTextures = null;
        tmpPixels = null;
        newColors = null;
        tmpBytes = null;
        EditorUtility.ClearProgressBar();
    }

    #endregion

}
#endif