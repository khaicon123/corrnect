using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Validates level data for errors and issues
/// </summary>
public class ValidationSystem
{
    private LevelEditorState editorState;
    private List<ValidationError> validationErrors;
    private Vector2 errorScrollPos;

    public ValidationSystem(LevelEditorState state)
    {
        editorState = state;
        validationErrors = new List<ValidationError>();
    }

    public void DrawGUI()
    {
        EditorGUILayout.LabelField("Level Validation", EditorStyles.boldLabel);

        if (GUILayout.Button("Validate Level", GUILayout.Height(30)))
        {
            ValidateLevel();
        }

        EditorGUILayout.Space();
        DrawValidationResults();
    }

    public void ValidateLevel()
    {
        validationErrors.Clear();

        if (editorState.CurrentLevel == null)
        {
            EditorUtility.DisplayDialog("Error", "No level loaded", "OK");
            return;
        }

        // Run all validation checks
        ValidateGridSize();
        ValidateTiles();
        ValidateLevelProperties();
        ValidateConnectivity();

        string message = validationErrors.Count == 0
            ? "✓ Level validation passed!"
            : $"✗ Found {validationErrors.Count} issues";

        EditorUtility.DisplayDialog("Validation Result", message, "OK");
    }

    private void ValidateGridSize()
    {
        if (editorState.CurrentLevel.GridSize.x <= 0 || editorState.CurrentLevel.GridSize.y <= 0)
        {
            AddError("Grid Size", "Grid dimensions must be greater than 0");
        }
    }

    private void ValidateTiles()
    {
        int expectedTileCount = editorState.CurrentLevel.GridSize.x * editorState.CurrentLevel.GridSize.y;
        if (editorState.CurrentLevel.Tiles.Count != expectedTileCount)
        {
            AddError("Tile Count", $"Expected {expectedTileCount} tiles, found {editorState.CurrentLevel.Tiles.Count}");
        }

        // Check for null tiles
        foreach (var tile in editorState.CurrentLevel.Tiles)
        {
            if (tile == null)
            {
                AddError("Tiles", "Found null tile in grid");
                break;
            }
        }
    }

    private void ValidateLevelProperties()
    {
        LevelProperties props = editorState.CurrentLevel.Properties;

        if (string.IsNullOrEmpty(editorState.CurrentLevel.LevelName))
        {
            AddError("Level Name", "Level name cannot be empty");
        }

        if (props.TargetScore < 0)
        {
            AddError("Target Score", "Target score cannot be negative");
        }

        if (props.TimeLimit < 0)
        {
            AddError("Time Limit", "Time limit cannot be negative");
        }
    }

    private void ValidateConnectivity()
    {
        // Check if there's at least one walkable tile
        bool hasWalkableTile = false;
        foreach (var tile in editorState.CurrentLevel.Tiles)
        {
            if (tile.IsWalkable)
            {
                hasWalkableTile = true;
                break;
            }
        }

        if (!hasWalkableTile)
        {
            AddWarning("Walkable Tiles", "No walkable tiles found in level");
        }
    }

    private void DrawValidationResults()
    {
        if (validationErrors.Count == 0)
        {
            EditorGUILayout.HelpBox("No validation issues found.", MessageType.Info);
            return;
        }

        EditorGUILayout.LabelField($"Validation Issues ({validationErrors.Count})", EditorStyles.boldLabel);

        errorScrollPos = EditorGUILayout.BeginScrollView(errorScrollPos, GUILayout.ExpandHeight(true));

        foreach (var error in validationErrors)
        {
            DrawValidationError(error);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawValidationError(ValidationError error)
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);

        GUI.backgroundColor = error.IsWarning ? Color.yellow : Color.red;
        EditorGUILayout.LabelField(error.Category, EditorStyles.boldLabel);
        GUI.backgroundColor = Color.white;

        EditorGUILayout.LabelField(error.Message, EditorStyles.wordWrappedLabel);

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    private void AddError(string category, string message)
    {
        validationErrors.Add(new ValidationError
        {
            Category = category,
            Message = message,
            IsWarning = false
        });
    }

    private void AddWarning(string category, string message)
    {
        validationErrors.Add(new ValidationError
        {
            Category = category,
            Message = message,
            IsWarning = true
        });
    }
}

[System.Serializable]
public class ValidationError
{
    public string Category;
    public string Message;
    public bool IsWarning;
}
