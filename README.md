# STY1004---Capstone-Project
STY1004 - Capstone Project for Software Development. A Traffic Light system architecture for Sudbury Ontario Downtown lights.

## Project Scope

### 1.1 Problem Statement

This capstone project is based on a sequential corridor of traffic lights in *downtown Sudbury, ON*. The objective is to replace or augment existing infrastructure with dynamic vehicle-counting and red-light monitoring functionalities. The current system faces clear efficiency bottlenecks during peak operational hours due to the close proximity of consecutive intersections. These intersections must be systematically controlled to permit a continuous, efficient flow of traffic by synchronizing the signal timing along the corridor. As highlighted by a Sudbury.com [1] news analysis, the municipality is actively working to remediate uncoordinated corridors where consecutive signals trigger red lights simultaneously. This project establishes a proposed solution utilizing modern IoT platform topology and simulated camera telemetry to intelligently optimize signal efficiency.


### 1.2 Project Objectives & Descriptions

The platform architecture is built using C# and .NET core frameworks. It utilizes a centralized rules engine to describe lighting sequences, combining traditional time-interval intervals with dynamic, count-based override logic. Simulated and mocked data payloads will emulate real-time edge-device telemetry from the cameras. The red-light camera modules will ingest these simulated events to log intersection infractions, capturing vehicle metadata and plate identifiers. Additionally, the camera module logic will calculate vehicle velocity to identify speed-trap violations across the target intersections.

An Entity Framework Core (EF Core) data access layer will manage data persistence, tracking real-time status updates and historical traffic counts. This historical data provides an analytical foundation to continuously test and train the logic rules (count-based overrides vs. static timing schedules) over time. System rules and traffic behaviors will undergo iterative evaluation and group-led experimental adjustments throughout the project lifecycle. The solution adheres strictly to clean architecture principles by organizing code into a decoupled multi-project topology: Domain, Application, Infrastructure, and an automated MSTest Suite. Each project contains explicit architectural artifacts detailed in `1.3 Deliverables & Artifacts`.

### 1.3 Deliverables & Artifacts

***Solution Topography:***

  * IoTPlatform.Domain: Pure business entities, value objects, domain exceptions. Zero dependencies.
  * IoTPlatform.Application: Interfaces for repositories, DTOs, and service contracts.
  * IoTPlatform.Infrastructure: EF Core DbContext, entity configurations, SQL Server repositories, and JSON seed loaders.
  * IoTPlatform.UnitTests: MSTest suit covering domain rules and repository contracts.

***Domain & Data Scope***:

  * Entities: `IntersectionDevice`, `TrafficTelemetry`, `SignalRule`, and `ViolationLog`.
  * Enforce business invariants: Enforcement of serial number format masks, positive ranges for operating boundaries, and explicit state-transition constraints for intersection lifecycles.
  * Relational mapping: configure primary keys, foreign keys, indexes, and constraints using EF Core Fluent API.

***Verification Suit***:
  * Automated unit tests achieving full verification coverage across all defined domain invariant methods.

***Architecture Blueprint Document (Single PDF)***:
  * System Overview: Problem statement and high-level platform scope.
  * Domain Model Diagram: Class diagram showing entities, value objects, and relationships.
  * Entity Relationship Diagram (ERD): Relational schema detailing keys, constraints, and data types.
  * Team Contributions Table: Specific responsibilities and git commits attributed to each team member.

### 1.4 Boundaries

***In Scope:*** 

* Processing of simulated AI and video metrics payloads
* Core traffic light signal state logic
* Visual speed-trap simulation logic
* Traffic Light Hard Invariants (e.g., Scenario prevention: ensuring a North-South Green safely locks an East-West Red)
* Integration mechanics between time-interval schedules and dynamic count overrides
* Real-time and historical transactional database design
* Interactive Console Application user interface
* Isolated MSTest Suite with zero external runtime dependencies


***Out of Scope:***
* Direct processing of raw live video streams
* Machine Learning model training and weight optimization
* Native video stream decoding/transcoding on the local platform
* Vehicle type recognition 
* Production hardware deployment to physical infrastructure
* Advanced municipal routing integrations (such as live GPS Emergency Vehicle Preemptions and Transit priority modules).

### 1.5 Definition of Done

- [ ] Multi-project solution compiles cleanly via dotnet build.
- [ ] All unit tests pass via dotnet test.
- [ ] EF Core migrations generate tables matching the ERD.
- [ ] Architecture Blueprint PDF contains all required diagrams and team contribution logs.
- [ ] Submitted via LMS by all group members with GitHub repository URL.

## 1.6 Referecences

[1] T. Clarke, "Hate red light waits? Sudbury project optimizing traffic flow," Sudbury.com, Aug. 15, 2023. [Online]. Available: https://www.sudbury.com/local-news/hate-red-light-waits-sudbury-project-optimizing-traffic-flow-7408291
