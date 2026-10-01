# Steering-Compatible-Turn UI Event — Research Note

Date observed: 2026-10-01  
Status: research-only, unresolved  
Surface: ChatGPT web client

## Observation

During an ordinary ChatGPT conversation, an inline red UI message appeared beneath a completed assistant response:

> Steering requires an active compatible turn

The Operator reports that they did **not** intentionally issue a steering, interrupt, modify, regenerate, or comparable direction that would explain the message.

The screenshot supplied in the research conversation shows the error immediately beneath the assistant response. The screenshot itself is not committed here; this note preserves the observed text and context without copying conversation imagery into the public repository.

## What is established

- The UI displayed the exact message: `Steering requires an active compatible turn`.
- The message appeared in ordinary chat context.
- The Operator reports no intentional steering action preceding the event.
- The event is therefore suitable for investigation as a client/system interaction whose actuator is not yet established.

## What is not established

This observation does **not** establish:

- that a hidden prompt was injected;
- that the model independently attempted to steer itself;
- that ChatGPT generated an undisclosed user instruction;
- that any Sanctuary component caused the event;
- that the event represents a security compromise;
- the exact client, server, or experiment path responsible.

Root cause remains unresolved.

## Candidate hypotheses

The following are investigation candidates only:

1. a client/backend turn-state race in which a steering request was attempted after the compatible turn had closed;
2. a stale UI control or state binding targeting a prior turn;
3. a browser/client retry or state-transition path invoking a steering endpoint without a fresh Operator action;
4. an experimental or partially exposed steering pathway whose guard rejected the current turn state.

No hypothesis is presently preferred as fact.

## Sanctuary research relevance

The event is useful because it demonstrates why action provenance should remain typed rather than collapsed into a generic "user action" record.

A minimal provenance distinction should preserve at least:

```text
user_actuated
client_actuated
system_actuated
model_actuated
unknown_actuator
```

and should permit:

```text
event observed
!=
actuator established
!=
intent established
!=
authority established
```

Where an event is visible but its actuator cannot yet be established, the correct return is unresolved rather than reassigned to the Operator by implication.

This is consistent with Sanctuary's broader governance posture:

```text
preserve event
preserve provenance
preserve uncertainty
do not infer authority from occurrence
```

## Suggested future instrumentation

If a comparable event can be observed in a controlled harness, record:

- client timestamp;
- turn identifier;
- active/completed state of the targeted turn;
- UI control or event source;
- request/response class at the connector boundary;
- actuator classification;
- whether the Operator generated an input event;
- whether a retry/replay occurred;
- guard result;
- resulting UI message;
- receipt linking the visible event to the transport event without storing sensitive payload content.

The research goal is not to prove fault from one UI artifact. It is to make future actuator provenance reconstructable.

## Disposition

Keep as a bounded research specimen. No production claim, security claim, or behavioral attribution is warranted from the present evidence.
