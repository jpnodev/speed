# Expressive 3D Platformer - Player Movement Logic & State Machine

## 🌊 Core Philosophy
The core of this movement system is **momentum preservation**, **fluid state synergy**, and **context-sensitive physics**. The player rarely comes to a hard stop; instead, states flow into one another based on speed, slopes, and player input.

---

## 🏗️ Universal Systems & Physics Overrides

### 1. Ground Snapping (Convex Slopes)
*   **Problem:** Cresting a hill at high speeds normally launches physics bodies into the air.
*   **Solution:** The `PlayerSensors` continuously cast a ray/sphere downward. If the ground falls away beneath the player (like reaching the top of a hill), the controller forcefully snaps the player to the surface, maintaining their state and momentum.
*   **Exception:** The player is only launched into a trajectory if they explicitly press Jump.

### 2. Speed-Proportional Aerial Control
*   **Concept:** Air turning and steering (`airSteerVector` and `turnSpeed`) dynamically scale inversely to the player's forward velocity (potentially using an inverse square curve).
*   **Effect:**
    *   **Low Speed Jump:** Excellent air control, snappy rotation, easy precision platforming.
    *   **Mach / Slide Jump:** Forward momentum dominates. Steerability and mesh rotation are highly restricted, committing the player to their chosen arc (Mario 64 / Sonic Adventure style).

### 3. Coyote Time & Input Buffering
*   **Coyote Time:** Walking off a ledge allows a ~0.15s window to input a jump despite being technically airborne.
*   **Input Buffering:** Jump inputs are cached slightly before landing. This enables "bunny hopping," maintaining slide or mach momentum flawlessly upon connecting with the ground or a slope.

### 4. Wall Bonks
*   Hitting a wall kills forward momentum instantly ("bonk" behavior). There are no horizontal physics bounces.

---

## 🗺️ State Definitions & Transitions

### 🚶 IdleState
*   **Logic:** Player is grounded, directional input is 0, velocity is ~0.
*   **Transitions:**
    *   ➔ **MoveState:** Directional input magnitude > 0.
    *   ➔ **JumpState:** Jump button pressed.
    *   ➔ **FallState:** Ground lost (without jump).
    *   ➔ **SlideState:** Player is pushed onto a steep slope by external forces.

### 🏃 MoveState
*   **Logic:** Standard 360-degree analog movement.
*   **Transitions:**
    *   ➔ **IdleState:** Directional input released.
    *   ➔ **MachState:** "Boot/Sprint" button held, OR speed naturally hits the mach threshold.
    *   ➔ **BrakeState:** Input aggressively pushed in the opposite direction of current velocity.
    *   ➔ **SlideState:** Crouch pressed.
    *   ➔ **JumpState:** Jump pressed.
    *   ➔ **FallState:** Ground lost.

### ⚡ MachState
*   **Logic:** High-speed movement state. Wide turn radius. Speed can be retained passively without the button if momentum allows.
*   **Transitions:**
    *   ➔ **Move / Idle:** Speed drops below mach threshold and Sprint button is not held.
    *   ➔ **BrakeState:** Hard reversal of directional input.
    *   ➔ **SlideState:** Crouch pressed (carries massive momentum forward).
    *   ➔ **JumpMachState:** Jump pressed (maintains mach speed into the air).
    *   ➔ **FallState:** Ground lost.

### 🏂 SlideState
*   **Logic:** Physics-driven surfing state.
    *   *Velocity Rules:* Accelerates down steep slopes. Decelerates gradually on flat ground. Loses speed rapidly when sliding *up* a slope.
    *   *Control Rules:* Limited turning radius.
    *   *Entry Rules:* **Cannot** enter by crouching at 0 speed on flat ground. Must have sufficient entry velocity OR be standing on a slope steep enough to mandate sliding.
*   **Transitions:**
    *   ➔ **Idle / Move:** Velocity hits 0, slope is flat, and player is not holding crouch.
    *   ➔ **JumpSlideState:** Jump pressed.
    *   ➔ **FallState:** Ground lost.
*   **Edge Cases:**
    *   *The Backward Slide:* If running up a steep slope, velocity drops to 0, and gravity pulls the player back down into a slide, the player model will auto-rotate to face downhill.
    *   *Bunny Hopping into Slopes:* Landing from a jump onto a downslope while holding crouch seamlessly transfers fall velocity into massive Slide velocity.

### 🛑 BrakeState
*   **Logic:** A brief transition "skid" state for reversing direction rapidly.
*   **Transitions:**
    *   ➔ **MachState:** Player perfectly reverses input during the window, granting a burst of speed in the new direction.
    *   ➔ **Move / Idle:** Skid animation finishes or velocity zeros out.

### 🦘 JumpState (Standard)
*   **Logic:** Triggered from Idle/Move. Highest mid-air maneuverability. Releasing jump early cuts vertical velocity.
*   **Transitions:**
    *   ➔ **FallState:** Vertex of the jump reached (Y velocity <= 0), or jump released early.

### 🚀 JumpMachState / JumpSlideState
*   **Logic:** Heavy momentum jumps.
    *   *JumpMach:* Triggers from MachState. Flatter arc, high horizontal speed.
    *   *JumpSlide:* "Long Jump". Triggers from SlideState. Suppresses Y-axis lift for massive Z/X-axis projection.
    *   *Both:* Drastically reduced mid-air turn speed and steering, based on velocity square logic.
*   **Transitions:**
    *   ➔ **FallState:** Vertex reached or jump button released.
*   **Edge Case (*The Premature Up-Slope Jump*):** If player is decelerating backwards on a slope and enters JumpSlide *before* the model natively rotates downhill, the jump will not calculate forward uphill lift; it will map to their crippled momentum, acting as a punishing vertical "hop".

### 🪂 FallState
*   **Logic:** Unified downward mid-air physics. Mid-air maneuverability parameters are inherited cleanly from whatever velocity the player had when entering the state. Can be entered via walking off a ledge, jump apex, etc.
*   **Transitions:**
    *   ➔ **JumpState:** If triggered within the Coyote Time window.
    *   ➔ **Idle / Move:** Landing at low speed.
    *   ➔ **MachState:** Landing at high speed (and holding sprint).
    *   ➔ **SlideState:** Landing while holding crouch on flat/down-slope, retaining speed as slide.
