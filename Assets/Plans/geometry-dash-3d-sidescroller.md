# Project Overview
- **Game Title:** Neon Dash 3D (3D Geometry Dash-style Sidescroller)
- **High-Level Concept:** A high-speed rhythmic 3D sidescroller where the player cube automatically traverses left to right across obstacle-laden geometric terrain. Players jump over spikes, bounce off launch pads, and instantly restart upon colliding with hazards, racing to complete the course at 100%.
- **Players:** Single-player
- **Inspiration / Reference Games:** Geometry Dash, The Impossible Game, Bit.Trip Runner
- **Tone / Art Direction:** Vibrant neon synthwave geometric aesthetic with glowing emissive URP materials, stylized bloom, contrasting dark floor matrices, and sharp geometric silhouettes (cubes, pyramids, glowing pads).
- **Target Platform:** WebGL / PC
- **Screen Orientation / Resolution:** Landscape 16:9 (1920x1080)
- **Render Pipeline:** Universal Render Pipeline (URP)

---

# Game Mechanics

## Core Gameplay Loop
1. **Constant Horizontal Propulsion:** The player cube moves continuously from left to right along the positive X-axis at a constant, unyielding forward velocity (e.g., 9.5 m/s).
2. **Arcade Gravity & Snappy Jumping:** The player is governed by custom snappy vertical physics (elevated gravity around -32 m/s² to prevent floatiness). Tapping or holding jump while grounded imparts an instantaneous vertical velocity impulse.
3. **Mid-Air Cube Rotation:** In mid-air, the cube smoothly spins around the Z-axis (90° per jump or continuous 360° rotation) and snaps flush to the nearest 90-degree alignment upon touching a flat surface.
4. **Interactive Bounce Surfaces (Jump Pads):** Landing on or touching glowing bounce pads instantly applies an upward impulse (e.g., +15 m/s) with a pad compression animation, propelling the player across wide hazard chasms without requiring manual jump input.
5. **Hazards & Instant Death:** Spikes (3D pyramid colliders) and frontal wall impacts trigger instant player destruction (cube shatter particle burst). The attempt counter increments and the player respawns at the beginning of the level (or checkpoint) with zero delay or scene reload lag.
6. **Level Progression & Finish Trigger:** A continuous progress calculation tracks the player's X position relative to the course end. Reaching the finish portal / goal trigger locks input and displays the victory fanfare.

## Controls and Input Methods
- **Active Backend:** Unity New Input System (`UnityEngine.InputSystem`).
- **Control Bindings:**
  - **Spacebar / Up Arrow / W** (Keyboard)
  - **Left Mouse Click** (Mouse)
  - **Primary Screen Tap / Hold** (Touchscreen for mobile/tablet WebGL)
  - **South Button / A** (Gamepad)
- **Input Buffering:** 0.1-second jump buffer queue so pressing the jump button right before landing executes immediately upon ground contact.

---

# UI

## HUD & Screen Flow
- **Top HUD:**
  - **Progress Bar:** Thin neon slider running across the top screen edge showing current progress from 0% to 100%.
  - **Progress Label:** TextMeshPro display (e.g., `48%`).
  - **Attempt Counter:** Clean corner indicator (e.g., `Attempt 1`).
- **Death Feedback:**
  - Immediate particle burst at player position, brief camera shake (0.15s), fast fade/snap back to spawn point.
- **Level Complete Modal:**
  - Congratulatory banner (`100% COMPLETE!`), total attempts summary, and "Play Again" button.

### UI Mockup Wireframe
```
+-----------------------------------------------------------------+
| [Attempt 1]               ======[ 54% ]======                   |
|                                                                 |
|                                                                 |
|       (Player Cube)                                             |
|        [#] -->                                                  |
|  ==========       /\  /\      [BOUNCE]      ==========   [GOAL] |
|  [Platform]      [Spikes]      [PAD]       [Platform]    [PORTAL|
+-----------------------------------------------------------------+
```

---

# Key Asset & Context

### Scripts (`Assets/Scripts/GeometryDash3D/`)
1. **`PlayerController3D.cs`:** 
   - Manages continuous X translation, vertical velocity with custom gravity, ground raycasting/boxcasting, mid-air rotation, bounce pad interaction, and death detection.
   - Constrains movement strictly to the X-Y plane (Z position locked at 0).
2. **`PlayerInputHandler.cs`:**
   - Reads jump trigger/hold events using the New Input System (`Keyboard.current`, `Mouse.current`, `Touchscreen.current`, or `InputAction`).
3. **`CameraController3D.cs`:**
   - Smooth 2.5D tracking camera that matches the player's X position, smoothly damps vertical Y changes, and maintains fixed Z distance with an appealing angled perspective.
4. **`Hazard.cs`:**
   - Attached to spikes, fatal obstacles, and side walls. Detects player collision and triggers `PlayerController3D.Die()`.
