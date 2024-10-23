using System;
using System.IO;
using UnityEngine;
using UnityEditor;

/// <summary>
/// This editor scripts gives the user enhancing functionality for creating level. Notable features include:Spawning a prefab and creating a prefab,distributing equally or snapping selected objects.
/// </summary>
public class LevelEditor : EditorWindow {
    #region Variables
    //Window style and formatting
    private static Texture2D texBackground;
    private GUIStyle boldBlackHeadingStyle;
    
    //Variable Declarations
    private GameObject[] prefabs;
    private SerializedProperty[] tagList;
    private int prefabIndex = -1, prefabTag, prefabLayer;
    private Vector3 position, scale = Vector3.one, gridSize = new(1f, 1f, 1f);
    private Quaternion rotation;
    private GameObject environmentParent;
    private enum Axis { X, Y, Z }
    private Axis alignAxis = Axis.X;
    private string nameFilter;
    private float distribution = 1;
    
    #endregion
    
    [MenuItem("Window/Level Editor")]
    public static void ShowWindow() {
        GetWindow<LevelEditor>("Level Editor");
    }
    
    private void OnEnable() {
        //Set texture to blue
        texBackground = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texBackground.SetPixel(0, 0, new Color(0.25f, 0.4f, 0.65f));
        texBackground.Apply();
    
        //Create new bold label style
        boldBlackHeadingStyle = new GUIStyle(EditorStyles.boldLabel) {
            normal = {
                textColor = Color.black
            },
            fontStyle = FontStyle.Bold,
            fontSize = 50
        };
    }

    private void OnGUI(){
        //Draw background
        GUI.DrawTexture(new Rect(0, 0, maxSize.x, maxSize.y), texBackground, ScaleMode.StretchToFill);

        //Placing Prefabs
        GUILayout.Label("Select a prefab to place in the scene", boldBlackHeadingStyle);
        PlacePrefab();

        //Shows the selected objects
        EditorGUILayout.Space();
        GUILayout.Label("Selected Objects", boldBlackHeadingStyle);
        EditorGUILayout.LabelField("Selected GameObjects:");
        var selectedObjects = Selection.gameObjects;
        foreach (var obj in selectedObjects)EditorGUILayout.LabelField(obj.name);
        if(selectedObjects.Length < 1 )EditorGUILayout.HelpBox("Nothing selected", MessageType.Warning);
        EditorGUILayout.Space();

        //Dropdown for selecting alignment axis
        alignAxis = (Axis)EditorGUILayout.EnumPopup("Alignment Axis", alignAxis);

        //Buttons for applying functions to selected objects
        if (GUILayout.Button("Align Selected"))AlignSelected();
        distribution = EditorGUILayout.FloatField("Distribution", distribution);
        if (GUILayout.Button("Distribute Selected"))DistributeSelected(alignAxis, distribution);
        gridSize = EditorGUILayout.Vector3Field("GridSize", gridSize);
        if (GUILayout.Button("Snap to Grid"))SnapToGrid();
        if (GUILayout.Button("Prefab Selected"))PrefabSelected();
        if (GUILayout.Button("Destroy Selected"))DestroySelected();
    }
    
