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
  MovingPlatform.cs, CubeRotator.cs, RocketJumpTester.cs   scene test props
  DefaultMode/
    DefaultMode.cs
    HorizontalMovement/             one folder per provider: provider + its states
      HorizontalMovementProvider.cs, IdleState.cs, MovingState.cs
    VerticalMovement/
    Rotation/
    GroundMotion/
    Push/
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
- An `ImpulseProvider` holding external pushes as its own velocity. It would need its own gravity and drag, and the vertical states would keep overwriting the axis it fights them for. Pushes are a separate velocity in `MovementMotor`, added after the mode (see External forces).
- A check for external pushes inside a state, or a motor or controller reaching into providers to apply one. Impulses never get below the motor.

## Architecture

Four nested layers, each a state machine or a container of them. Understanding the velocity flow between them is the thing that requires reading several files at once.

```
MovementController (MonoBehaviour)  drives everything, owns CharacterController
  └─ MovementMotor                  picks the active MovementMode
       └─ MovementMode              e.g. DefaultMode; holds several providers
            └─ MovementProvider     Rotation, Horizontal, Vertical; one state machine each
                 └─ MovementState   e.g. Grounded, Jumping, Falling
```

**Providers are summed, not chained.** `DefaultMode.Process()` zeroes its velocity, runs every provider, and adds each provider's `Velocity` into the total. Each provider is therefore responsible for a disjoint slice of the vector: `HorizontalMovementProvider` writes X and Z, `VerticalMovementProvider` writes Y, `RotationProvider` writes none. A provider that writes an axis another provider owns will silently double it. New providers must claim an unowned axis or be designed as an additive offset. `GroundMotionProvider` and `PushProvider` are the additive offsets: they own no axis and add on all three, the ground's velocity and the push away from moving colliders (see Ground motion and Pushing).
**Rotations compose by multiplication.** Same rule, different operator. `DefaultMode.Process()` resets `Rotation` and `CameraRotation` to identity and multiplies each provider's in, so a provider that produces no rotation contributes nothing — which is why `MovementProvider.Rotation` defaults to identity rather than `default`, since a zeroed quaternion would annihilate the product. Only `RotationProvider` claims either slice today.

**Two ordering dependencies.** The velocity sum is order-independent, but two pairs of providers are coupled through the context, so their order in `DefaultMode.CreateMovementProviders()` matters:

- `RotationProvider` before `HorizontalMovementProvider`. `RotatingState` writes `PhysicsContext.Rotation` and `MovingState` reads it to steer, so the reverse order leaves movement a physics step behind the camera.
- `GroundMotionProvider` before `PushProvider`. `CarriedState` records this step's `GroundTransform` and `PushedState` skips that collider, so the reverse order skips last step's ground and lets a rising platform both carry and push the character.

Anything else that couples two providers through the context will need the same care.

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

- **The transform lies between steps.** Outside `FixedUpdate` it holds the drawn pose, not the simulated one. Read `PhysicsContext.Position` and `PhysicsContext.Rotation` instead. `Move()` does not call `Physics.SyncTransforms()`, which is expensive. `CharacterController.Move()` starts from the physics scene's copy of the transform, and with `autoSyncTransforms` off that copy stays wherever the last `Move()` left it. That is always `PhysicsContext.Position`, because the only transform change since then is the extrapolated pose, and `Rotate()` undoes it before `Move()`.
- **To teleport, write `PhysicsContext.Position`, set `transform.position` to the same value, and call `Physics.SyncTransforms()` once.** Writing `Position` alone lasts one drawn frame: the next `Move()` starts from the physics scene's stale copy and snaps the character back.

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

## Air control

Both horizontal states branch on `CharacterController.isGrounded`. On the ground they run the model above. In the air they follow Quake's rules:

- `MovingState` only **adds** speed along the direction the input asks for, at `AirAcceleration`, until the velocity's share of that direction reaches the target speed. It never takes speed away. Holding forward after a blast keeps the excess speed, and strafing turns the character by adding speed in a direction it has little of.
- `IdleState` bleeds speed off at `AirDrag` instead of `HorizontalDrag`. At the default of 0, the character coasts until it lands, where ground friction takes over.

These are settings, not states: the air is not a phase of horizontal movement, only different numbers and one different rule inside the same two states.

## External forces

