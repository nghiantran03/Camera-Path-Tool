using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class CameraKeyframe
{
    public float time;
    public Vector3 position;
    public Quaternion rotation;
    public float fov;
    public float near;
    public float far;
}

[System.Serializable]
public class CameraPathData
{
    public List<CameraKeyframe> path = new List<CameraKeyframe>();
    public float totalDuration;
    public int fps;
}

public class CameraPathController : MonoBehaviour
{
    [Header("Camera Motion Configuration")]
    public List<CameraKeyframe> keyframes = new List<CameraKeyframe>();
    public float totalDuration = 5f;
    public int fps = 30;

    [Header("Display Options")]
    public bool showPath = true;
    public bool showKeyframes = true;
    public Color pathColor = Color.green;
    public Color keyframeColor = Color.red;

    [Header("Frame Export Options")]
    public string exportFolder = "ExportedFrames";
    public string framePrefix = "frame_";

    [Header("Sphere Visualization Settings")]
    public Vector3 sphereCenter = Vector3.zero;
    public float sphereRadius = 0f;
    public bool showSphere = false;

    [Header("Multi-Path Visualization")]
    [SerializeField] private List<CameraPathData> multiCameraPaths = new List<CameraPathData>();
    [SerializeField] public string jsonFolderPath = "Assets/JSONPaths"; // Đã là public từ trước

    private float timer = 0f;
    private bool isPlaying = false;
    private Camera targetCamera;

    void Awake()
    {
        targetCamera = GetComponent<Camera>() ?? Camera.main;
        if (targetCamera == null)
        {
            Debug.LogError("No Camera found on this GameObject or as Main Camera in the scene!");
        }
    }

    void Update()
    {
        if (!isPlaying || keyframes.Count < 2) return;

        timer += Time.deltaTime;
        timer = Mathf.Clamp(timer, 0f, totalDuration);
        UpdateCameraPosition();

        if (timer >= totalDuration) isPlaying = false;
    }

    private void UpdateCameraPosition()
    {
        if (targetCamera == null || keyframes.Count < 2) return;

        CameraKeyframe kf0 = keyframes[0];
        CameraKeyframe kf1 = keyframes[1];

        for (int i = 0; i < keyframes.Count - 1; i++)
        {
            if (timer >= keyframes[i].time && timer <= keyframes[i + 1].time)
            {
                kf0 = keyframes[i];
                kf1 = keyframes[i + 1];
                break;
            }
        }

        if (timer < keyframes[0].time)
        {
            kf0 = kf1 = keyframes[0];
        }
        else if (timer > keyframes[keyframes.Count - 1].time)
        {
            kf0 = kf1 = keyframes[keyframes.Count - 1];
        }

        float segmentDuration = kf1.time - kf0.time;
        float localT = segmentDuration > 0 ? (timer - kf0.time) / segmentDuration : 1f;

        transform.position = Vector3.Lerp(kf0.position, kf1.position, localT);
        transform.rotation = Quaternion.Slerp(kf0.rotation, kf1.rotation, localT);
        targetCamera.fieldOfView = Mathf.Lerp(kf0.fov, kf1.fov, localT);
        targetCamera.nearClipPlane = Mathf.Lerp(kf0.near, kf1.near, localT);
        targetCamera.farClipPlane = Mathf.Lerp(kf0.far, kf1.far, localT);
    }

    public void MoveToKeyframe(int keyframeIndex)
    {
        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>() ?? Camera.main;
            if (targetCamera == null)
            {
                Debug.LogError("No Camera found to move!");
                return;
            }
        }

        if (keyframeIndex < 0 || keyframeIndex >= keyframes.Count)
        {
            Debug.LogWarning($"Index {keyframeIndex} is invalid! Must be between 0 and {keyframes.Count - 1}.");
            return;
        }

        CameraKeyframe targetKeyframe = keyframes[keyframeIndex];
        transform.position = targetKeyframe.position;
        transform.rotation = targetKeyframe.rotation;
        targetCamera.fieldOfView = targetKeyframe.fov;
        targetCamera.nearClipPlane = targetKeyframe.near;
        targetCamera.farClipPlane = targetKeyframe.far;