    /// <summary>
    /// Allows user to spawn a prefab
    /// </summary>
    private void PlacePrefab() {
        nameFilter = EditorGUILayout.TextField("Prefab Search", nameFilter);
        //Display dropdown for selecting prefabs
        prefabIndex = EditorGUILayout.Popup("Prefab", prefabIndex, GetPrefabs(nameFilter));
        

        //Get the selected prefab
        if (prefabIndex >= 0 && prefabIndex < prefabs.Length) {
            
            GUILayout.Label(AssetPreview.GetAssetPreview(prefabs[prefabIndex]), GUILayout.Width(100), GUILayout.Height(100));
            
            //Vector settings
            position = EditorGUILayout.Vector3Field("Position Vector", position);
            scale = EditorGUILayout.Vector3Field("Scale Vector", scale);
            rotation = Quaternion.Euler(EditorGUILayout.Vector3Field("Rotation Vector", rotation.eulerAngles));
            
           
            //Layer and tag options
            //Retrieve tags
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var tagsProp = tagManager.FindProperty("tags");
            var layersProp = tagManager.FindProperty("layers");
            
            //Convert to acceptable arrays
            var tagArray = new string[tagsProp.arraySize + 7];
            var layerArray = new string[layersProp.arraySize];
            for (var i = 0; i < tagsProp.arraySize; i++)tagArray[i] = tagsProp.GetArrayElementAtIndex(i).stringValue;
            for (var i = 0; i < layersProp.arraySize; i++)layerArray[i] = layersProp.GetArrayElementAtIndex(i).stringValue;
            
            //Default tags not stored so need to be added here
            tagArray[tagsProp.arraySize] = "Untagged";
            tagArray[tagsProp.arraySize + 1] = "Respawn";
            tagArray[tagsProp.arraySize + 2] = "Finish";
            tagArray[tagsProp.arraySize + 3] = "EditorOnly";
            tagArray[tagsProp.arraySize + 4] = "MainCamera";
            tagArray[tagsProp.arraySize + 5] = "Player";
            tagArray[tagsProp.arraySize + 6] = "GameController";
            
            //Selections
            prefabTag = EditorGUILayout.Popup("Prefab tag", prefabTag, tagArray);
            prefabLayer = EditorGUILayout.Popup("Prefab layer", prefabLayer, layerArray);
            
            // Place button to spawn the selected prefab
            if (!GUILayout.Button("Place Prefab")) return;
            var prefab = prefabs[prefabIndex];
            var newObject = Instantiate(prefab);
                
            //If an environment parent doesn't exist, create one
            environmentParent = GameObject.Find("Environment");
            if (environmentParent == null) environmentParent = new GameObject("Environment");
            
            //Set properties
            newObject.transform.position = position;
            newObject.transform.parent = environmentParent.transform;
            newObject.transform.localScale = scale;
            newObject.transform.rotation = rotation;
            newObject.name =  prefabs[prefabIndex].name;
            newObject.tag = tagArray[prefabTag];
            newObject.layer = prefabLayer;
            
            
            //Link to prefab
            PrefabUtility.ReplacePrefab(newObject, prefab, ReplacePrefabOptions.ConnectToPrefab);
                
            //Select the newly created object
            Selection.activeGameObject = newObject;
        }
        else {
            EditorGUILayout.HelpBox("Nothing selected", MessageType.Warning);
        }
    }
    


    /// <summary>
    /// Returns prefab names
    /// </summary>
    /// <returns></returns>
    private string[] GetPrefabs(string nameFilter) {
        //Load available prefabs from the project using the filter
        var prefabID = AssetDatabase.FindAssets(nameFilter + " t:Prefab");
        prefabs = new GameObject[prefabID.Length];

        for (var i = 0; i < prefabID.Length; i++) {
            var path = AssetDatabase.GUIDToAssetPath(prefabID[i]);
            prefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }
        
        //Extract prefab names
        var prefabNames = new string[prefabs.Length];
        for (var i = 0; i < prefabs.Length; i++)prefabNames[i] = prefabs[i].name;
        
        return prefabNames;
    }
    
