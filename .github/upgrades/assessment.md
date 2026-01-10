# Projects and dependencies analysis

This document provides a comprehensive overview of the projects and their dependencies in the context of upgrading to .NETCoreApp,Version=v10.0.

## Table of Contents

- [Executive Summary](#executive-Summary)
  - [Highlevel Metrics](#highlevel-metrics)
  - [Projects Compatibility](#projects-compatibility)
  - [Package Compatibility](#package-compatibility)
  - [API Compatibility](#api-compatibility)
- [Aggregate NuGet packages details](#aggregate-nuget-packages-details)
- [Top API Migration Challenges](#top-api-migration-challenges)
  - [Technologies and Features](#technologies-and-features)
  - [Most Frequent API Issues](#most-frequent-api-issues)
- [Projects Relationship Graph](#projects-relationship-graph)
- [Project Details](#project-details)

  - [Taurit.TodoistTools.Review\Taurit.TodoistTools.Review.csproj](#taurittodoisttoolsreviewtaurittodoisttoolsreviewcsproj)


## Executive Summary

### Highlevel Metrics

| Metric | Count | Status |
| :--- | :---: | :--- |
| Total Projects | 1 | All require upgrade |
| Total NuGet Packages | 4 | 1 need upgrade |
| Total Code Files | 18 |  |
| Total Code Files with Incidents | 4 |  |
| Total Lines of Code | 586 |  |
| Total Number of Issues | 8 |  |
| Estimated LOC to modify | 5+ | at least 0,9% of codebase |

### Projects Compatibility

| Project | Target Framework | Difficulty | Package Issues | API Issues | Est. LOC Impact | Description |
| :--- | :---: | :---: | :---: | :---: | :---: | :--- |
| [Taurit.TodoistTools.Review\Taurit.TodoistTools.Review.csproj](#taurittodoisttoolsreviewtaurittodoisttoolsreviewcsproj) | net9.0 | 🟢 Low | 2 | 5 | 5+ | AspNetCore, Sdk Style = True |

### Package Compatibility

| Status | Count | Percentage |
| :--- | :---: | :---: |
| ✅ Compatible | 3 | 75,0% |
| ⚠️ Incompatible | 0 | 0,0% |
| 🔄 Upgrade Recommended | 1 | 25,0% |
| ***Total NuGet Packages*** | ***4*** | ***100%*** |

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 5 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 1376 |  |
| ***Total APIs Analyzed*** | ***1381*** |  |

## Aggregate NuGet packages details

| Package | Current Version | Suggested Version | Projects | Description |
| :--- | :---: | :---: | :--- | :--- |
| Lindhart.Analyser.MissingAwaitWarning | * |  | [Taurit.TodoistTools.Review.csproj](#taurittodoisttoolsreviewtaurittodoisttoolsreviewcsproj) | ✅Compatible |
| Microsoft.AspNetCore.Mvc.NewtonsoftJson | 9.0.11 | 10.0.1 | [Taurit.TodoistTools.Review.csproj](#taurittodoisttoolsreviewtaurittodoisttoolsreviewcsproj) | NuGet package upgrade is recommended |
| Microsoft.Extensions.Hosting.WindowsServices | 10.0.1 |  | [Taurit.TodoistTools.Review.csproj](#taurittodoisttoolsreviewtaurittodoisttoolsreviewcsproj) | ✅Compatible |
| Microsoft.TypeScript.MSBuild | 5.9.3 |  | [Taurit.TodoistTools.Review.csproj](#taurittodoisttoolsreviewtaurittodoisttoolsreviewcsproj) | ✅Compatible |

## Top API Migration Challenges

### Technologies and Features

| Technology | Issues | Percentage | Migration Path |
| :--- | :---: | :---: | :--- |

### Most Frequent API Issues

| API | Count | Percentage | Category |
| :--- | :---: | :---: | :--- |
| T:System.Net.Http.HttpContent | 4 | 80,0% | Behavioral Change |
| M:Microsoft.Extensions.DependencyInjection.HttpClientFactoryServiceCollectionExtensions.AddHttpClient(Microsoft.Extensions.DependencyInjection.IServiceCollection) | 1 | 20,0% | Behavioral Change |

## Projects Relationship Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart LR
    P1["<b>📦&nbsp;Taurit.TodoistTools.Review.csproj</b><br/><small>net9.0</small>"]
    click P1 "#taurittodoisttoolsreviewtaurittodoisttoolsreviewcsproj"

```

## Project Details

<a id="taurittodoisttoolsreviewtaurittodoisttoolsreviewcsproj"></a>
### Taurit.TodoistTools.Review\Taurit.TodoistTools.Review.csproj

#### Project Info

- **Current Target Framework:** net9.0
- **Proposed Target Framework:** net10.0
- **SDK-style**: True
- **Project Kind:** AspNetCore
- **Dependencies**: 0
- **Dependants**: 0
- **Number of Files**: 35
- **Number of Files with Incidents**: 4
- **Lines of Code**: 586
- **Estimated LOC to modify**: 5+ (at least 0,9% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph current["Taurit.TodoistTools.Review.csproj"]
        MAIN["<b>📦&nbsp;Taurit.TodoistTools.Review.csproj</b><br/><small>net9.0</small>"]
        click MAIN "#taurittodoisttoolsreviewtaurittodoisttoolsreviewcsproj"
    end

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 5 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 1376 |  |
| ***Total APIs Analyzed*** | ***1381*** |  |

