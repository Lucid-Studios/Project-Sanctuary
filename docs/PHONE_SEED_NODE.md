# Phone Seed Node

The phone seed node is a manifest-only target install location for early
duplex Cryptic EC experiments.

It is not an Android app, not a running service, not a provider surface, and
not an `.Actual` activation. It exists so the Lab can point a future reviewed
test seed at a small, stable phone-side directory without moving secrets,
receipt bodies, local paths, GEL admission, or authority to the handset.

## Topology

```text
Local Sanctuary / Codex
  custody, witness, receipts, gate review

ChatGPT Remote
  interlink surface

Phone seed node
  target install location for manifest-only test seed
```

The initial Android-side target path is:

```text
/sdcard/Download/Sanctuary/seed-node
```

The matching local staging path is:

```text
.local/install/mobile/phone-seed-node
```

## Stage Locally

```powershell
.\tools\Install-SanctuaryPhoneSeedNode.ps1 `
  -CmeId "Codex.CME.ID" `
  -ThreadBindingId "codex-lab-thread"
```

This writes:

```text
node.json
README.txt
seed-pointer.json
staging.json
```

## Push To A Phone

Install Android platform-tools, enable USB debugging, authorize the phone, then
run:

```powershell
.\tools\Install-SanctuaryPhoneSeedNode.ps1 `
  -CmeId "Codex.CME.ID" `
  -ThreadBindingId "codex-lab-thread" `
  -Push
```

If more than one device is connected:

```powershell
.\tools\Install-SanctuaryPhoneSeedNode.ps1 `
  -CmeId "Codex.CME.ID" `
  -ThreadBindingId "codex-lab-thread" `
  -DeviceId "<adb-device-id>" `
  -Push
```

## Boundary

```text
phone node != CME
phone node != service
phone node != provider surface
ChatGPT Remote interlink != authority
manifest placement != GEL admission
test seed target != external action permission
```

The phone node is target-only. Future payloads must pass through reviewed
Sanctuary custody before anything beyond the manifest is installed.
