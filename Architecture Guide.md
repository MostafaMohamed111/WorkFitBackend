# Architecture Guide

A short manual for developers working on this modular monolith.

## 1) What this solution is

This project is a **modular monolith**:
- One deployable application (single host).
- Multiple business modules (for example: Organizations).
- Each module owns its domain, data access, and features.

Main idea for daily work:
- Build features inside a module.
- Keep modules isolated.
- Communicate across modules through contracts, not direct internal dependencies.

## 2) Core flow you will use every day

For an API action, follow this simple flow:
1. Endpoint receives request.
2. Endpoint sends a command/query through `IMediator`.
3. Handler orchestrates the use case.
4. Domain entities/behaviors execute business rules.
5. Handler uses repositories/DbContext to persist changes.
6. Handler can publish integration event(s) through `IMediator.Publish(...)`.
7. Handler returns response.
8. If exceptions happen, host global exception handler converts them to a uniform error payload.

Think of this as: **Endpoint -> Mediator -> Handler -> Domain/Data**.

## 3) How to create a new module (minimum setup)

Create two projects:
- `WorkFit.<ModuleName>`: real module implementation.
- `WorkFit.<ModuleName>.Contracts`: interfaces + DTOs for other modules.

Inside `WorkFit.<ModuleName>` create this minimum structure:
- `Domain/Entities`
- `Domain/Exceptions`
- `Infrastructure/Data`
- `Features/<FeatureName>`
- `Register<ModuleName>ModuleServices.cs` (implements `IRegisterModuleServices`)
- `ModuleMarker.cs`

`ModuleMarker.cs` must expose a static module name (for example `public static string ModuleName { get; } = "Identity_Module";`).
This value is required by Shared Kernel exceptions to build stable error codes.

Then wire it:
1. Implement `IRegisterModuleServices` inside the module project.
2. Register module DbContext/services inside `RegisterServices(...)`.
3. Register mediator handlers from module assembly using `AddMediatorHandlers<ModuleMarker>()`.
4. Let host assembly scanning discover and execute all `IRegisterModuleServices` implementations (no per-module edits in `Program.cs`).

## 4) Entity rules (connect to BaseEntity)

All aggregate/domain entities must inherit from `BaseEntity`.

Use this pattern:
- Inherit from `BaseEntity`.
- Keep setters private.
- Use factory methods (`Create(...)`) for validation.
- Throw domain exceptions when business rules fail.

Why:
- You automatically get `Id`, `CreatedAt`, and update tracking behavior.
- You keep consistency with existing modules.

## 5) Vertical slicing (how to organize features)

Use **feature folders**. Each use case should live in one folder.

Example folder:
- `Features/CreateOrganization/`

Recommended files per slice:
- `CreateOrganizationRequest.cs` (HTTP request contract for endpoint)
- `CreateOrganizationEndPoint.cs` (FastEndpoints endpoint)
- `CreateOrganizationCommand.cs` (mediator request)
- `CreateOrganizationCommandHandler.cs` (orchestrator only)

Keep this rule:
- If you add a new use case, create a new feature folder.
- Do not create one huge service class for all use cases.

## 6) Mediator usage (how to wire and call)

What must exist:
- `IMediator` registration in infrastructure via a class implementing `IRegisterModuleServices`.
- Handler scanning per module via `AddMediatorHandlers<ModuleMarker>()`.
- Integration event handler scanning via `IIntegrationEventHandler<TIntegrationEvent>` (registered by `AddMediatorHandlers<ModuleMarker>()`).

How to use in endpoint:
1. Inject `IMediator` in endpoint constructor.
2. Map endpoint request to command/query.
3. Call `await _mediator.Send(...)`.
4. Return result.

How to raise integration events when something happens:
1. Define the event in the owning module contracts project (`WorkFit.<ModuleName>.Contracts`) and implement `IIntegrationEvent`.
2. Create one or more handlers implementing `IIntegrationEventHandler<TEvent>`.
3. In the owning module handler/use case, call `await _mediator.Publish(new YourEvent(...))` after the important state change.
4. Interested modules reference that contracts project and implement handlers for the event.
5. Event ownership stays with the module that raised it; handling is open to any interested module.