        timer = targetKeyframe.time;
        Debug.Log($"Moved camera to keyframe {keyframeIndex} at time {targetKeyframe.time:F2}s");
    }

    public void GenerateSphericalKeyframes(Vector3 center, float radius, int numKeyframes)
    {
        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>() ?? Camera.main;
            if (targetCamera == null)
            {
                Debug.LogError("No Camera found to generate keyframes!");
                return;
            }
        }

        if (numKeyframes < 1)
        {
            Debug.LogWarning("Number of keyframes must be greater than 0!");
            return;
        }

        if (radius <= 0)
        {
            Debug.LogWarning("Sphere radius must be greater than 0!");
            return;
        }

        keyframes.Clear();

        float timeStep = totalDuration / (numKeyframes > 1 ? numKeyframes - 1 : 1);
        for (int i = 0; i < numKeyframes; i++)
        {
            float u = Random.value;
            float v = Random.value;

            float theta = 2f * Mathf.PI * u;
            float phi = Mathf.Acos(2f * v - 1f);

            float x = radius * Mathf.Sin(phi) * Mathf.Cos(theta);
            float y = radius * Mathf.Cos(phi);
            float z = radius * Mathf.Sin(phi) * Mathf.Sin(theta);
            Vector3 position = center + new Vector3(x, y, z);

            Quaternion rotation = Quaternion.LookRotation(center - position, Vector3.up);

            CameraKeyframe newKeyframe = new CameraKeyframe
            {
                time = i * timeStep,
                position = position,
                rotation = rotation,
                fov = targetCamera.fieldOfView,
                near = targetCamera.nearClipPlane,
                far = targetCamera.farClipPlane
            };
            keyframes.Add(newKeyframe);
            Debug.Log($"Added keyframe {i} on sphere at ({position.x:F2}, {position.y:F2}, {position.z:F2})");
        }

        totalDuration = numKeyframes > 1 ? keyframes[numKeyframes - 1].time : 0f;
        sphereCenter = center;
        sphereRadius = radius;
        showSphere = true;
        Debug.Log($"Generated {numKeyframes} random keyframes on sphere, center: {center}, radius: {radius}");
    }

    public void AddKeyframe()
    {
        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>() ?? Camera.main;
            if (targetCamera == null)
            {
                Debug.LogError("No Camera found to add keyframe!");
                return;
            }
        }

        CameraKeyframe newKeyframe = new CameraKeyframe
        {
            time = keyframes.Count > 0 ? keyframes[keyframes.Count - 1].time + 1f : 0f,
            position = transform.position,
            rotation = transform.rotation,
            fov = targetCamera.fieldOfView,
            near = targetCamera.nearClipPlane,
            far = targetCamera.farClipPlane
        };
        keyframes.Add(newKeyframe);
        Debug.Log($"Added keyframe at time {newKeyframe.time:F2}s");
    }

    public void RemoveKeyframe(int index)
    {
        if (index >= 0 && index < keyframes.Count)
        {
            keyframes.RemoveAt(index);
        }
    }

    public void ClearAllKeyframes()
    {
        keyframes.Clear();
    }

    public void SortKeyframes()
    {
        keyframes.Sort((a, b) => a.time.CompareTo(b.time));
        if (keyframes.Count > 0) totalDuration = keyframes[keyframes.Count - 1].time;
    }

    public void LoadFromJSON(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("JSON file not found: " + filePath);
            return;
        }

        string json = File.ReadAllText(filePath);
        CameraPathData data = JsonUtility.FromJson<CameraPathData>(json);

        keyframes = data.path;
        totalDuration = data.totalDuration;
        fps = data.fps;

        Debug.Log("Loaded camera path from: " + filePath);
    }

    public void ExportToJSON(string filePath)
    {
        CameraPathData data = new CameraPathData { path = keyframes, totalDuration = totalDuration, fps = fps };
        File.WriteAllText(filePath, JsonUtility.ToJson(data, true));
        Debug.Log("Saved JSON to: " + filePath);
    }

    public void ExportFrames()
    {
        StartCoroutine(ExportFramesFromKeyframesCoroutine());
    }

    private IEnumerator ExportFramesFromKeyframesCoroutine()
    {
        if (keyframes.Count == 0)
        {
            Debug.LogWarning("No keyframes available to export images!");
            yield break;
        }

        string folderPath = Path.Combine(Application.dataPath, exportFolder);
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        float originalTimer = timer;
        bool originalPlaying = isPlaying;

        Debug.Log($"Starting export of {keyframes.Count} images from keyframes");

        for (int i = 0; i < keyframes.Count; i++)
        {
            timer = keyframes[i].time;
            UpdateCameraPosition();

            yield return new WaitForEndOfFrame();

            string fileName = $"{framePrefix}{i:D03}.png";
            string filePath = Path.Combine(folderPath, fileName);
            ScreenCapture.CaptureScreenshot(filePath);
            Debug.Log($"Exported image for keyframe {i}: {fileName}");
        }

        isPlaying = originalPlaying;
        timer = originalTimer;
        Debug.Log($"Export of {keyframes.Count} images completed at: {folderPath}");
    }

    public void PlayPath()
    {
        if (keyframes.Count < 2)
        {
            Debug.LogWarning("At least 2 keyframes are required to play!");
            return;
        }
        timer = 0f;
        isPlaying = true;
    }

    public void LoadAndDrawAllPathsFromFolder()
    {
        multiCameraPaths.Clear();

        if (!Directory.Exists(jsonFolderPath))
        {
            Debug.LogError($"Folder {jsonFolderPath} does not exist!");
            return;
        }

        string[] jsonFiles = Directory.GetFiles(jsonFolderPath, "*.json");
        foreach (string filePath in jsonFiles)
        {
            string json = File.ReadAllText(filePath);
            CameraPathData data = JsonUtility.FromJson<CameraPathData>(json);
            if (data != null && data.path != null && data.path.Count > 0)
            {
                multiCameraPaths.Add(data);
                Debug.Log($"Loaded camera path from {Path.GetFileName(filePath)} with {data.path.Count} keyframes.");
            }
        }

        if (multiCameraPaths.Count == 0)
            Debug.LogWarning("No valid JSON files found in the folder.");
    }

    public void ClearAllMultiPaths()
    {
        multiCameraPaths.Clear();
        Debug.Log("Cleared all multi-camera paths.");
    }

    void OnDrawGizmos()
    {
        if (showSphere && sphereRadius > 0)
        {
            Gizmos.color = new Color(1, 1, 0, 0.3f);
            Gizmos.DrawWireSphere(sphereCenter, sphereRadius);
            Gizmos.color = new Color(1, 1, 0, 0.1f);
            Gizmos.DrawSphere(sphereCenter, sphereRadius);
        }

        if (!showPath || keyframes.Count < 2) return;

        Gizmos.color = pathColor;
        for (int i = 0; i < keyframes.Count - 1; i++)
        {
            Gizmos.DrawLine(keyframes[i].position, keyframes[i + 1].position);
        }

        if (showKeyframes)
        {
            Gizmos.color = keyframeColor;
            for (int i = 0; i < keyframes.Count; i++)
            {
                Gizmos.DrawSphere(keyframes[i].position, 0.1f);
                Handles.Label(keyframes[i].position + Vector3.up * 0.2f, $"KF {i}\nTime: {keyframes[i].time:F2}s");
            }
        }

        if (multiCameraPaths.Count > 0)
        {
            for (int pathIndex = 0; pathIndex < multiCameraPaths.Count; pathIndex++)
            {
                var path = multiCameraPaths[pathIndex].path;
                if (path == null || path.Count < 2) continue;

                Gizmos.color = Color.HSVToRGB((float)pathIndex / multiCameraPaths.Count, 1f, 1f);
                for (int i = 0; i < path.Count - 1; i++)
                {
                    Gizmos.DrawLine(path[i].position, path[i + 1].position);
                }

                Gizmos.color = Gizmos.color * 0.8f; 
                for (int i = 0; i < path.Count; i++)
                {
                    Gizmos.DrawSphere(path[i].position, 0.08f);
                }
            }
        }
    }

    public string GetDebugInfo()
    {
        return $"Keyframes: {keyframes.Count}\n" +
               $"Total Duration: {totalDuration:F2}s\n" +
               $"FPS: {fps}\n" +
               $"Playing: {isPlaying}\n" +
               $"Current Time: {timer:F2}s";
    }
}