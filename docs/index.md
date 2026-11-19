# Casino ERP — Project Documentation

Documentation site for the **Casino ERP** project, developed for the Software Engineering II course (6th semester).

## 1. Project Overview

**Problem:** Fragmented and inefficient management of casino operations (player tracking, table/game allocation, promotions, and staff certification), resulting in revenue leakage, inconsistent guest experience, and regulatory risk.

**Solution:** A Scrum-managed ERP platform that centralizes player profiles, personalized game recommendations, promotional campaign management, and staff skills/compliance validation.

**End users:** Casino guests/players, marketing staff, floor staff and compliance officers, and casino management.

## 2. Project Management

- **Methodology:** Scrum
- **Tool:** [Jira — Casino ERP (CER)](https://jorgediazlozano1170709.atlassian.net/jira/software/projects/CER/boards/2)
- **Access:** Private workspace

### Epics

| Key | Epic | Description |
|---|---|---|
| CER-1 | Player Profile Management | Registration, editing, and viewing of casino player profiles |
| CER-2 | Game Search and Recommendation | Search for games/tables and personalized recommendations for players |
| CER-3 | Promotions and Offers Management | Creation and management of casino promotions and offers by the marketing team |
| CER-4 | Staff Skills Validation | Tests and certifications to validate dealer and staff competencies |

## 3. Repository Structure

- `/src` — C# source code (.NET 8): `CasinoErp.Dominio`, `CasinoErp.Infraestructura`, `CasinoErp.Api` (REST API + ERP front in `wwwroot`)
- `/tests` — Unit tests (xUnit) and API integration tests
- `/pruebas` — CRUD testing workshop (Gherkin, Jest, JUnit 5, Cucumber, Reqnroll)
- `/desarrollo` — Sprint 2 work plans, contracts, decision records and team logs (start with [`AGENT.md`](../AGENT.md))
- `/docs` — Project documentation (this site); architecture in [`docs/arc42/README.md`](arc42/README.md)

## 4. Team Workflow

- Integration branch: `develop`; `main` only receives the end-of-sprint release
- One branch per task (e.g. `feature/HU-04-empleados`), created from an up-to-date `develop`
- Pull requests into `develop`, reviewed by another member, with CI green
- Commit messages reference the user story or Jira key (e.g. `HU-04: ...`, `CER-8: ...`)

## 5. Contributing

All team members are collaborators on this repository. Read [`AGENT.md`](../AGENT.md) and your plan in `desarrollo/planes/` before contributing, branch from `develop`, and open a pull request for review before merging.

## 6. Testing

The `pruebas/` folder holds the CRUD testing workshop applied to the casino modules (player profiles, table catalog, retention offers, chip fills and vault inventory):

- Gherkin `.feature` files in Spanish shared by Cucumber-JS, Cucumber-JVM and Reqnroll (37 scenarios)
- Unit tests with Jest (63) and JUnit 5 (46)
- HTML test report with Font Awesome icons: 185 tests, 0 failures

See [`pruebas/README.md`](../pruebas/README.md) for how to run each tool.
