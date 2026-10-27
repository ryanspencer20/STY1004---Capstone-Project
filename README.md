# STY1004---Capstone-Project
STY1004 - Capstone Project for Software Development. A Traffic Light system architecture for Sudbury Ontario Downtown lights.

## Project Scope

### 1.1 Problem Statement

This capstone project will be based from a set of traffic lights in Sudbury, ON Downtown location to replace or add red light cameras with real time cameras counting functionality vehicles during peak hours to control the set of lights. The problem is the set of lights during peak hours must be controlled because of close proximity of the set lights in row. They must be controlled in a way that let's the flow of traffic through while timing the set of lights together with the best efficiency possible.

### 1.2 Project Objectives & Descriptions

The architecture will be based on C# dotnet, using a set of rules describing the lighting sequence using count logic to control the lights adding from the timing logic. An EFCore database will be implemented for data analyst and storing current and historical count sequencing for training data on the most efficient logic count vs timing logic over time. Evaluations throughout project lifecycle will determine sequencing for traffic logic. Logic may be subject to change based on experimentations; However, rules will be determined through group commitment discussions and research. The proper standards of dotnet will include the following projects: Domain, Application, Infrastructure and MS Tests Suite. Each will contain the proper architecture artifacts, as described in [1.3 Deliverables]. 

### 1.3 Deliverables & Artifacts

***Solution Topography:***

  * IoTPlatform.Domain: Pure business entities, value objects, domain exceptions. Zero dependencies.
  * IoTPlatform.Application: Interfaces for repositories, DTOs, and service contracts.
  * IoTPlatform.Infrastructure: EF Core DbContext, entity configurations, SQL Server repositories, and JSON seed loaders.
  * IoTPlatform.UnitTests: MSTest suite covering domain rules and repository contracts.

***Domain & Data Scope***:

  * Entities: DeviceAsset, TelemetryMetric, AlertRule, and MaintenanceLog.
  * Enforce business invariants: serial numbers must follow format masks, operating limits must be positive ranges, and status transitions must follow lifecycle rules.
  * Relational mapping: configure primary keys, foreign keys, indexes, and constraints using EF Core Fluent API.

***Verification Suite***:
  * Unit tests achieving coverage across all domain invariant methods.

***Architecture Blueprint Document (Single PDF)***:
  * System Overview: Problem statement and high-level platform scope.
  * Domain Model Diagram: Class diagram showing entities, value objects, and relationships.
  * Entity Relationship Diagram (ERD): Relational schema detailing keys, constraints, and data types.
  * Team Contributions Table: Specific responsibilities and git commits attributed to each team member.

### 1.4 Boundaries

***In Scope:*** 

* Camera logic
* Traffic Lights Hard Rules
  * Red Light Camera Function
  * Scenario Preventions (***eg.*** Red light North-South, Green light East-West)
  * Timing vs Count Integrations
* Traffic Lights logic
* Database Current Real-Time
* Database Historical Data
* MS Test suit (*Zero Dependencies*)

***Out of Scope:***
* AI Integration

### 1.5 Definition of Done

- [ ] Multi-project solution compiles cleanly via dotnet build.
- [ ] All unit tests pass via dotnet test.
- [ ] EF Core migrations generate tables matching the ERD.
- [ ] Architecture Blueprint PDF contains all required diagrams and team contribution logs.
- [ ] Submitted via LMS by all group members with GitHub repository URL.
