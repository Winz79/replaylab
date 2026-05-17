# BRD 0001: Product Vision

## Status

Draft

## Intent

ReplayLab should become a lightweight, generic replay/testing toolkit for developers who need to load structured messages from files, inspect them, select or filter them, and replay them through configurable sender adapters.

## Problem

Message replay is often useful for local testing, debugging, regression checks, and learning how systems behave with known inputs. Existing replay tools are often tied to a specific business domain, transport, infrastructure stack, or internal data format.

ReplayLab should provide the generic foundation without importing those private assumptions into the public repository.

## Desired Outcome

- Developers can model replayable messages without knowing a business domain.
- Developers can parse structured files into replay batches.
- Developers can send messages through generic sender adapters.
- Contributors can add parsers and adapters without coupling them to proprietary systems.
- Future CLI, UI, and packaging work can build on stable public concepts.

## Value

- Faster local investigation of message-processing behavior.
- Repeatable replay scenarios for testing and demos.
- Clear separation between public reusable tooling and private business adapters.
- A practical first product for the AI Engineering Toolkit methodology.

## Non-Goals

- ReplayLab is not a proprietary message mapping repository.
- ReplayLab is not a WCF client library.
- ReplayLab is not a certificate or secrets management tool.
- ReplayLab is not a persistence system.
- ReplayLab is not a business-specific test harness.

WCF and business-specific adapters must live outside the public repository.

## Assumptions

- The first public value is developer productivity, not enterprise workflow automation.
- The public repository should remain useful without access to private systems.
- The product can start as libraries and later add CLI and UI experiences.

## Open Questions

- Should ReplayLab position itself primarily as a library, a CLI, or a toolkit that includes both?
- Which replay workflow should be treated as the first end-to-end product proof?
- What is the minimum level of polish required before calling the project usable?
