# Unity Movement Framework

A state-machine-driven framework for first-person character movement in Unity, with a sample character built on it.

> **Work in progress.** The architecture is settled, but the sample has one movement mode so far and the APIs may still change.

## Why

Character controllers often start as one `Update()` method and grow until every new ability puts the old ones at risk. This framework keeps movement code composable as it grows:

- **Abilities are isolated.** Each ability is its own mode, built from small state machines, so adding a slide or a wallrun does not mean editing jumping.
- **The core is generic.** It knows nothing about any concrete ability, scene or input asset. Everything concrete lives in the sample, which you extend or replace.
- **Movement is frame-rate independent.** The simulation runs only on the physics clock, and drawing carries the latest physics step forward to the current frame, so motion and mouse look stay smooth.
- **Input is never dropped.** Presses and mouse movement read each frame are carried to the next physics step, so none are lost between them.
- **Everything is tunable live.** Every tuning value is exposed in the inspector and can be changed during Play mode.

## Getting started

Requires Unity **6000.3.9f1**. The Universal Render Pipeline and Input System 1.18.0 are listed in `Packages/manifest.json`, so Unity installs them on first open.

1. Clone the repository and open the folder in Unity Hub with Unity 6000.3.9f1.
2. Open `Assets/Src/Radknee/MovementFramework/Examples/ExampleScene.unity` and press Play.

The cursor locks when Play starts. Press Escape to release it, and click in the Game view to lock it again.

| Input | Action |
| --- | --- |
| W A S D | Move |
| Mouse | Look |
| Space | Jump: tap for a hop, hold for a full jump, press again in the air to double jump |
| Left Shift | Sprint |
| Left mouse button | Fire a test rocket launcher that blasts the character around |

## Installing in a game

The framework is a UPM package, `com.radknee.movementframework`, with the example included. In your game, open **Window > Package Manager**, choose **+ > Install package from git URL**, and enter:

```
https://github.com/RandyRoberts99/UnityMovementFramework.git?path=/Assets/Src/Radknee/MovementFramework#v0.1.0
```

Change the tag at the end to move to another release. The Input System package is installed with it.

Your project needs these settings:

- **Edit > Project Settings > Player > Active Input Handling** set to **Input System Package** (or **Both**).
- **Project-wide actions** with an action map named `Default`. `InputService` reads that map and throws on startup without it. The simplest start is to copy `Examples/Inputs.inputactions` into your `Assets/` folder and assign it under **Edit > Project Settings > Input System Package**.
- **Edit > Project Settings > Physics > Auto Sync Transforms** off, which is Unity's default. The motor relies on it.

The example scene uses URP materials, so it only renders correctly in a URP project. Scripts work with any pipeline.

## Tuning

Select the **Scripts** object in the scene. Changes made during Play mode take effect immediately, but Unity discards them when you stop, so copy any values you want to keep back in edit mode.

- **Physics Provider**: speeds, acceleration and drag, jump power, gravity, coyote time, jump buffer, air jumps and pitch limits.
- **Input Provider**: look sensitivity.
- **Rocket Jump Tester**: the test blast's range, radius, strength and fade.

## Structure

```
MovementController (MonoBehaviour)  builds and enables the motor, and drives its clocks
  └─ MovementMotor                  runs the active mode, switches modes, moves and draws the character
       └─ MovementMode              one ability, e.g. DefaultMode; holds several providers
            └─ MovementProvider     one slice of the output; a state machine
                 └─ MovementState   one phase of that slice, e.g. Grounded, Jumping, Falling
```

The sample's `DefaultMode` has five providers:

| Provider | Owns | States |
| --- | --- | --- |
| `RotationProvider` | Character yaw and camera pitch | `RotatingState` |
| `HorizontalMovementProvider` | Velocity X and Z | `IdleState`, `MovingState` |
| `VerticalMovementProvider` | Velocity Y | `GroundedState`, `JumpingState`, `FallingState` |
| `CarryProvider` | No axis: adds the velocity of whatever the character stands on | `CarriedState`, `ReleasedState` |
| `PushProvider` | No axis: adds a push out of the way of moving colliders | `PushedState` |

A mode combines its providers' outputs each physics step. It **adds** their velocities and **multiplies** their rotations together. Each provider therefore owns a part of the output that no other provider writes: an axis of velocity, or a rotation. If two providers write the same axis, the values are added together and the movement doubles without any error. `CarryProvider` and `PushProvider` are the deliberate exceptions: they own no axis, and add an offset from the world on top of the character's own movement.

## Building on it

New behaviour goes in `Assets/Src/Radknee/MovementFramework/Examples/`, and the core at the root of `MovementFramework/` stays abstract. A new ability is a new mode under `Examples/Modes/`, a new phase of existing movement is a new state in an existing provider, and an external force is applied through `ImpulseEvents.ApplyImpulse()`.

The `CLAUDE.md` files in the repository root, `Assets/Src/Radknee/MovementFramework/` and `Assets/Src/Radknee/MovementFramework/Examples/Services/` are the full design notes: how the framework works, where each kind of code goes, and the rules that keep it working. They are written for both people and AI coding assistants.

The framework compiles into two assemblies: `Radknee.MovementFramework` for the core and `Radknee.MovementFramework.Examples` for the example. To compile without opening Unity, run `dotnet build Radknee.MovementFramework.Examples.csproj`. The project files are generated by Unity and not committed. If they are missing or out of date, regenerate them under **Edit > Preferences > External Tools > Regenerate project files**.

## License

Released under the MIT License. See [LICENSE](LICENSE).
