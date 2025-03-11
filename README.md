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
