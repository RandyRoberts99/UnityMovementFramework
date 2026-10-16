# Changelog

All notable changes to this package are documented here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and versions follow [Semantic Versioning](https://semver.org/).

## [0.1.0] - 2026-10-16

First release.

### Added

- The core: `MovementMotor`, `MovementMode`, `MovementProvider` and `MovementState`, the input and physics contexts, `ImpulseEvents` and `PhysicsUtilities`, in the `Radknee.MovementFramework` assembly.
- `MovementMotor` runs the active mode on the physics clock, moves the `CharacterController`, and draws the character between physics steps so motion and mouse look stay smooth at any frame rate.
- `MovementMotor.Enable()`/`Disable()`, tied to the component, and `Lock()`/`Unlock()` to pause movement without disabling it. `Unlock(dropImpulses)` keeps or drops the impulses held during the lock.
- External forces through `ImpulseEvents.ApplyImpulse()`, each fading over its own decay time with an optional falloff curve.
- The example, in the `Radknee.MovementFramework.Examples` assembly: `MovementController`, the input and physics services, the `PhysicsProvider` and `InputProvider` inspector settings, and `DefaultMode`, with:
  - Ground movement with acceleration, drag and sprint, and Quake-style air control.
  - Variable-height jumps, coyote time, a jump buffer and air jumps.
  - Yaw on the character and clamped pitch on the camera.
  - Carrying on moving and rotating platforms, and pushing out of the way of moving colliders.
- `ExampleScene` with test platforms and a rocket-jump tester.

### Known limitations

- Wallrunning, climbing and sliding are not implemented: `DefaultMode` has only placeholder checks for them.
- Impulses ignore collisions, so a push into a wall or ceiling keeps pressing until it fades.
- The example scene uses URP materials.