Anything outside the movement layer, such as a rocket blast, a launch pad or wind, pushes a character by raising an impulse on the static router: `ImpulseEvents.ApplyImpulse(GameObject target, Vector3 velocity, float decayTime, AnimationCurve falloff = null)`. The target is the character's `GameObject`, the one carrying its `MovementController` and `CharacterController`. The **source owns the force**: it works out direction, falloff with distance, strength, how long the push lasts and, optionally, the shape of its fade, then hands over only those. `ImpulseEvents` is the only channel. Do not add a second channel, an impulse provider, an `IImpulseReceiver` interface, or a method on `MovementController` that takes impulses. `RocketJumpTester` is the reference caller.

**Impulses are a separate velocity, held in the motor and added after the mode.** `ImpulseEvents` routes each impulse to the one receiver registered for its target, so a motor never sees an impulse meant for another character and does not know which `GameObject` it moves. `MovementController` registers its motor's `AddImpulse()` under its own `GameObject`. `MovementMotor.Velocity` is the current mode's velocity plus every impulse still fading, and `MovementController` moves by it alone. The motor reads no context for this. Modes, providers, states and contexts never see impulses: none of them knows about them. Keep it that way. Do not reach into providers, and do not add impulse checks to a state.

**The registration must be released.** `ImpulseEvents` is static, so its table keeps every registered motor alive. `MovementController` registers in `OnEnable()` and calls `ImpulseEvents.Unregister()` in `OnDisable()`, so a disabled character is not pushed and impulses do not pile up while it is off. Anything else that builds a motor must do the same.

Naming:

| Name | What it is |
| --- | --- |
| `ImpulseEvents` | Static class at the core root. Holds only a table from target `GameObject` to receiver; `Register()` and `Unregister()` maintain it, and `ApplyImpulse()` calls the target's receiver, dropping the impulse if there is none. |
| `MovementMotor.Impulse` | One push: a private nested struct holding `Velocity` (full strength), `DecayTime`, `Falloff` and `Age`, all in seconds where timed. `CurrentVelocity` is the velocity scaled by the falloff curve at `Age / DecayTime`, or faded linearly when `Falloff` is null; `IsExpired` is age past decay time. |
| `MovementMotor.AddImpulse()` | The receiver. Adds one new `Impulse`, with no target check. Called only through `ImpulseEvents`, never by a source directly. A null curve, or one with no keys, is stored as null and fades linearly. |
| `MovementMotor._impulses` | Every impulse still fading. |
| `MovementMotor.TakeImpulseVelocity()` | After the mode runs. Sums every impulse's `CurrentVelocity` into `Velocity`, then ages each by `fixedDeltaTime` and removes expired ones. |

- **The falloff curve maps progress to strength.** X runs from 0, when the impulse is applied, to 1, at its decay time; Y is the fraction of `Velocity` applied. A curve that stays above zero at X = 1 ends with a step down when the impulse expires. A decay time of zero ignores the curve.
- **Each impulse is its own instance on its own clock.** Two blasts in quick succession stack, and each fades over its own decay time. A decay time of zero lasts exactly one physics step.
- **Safe from either clock.** An impulse is only read and aged in `MovementMotor.Process()`, on the physics clock, so one applied from Update, FixedUpdate or a trigger callback starts at full strength on the next step. A steady force, such as wind, is a zero-decay impulse applied once per step, from `FixedUpdate()`. Applied from `Update()`, it would be applied several times on frames between steps.
- **Gravity does not act on impulses.** Their decay time is what brings a launch back down, alongside gravity on the character's own fall in the vertical states.
- **Launches need no help from the states.** An upward push outweighs `GroundingForce`, so the move lifts the character. `GroundedState` then sees it airborne and hands over to `FallingState` as for walking off a ledge.
- **Known consequences of keeping it separate:** coyote time is still live during a launch, so a jump can be added on top. The jump cut trims only the jump, never the push. Impulses ignore collisions entirely: a push into a wall, floor or ceiling keeps pressing against it until it decays. A blast straight up into a low ceiling therefore pins the character there for the decay time. This is deliberate for now; clipping would need the motor to hear about each move's collisions.

## Ground motion

`GroundMotionProvider` carries the character with whatever it stands on. It has two states: `CarriedState` while `isGrounded` and `ReleasedState` otherwise.

