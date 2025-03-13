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
    public float fov; // Chỉ dùng trong runtime, không lưu trong trajectory
    public float near; // Chỉ dùng trong runtime, không lưu trong trajectory
    public float far;  // Chỉ dùng trong runtime, không lưu trong trajectory
}

[System.Serializable]
public class CameraTrajectoryData
{
    public float fovy;
    public string aspect = "1024/768";
    public float near;
    public float far;
    public float totalDuration;
    public int fps;
    public float scale = 1f;
    public List<CameraTrajectoryKeyframe> trajectory = new List<CameraTrajectoryKeyframe>();
}

[System.Serializable]
public class CameraTrajectoryKeyframe
{
    public float time;
    public float[] position; // Thay Vector3 bằng mảng float[3]
    public float[] rotation; // Thay Quaternion bằng mảng float[4] (x, y, z, w)
}

[System.Serializable]
public class CameraPathData
{
    public CameraTrajectoryData camera = new CameraTrajectoryData();
}

[System.Serializable]
public class CameraKeyframeOption2
{
    public float time;
    public float[] position; // Thay Vector3 bằng mảng float[3]
    public float[] lookAt;   // Thay Vector3 bằng mảng float[3]
    public float[] up;       // Thay Vector3 bằng mảng float[3]
}

[System.Serializable]
public class CameraTrajectoryDataOption2
{
    public float fovy;
    public string aspect = "1024/768";
    public float near;
    public float far;
    public float totalDuration;
    public int fps;
    public float scale;
    public List<CameraKeyframeOption2> trajectory = new List<CameraKeyframeOption2>();
}

[System.Serializable]
public class CameraPathDataOption2
{
    public CameraTrajectoryDataOption2 camera = new CameraTrajectoryDataOption2();
}

public class CameraPathController : MonoBehaviour
{
    [Header("Camera Motion Configuration")]
    public List<CameraKeyframe> keyframes = new List<CameraKeyframe>();
    public float totalDuration = 5f;
    public int fps = 30;
    public float scale = 1f;

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
    [SerializeField] public string jsonFolderPath = "Assets/JSONPaths";

    [Header("Camera Path Export")]
    [SerializeField] public string fullPathJsonPath = "Assets/FullCameraPath.json";

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