Handler requirements:
- Implement `IRequestHandler<TRequest>` or `IRequestHandler<TRequest, TResponse>`.
- Keep handler focused on one use case orchestration.
- Do not place business rules in handler methods.
- Put business logic in domain behaviors (entity methods/factory methods/value objects).

## 7) Exceptions (updated current approach)

Use the Shared Kernel hierarchy now implemented in code:
- `WorkFitException` is the root app exception and stores:
   - `ModuleName`
   - `Code` (normalized to `MODULE_CODE`, uppercase)
   - technical message (`Message`)
   - `UserFriendlyMessage` (defaults if not provided)
- `DomainException` is for domain invariants/business rules inside entities/value objects.
- `FeatureException` is for feature/use-case level failures.

Current specialized feature exceptions used across modules:
- `EntityNotFoundException`
- `EntityAlreadyExistsException`

How to choose exception type:
1. Domain object validation/invariants -> throw `DomainException` (example: `FeildIsNullOrEmptyException` in entity constructors).
2. Use-case/feature validation and known business flow failures -> throw `FeatureException` (example: password confirmation mismatch, login email not found, duplicate entity).
3. Technical/misconfiguration/runtime wiring failures -> `InvalidOperationException` is currently used in infrastructure/startup paths (for example missing connection string, mediator handler wiring failures, missing current user id, identity create failures).

HTTP mapping is centralized in host global exception handler:
1. `EntityNotFoundException` -> `404 Not Found`
2. `EntityAlreadyExistsException` -> `409 Conflict`
3. `DomainException` -> `400 Bad Request`
4. `FeatureException` -> `400 Bad Request`
5. Any other exception -> `500 Internal Server Error`

Error response shape returned by host:
- `errorCode`
- `errorMessage`
- `userFriendlyMessage`

Module code convention:
- Always pass module name from module marker (example: `ModuleMarker.ModuleName`) when creating custom exceptions.
- Keep exception codes stable and machine-friendly (`UPPER_SNAKE_CASE`).

## 8) Contracts between modules

Use module contracts project for cross-module communication:
- Put DTOs and service interfaces in `WorkFit.<ModuleName>.Contracts`.
- Keep contracts clean and implementation-free.

Pattern:
1. Define interface in contracts project.
2. Implement interface inside module project.
3. Register implementation in the module `IRegisterModuleServices` class.
4. Other modules depend only on contracts project and interface.

For integration events between modules:
1. Put event contract types in `WorkFit.<ModuleName>.Contracts` of the owning module.
2. Raise/publish events from the owning module when its state changes.
3. Let interested modules reference the contracts project and implement `IIntegrationEventHandler<TEvent>`.
4. Keep event payloads stable and implementation-free.

Important:
- Do not expose module internals to other modules.
- Do not reference another module's domain/entities directly.

## 9) Lean development checklist (copy for every new feature)

1. Create new folder under `Features/<UseCaseName>`.
2. Add request model.
3. Add command/query model implementing `IRequest` or `IRequest<TResponse>`.
4. Add handler implementing `IRequestHandler<...>`.
5. Add endpoint and call mediator.
6. Put business rules in domain behaviors, not handlers.
7. Use/extend module entities (inheriting from `BaseEntity`).
8. Add domain exception(s) if business rules exist.
9. If needed by other modules, add/update contracts DTO/interface.
10. Register any new services in module `IRegisterModuleServices` implementation.
11. Keep changes inside module boundaries.

## 10) Team conventions for this codebase

- Namespace root uses plural module name style (example: `WorkFit.Organizations`).
- Keep module boundaries strict.
- Keep each vertical slice small and self-contained.
- Prefer clear names over generic names.
- Keep contracts simple and stable.
- Avoid direct per-module registration calls in host startup; rely on assembly scanning + `IRegisterModuleServices`.

## 11) Quick do/don't

Do:
- Build by feature slice.
- Use mediator from endpoints.
- Keep handlers as orchestrators.
- Publish integration events when a state change should notify interested handlers.
- Validate in entity factory methods.
- Throw domain exceptions for business rules.
- Communicate through contracts.

Don't:
- Put all logic in endpoints.
- Put business logic in handlers.
- Share module internals directly.
- Add module-by-module service registration calls in `Program.cs`.
- Bypass `BaseEntity` for domain entities.
- Create large cross-feature god classes.

---

If you follow this guide, new modules and features will stay consistent, lean, and easy to maintain.