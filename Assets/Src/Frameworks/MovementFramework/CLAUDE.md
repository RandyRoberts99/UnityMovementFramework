# Movement framework

The **core** is the files at this folder's root: the four abstract layers, the contexts and the output interfaces, namespace `Radknee.MovementFramework`. It is generic and must never reference `Examples/`.

**`Examples/`** is the reference sample, namespace `Radknee.MovementFramework.Examples`. Every concrete mode, provider and state lives there, and so does `MovementController`, the `MonoBehaviour` that drives the framework. A new movement ability is sample content. The core changes only when the ability needs a new *generic* contract, such as a property on `IPhysicsContext`.

## Where movement code goes

Pick the layer first, by what the new behaviour does:

| The behaviour… | Layer | Goes in |
| --- | --- | --- |
| Is a distinct ability (slide, dash, wallrun, climb), even if it changes only one slice | New mode, with its own providers | `Examples/<Name>Mode/<Name>Mode.cs` |
| Needs an output slice no provider in its mode owns (an axis of velocity, or a rotation) | New provider and its states | `Examples/<Mode>/<Slice>/` |
| Is a new phase of the locomotion a provider already runs (e.g. a landing phase between `FallingState` and `GroundedState`) | New state in that provider | Beside that provider |
| Changes what an existing phase does | That state's `Start()`/`Process()` | — |
| Changes when phases change | That state's `Switch()` | — |
| Moves between modes | The *current* mode's `Switch()` | — |
| Needs a tuned number | Settings property on `IPhysicsContext` | See `Assets/Src/Services/CLAUDE.md` |
| Needs memory across steps, entries or states (timers, crouch height) | Runtime property on `IPhysicsContext`, reset in `Start()` if per-entry | — |
| Checks or spends a resource (stamina, dash charges) | Reads and spends through `IPlayerContext`; never regenerates it | Planned: see `Assets/Src/Services/CLAUDE.md` |
| Needs to know about the world (a wall, a ledge) | A `Physics.*` query in the state or mode whose `Switch()`/`Process()` makes the decision | — |
| Changes how the character's pose is drawn between steps | `MovementController.Extrapolate()`, from motor outputs and contexts only | — |
| Is a camera effect driven by movement (head bob, FOV kick) | Its own `MonoBehaviour` in `Examples/` | See below |

A distinct ability is a mode even when it touches one slice, which is why `DefaultMode` carries `CanSlide()` beside `CanWallrun()` and `CanClimb()`. States are for the phases of a mode's own locomotion (idle and moving; grounded, jumping and falling), not for abilities.

**Mode or modifier.** It is a mode when it changes the *rules*: what the character may do, its capsule, or which transitions exist. It is a modifier when it only swaps a number an existing state already uses. So **crouch is `CrouchMode`**, because it changes capsule height and blocks jumping. **Sprint is not a mode**: `MovingState` reads a sprint *level* from `InputContext` and targets a `SprintSpeed` setting instead of `MovementSpeed`, spending stamina through `IPlayerContext`.

**World queries** use the `CharacterController` on `PhysicsContext` for the capsule's position, radius and height, and run where the decision is made: a mode's `Switch()` for entry (`DefaultMode.CanWallrun()`), a state's `Switch()` or `Process()` inside a mode. Nothing caches query results on a context for other code to read, unless two providers genuinely need the same result in one step.

**Camera-effect components** are sample `MonoBehaviour`s on the camera, in namespace `Radknee.MovementFramework.Examples`. They read the contexts through `ServiceManager` in `LateUpdate()`, so they see the pose `MovementController.Update()` has just drawn. They must not write what `MovementController` writes: the character's transform and the camera's `localRotation`. Head bob belongs in the camera's `localPosition`, and FOV in `Camera.fieldOfView`.

**Current layout.** New files follow it until a deliberate migration:

```
Examples/
  MovementController.cs
  Inputs.inputactions               project-wide actions asset
  DefaultMode/
    DefaultMode.cs
    HorizontalMovement/             one folder per provider: provider + its states
      HorizontalMovementProvider.cs, IdleState.cs, MovingState.cs
    VerticalMovement/
    Rotation/
  <Name>Mode/                       a new mode takes the same shape
```

One class per file. Names are `<Name>Mode`, `<Slice>Provider` and `<Phase>State`. A mode that needs a provider living in another mode's folder references it where it is.

