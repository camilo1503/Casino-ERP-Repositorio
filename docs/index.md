# Casino ERP — Project Documentation

Documentation site for the **Casino ERP** project, developed for the Software Engineering II course (6th semester).

## 1. Project Overview

**Problem:** Fragmented and inefficient management of casino operations (player tracking, table/game allocation, promotions, and staff certification), resulting in revenue leakage, inconsistent guest experience, and regulatory risk.

**Solution:** A Scrum-managed ERP platform that centralizes player profiles, personalized game recommendations, promotional campaign management, and staff skills/compliance validation.

**End users:** Casino guests/players, marketing staff, floor staff and compliance officers, and casino management.

## 2. Project Management

- **Methodology:** Scrum
- - **Tool:** [Jira — Casino ERP (CER)](https://jorgediazlozano1170709.atlassian.net/jira/software/projects/CER/boards/2)
  - - **Access:** Private workspace
   
    - ### Epics
   
    - | Key | Epic | Description |
    - |---|---|---|
    - | CER-1 | Player Profile Management | Registration, editing, and viewing of casino player profiles |
    - | CER-2 | Game Search and Recommendation | Search for games/tables and personalized recommendations for players |
    - | CER-3 | Promotions and Offers Management | Creation and management of casino promotions and offers by the marketing team |
    - | CER-4 | Staff Skills Validation | Tests and certifications to validate dealer and staff competencies |
   
    - ## 3. Repository Structure
   
    - - `/` — Source code (Python)
      - - `/docs` — Project documentation (this site)
       
        - ## 4. Team Workflow
       
        - - Feature branches per Jira issue (e.g. `feature/CER-5-registro-usuarios`)
          - - Pull requests reviewed and merged into `main`
            - - Commit messages reference the related Jira issue key (e.g. `CER-5: ...`)
             
              - ## 5. Contributing
             
              - All team members are added as collaborators on this repository. Please branch from `main`, reference the corresponding Jira issue key in your commits, and open a pull request for review before merging.
              - 
