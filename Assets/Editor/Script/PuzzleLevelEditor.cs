using UnityEngine;
using UnityEditor;

/// <summary>
/// Professional Level Editor window - dockable tab in Unity Editor
/// Similar to Inspector, Project, Console
/// </summary>
public class PuzzleLevelEditor : EditorWindow
{
    private LevelEditorState editorState;
    private GridEditor gridEditor;
    private TilePaletteEditor tilePaletteEditor;
    private LevelDataManager levelDataManager;
    private SaveLoadSystem saveLoadSystem;
    private ValidationSystem validationSystem;
    private PreviewSystem previewSystem;

    private Vector2 mainScrollPos;
    private int selectedTab = 0;
    private string[] tabNames = { "Grid", "Palette", "Properties", "Validation", "Preview" };

    private const float TOOLBAR_HEIGHT = 20f;
    private const float TAB_HEIGHT = 25f;

    // Add menu item to open the window
    [MenuItem("Window/Puzzle Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<PuzzleLevelEditor>("Level Editor");
    }

    private void OnEnable()
    {
        InitializeEditorSystems();
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        
        // Save state on close
        if (editorState != null)
        {
            editorState.SaveState();
        }
    }

    private void InitializeEditorSystems()
    {
        if (editorState == null)
            editorState = new LevelEditorState();

        if (gridEditor == null)
            gridEditor = new GridEditor(editorState);

        if (tilePaletteEditor == null)
            tilePaletteEditor = new TilePaletteEditor(editorState);

        if (levelDataManager == null)
            levelDataManager = new LevelDataManager(editorState);

        if (saveLoadSystem == null)
            saveLoadSystem = new SaveLoadSystem(editorState);

        if (validationSystem == null)
            validationSystem = new ValidationSystem(editorState);

        if (previewSystem == null)
            previewSystem = new PreviewSystem(editorState);
    }

    private void OnGUI()
    {
        DrawToolbar();
        DrawTabBar();
        DrawContent();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(TOOLBAR_HEIGHT));

        if (GUILayout.Button("New Level", EditorStyles.toolbarButton, GUILayout.Width(80)))
        {
            levelDataManager.CreateNewLevel();
        }

        if (GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Width(60)))
        {
            saveLoadSystem.ShowLoadDialog();
        }

        if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(60)))
        {
            saveLoadSystem.QuickSave();
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Validate", EditorStyles.toolbarButton, GUILayout.Width(70)))
        {
            validationSystem.ValidateLevel();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawTabBar()
    {
        EditorGUILayout.BeginHorizontal(GUILayout.Height(TAB_HEIGHT));
        
        selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
        
        EditorGUILayout.EndHorizontal();
    }

    private void DrawContent()
    {
        EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
        
        mainScrollPos = EditorGUILayout.BeginScrollView(mainScrollPos);

        switch (selectedTab)
        {
            case 0: // Grid Tab
                gridEditor.DrawGUI();
                break;
            case 1: // Palette Tab
                tilePaletteEditor.DrawGUI();
                break;
            case 2: // Properties Tab
                levelDataManager.DrawPropertiesGUI();
                break;
            case 3: // Validation Tab
                validationSystem.DrawGUI();
                break;
            case 4: // Preview Tab
                previewSystem.DrawGUI();
                break;
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        switch (state)
        {
            case PlayModeStateChange.ExitingEditMode:
                editorState.SaveState();
                break;
            case PlayModeStateChange.EnteredEditMode:
                editorState.RestoreState();
                break;
        }
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }
}
