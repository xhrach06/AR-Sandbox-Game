# AR Sandbox Tower Defense

## Project Structure

The project is organized into the following folders:

- `app/`  
  Contains the compiled and built game `AR Sandbox Tower Defense.exe` along with all required runtime files (e.g., `*_Data/`, DLL libraries, Mono runtime). This is the standalone executable version of the game.
- `src/`  
  Contains all Unity source files: scripts, scenes, prefabs, models, materials, textures, and other assets. This folder is meant to be opened and edited in the Unity Editor.
- `txt/`  
  Contains all LaTex source files needed for compilation.
- `readme.md`  
  This file — contains project description and build instructions.
- `plagat.pdf`  
  A poster describing the project.
- `xhrach06-AR_Sanbox_Game.pdf` Text of the thesis

---

## How to Build the Project (in Unity)

1. **Open Unity Hub**
   - Click "Open" and select the `src/` folder as the Unity project.
2. **Build the game**
   - Go to `File > Build Settings`
   - Select **PC, Mac & Linux Standalone**
   - Platform: **Windows**, Target: **x86_64**
   - Add all relevant scenes to the "Scenes In Build" list (MainMenu, CalibrationScene, KinectGameplayScene)
   - Click **Player Settings**
     - In the **Other Settings** tab, under **Configuration**, make sure: 
       - **Scripting Backend** is set to **Mono**
   - Close Player Settings
   - Click **Build**
   - Choose the `app/` folder as the output location (to match the provided structure)
3. **After the build**
   - Unity will generate an `.exe` file and corresponding folders/files:. These are required to run the game.
   - Copy KinectUnityAddin.dll from `src/ `directory into  AR Sandbox Tower Defense_Data/Plugins/x86_64 when building in a new folder

---

### Requirements

- **Unity Editor** (recommended version: 2020.3 LTS or compatible)
- **Windows OS** (build target is Windows x86_64)
- **Mono scripting backend** (set in Player Settings)
- **Kinect for Windows SDK 2.0**
- **Kinect v2 sensor**
- USB 3.0 port and compatible PC hardware

> ⚠️ This application is designed to work with Kinect v2 (Kinect for Xbox One) and uses the Kinect SDK 2.0. Make sure the SDK and drivers are properly installed before running the app.

---

## How to Play

Run `AR Sandbox Tower Defense.exe` from the `app/` directory. Make sure all accompanying folders and files are in the same directory.

## Gameplay Preview 🎮

Curious what the game looks like in action?  
Check out a short gameplay preview on YouTube:

👉 [Watch the demo here](https://www.youtube.com/watch?v=YOUR_VIDEO_ID)