**Target layout.** It is reached in one deliberate migration, never piecemeal inside a feature: `Examples/Modes/<Name>Mode.cs` and `Examples/Providers/<Slice>/` (provider and its states). Do not move existing files as part of other work.

**Wrong placements**, each tempting:

- A `VerticalMovementState` base between `MovementState` and the vertical states. See Conventions.
- A state that takes or casts to a concrete provider type (`(VerticalMovementProvider)movementProvider`).
- An `AirJumpState`. The air jump is `JumpingState` entered from the air.
- A mutable field on a state (`private float _timer`). States are singletons; use a runtime property on `PhysicsContext`.
- A horizontal state writing `Velocity.y`, or any provider writing an axis another provider owns.
- A state reading `transform`, `Time.deltaTime`, or the Input System directly. States see the world only through the contexts and `Physics.*` queries, on the physics clock.
- A state or provider regenerating a resource. The player-values service regenerates; states only check and spend.
- A `SlidingState` or `DashingState` added to `HorizontalMovementProvider`. Abilities are modes.
- A method on `MovementState` or `MovementProvider` shared by several states.
- Drawing or extrapolation logic in a provider, mode or motor. The framework runs only on the physics clock.

## Architecture

Four nested layers, each a state machine or a container of them. Understanding the velocity flow between them is the thing that requires reading several files at once.

```
MovementController (MonoBehaviour)  drives everything, owns CharacterController
  └─ MovementMotor                  picks the active MovementMode
       └─ MovementMode              e.g. DefaultMode; holds several providers
            └─ MovementProvider     Rotation, Horizontal, Vertical; one state machine each
                 └─ MovementState   e.g. Grounded, Jumping, Falling
```

**Providers are summed, not chained.** `DefaultMode.Process()` zeroes its velocity, runs every provider, and adds each provider's `Velocity` into the total. Each provider is therefore responsible for a disjoint slice of the vector: `HorizontalMovementProvider` writes X and Z, `VerticalMovementProvider` writes Y, `RotationProvider` writes none. A provider that writes an axis another provider owns will silently double it. New providers must claim an unowned axis or be designed as an additive offset.

**Rotations compose by multiplication.** Same rule, different operator. `DefaultMode.Process()` resets `Rotation` and `CameraRotation` to identity and multiplies each provider's in, so a provider that produces no rotation contributes nothing — which is why `MovementProvider.Rotation` defaults to identity rather than `default`, since a zeroed quaternion would annihilate the product. Only `RotationProvider` claims either slice today.

**One ordering dependency.** The velocity sum is order-independent, but `RotationProvider` must be registered *before* `HorizontalMovementProvider` in `DefaultMode.CreateMovementProviders()`. `RotatingState` writes `PhysicsContext.Rotation` and `MovingState` reads it to steer, so the reverse order leaves movement a physics step behind the camera. This is the only place provider order matters; anything else that couples two providers through the context will need the same care.

**Modes switch like states.** `MovementMotor.Process()` runs the current mode's `Switch()` and swaps modes on a non-null result, so each mode decides its own exits: `DefaultMode.Switch()` tests the entry condition for another mode and returns it, and that mode's `Switch()` decides when to hand back. The motor stays generic.

**State machine contract.** `MovementProvider.Process()` runs `Switch()` first, then `End()`/`Start()` on a transition, then `Process()` on the now-current state. So `Start()` and `Process()` both run on the frame a state is entered.

- `Switch()` is a query. It decides the next state and returns null to stay. Keep it free of side effects.
- `Start()` is where entry effects belong, including consuming an input latch.
- Every `MovementState` subclass takes the base `MovementProvider` in its constructor, uniformly. States must never narrow that parameter or cast to a concrete provider type. A state sees the world only through `movementProvider.PhysicsContext` and `movementProvider.InputContext`, both of which exist on every provider regardless of its type.
- States are **preallocated singletons**. `CreateStates()` builds one instance of each and `RequestState<T>()` finds it by type, so the same object is reused for every visit. States therefore hold no mutable fields of their own: anything that varies per entry, such as `JumpCutApplied`, lives in a context and is reset in `Start()`. A field initialized only at construction would be correct on the first visit and stale on every one after it.

**Contexts are the only window onto the world.** States read `IInputContext` and `IPhysicsContext` off their provider (and `IPlayerContext` once it exists), and touch no Unity singleton directly, with two exceptions: `Time.fixedDeltaTime`, the physics step every state scales by, and `Physics.*` queries, positioned from the context's `CharacterController`. `ServiceManager` is a static service locator populated in `MovementController.Awake()`.

