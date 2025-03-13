using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(CameraPathController))]
public class CameraPathEditor : Editor
{
    private string jsonFilePath = "Assets/CameraPathData.json";
    private Vector3 lastSphereCenter = Vector3.zero;
    private float lastSphereRadius = 5f;
    private int lastSphereKeyframes = 8;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        CameraPathController controller = (CameraPathController)target;

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) { margin = new RectOffset(5, 5, 2, 2) };

        // Keyframe Management
        GUILayout.Space(15);
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("🎮 Keyframe Management", EditorStyles.boldLabel);

        if (GUILayout.Button("➕ Add Keyframe", buttonStyle))
        {
            controller.AddKeyframe();
            EditorUtility.SetDirty(controller);
        }

        if (controller.keyframes.Count > 0)
        {
            if (GUILayout.Button("🔄 Sort Keyframes", buttonStyle))
            {
                controller.SortKeyframes();
                EditorUtility.SetDirty(controller);
            }

            if (GUILayout.Button("🗑️ Clear All Keyframes", buttonStyle))
            {
                controller.ClearAllKeyframes();
                EditorUtility.SetDirty(controller);
            }

            GUILayout.Label("Keyframes list:");
            for (int i = 0; i < controller.keyframes.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"KF {i}: {controller.keyframes[i].time:F2}s", GUILayout.Width(100));
                if (GUILayout.Button("➡️", buttonStyle, GUILayout.Width(30)))
                {
                    controller.MoveToKeyframe(i);
                    EditorUtility.SetDirty(controller);
                    SceneView.RepaintAll();
                }
                if (GUILayout.Button("🗑️", buttonStyle, GUILayout.Width(30)))
                {
                    controller.RemoveKeyframe(i);
                    EditorUtility.SetDirty(controller);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndVertical();

        // Spherical Keyframe Generation
        GUILayout.Space(10);
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("🌐 Generate Spherical Keyframes", EditorStyles.boldLabel);

        lastSphereCenter = EditorGUILayout.Vector3Field("Sphere Center", lastSphereCenter);
        lastSphereRadius = EditorGUILayout.FloatField("Radius", lastSphereRadius);
        lastSphereKeyframes = EditorGUILayout.IntField("Number of Keyframes", lastSphereKeyframes);
        controller.showSphere = EditorGUILayout.Toggle("Show Sphere", controller.showSphere);

        if (GUILayout.Button("🟢 Generate Spherical Keyframes", buttonStyle))
        {
            controller.GenerateSphericalKeyframes(lastSphereCenter, lastSphereRadius, lastSphereKeyframes);
            EditorUtility.SetDirty(controller);
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndVertical();

        // JSON Operations
        GUILayout.Space(10);
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("📂 JSON Operations", EditorStyles.boldLabel);
        jsonFilePath = EditorGUILayout.TextField("JSON Path", jsonFilePath);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("📂 Load From JSON rotation", buttonStyle))
        {
            controller.LoadFromJSON(jsonFilePath);
            EditorUtility.SetDirty(controller);
        }
        if (GUILayout.Button("📂 Load From JSON LookAt+Up", buttonStyle))
        {
            controller.LoadFromJSON(jsonFilePath, true);
            EditorUtility.SetDirty(controller);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("💾 Export JSON rotation", buttonStyle))
        {
            controller.ExportToJSON(jsonFilePath);
        }
        if (GUILayout.Button("💾 Export JSON LookAt+Up", buttonStyle))
        {
            controller.ExportToJSON(jsonFilePath, true);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        // Multi-Path Visualization
        GUILayout.Space(10);
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("🌍 Multi-Path Visualization", EditorStyles.boldLabel);
        controller.jsonFolderPath = EditorGUILayout.TextField("JSON Folder Path", controller.jsonFolderPath);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("📂 Load and Draw All Paths", buttonStyle))
        {
            controller.LoadAndDrawAllPathsFromFolder();
            EditorUtility.SetDirty(controller);
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("📂 Load From JSON LookAt+Up", buttonStyle))
        {
            controller.LoadAndDrawAllPathsFromFolder(true);
            EditorUtility.SetDirty(controller);
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("🗑️ Clear All Paths", buttonStyle))
        {
            controller.ClearAllMultiPaths();
            EditorUtility.SetDirty(controller);
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        // Camera Path Export
        GUILayout.Space(10);
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("📹 Camera Path Export", EditorStyles.boldLabel);
        controller.fullPathJsonPath = EditorGUILayout.TextField("Full Path JSON Path", controller.fullPathJsonPath);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("💾 Export Full Camera Path Rotation", buttonStyle))
        {
            controller.ExportFullCameraPath();
            EditorUtility.SetDirty(controller);
        }
        if (GUILayout.Button("💾 Export Full Camera Path LookAt+Up", buttonStyle))
        {
            controller.ExportFullCameraPath(true);
            EditorUtility.SetDirty(controller);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        // Frame Export
        GUILayout.Space(10);
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("📸 Frame Export", EditorStyles.boldLabel);
        if (GUILayout.Button("📷 Export Keyframe Images", buttonStyle))
        {
            controller.ExportFrames();
        }
        EditorGUILayout.EndVertical();

        // Playback
        GUILayout.Space(10);
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("▶️ Playback", EditorStyles.boldLabel);
        if (GUILayout.Button("▶️ Play", buttonStyle))
        {
            controller.PlayPath();
        }
        EditorGUILayout.EndVertical();

        // Debug Info
        GUILayout.Space(10);
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("ℹ️ Debug Info", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("", controller.GetDebugInfo(), EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndVertical();

        if (GUI.changed) EditorUtility.SetDirty(controller);
    }
}