    /// <summary>
    /// Aligns all selected objects to the average selected axis value
    /// </summary>
    private void AlignSelected() {
        var selectedObjects = Selection.gameObjects;
        var alignmentPosition = Vector3.zero;
        
        //Calculate the sum of positions along the selected axis for all selected objects
        foreach (var obj in selectedObjects) {
            switch (alignAxis) {
                case Axis.X:
                    alignmentPosition.x += obj.transform.position.x;
                    break;
                case Axis.Y:
                    alignmentPosition.y += obj.transform.position.y;
                    break;
                case Axis.Z:
                    alignmentPosition.z += obj.transform.position.z;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        //Calculate the average alignment position
        alignmentPosition /= selectedObjects.Length; 
        
        //Move each selected object to the calculated alignment
        foreach (var obj in selectedObjects) {
            var newPosition = obj.transform.position;
            switch (alignAxis)
            {
                case Axis.X:
                    newPosition.x = alignmentPosition.x;
                    break;
                case Axis.Y:
                    newPosition.y = alignmentPosition.y;
                    break;
                case Axis.Z:
                    newPosition.z = alignmentPosition.z;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            obj.transform.position = newPosition;
        }   
    }


    #region Distribute
    
    /// <summary>
    /// Distributes all selected objects with equal spacing along the specified axis
    /// </summary>
    /// /// <param name="axis">The axis along which to distribute the objects</param>
    /// /// <param name="distribution">The amount of distance to put between object</param>
    private void DistributeSelected(Axis axis, float distribution) {
        var selectedObjects = Selection.gameObjects;
        var objectCount = selectedObjects.Length;

        //If there are less than 2 objects selected, spacing is not needed
        if (objectCount < 2) {
            Debug.LogWarning("At least two objects are required for spacing.");
            return;
        }

        //Sort the selected objects based on their positions on the selected axis
        Array.Sort(selectedObjects, (a, b) => GetPositionOnAxis(a.transform.position, axis).CompareTo(GetPositionOnAxis(b.transform.position, axis)));

        //Arrange the objects with equal spacing
        var currentPosition = GetPositionOnAxis(selectedObjects[0].transform.position, axis) + distribution; // Start distributing from the second position
        foreach (var obj in selectedObjects){
            var newPosition = obj.transform.position;
            SetPositionOnAxis(ref newPosition, axis, currentPosition);
            obj.transform.position = newPosition;
            currentPosition += distribution;
        }
    }
    
    /// <summary>
    /// Gets the position value of a Vector3 along the specified axis.
    /// </summary>
    private float GetPositionOnAxis(Vector3 position, Axis axis) {
        return axis switch {
            Axis.X => position.x,
            Axis.Y => position.y,
            Axis.Z => position.z,
            _ => 0f
        };
    }
    
    /// <summary>
    /// Sets the position value of a Vector3 along the specified axis.
    /// </summary>
    private void SetPositionOnAxis(ref Vector3 position, Axis axis, float value) {
        switch (axis) {
            case Axis.X:
                position.x = value;
                break;
            case Axis.Y:
                position.y = value;
                break;
            case Axis.Z:
                position.z = value;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
        }
    }
    #endregion
    
    /// <summary>
    /// Snaps the positions of all selected objects to the nearest grid position.
    /// </summary>
    private void SnapToGrid() {
        var selectedObjects = Selection.gameObjects;
        foreach (var obj in selectedObjects) {
            var newPosition = RoundToGrid(obj.transform.position);
            obj.transform.position = newPosition;
        }
    }
    
    /// <summary>
    /// Rounds the given position to the nearest grid position using the grid size.
    /// </summary>
    private Vector3 RoundToGrid(Vector3 position) {
        var roundedPosition = new Vector3(
            Mathf.Round(position.x / gridSize.x) * gridSize.x,
            Mathf.Round(position.y / gridSize.y) * gridSize.y,
            Mathf.Round(position.z / gridSize.z) * gridSize.z
        );
        return roundedPosition;
    }
    
    /// <summary>
    /// Creates a prefab from the currently selected objects.
    /// </summary>
    private void PrefabSelected() {
        var selectedObjects = Selection.gameObjects;
        var emptyParent = new GameObject("Prefab");
        const string folderPath = "Assets/Prefabs";
        
        foreach (var obj in selectedObjects)obj.transform.parent = emptyParent.transform;
        
        //Create folder path if it does not exist
        if (!AssetDatabase.IsValidFolder(folderPath)) {
            Directory.CreateDirectory(folderPath);
            AssetDatabase.Refresh();
        }
        
        //Get all prefab paths in the specified folder and assign a name
        var prefabPath = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
        var prefabName = "Prefab" + prefabPath.Length;
        
        //Save prefab to file location
        var prefabSaveLocation = "Assets/Prefabs/" + prefabName + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(emptyParent, prefabSaveLocation);
        
        //Associate the selected objects with the newly created prefab
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabSaveLocation);
        emptyParent.name = prefabName;
        PrefabUtility.ReplacePrefab(emptyParent, prefab, ReplacePrefabOptions.ConnectToPrefab);

        //Deselect all objects after prefabbing
        Selection.objects = new UnityEngine.Object[0];
    }
    
    /// <summary>
    /// Destroy selected objects, now instead of clicking delete on your keyboard you can click it in a fancy window 
    /// </summary>
    private void DestroySelected() {
        var selectedObjects = Selection.gameObjects;
        foreach (var obj in selectedObjects)DestroyImmediate(obj);
    }
}