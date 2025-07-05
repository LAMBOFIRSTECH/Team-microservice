# 🧩 Architecture du Projet

> Une vue d’ensemble des différentes couches et fichiers de l’application.

---

## 📦 API.Layer

**Contrôleurs**
- 📄 `TeamController.cs`

**DTOs**
- 📄 `DeleteTeamMemberDto.cs`  
- 📄 `TeamDto.cs`  
- 📄 `TeamRequestDto.cs`  
- 📄 `TransfertMemberDto.cs`

**Middlewares**
- 📄 `ExceptionHandlerMiddleware.cs`  
- 📄 `HandlerException.cs`  
- 📄 `JwtBearerAuthenticationMiddleware.cs`

**Mappings**
- 📄 `TeamProfile.cs`  
- 📄 `ValidationErrorMapper.cs`

**Autres**
- 📄 `DependancyInjection.cs`  
- 📄 `Program.cs`  
- 📄 `appsettings.Development.json`

---

## 🧠 APP.Layer

**CQRS / Commands**
- 📄 `CreateTeamCommand.cs`  
- 📄 `DeleteTeamCommand.cs`  
- 📄 `DeleteTeamMemberCommand.cs`  
- 📄 `UpdateTeamCommand.cs`

**CQRS / Events**
- 📄 `EmployeeCreatedEventHandler.cs`

**CQRS / Handlers**
- 📄 `CreateTeamCommandHandler.cs`  
- 📄 `DeleteTeamCommandHandler.cs`  
- 📄 `DeleteTeamMemberCommandHandler.cs`  
- 📄 `GetAllTeamsQueryHandler.cs`  
- 📄 `GetTeamQueryHandler.cs`  
- 📄 `GetTeamsByManagerQueryHandler.cs`  
- 📄 `GetTeamsByMemberQueryHandler.cs`  
- 📄 `UpdateTeamCommandHandler.cs`

**CQRS / Queries**
- 📄 `GetAllTeamsQuery.cs`  
- 📄 `GetTeamQuery.cs`  
- 📄 `GetTeamsByManagerQuery.cs`  
- 📄 `GetTeamsByMemberQuery.cs`

**CQRS / Validators**
- 📄 `AddTeamMemberRecordValidator.cs`  
- 📄 `CreateTeamCommandValidator.cs`  
- 📄 `DeleteTeamCommandValidator.cs`  
- 📄 `UpdateTeamCommandValidator.cs`

**Configurations**
- 📄 `AuthorizationConfiguration.cs`  
- 📄 `OpenTelemetryConfiguration.cs`

**Services & Interfaces**
- 📄 `IEmployeeService.cs`  
- 📄 `EmployeeService.cs`  
- 📄 `ProjectService.cs`

**Autres**
- 📄 `DependancyInjection.cs`

---

## 🧱 CORE.Layer

**Business Exceptions**
- 📄 `DomainException.cs`

**Entités**
- 📄 `Team.cs`

**Interfaces**
- 📄 `ITeamRepository.cs`

**Models**
- 📄 `TeamMemberAction.cs`  
- 📄 `Message.cs`

**Value Objects**
- 📄 `TeamValue.cs`

**Autres**
- 📄 `DependancyInjection.cs`

---

## ⚙️ INFRA.Layer

**External Services**
- 📄 `EmployeeExternalService.cs`

**Persistence**
- 📂 `Migration/` *(contenu non détaillé)*  
- 📂 `Repositories/`  
  - 📄 `TeamRepository.cs`  
- 📄 `TeamDbContext.cs`

**Autres**
- 📄 `DependancyInjection.cs`

---

## 📘 Légende

| Emoji | Signification           |
|-------|-------------------------|
| 📁 / 📂 | Dossier / Sous-dossier    |
| 📄     | Fichier                  |

