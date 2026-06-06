# Croc Hunter — Unity Game

## Project Overview
A Unity 6 game built with the Universal Render Pipeline (URP).

## Unity Version
**Unity 6000.3.5f2** (Unity 6)

## Key Packages
- **URP** (`com.unity.render-pipelines.universal` 17.3.0) — rendering pipeline
- **New Input System** (`com.unity.inputsystem` 1.17.0) — all input handling
- **Blamcon Lightguns** (`com.blamcon.lightguns` 1.0.0) — local package in `Packages/com.blamcon.lightguns/`; extends the Input System with HID support for the Blamcon lightgun (position, recoil, rumble, LED, ammo)
- **AI Navigation** (`com.unity.ai.navigation` 2.0.9) — NavMesh agents
- **Timeline** (`com.unity.timeline` 1.8.10)
- **Test Framework** (`com.unity.test-framework` 1.6.0)

## Project Structure
```
Assets/
  Scenes/               # Unity scene files
  Scripts/
    Lightgun/           # your own lightgun game scripts go here
  Samples/
    Blamcon Lightguns for Unity/1.0.0/
      Lightgun Crosshair/       # LightgunCrosshair.cs — crosshair movement sample
      Lightgun Recoil Command/  # LightgunTriggerAction.cs — trigger/recoil sample
  Input/
    LightgunInputActions.inputactions  # Input action map for the lightgun
  Settings/             # URP renderer and pipeline assets

Packages/
  com.blamcon.lightguns/   # Blamcon lightgun local package (Runtime scripts + Samples~)
```

## C# Scripting
- Target **.NET Standard 2.1** (Unity 6 default)
- Use the **New Input System** (`UnityEngine.InputSystem`), not the legacy `Input` class
- URP shaders: use `Shader Graphs` or `URP/Lit`, not `Standard` shader
- NavMesh uses `Unity.AI.Navigation` namespace (not legacy `UnityEngine.AI` NavMeshSurface)
- Blamcon gun accessed via `Lightgun` class from `Blamcon.Lightguns` namespace; requires firmware 1.0.16+

## Working with Claude Code
- Claude can read and write C# scripts, scene YAML, prefab YAML, and asset files
- Scene/prefab changes written by Claude take effect when Unity reimports them — keep Unity open or reimport manually
- Claude cannot run Play Mode, trigger builds, or execute Unity Editor scripts directly
- To test changes: switch to Unity Editor and press Play

## Conventions
- Scripts go in `Assets/Scripts/` (or a relevant subfolder)
- One MonoBehaviour per file, filename matches class name
- Use `[SerializeField] private` instead of `public` for Inspector-exposed fields
