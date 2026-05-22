# ReplayLab Architecture

This document describes ReplayLab with a lightweight C4-style view.

ReplayLab is split between a reusable public toolkit and consumer applications. The public repository owns generic contracts, parsers, adapters, and hostable UI surfaces. Consumer applications own their own domain-specific parsers, senders, mappings, and deployment choices.

## C1 — System Context

```mermaid
flowchart TB
    User[Developer / Test Engineer / Operator]
    ReplayLab[ReplayLab<br/>Local replay toolkit]
    Input[Replay input<br/>CSV today, custom formats later]
    Target[Replay target<br/>Mock, HTTP, or consumer-owned system]
    ConsumerTool[Consumer replay tool<br/>built outside this repository]

    User -->|loads, inspects, edits, replays| ReplayLab
    Input -->|structured records| ReplayLab
    ReplayLab -->|sends replay messages| Target
    ConsumerTool -->|references and composes| ReplayLab
```

ReplayLab provides the generic foundation: parse inputs, inspect and edit messages, replay selected rows, expose CLI/Web/Desktop entry points, and keep extension boundaries explicit.

## C2 — Containers

```mermaid
flowchart TB
    User[User]

    subgraph Apps[Public apps]
        direction LR
        CLI[ReplayLab.Cli]
        Web[ReplayLab.Web]
        Desktop[ReplayLab.Desktop]
    end

    subgraph Hosting[Hostable surfaces]
        direction LR
        CliHosting[ReplayLab.Cli.Hosting]
        WebHosting[ReplayLab.Web.Hosting]
    end

    subgraph Consumer[Consumer repository]
        direction LR
        ConsumerHost[Consumer host app]
        CustomParser[Custom parser]
        CustomSender[Custom sender]
        Mapping[Consumer mapping]
    end

    subgraph Toolkit[Reusable toolkit]
        direction TB
        Core[ReplayLab.Core]

        subgraph Inputs[Parsers]
            direction LR
            Csv[ReplayLab.Parsers.Csv]
        end

        subgraph Outputs[Adapters]
            direction LR
            Mock[ReplayLab.Adapters.Mock]
            Http[ReplayLab.Adapters.Http]
            Example[ReplayLab.Adapters.Example]
        end
    end

    User --> Apps

    CLI --> CliHosting
    Web --> WebHosting
    Desktop --> WebHosting

    CliHosting --> Core
    WebHosting --> Core

    ConsumerHost --> CliHosting
    ConsumerHost --> WebHosting
    ConsumerHost --> Core
    CustomParser --> Core
    CustomSender --> Core
    CustomSender --> Mapping

    Csv --> Core
    Mock --> Core
    Http --> Core
    Example --> Core
```

### Container notes

- `ReplayLab.Core` is the dependency root for public contracts and models.
- Parser and adapter projects depend on `ReplayLab.Core`, never the opposite.
- CLI, Web, and Desktop compose reusable hosting surfaces.
- Consumer hosts should own their own composition root.
- Consumer parser and sender implementations should stay outside the public repository.

## C3 — Core Components

```mermaid
flowchart LR
    Parser[IMessageParser]
    Sender[IReplaySender]
    Message[ReplayMessage]
    Batch[ReplayBatch]
    Result[ReplayResult]
    Engine[SequentialReplayEngine]

    Parser -->|produces| Batch
    Batch -->|contains| Message
    Engine -->|reads| Batch
    Engine -->|sends each message through| Sender
    Sender -->|returns| Result
```

| Component | Responsibility |
| --- | --- |
| `ReplayMessage` | Generic message envelope with id, payload, and headers. |
| `ReplayBatch` | Collection of parsed replay messages. |
| `ReplayResult` | Per-message replay outcome. |
| `IMessageParser` | Converts an input stream or file into a `ReplayBatch`. |
| `IReplaySender` | Sends one replay message to a target. |
| `SequentialReplayEngine` | Orchestrates sequential replay and collects results. |

## Hosting and Composition

```mermaid
flowchart TB
    Root[Composition root<br/>public app or consumer host]
    Services[IServiceCollection]
    ParserImpl[IMessageParser implementation]
    SenderImpl[IReplaySender implementation]
    Hosting[CLI / Web hosting surface]
    Workflow[Replay workflow]

    Root --> Services
    Services --> ParserImpl
    Services --> SenderImpl
    Root --> Hosting
    Hosting --> Workflow
    Workflow --> ParserImpl
    Workflow --> SenderImpl
```

ReplayLab favors static composition through .NET dependency injection.

The intended extension path is:

1. reference ReplayLab packages;
2. register a parser implementation;
3. register a sender implementation;
4. host CLI, Web, or Desktop behavior from the consumer app;
5. keep domain-specific mapping outside the public repo.

Dynamic plugin loading is not the default architecture. It can be revisited later if package/reference composition proves insufficient.

## Public / Consumer Boundary

```mermaid
flowchart LR
    subgraph Public[Public ReplayLab repository]
        Core[Generic contracts]
        PublicParsers[Generic parsers]
        PublicAdapters[Generic adapters]
        Hosting[Hostable entry points]
        Samples[Fictional samples]
    end

    subgraph Consumer[Consumer solution]
        DomainParser[Domain parser]
        DomainMapping[Mapping rules]
        DomainContracts[Domain contracts]
        DomainSender[Domain sender]
    end

    Public -->|referenced by| Consumer
    DomainParser --> Core
    DomainSender --> Core
    DomainSender --> DomainMapping
    DomainMapping --> DomainContracts
```

The public repository may contain generic contracts, parsers, senders, local mock/test adapters, HTTP adapters, fictional samples, hostable entry points, and public documentation.

The public repository should not contain consumer-specific contracts, payloads, mappings, senders, operational assumptions, or real customer data.

## Package Consumption View

```mermaid
flowchart TB
    Packages[ReplayLab packages<br/>local NuGet feed first]
    Consumer[Consumer solution]
    ParserPkg[Custom parser]
    SenderPkg[Custom sender]
    Tool[Custom replay tool<br/>CLI, Web, or Desktop]

    Packages --> Consumer
    ParserPkg --> Consumer
    SenderPkg --> Consumer
    Consumer --> Tool
```

The M10A/M10B direction is to prove this flow locally before any public NuGet publishing:

- pack selected ReplayLab projects into `artifacts/packages`;
- restore them from an external-style sample through `PackageReference`;
- demonstrate custom parser/sender composition;
- keep publishing, signing, installer work, and dynamic plugins out of scope.

## Current Architectural Decisions

| Decision | Current stance |
| --- | --- |
| Core dependency direction | `ReplayLab.Core` must stay independent of parsers, adapters, UI, hosting, and consumer concerns. |
| Extension model | Prefer package/reference composition and DI registration. |
| Consumer adapters | Keep outside the public repo. |
| Desktop shell | Public `ReplayLab.Desktop` hosts the Web UI through Photino.NET. Reusable desktop hosting may be extracted later. |
| Persistence | Deferred until UX and package adoption are proven. |
| Dynamic plugins | Deferred until static composition proves insufficient. |

## Related Documentation

- [Roadmap](roadmap.md)
- [M10A Packageable SDK plan](plans/m10-packageable-sdk.md)
- [Extension model ADR](adr/0008-extension-model.md)
- [Hostable entry points ADR](adr/0009-hostable-entry-points.md)
- [Hostable entry points milestone](milestones/m7-hostable-entry-points.md)