`MovementProvider.Process()` is `virtual` so a provider can do work before its states run, and anything overriding it must call `base.Process()` or the state machine stops advancing. Per-step bookkeeping that states own belongs in the states, not in that override.

## The Update / FixedUpdate split

This is the most important constraint in the codebase and the source of a whole class of bugs.

- `MovementController.Update()` polls input through `ServiceManager.Process()`, then `Extrapolate()` draws the character by carrying the latest step's result forward.
- `MovementController.FixedUpdate()` runs `MovementMotor.Process()`, places the character back at `PhysicsContext.Position` with the new rotations, and calls `CharacterController.Move()`. After the move it writes where the character actually stopped to `PhysicsContext.Position`. That is the only runtime state the controller writes, because only it sees the result of `Move()`.

**Rendering is extrapolated, at the top, from what the motor hands over.** The movement framework runs only on the physics clock and knows nothing about drawing. Its outputs are the summed `Velocity` and the composed `Rotation` and `CameraRotation`. `MovementController.Extrapolate()` carries those forward by the seconds since the last step (`Time.time - Time.fixedTime`, clamped to one step). Nothing about earlier steps is kept.

- **Position is `PhysicsContext.Position + CharacterController.velocity × elapsed`.** That is the providers' summed velocity as the last `Move()` actually carried it out. **Never extrapolate by `MovementMotor.Velocity`**, the raw sum: it points into anything blocking the move. While standing it holds `GroundingForce`, which drew the camera up to `2 m/s × 0.02 s` = 4 cm into the floor and snapped it back every step. That vertical sawtooth made pitch visibly choppier than yaw, because it runs along the same axis as the scene moving when you look up and down. Walls did the same sideways.
- **Rotation is carried forward by pending input, not by time.** The controller turns the motor's rotations by the summed `LookInput` without draining it, applying exactly the turn `RotatingState` will. So the view turns on the frame the mouse moves, and the next step lands on precisely what was drawn. The two copies of that turn, `PitchOf()` included, must be kept in step.

Consequences:

- **The transform lies between steps.** Outside `FixedUpdate` it holds the drawn pose, not the simulated one. Read `PhysicsContext.Position` and `PhysicsContext.Rotation` instead. `Move()` calls `Physics.SyncTransforms()` first, because `autoSyncTransforms` is off and `CharacterController.Move()` starts from the physics scene's copy of the transform, not from the transform itself.
- **To teleport, write `PhysicsContext.Position`.** The next pass places the character there.

Rendering frames and physics steps do not correspond one to one, so **any input read in Update that is not a steady level is lost unless it is carried over.** Reading `InputAction.triggered` or `WasPressedThisFrame()` and acting on it in FixedUpdate will drop inputs intermittently. `MovementInput` is exempt because it is a level, not an event: sampling it late is merely sampling it late. Input System runs in its default dynamic-update mode, so `WasPressedThisFrame()` is only meaningful inside Update.

Two inputs need carrying over, in two different ways:

| Input | Kind | Carried by | Drained by |
| --- | --- | --- | --- |
| `JumpPressed` | edge | latched true, never cleared by the service | `JumpingState.Start()` |
| `LookInput` | per-frame delta | **summed** by the service, `+=` not `=` | `RotatingState.Process()`; read undrained by `MovementController.Extrapolate()` |

The look case is the subtler one. Mouse delta is a displacement reported once per frame, so on a frame with no physics step an assignment throws that displacement away for good — and since the number of skipped frames varies with frame rate, so does the total rotation. Summing makes the accumulated delta frame-rate independent. For the same reason `RotatingState` does **not** scale it by `fixedDeltaTime`: it is already a displacement, and scaling a displacement by a time step ties sensitivity to the physics rate.

Jump shows the intended division of labour, and any future edge input (fire, dash, slide) should follow it:

- `InputService` only **latches**. It sets `InputContext.JumpPressed` true on the press and never clears it. It owns no timers and knows nothing about buffering.
- The **states drain and age** the latch, each owning the part of the buffer it needs. `MovementState` stays a bare contract and the provider does nothing; there is no shared helper, so read the three states together:
  - `FallingState.UpdateJumpBuffer()`, called first thing in its `Process()`, turns the latch into a pending press and counts it down. This is where a buffered press actually has to survive.
  - `JumpingState.UpdateJumpBuffer()` is a deliberate copy of it, for presses made during the rise. It is not optional: without it the latch sits set for the whole rise and `FallingState` then reads it as a press made at the apex, granting a full buffer window that was never earned.
  - `GroundedState` has none, on purpose. It takes the jump on the first `Switch()` after any press, so nothing can linger long enough to need ageing.