    public void LoadFromJSON(string filePath, bool useLookAtUp = false)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("JSON file not found: " + filePath);
            return;
        }

        string json = File.ReadAllText(filePath);
        if (useLookAtUp)
        {
            CameraPathDataOption2 dataOption2 = JsonUtility.FromJson<CameraPathDataOption2>(json);
            List<CameraKeyframe> loadedKeyframes = new List<CameraKeyframe>();
            foreach (var kf in dataOption2.camera.trajectory)
            {
                Vector3 realPosition = new Vector3(kf.position[0], kf.position[1], kf.position[2]) * dataOption2.camera.scale;
                Vector3 lookAt = new Vector3(kf.lookAt[0], kf.lookAt[1], kf.lookAt[2]);
                Vector3 up = new Vector3(kf.up[0], kf.up[1], kf.up[2]);
                loadedKeyframes.Add(new CameraKeyframe
                {
                    time = kf.time,
                    position = realPosition,
                    rotation = Quaternion.LookRotation(lookAt, up),
                    fov = dataOption2.camera.fovy,
                    near = dataOption2.camera.near,
                    far = dataOption2.camera.far
                });
            }
            keyframes = loadedKeyframes;
            totalDuration = dataOption2.camera.totalDuration;
            fps = dataOption2.camera.fps;
            scale = dataOption2.camera.scale;
        }
        else
        {
            CameraPathData data = JsonUtility.FromJson<CameraPathData>(json);
            float loadedScale = data.camera.scale;
            List<CameraKeyframe> loadedKeyframes = new List<CameraKeyframe>();
            foreach (var kf in data.camera.trajectory)
            {
                Vector3 position = new Vector3(kf.position[0], kf.position[1], kf.position[2]) * loadedScale;
                Quaternion rotation = new Quaternion(kf.rotation[0], kf.rotation[1], kf.rotation[2], kf.rotation[3]);
                loadedKeyframes.Add(new CameraKeyframe
                {
                    time = kf.time,
                    position = position,
                    rotation = rotation,
                    fov = data.camera.fovy,
                    near = data.camera.near,
                    far = data.camera.far
                });
            }
            keyframes = loadedKeyframes;
            totalDuration = data.camera.totalDuration;
            fps = data.camera.fps;
            scale = data.camera.scale;
        }

        Debug.Log("Loaded camera path from: " + filePath + (useLookAtUp ? " (LookAt+Up)" : " (rotation)"));
    }

    public void ExportToJSON(string filePath, bool useLookAtUp = false)
    {
        if (!filePath.EndsWith(".json", System.StringComparison.OrdinalIgnoreCase))
        {
            filePath += ".json";
        }

        if (useLookAtUp)
        {
            CameraPathDataOption2 dataOption2 = new CameraPathDataOption2();
            dataOption2.camera = new CameraTrajectoryDataOption2
            {
                fovy = targetCamera != null ? targetCamera.fieldOfView : 60f,
                near = targetCamera != null ? targetCamera.nearClipPlane : 0.1f,
                far = targetCamera != null ? targetCamera.farClipPlane : 1000f,
                totalDuration = totalDuration,
                fps = fps,
                scale = scale,
                trajectory = new List<CameraKeyframeOption2>()
            };

            foreach (var kf in keyframes)
            {
                Vector3 scaledPosition = kf.position / scale;
                Vector3 lookAt = kf.rotation * Vector3.forward;
                Vector3 up = kf.rotation * Vector3.up;
                dataOption2.camera.trajectory.Add(new CameraKeyframeOption2
                {
                    time = kf.time,
                    position = new float[] { scaledPosition.x, scaledPosition.y, scaledPosition.z },
                    lookAt = new float[] { lookAt.x, lookAt.y, lookAt.z },
                    up = new float[] { up.x, up.y, up.z }
                });
            }

            File.WriteAllText(filePath, JsonUtility.ToJson(dataOption2, true));
        }
        else
        {
            CameraPathData data = new CameraPathData();
            data.camera = new CameraTrajectoryData
            {
                fovy = targetCamera != null ? targetCamera.fieldOfView : 60f,
                near = targetCamera != null ? targetCamera.nearClipPlane : 0.1f,
                far = targetCamera != null ? targetCamera.farClipPlane : 1000f,
                totalDuration = totalDuration,
                fps = fps,
                scale = scale,
                trajectory = new List<CameraTrajectoryKeyframe>()
            };

            foreach (var kf in keyframes)
            {
                Vector3 scaledPosition = kf.position / scale;
                data.camera.trajectory.Add(new CameraTrajectoryKeyframe
                {
                    time = kf.time,
                    position = new float[] { scaledPosition.x, scaledPosition.y, scaledPosition.z },
                    rotation = new float[] { kf.rotation.x, kf.rotation.y, kf.rotation.z, kf.rotation.w }
                });
            }

            File.WriteAllText(filePath, JsonUtility.ToJson(data, true));
        }
        Debug.Log("Saved JSON to: " + filePath + (useLookAtUp ? " (LookAt+Up)" : " (rotation)"));
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

    public void LoadAndDrawAllPathsFromFolder(bool useLookAtUp = false)
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
            if (useLookAtUp)
            {
                CameraPathDataOption2 dataOption2 = JsonUtility.FromJson<CameraPathDataOption2>(json);
                List<CameraKeyframe> loadedKeyframes = new List<CameraKeyframe>();
                foreach (var kf in dataOption2.camera.trajectory)
                {
                    Vector3 realPosition = new Vector3(kf.position[0], kf.position[1], kf.position[2]) * dataOption2.camera.scale;
                    Vector3 lookAt = new Vector3(kf.lookAt[0], kf.lookAt[1], kf.lookAt[2]);
                    Vector3 up = new Vector3(kf.up[0], kf.up[1], kf.up[2]);
                    loadedKeyframes.Add(new CameraKeyframe
                    {
                        time = kf.time,
                        position = realPosition,
                        rotation = Quaternion.LookRotation(lookAt, up),
                        fov = dataOption2.camera.fovy,
                        near = dataOption2.camera.near,
                        far = dataOption2.camera.far
                    });
                }
                multiCameraPaths.Add(new CameraPathData
                {
                    camera = new CameraTrajectoryData
                    {
                        trajectory = new List<CameraTrajectoryKeyframe>(),
                        totalDuration = dataOption2.camera.totalDuration,
                        fps = dataOption2.camera.fps,
                        scale = dataOption2.camera.scale
                    }
                });
                foreach (var kf in loadedKeyframes)
                {
                    multiCameraPaths[multiCameraPaths.Count - 1].camera.trajectory.Add(new CameraTrajectoryKeyframe
                    {
                        time = kf.time,
                        position = new float[] { kf.position.x, kf.position.y, kf.position.z },
                        rotation = new float[] { kf.rotation.x, kf.rotation.y, kf.rotation.z, kf.rotation.w }
                    });
                }
            }
            else
            {
                CameraPathData data = JsonUtility.FromJson<CameraPathData>(json);
                float loadedScale = data.camera.scale;
                List<CameraKeyframe> loadedKeyframes = new List<CameraKeyframe>();
                foreach (var kf in data.camera.trajectory)
                {
                    Vector3 position = new Vector3(kf.position[0], kf.position[1], kf.position[2]) * loadedScale;
                    Quaternion rotation = new Quaternion(kf.rotation[0], kf.rotation[1], kf.rotation[2], kf.rotation[3]);
                    loadedKeyframes.Add(new CameraKeyframe
                    {
                        time = kf.time,
                        position = position,
                        rotation = rotation,
                        fov = data.camera.fovy,
                        near = data.camera.near,
                        far = data.camera.far
                    });
                }
                multiCameraPaths.Add(new CameraPathData
                {
                    camera = new CameraTrajectoryData
                    {
                        trajectory = new List<CameraTrajectoryKeyframe>(),
                        totalDuration = data.camera.totalDuration,
                        fps = data.camera.fps,
                        scale = data.camera.scale
                    }
                });
                foreach (var kf in loadedKeyframes)
                {
                    multiCameraPaths[multiCameraPaths.Count - 1].camera.trajectory.Add(new CameraTrajectoryKeyframe
                    {
                        time = kf.time,
                        position = new float[] { kf.position.x, kf.position.y, kf.position.z },
                        rotation = new float[] { kf.rotation.x, kf.rotation.y, kf.rotation.z, kf.rotation.w }
                    });
                }
            }
            Debug.Log($"Loaded camera path from {Path.GetFileName(filePath)} with {multiCameraPaths[multiCameraPaths.Count - 1].camera.trajectory.Count} keyframes" + (useLookAtUp ? " (LookAt+Up)" : " (rotation)"));
        }

        if (multiCameraPaths.Count == 0)
            Debug.LogWarning("No valid JSON files found in the folder.");
    }

    public void ClearAllMultiPaths()
    {
        multiCameraPaths.Clear();
        Debug.Log("Cleared all multi-camera paths.");
    }

    public void ExportFullCameraPath(bool useLookAtUp = false)
    {
        if (keyframes.Count < 2)
        {
            Debug.LogWarning("At least 2 keyframes are required to export full camera path!");
            return;
        }

        int totalFrames = Mathf.FloorToInt(totalDuration * fps) + 1;
        float frameTimeStep = totalDuration / (totalFrames - 1);

        List<CameraKeyframe> fullPath = new List<CameraKeyframe>();

        for (int frame = 0; frame < totalFrames; frame++)
        {
            float currentTime = frame * frameTimeStep;

            CameraKeyframe kf0 = keyframes[0];
            CameraKeyframe kf1 = keyframes[1];
            for (int i = 0; i < keyframes.Count - 1; i++)
            {
                if (currentTime >= keyframes[i].time && currentTime <= keyframes[i + 1].time)
                {
                    kf0 = keyframes[i];
                    kf1 = keyframes[i + 1];
                    break;
                }
            }

            if (currentTime < keyframes[0].time)
            {
                kf0 = kf1 = keyframes[0];
            }
            else if (currentTime > keyframes[keyframes.Count - 1].time)
            {
                kf0 = kf1 = keyframes[keyframes.Count - 1];
            }

            float segmentDuration = kf1.time - kf0.time;
            float localT = segmentDuration > 0 ? (currentTime - kf0.time) / segmentDuration : 1f;

            CameraKeyframe interpolatedFrame = new CameraKeyframe
            {
                time = currentTime,
                position = Vector3.Lerp(kf0.position, kf1.position, localT),
                rotation = Quaternion.Slerp(kf0.rotation, kf1.rotation, localT),
                fov = Mathf.Lerp(kf0.fov, kf1.fov, localT),
                near = Mathf.Lerp(kf0.near, kf1.near, localT),
                far = Mathf.Lerp(kf0.far, kf1.far, localT)
            };

            fullPath.Add(interpolatedFrame);
        }

        if (!fullPathJsonPath.EndsWith(".json", System.StringComparison.OrdinalIgnoreCase))
        {
            fullPathJsonPath += ".json";
        }

        if (useLookAtUp)
        {
            CameraPathDataOption2 dataOption2 = new CameraPathDataOption2();
            dataOption2.camera = new CameraTrajectoryDataOption2
            {
                fovy = targetCamera != null ? targetCamera.fieldOfView : 60f,
                near = targetCamera != null ? targetCamera.nearClipPlane : 0.1f,
                far = targetCamera != null ? targetCamera.farClipPlane : 1000f,
                totalDuration = totalDuration,
                fps = fps,
                scale = scale,
                trajectory = new List<CameraKeyframeOption2>()
            };

            foreach (var kf in fullPath)
            {
                Vector3 scaledPosition = kf.position / scale;
                Vector3 lookAt = kf.rotation * Vector3.forward;
                Vector3 up = kf.rotation * Vector3.up;
                dataOption2.camera.trajectory.Add(new CameraKeyframeOption2
                {
                    time = kf.time,
                    position = new float[] { scaledPosition.x, scaledPosition.y, scaledPosition.z },
                    lookAt = new float[] { lookAt.x, lookAt.y, lookAt.z },
                    up = new float[] { up.x, up.y, up.z }
                });
            }

            File.WriteAllText(fullPathJsonPath, JsonUtility.ToJson(dataOption2, true));
        }
        else
        {
            CameraPathData data = new CameraPathData();
            data.camera = new CameraTrajectoryData
            {
                fovy = targetCamera != null ? targetCamera.fieldOfView : 60f,
                near = targetCamera != null ? targetCamera.nearClipPlane : 0.1f,
                far = targetCamera != null ? targetCamera.farClipPlane : 1000f,
                totalDuration = totalDuration,
                fps = fps,
                scale = scale,
                trajectory = new List<CameraTrajectoryKeyframe>()
            };

            foreach (var kf in fullPath)
            {
                Vector3 scaledPosition = kf.position / scale;
                data.camera.trajectory.Add(new CameraTrajectoryKeyframe
                {
                    time = kf.time,
                    position = new float[] { scaledPosition.x, scaledPosition.y, scaledPosition.z },
                    rotation = new float[] { kf.rotation.x, kf.rotation.y, kf.rotation.z, kf.rotation.w }
                });
            }

            File.WriteAllText(fullPathJsonPath, JsonUtility.ToJson(data, true));
        }

        AssetDatabase.Refresh();
        Debug.Log($"Exported full camera path with {totalFrames} frames to: {fullPathJsonPath}" + (useLookAtUp ? " (LookAt+Up)" : " (rotation)"));
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
                var trajectory = multiCameraPaths[pathIndex].camera.trajectory;
                if (trajectory == null || trajectory.Count < 2) continue;

                Gizmos.color = Color.HSVToRGB((float)pathIndex / multiCameraPaths.Count, 1f, 1f);
                for (int i = 0; i < trajectory.Count - 1; i++)
                {
                    Vector3 pos1 = new Vector3(trajectory[i].position[0], trajectory[i].position[1], trajectory[i].position[2]);
                    Vector3 pos2 = new Vector3(trajectory[i + 1].position[0], trajectory[i + 1].position[1], trajectory[i + 1].position[2]);
                    Gizmos.DrawLine(pos1, pos2);
                }

                Gizmos.color = Gizmos.color * 0.8f;
                for (int i = 0; i < trajectory.Count; i++)
                {
                    Vector3 pos = new Vector3(trajectory[i].position[0], trajectory[i].position[1], trajectory[i].position[2]);
                    Gizmos.DrawSphere(pos, 0.08f);
                }
            }
        }
    }

    public string GetDebugInfo()
    {
        return $"Keyframes: {keyframes.Count}\n" +
               $"Total Duration: {totalDuration:F2}s\n" +
               $"FPS: {fps}\n" +
               $"Scale: {scale}\n" +
               $"Playing: {isPlaying}\n" +
               $"Current Time: {timer:F2}s";
    }
}