5. **`BounceSurface.cs`:**
   - Configurable launch impulse, trigger/collision behavior, and bounce squash visual feedback.
6. **`GoalTrigger.cs`:**
   - Detects when the player crosses the finish threshold and triggers level victory.
7. **`LevelManager.cs`:**
   - Coordinates player spawning, respawn timing, attempt count, progress calculation, and UI communication.
8. **`LevelProgressUI.cs`:**
   - Drives the TextMeshPro UI elements and progress slider.
9. **`DemoLevelBuilder.cs` (Editor/Runtime setup):**
   - Spawns a complete, playable obstacle course with ground platforms, staggered spike pits, elevated steps, bounce pads, and a finish portal for immediate testing.

### Materials & Visual Assets
- Neon Cyan Glow Material (Player Cube)
- Danger Neon Red/Orange Material (Spike Pyramids / Hazards)
- Neon Yellow/Lime Material (Bounce Pads)
- Dark Tech / Midnight Blue Material (Floor & Platforms)
- Gold / Holographic Material (Finish Goal)

---

# Implementation Steps

### Step 1: Input and Player Controller Setup
- **Files:** `Assets/Scripts/GeometryDash3D/PlayerInputHandler.cs`, `Assets/Scripts/GeometryDash3D/PlayerController3D.cs`
- **Actions:**
  - Create `PlayerInputHandler` with New Input System support for Space, Left Click, and Touch.
  - Create `PlayerController3D` with constant horizontal speed, gravity acceleration, jump impulse, ground detection, and Z-axis locking.
  - Implement mid-air rotation around Z and ground snapping.
- **Dependencies:** None.

### Step 2: Camera Tracking System
- **Files:** `Assets/Scripts/GeometryDash3D/CameraController3D.cs`
- **Actions:**
  - Create camera follow script with configurable forward lead offset, vertical dampening, and fixed Z depth.
  - Add death camera shake effect hook.
- **Dependencies:** Step 1.

### Step 3: Interactive Hazards & Bounce Surfaces
- **Files:** `Assets/Scripts/GeometryDash3D/Hazard.cs`, `Assets/Scripts/GeometryDash3D/BounceSurface.cs`, `Assets/Scripts/GeometryDash3D/GoalTrigger.cs`
- **Actions:**
  - Implement `Hazard` component with tag/layer/interface check triggering instant player death.
  - Implement `BounceSurface` that overrides vertical velocity with configurable bounce force and triggers pad squash feedback.
  - Implement `GoalTrigger` notifying level completion.
- **Dependencies:** Step 1.

### Step 4: Level Management & UI
- **Files:** `Assets/Scripts/GeometryDash3D/LevelManager.cs`, `Assets/Scripts/GeometryDash3D/LevelProgressUI.cs`
- **Actions:**
  - Implement `LevelManager` tracking spawn position, attempt count, active state (Playing, Dead, Complete), and fast respawn without scene reloading.
  - Implement `LevelProgressUI` linking progress bar and attempt counter to player X position relative to goal X.
- **Dependencies:** Steps 1, 3.

### Step 5: Demo Level Construction & Scene Integration
- **Files:** `Assets/Scripts/GeometryDash3D/DemoLevelBuilder.cs`, `Assets/Scenes/GameScene.unity` (or dedicated setup)
- **Actions:**
  - Generate materials with URP Lit shaders and vibrant emissive colors.
  - Construct a full playable demo level featuring:
    - Starting safe runway
    - Single and double spike hazard jumps
    - Stepped platform climbs
    - Spring bounce pad jump over deep hazard pit
    - Ceiling hazard clearance section
    - Level finish portal
  - Configure lighting, Main Camera, and Canvas HUD.
- **Dependencies:** Steps 1, 2, 3, 4.

---

# Verification & Testing

### 1. Movement & Input Verification
- Confirm player automatically moves right at constant speed without horizontal deceleration.
- Press Spacebar, Left Mouse button, or Touch to verify responsive jump execution with zero input lag.
- Verify jumping while in mid-air is disallowed unless hitting a bounce pad.
- Verify player Z position remains locked at 0.0 at all times.

### 2. Bounce Surface Verification
- Move player onto a bounce pad without pressing jump; verify player is launched high into the air with the specified bounce impulse.

### 3. Hazard & Death Verification
- Walk or land directly into a spike hazard; verify immediate death trigger, cube deactivation/burst, attempt counter increment, and instant respawn at the start.
- Run into the front face of a block obstacle; verify it registers as a fatal crash and restarts the level.

### 4. Progression & Victory Verification
- Monitor the HUD progress bar to ensure it smoothly advances from 0% to 100% as the player progresses.
- Cross the finish portal; verify player movement stops, completion UI appears, and the player cannot trigger further deaths.

### 5. WebGL & Performance Validation
- Verify script compilation with zero errors or warnings.
- Confirm full compatibility with Unity 6000.3 and Universal Render Pipeline (URP).