- `Switch()` tests `InputContext.JumpPressed` **and** `JumpBufferRemaining`, never the buffer alone. The buffer is refilled in `Process()`, which runs *after* `Switch()`, so testing the buffer by itself would delay every jump by a physics step.
- `JumpingState.Start()` **consumes** the press by clearing both the latch and `JumpBufferRemaining`.

The buffer is single, shared runtime state on the physics context, so **exactly one provider's states may age it.** Ageing it from the horizontal states as well would halve the window. That is also why the duplication between `FallingState` and `JumpingState` is not worth hoisting: a helper reachable from every state is a helper the horizontal states can wrongly call.

## Air jumps

`AirJumpCount` is a setting on `IPhysicsContext` — 1 gives the double jump, 0 disables air jumping — and `AirJumpsRemaining` is the runtime counter beside it, so `PhysicsProvider` writes the first and must never write the second. There is no air-jump state: the air jump is the ordinary `JumpingState` entered from the air, and the whole feature is three small pieces.

- `GroundedState.Process()` refills `AirJumpsRemaining` from `AirJumpCount`, in the same place and for the same reason it refreshes the coyote window.
- `FallingState.Switch()` and `JumpingState.Switch()` each take the jump when a press or a live buffer meets `AirJumpsRemaining > 0`. The falling branch sits *after* the coyote branch so a jump the ground still owes the player is never charged for; the jumping branch sits after the ceiling/apex check and **returns `JumpingState` itself**. A self-transition is a legal move here: `MovementProvider.Process()` runs `End()` then `Start()` whenever `Switch()` returns non-null, and it is `Start()` that resets the velocity, so a double jump tapped during the rise fires at once rather than waiting for the apex with a buffer that may have aged out.
- `JumpingState.Start()` decides who pays. A jump with `CharacterController.isGrounded` or `CoyoteTimeRemaining > 0` is the ground's and costs nothing but the coyote window; anything else debits `AirJumpsRemaining`. **Clearing the coyote countdown is what draws that line** — only `GroundedState` refreshes it, so after the first jump the character has no claim on the ground until it lands. `isGrounded` is read alongside it only so a `CoyoteTime` of 0 does not make the jump off the ground itself cost an air jump.

Every air jump is a full `JumpPower` and can be cut short like any other, since `Start()` clears `JumpCutApplied`. A weaker second jump would be another setting here, not another state.

A related trap: `CharacterController.isGrounded` is only true while the controller is actively pushed into the ground. `GroundedState` holds a small downward velocity (`GroundingForce`) instead of zeroing Y for exactly this reason. Zeroing vertical velocity while grounded makes `isGrounded` flicker and the state machine thrash.

## Horizontal movement

The two horizontal states are not "stopped" and "moving" so much as two halves of one inertia model, and neither one ever assigns a velocity outright.

- `MovingState` turns the input into a *target* velocity — heading times input times `MovementSpeed` — and moves the current velocity toward it by `HorizontalAcceleration * fixedDeltaTime`. Turning costs the same as speeding up, because a new direction is just a target the current velocity is far from.
- `IdleState` targets zero instead and closes on it at `HorizontalDrag`, so releasing the input coasts rather than stops. A `HorizontalDrag` of zero is frictionless: the character keeps its velocity until it is steered again.

The velocity that carries between physics steps, and between the two states, is `MovementProvider.Velocity` itself: `DefaultMode.Process()` zeroes its own total each step but never the providers', so the horizontal provider's slice survives. That is why **neither state may zero it in `Start()`** — that assignment is the instant stop the model exists to remove — and why the inertia needs no new property on `PhysicsContext`. It is the one piece of movement state already reachable from every state without a cast.

## Rotation

`RotationProvider` owns a single state, `RotatingState`, which turns two quaternions on the physics context by the drained look delta and then outputs them. Rotation is held only as quaternions; there are no stored angles.