- **The ground's velocity is measured, not declared.** Each grounded step, `CarriedState` finds the collider underfoot with a `SphereCast` and records its transform and `localToWorldMatrix` in `PhysicsContext.GroundTransform` and `GroundLocalToWorld`. The next step maps `Position` through the old matrix and the current one; the difference over `fixedDeltaTime` is the velocity. Translation and rotation both come through, and platforms need no component or interface: anything whose transform moves carries the character. Do not add an `IMovingPlatform` for this.
- **The first step on new ground carries nothing**, because there is no earlier matrix to compare against. `ReleasedState.Start()` clears `GroundTransform` for this reason: landing back on the platform just left must not compare against a matrix several steps stale, which would read as a huge velocity.
- **Leaving the ground keeps horizontal momentum, not vertical.** `ReleasedState` holds the ground's X and Z unchanged until landing. It drops Y, because a constant offset that gravity never acts on would carry the character upward for the whole flight. On landing, `CarriedState` replaces the momentum with the new ground's velocity, so it stops at once rather than bleeding off.
- **A platform must hold the step's pose twice before the character moves.** `CarriedState` measures the platform's *transform*, and `CharacterController.Move()` collides with its *physics* copy. So `MovingPlatform` and `CubeRotator` run at execution order -200, ahead of `MovementController`'s -100. Each `FixedUpdate()` sets the pose for `Time.fixedTime` on the transform and on a kinematic `Rigidbody`, which moves the collider in the physics scene at once, with no `Physics.SyncTransforms()`. A platform that moves its transform alone this early leaves its collider a step behind the measurement. A rising platform then carries the character off it, and `isGrounded` flickers.
- **Platforms draw like the character.** Each platform's pose is a function of time, so its `Update()` draws the exact pose for `Time.time`, moving at the frame rate beside the character's extrapolated drawing. The drawn pose never reaches physics: `autoSyncTransforms` is off, and the simulation syncs only after `FixedUpdate()` has put the step's pose back. A platform that moves only in `FixedUpdate()` is drawn stepping at the physics rate and visibly judders against the camera.

## Pushing

`PushProvider` has one state, `PushedState`, which runs every step, grounded or not, and moves the character out of the way of moving colliders.

- **`CharacterController` does not depenetrate from moving colliders.** Its overlap recovery works only against static colliders. So nothing else gets the character out of a kinematic body that has moved into it.
- **The push is an additive velocity, worked out from the pushers alone.** `PushedState` sees only the contexts, not the other providers' velocities. For each pusher near the capsule it finds which way is out and how far the pusher is from the capsule's core segment. It outputs the velocity that takes the character back to the hold distance, added to the sum like any other provider's. Each step's push equals the distance the pusher moved, so `CharacterController.velocity` carries it and `Extrapolate()` draws the character moving with the pusher. The push carries no momentum: it stops when the pusher stops or moves away.
- **Walls are held a step's worth of the character's speed beyond the skin.** The push and the character's own movement go out in one sweep. A pusher inside the skin blocks that sweep along its normal, which cancels the push together with the step towards it. The pusher then gains on the character every step until it is inside, which caused the choppy pushing and the walk-throughs. The hold distance is therefore `radius + skinWidth + lookahead`, where `lookahead` is the larger of `MovementSpeed` and the controller's last speed, times `fixedDeltaTime`. A step towards the pusher can never close that, so the push always outweighs it and the sweep starts clear. The character can still walk right up to the skin. The cost is a gap of up to `lookahead` (10 cm at the defaults) between the character and a pusher it is not walking into.
- **Something walkable is held only at the skin.** A pusher whose normal is within `slopeLimit` of up is a surface the character could stand on, rising into it from below. It gets no lookahead, because holding it further out would leave the character hovering where the sweep never touches it and `isGrounded` never comes.
- **A core inside a pusher falls back to `Physics.ComputePenetration`.** The nearest-points measure cannot tell which way is out once the capsule's core segment is inside the collider. That takes a pusher moving more than the skin in one step, or a teleport into one. Skipping the collider then would let the character walk through it.
- **A pusher is a collider on a kinematic `Rigidbody`**, which is what `MovingPlatform` and `CubeRotator` add to themselves. Static colliders are skipped because the controller handles them, and so are non-convex meshes, which `Collider.ClosestPoint` cannot measure. The current ground is skipped because `GroundMotionProvider` already carries the character with it.
- **A pusher must move before the character.** The push measures the pusher where it is now, so a platform must follow the Ground motion rules: execution order ahead of -100, with its pose set on the body in `FixedUpdate()`.

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

- `MovementMotor.Move()`, `Rotate()` and `RotateCamera()` are empty.
- `DefaultMode.CanWallrun()`, `CanClimb()` and `CanSlide()` return false, and only one mode is registered.
- `IPlayerContext` does not exist yet. The first resource to be added brings it into being: the interface and implementation go at the core root beside the other contexts, and `MovementProvider` gains a `PlayerContext` property set in every provider's constructor like the other two.
- `MovementController` declares `namespace Radknee.Gameplay`, a legacy name. New sample files use `Radknee.MovementFramework.Examples`.
