# Services

Framework infrastructure: the service locator, the services that fill the movement contexts, and app-level Unity concerns. Namespace `Radknee.Services` for every new file. `ServiceManager`, `PhysicsService` and `PhysicsProvider` sit in the global namespace as legacy; leave them.

## Lifecycle

- `MovementController.CreateServices()` registers every service in `Awake()`. A new service is registered there.
- `ServiceManager.Process()` calls every service's `Process()` once per **rendered frame**, from `MovementController.Update()`. Services run on the render clock, never on the physics clock.
- Nothing in a service simulates movement. A service fills a context or talks to Unity (cursor, application); the movement states act on what it wrote.

## Filling `InputContext`

`InputService` is the only writer of raw input. It reads the project-wide actions asset, `Examples/Inputs.inputactions`, map `Default`. Because it runs in Update while the states run in FixedUpdate:

- A **level** (move stick, button held) is assigned: `=`.
- An **edge** (button pressed) is **latched**: set true on the press and never cleared here. The consuming state clears it.
- A **per-frame delta** (mouse look) is **summed**: `+=`. The consuming state drains it.

`InputService` owns no timers and no buffering. `LookSensitivity` is assigned here and nowhere else.

**`SprintPressed` and `CrouchPressed` are not safe to use as they are.** They are assigned from `InputAction.triggered`, an unlatched edge, so a state reading them in FixedUpdate will miss presses. Before any state consumes one, convert it the way `JumpPressed` works: latch it here and clear it in the consuming state's `Start()`. If it is really a hold, assign it from `IsPressed()` as a level instead.

Known leak, not to be copied: `InputService` hardcodes the sample's map name, and `PhysicsProvider` imports `Radknee.MovementFramework.Examples` without using it. New service code must not reference `Examples/`.

## Player values (planned)

Player **resources**, values that are spent and regenerate such as stamina and dash charges, belong to a dedicated service, provisionally `PlayerService`. It owns them, regenerates them in its `Process()` (render clock, so it scales by `Time.deltaTime`), and exposes them through `IPlayerContext`, which providers carry beside the input and physics contexts. Movement states check and spend them through that context and never regenerate them. `MovementController` may also read and modify them. It is registered in `MovementController.CreateServices()` like every other service.

Movement timers and state (coyote time, the jump buffer, crouch height, an ability's time remaining) are not resources and stay on `PhysicsContext`.

## `PhysicsProvider`: the settings bridge

`PhysicsProvider` is the scene component that exposes `PhysicsContext` settings in the inspector, and the only such component. Adding a setting means editing three files in step:

1. `IPhysicsContext` — declare the property.
2. `PhysicsContext` — implement it with a sensible default.
3. `PhysicsProvider` — add the inspector field and the assignment in `Update()`.

Skipping step 3 means the inspector silently does not control the value.

**Runtime state never goes in `PhysicsProvider`.** Its `Update()` reasserts every value it knows about on every frame, so a runtime property added there is stamped over each frame. For the same reason nothing else may write a settings property at runtime.