- `PhysicsContext.Rotation` — the character's heading, a rotation about world up only. The **single source of truth for which way the character faces**, and what `MovingState` steers by, so it is also what makes movement relative to the camera. Read it rather than `transform.rotation`: the transform is only rotated by `MovementController` after the whole motor has run, so during a step it still holds the previous step's heading, and between steps it holds an extrapolated one. Yaw is applied by multiplication and the result normalized, so repeated products cannot drift.
- `PhysicsContext.CameraRotation` — the camera's pitch, a local rotation about X only, kept within `MinPitchAngle`..`MaxPitchAngle`. Negative looks up, because looking up is a negative rotation about X. A clamp needs an angle, so the pitch is read back out with `PitchOf()`, moved, clamped, and rebuilt. `PitchOf()` is exact because the quaternion holds only `w` and `x`, and `w` stays positive inside ±90. The limits stop just short of ±90 so it never has to read a pitch at the pole.
- Both start at `Quaternion.identity`, never `default`, since a zeroed quaternion annihilates anything it multiplies.

Both are runtime state, so `PhysicsProvider` must not touch them; only the limits are settings.

**The split is the provider's to make, not the controller's.** `RotatingState` outputs yaw only as `Rotation` and pitch only as `CameraRotation`, and `MovementController.Rotate()` assigns them straight across, with `Extrapolate()` turning each only about its own axis — body from `Rotation`, camera from `CameraRotation`. The body must never take pitch, or the capsule tips and `CharacterController` starts fighting the ground; the movement vector must never take pitch either, or looking up walks the character into the air.

**The camera must be a child of the character.** Pitch is written to the camera's *local* rotation so it composes with the body's yaw. A camera parented anywhere else gets pitch but never yaw, and the view will not turn.

## `PhysicsContext` is the movement context

`IPhysicsContext` is the single home for movement data that outlives one state object or is shared between states or providers, whether or not it is strictly physics: tuning values, `CoyoteTimeRemaining`, `Rotation`, and future values like crouch height or an ability's time remaining. That is what keeps states free of casts: the context is reachable from any `MovementProvider`.

The one exception is **player resources**: values that are spent and regenerate, such as stamina or dash charges. They belong to the player-values service and its `IPlayerContext`, not to `PhysicsContext`. No other context type is to be added.

It holds two kinds of property, written by different owners:

- **Settings** are owned by the `PhysicsProvider` scene component and follow the three-file rule in `Assets/Src/Services/CLAUDE.md`. Nothing else writes them at runtime.
- **Runtime state** is owned by the movement states, plus `Position`, which `MovementController` writes after each `Move()`. It must never appear in `PhysicsProvider`, which reasserts everything it knows every frame.

## Conventions

- **Do not add a class to hold shared behaviour.** The four layers are the whole vocabulary, and a new class is warranted only when it is a new *concept* — another mode, provider, or state — never when it is a home for a helper. An intermediate base such as a `VerticalMovementState` between `MovementState` and the vertical states is the wrong answer: it adds a layer to a framework whose layers are the thing you have to hold in your head, plus a file and a `.meta`, to save a few lines.
- **`MovementState` and `MovementProvider` are contracts, not toolboxes.** `MovementState` declares `Start`/`Process`/`End`/`Switch` and holds the provider reference; that is all it may hold. Behaviour belongs in the concrete state that performs it, even when two states perform something similar — see the duplicated `UpdateJumpBuffer()` in `FallingState` and `JumpingState`. Shared *data* goes on `PhysicsContext`; shared *behaviour* is duplicated or left out, whichever the state genuinely needs. A helper on the base is reachable from every state in every provider, which is how a per-provider concern gets run twice a step.
- **The handler interfaces are vestigial.** `IRotationHandler` and `ICameraRotationHandler` are implemented by `RotatingState`, and `IMovementHandler` by nothing. Do not implement them in new states or add new ones.

## Known gaps

Scaffolding that exists but does nothing yet. Do not assume these work.

- **No air control distinction.** The horizontal provider runs identically whether grounded or airborne, so a jump is steered with exactly the ground's acceleration and drag. Splitting them means new settings on `IPhysicsContext`, not new states.
- `MovementMotor.Move()`, `Rotate()` and `RotateCamera()` are empty.
- `DefaultMode.CanWallrun()`, `CanClimb()` and `CanSlide()` return false, and only one mode is registered.
- `IPlayerContext` does not exist yet. The first resource to be added brings it into being: the interface and implementation go at the core root beside the other contexts, and `MovementProvider` gains a `PlayerContext` property set in every provider's constructor like the other two.
- `MovementController` declares `namespace Radknee.Gameplay`, a legacy name. New sample files use `Radknee.MovementFramework.Examples`.
