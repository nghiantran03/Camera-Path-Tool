# Camera-Path-Tool
# Camera Path Controller

![Unity Version](https://img.shields.io/badge/Unity-2020.3%2B-blue.svg) ![License](https://img.shields.io/badge/License-MIT-green.svg)

A Unity tool designed to control camera trajectories using keyframes, enabling smooth path interpolation, spherical keyframe generation, and image export for viewport analysis in volumetric video streaming and cinematic visualization.

## Table of Contents
- [Introduction](#introduction)
- [Features](#features)
- [Requirements](#requirements)
- [Installation](#installation)
- [Usage](#usage)
- [Configuration](#configuration)
- [Contributing](#contributing)
- [License](#license)

## Introduction
The `CameraPathController` is a Unity script that facilitates the creation and management of camera paths through keyframes. It provides an intuitive interface within the Unity Inspector for adding, removing, and sorting keyframes, as well as generating spherical trajectories and exporting images from specific viewpoints. This tool is particularly useful for researchers and developers working on viewport adaptive volumetric video streaming or cinematic sequence generation.

## Features
- 🎮 **Keyframe Management**: Add, remove, sort, and navigate keyframes with real-time updates.
- 🌐 **Spherical Keyframe Generation**: Automatically generate keyframes on a spherical surface with uniform distribution.
- ▶️ **Path Playback**: Smoothly interpolate between keyframes using Lerp (positions) and Slerp (rotations).
- 📸 **Image Export**: Capture screenshots from keyframes for analysis.
- 📂 **JSON Serialization**: Save and load camera paths with parameters like FPS and duration.
- ℹ️ **Debug Info**: Display real-time debugging information in the Inspector.

## Requirements
- **Unity Version**: 2020.3 or later (tested on 2021.3 LTS).
- **Dependencies**: None (pure Unity C# scripts).
- **Operating System**: Windows, macOS, or Linux (Unity Editor compatible).

## Installation
1. **Clone or Download the Repository**:
   ```bash
   git clone https://github.com/nghiantran03/Camera-Path-Tool.git
2. **Open the Project in Unity**
- Browse to the CameraPathController folder (where Assets, Packages, and ProjectSettings folders reside) and select it.
- Click "Open" to add the project to Unity Hub.
- After opening, confirm the following files are present in the Project window:
+ Assets/Scripts/CameraPathController.cs: The main script for camera path control.
+ Assets/Scripts/CameraPathEditor.cs: The custom editor script for the Inspector interface. If these files are missing, ensure you downloaded the full repository correctly.
3. **Set Up in a Scene**
In the Hierarchy, right-click and select "Create Empty" to create a new GameObject.
Select the GameObject, click "Add Component" in the Inspector, and search for "CameraPathController".
Add a Camera component if not already present, then add tag named MainCamera for that added Camera.
## Usage
Select the GameObject with CameraPathController in the Hierarchy.
The custom editor interface will appear in the Inspector (powered by CameraPathEditor.cs).
- **Manage Keyframes:**
+ Add Keyframe (➕): Click "Add Keyframe" to capture the current camera position and settings.
+ Remove Keyframe (🗑️): Click "🗑️" next to a keyframe in the list to delete it.
+ Sort Keyframes (🔄): Click "Sort Keyframes" to reorder by time.
+ Navigate (➡️): Click "➡️" next to a keyframe to move the camera to that position.
- **Generate Spherical Keyframes:**
+ Under "Generate Spherical Keyframes":
+ Set Sphere Center (e.g., (0, 0, 0)).
+ Set Radius (e.g., 5).
+ Set Number of Keyframes (e.g., 8).
+ Click "🟢 Generate Spherical Keyframes" to create a random spherical distribution.
- **Playback the Path:**
+ Click "▶️ Play" to animate the camera along the defined path.
+ Monitor the movement in the Scene View (gizmos) or Game View.
- **Export Images:**
+ Configure Export Folder (e.g., ExportedFrames) and Frame Prefix (e.g., frame_) in "Frame Export Options".
+ Click "📷 Export Keyframe Images" to save PNGs to Assets/ExportedFrames.
- **Save or Load Paths:**
+ Set JSON Path (e.g., Assets/CameraPathData.json).
+ Click "💾 Export to JSON" to save or "📂 Load from JSON" to load a path.
## Configuration
- **Display Options:**
Show Path: Toggle to display the camera path (default: true).
Show Keyframes: Toggle to display keyframe positions (default: true).
Path Color: Customize the path color (default: green).
Keyframe Color: Customize the keyframe color (default: red).
Sphere Visualization:
Show Sphere: Toggle to display the spherical trajectory (default: false).
Sphere Center & Sphere Radius: Set via script or Inspector.
- **Frame Export:**
Default folder: Assets/ExportedFrames.
Files are named as [Frame Prefix][Index].png (e.g., frame_001.png).
- **Playback Settings:**
Total Duration: Total time of the path in seconds (default: 5).
FPS: Frames Per Second for metadata (default: 30).
- **Example JSON Output:**

