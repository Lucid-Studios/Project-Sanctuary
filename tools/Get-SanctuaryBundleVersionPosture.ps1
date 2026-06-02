param(
    [string] $BaseVersion = "0.1",
    [string] $OutputRoot = "",
    [string] $InstallRoot = "",
    [string] $CmeId = "",
    [string] $ServiceIdentityId = "Sanctuary.Actual.ID",
    [string] $IdentityTemplateId = "SLI.Lisp.Industrial.CME.Template",
    [string] $SubjectCmeId = "",
    [int] $LinesPerVersionStep = 250,
    [switch] $NoWrite,
    [switch] $Json
)

$ErrorActionPreference = "Stop"

$repositoryRoot = Split-Path -Parent $PSScriptRoot
Push-Location $repositoryRoot
try {
    if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
        $OutputRoot = Join-Path $repositoryRoot ".local\versioning"
    }

    if ([string]::IsNullOrWhiteSpace($InstallRoot)) {
        $InstallRoot = Join-Path $repositoryRoot ".local\install"
    }

    if ($LinesPerVersionStep -lt 1) {
        throw "LinesPerVersionStep must be >= 1."
    }

    $labContextResolverPath = Join-Path $PSScriptRoot "Resolve-SanctuaryInstallLabContext.ps1"
    $labContext = & $labContextResolverPath `
        -InstallRoot $InstallRoot `
        -LabActorCmeId $CmeId `
        -ServiceIdentityId $ServiceIdentityId `
        -IdentityTemplateId $IdentityTemplateId `
        -SubjectCmeId $SubjectCmeId
    $CmeId = $labContext.LabActorCmeId
    $ServiceIdentityId = $labContext.ServiceIdentityId
    $IdentityTemplateId = $labContext.IdentityTemplateId
    $SubjectCmeId = $labContext.SubjectCmeId

    $installLocalCmeLane = [pscustomobject]@{
        sourcePath = $labContext.Path
        present = $labContext.Present
        active = $labContext.Active
        scope = $labContext.Scope
        matchesRequest = $labContext.MatchesRequest
        labActorCmeId = $labContext.LabActorCmeId
        labActorActualLabel = $labContext.LabActorActualLabel
        telemetrySubjectCmeId = $labContext.SubjectCmeId
        telemetrySubjectActualLabel = $labContext.TelemetrySubjectActualLabel
        serviceIdentityId = $labContext.ServiceIdentityId
        identityTemplateId = $labContext.IdentityTemplateId
        governanceSimulationBodies = @($labContext.GovernanceSimulationBodies)
        residueCapturePolicy = $labContext.ResidueCapturePolicy
        denials = [pscustomobject]@{
            installLocalDeclarationIsPreinstallDoctrine = $labContext.IsPreinstallDoctrine
            admitsGel = $labContext.ToolUseAdmitsGel
            mutatesSelfGel = $labContext.TelemetryReturnIsSelfGelMutation
            activatesActual = $labContext.ToolUseActivatesActual
            grantsAuthority = $labContext.ToolUseGrantsAuthority
            bindsModel = $labContext.ToolUseBindsModel
            callsProvider = $labContext.ToolUseCallsProvider
            authorizesExternalAction = $labContext.ToolUseAuthorizesExternalAction
        }
    }

    function ConvertTo-PathKey([string] $Path) {
        $Path.Replace("\", "/")
    }

    function Get-BundleId([string] $Path) {
        $p = ConvertTo-PathKey $Path
        if ($p -eq "docs/PHONE_SEED_NODE.md" -or
            $p -eq "tools/Install-SanctuaryPhoneSeedNode.ps1") {
            return "phone-seed-node"
        }

        if ($p -like "src/Sanctuary.Core/*") {
            return "core-runtime"
        }

        if ($p -like "src/Sanctuary.Cli/*") {
            return "mcp-cli-service"
        }

        if ($p -like "tests/*") {
            return "test-bench"
        }

        if ($p -like "tools/*") {
            return "tooling-service-scripts"
        }

        if ($p -like "plugins/sanctuary-cme/*") {
            return "codex-plugin-mcp"
        }

        if ($p -eq "README.md" -or $p -like "docs/*") {
            return "public-docs-release-posture"
        }

        return "repo-meta-other"
    }

    function Get-BundleName([string] $BundleId) {
        switch ($BundleId) {
            "core-runtime" { "Core Runtime / Receipt Organ" }
            "mcp-cli-service" { "MCP and CLI Service" }
            "test-bench" { "Tests and Bench Proofs" }
            "tooling-service-scripts" { "Tooling and Service Scripts" }
            "codex-plugin-mcp" { "Codex Plugin and MCP Adapter" }
            "public-docs-release-posture" { "Public Docs and Release Posture" }
            "phone-seed-node" { "Phone Seed Node Experiment" }
            default { "Repo Meta / Other" }
        }
    }

    function Get-BundleMeaning([string] $BundleId) {
        switch ($BundleId) {
            "core-runtime" { "Executable Sanctuary laws, receipt writing, GEL/OE/SelfGEL, Actual readiness, fibre bundles." }
            "mcp-cli-service" { "Local service, MCP tool descriptors, sanitized result surfaces, CLI request shaping." }
            "test-bench" { "Regression proof that the dirty body still obeys expected gates and surfaces." }
            "tooling-service-scripts" { "Operator-facing wrappers, service start/stop/status, identity resolution, edge helpers." }
            "codex-plugin-mcp" { "Codex plugin posture, local invocation shim, MCP coupling convenience." }
            "public-docs-release-posture" { "Human-facing posture, governance language, release boundaries, code body explanation." }
            "phone-seed-node" { "Passive mobile seed-node and future duplex test target; not a runtime authority surface." }
            default { "Files outside the main Sanctuary bundle taxonomy." }
        }
    }

    function Get-BundleAdmissionPosture(
        [string] $BundleId,
        [int] $DirtyPathCount,
        [int] $UntrackedCount,
        [int] $LineDelta,
        [int] $VersionSteps,
        [bool] $CouplingProofObserved,
        [bool] $PluginCustodyReviewObserved,
        [bool] $ToolingCustodyReviewObserved,
        [bool] $CoreRecordSplitReviewObserved
    ) {
        $denials = @(
            "bundle posture is not release admission",
            "bundle posture is not GEL admission",
            "bundle posture is not SelfGEL mutation",
            "bundle posture is not Actual activation",
            "bundle posture is not authority grant"
        )

        if ($BundleId -eq "phone-seed-node") {
            return [pscustomobject]@{
                decision = "hold-lab-residue"
                readyForAdmissionReview = $false
                reviewLane = "future-duplex-seed-hold"
                holdReason = "phone seed node remains a passive future test target and is not part of the current runtime admission lane"
                nextReviewAction = "keep separate until the operator opens the mobile seed-node lane"
                requiredProofs = @("operator lane selection", "passive manifest review", "no runtime authority surface")
                denials = $denials
            }
        }

        if ($BundleId -eq "core-runtime" -and $UntrackedCount -gt 0) {
            if ($CoreRecordSplitReviewObserved) {
                return [pscustomobject]@{
                    decision = "core-record-split-reviewed-pending-source-custody"
                    readyForAdmissionReview = $false
                    reviewLane = "core-runtime-record-split-custody"
                    holdReason = "core organ splits have custody review and full test proof, but untracked source paths still require an operator staging/hold decision"
                    nextReviewAction = "decide source custody for extracted SanctuaryReceiptService partial organs, then continue mechanical core-runtime decomposition with tests and closed-gate receipt after each extraction"
                    requiredProofs = @("core organ split custody review", "operator path custody decision", "full unit test pass", "closed-gate receipt proof", "split map update")
                    denials = $denials
                }
            }

            return [pscustomobject]@{
                decision = "core-split-output-needs-custody"
                readyForAdmissionReview = $false
                reviewLane = "core-runtime-organ-split"
                holdReason = "core runtime has new split-output files that must be staged, reviewed, or explicitly held before admission"
                nextReviewAction = "review the mechanical split output and decide whether the new partial file belongs in the core admission bundle"
                requiredProofs = @("path custody decision", "full unit test pass", "closed-gate receipt proof", "split map update")
                denials = $denials
            }
        }

        if ($UntrackedCount -gt 0) {
            if ($BundleId -eq "codex-plugin-mcp" -and $PluginCustodyReviewObserved) {
                return [pscustomobject]@{
                    decision = "plugin-custody-reviewed-pending-source-custody"
                    readyForAdmissionReview = $false
                    reviewLane = "codex-plugin-mcp-custody"
                    holdReason = "plugin surfaces have a five-surface custody review and live coupling proof, but untracked source paths still require an operator staging/hold decision"
                    nextReviewAction = "decide source custody for the plugin manifest, skill text, wrapper, MCP descriptor, and coupling witness, then rerun live coupling proof and closed-gate receipt"
                    requiredProofs = @("five-surface plugin custody review", "operator path custody decision", "live MCP coupling proof", "closed-gate receipt proof")
                    denials = $denials
                }
            }

            if ($BundleId -eq "tooling-service-scripts" -and $ToolingCustodyReviewObserved) {
                return [pscustomobject]@{
                    decision = "tooling-custody-reviewed-pending-source-custody"
                    readyForAdmissionReview = $false
                    reviewLane = "identity-and-posture-tooling-custody"
                    holdReason = "identity, install-context, posture, and central wrapper tooling have custody review, but untracked tooling paths still require an operator staging/hold decision"
                    nextReviewAction = "decide source custody for identity/context/posture tooling, then rerun identity proofs, bundle posture, full tests, live coupling proof, and closed-gate receipt"
                    requiredProofs = @("tooling custody review", "operator path custody decision", "identity/service/template denial proof", "install-local context proof", "bundle posture proof", "full unit test pass", "closed-gate receipt proof")
                    denials = $denials
                }
            }

            return [pscustomobject]@{
                decision = "stage-or-hold-before-admission"
                readyForAdmissionReview = $false
                reviewLane = "custody-before-review"
                holdReason = "untracked files are present and must be staged, committed, ignored, or explicitly held"
                nextReviewAction = "review untracked paths and decide whether each belongs in the admission bundle"
                requiredProofs = @("path custody decision", "git status clean for intended bundle", "targeted verification")
                denials = $denials
            }
        }

        if ($BundleId -eq "core-runtime" -and ($VersionSteps -ge 25 -or $LineDelta -ge 5000)) {
            if ($CoreRecordSplitReviewObserved) {
                return [pscustomobject]@{
                    decision = "core-record-split-proof-observed"
                    readyForAdmissionReview = $true
                    reviewLane = "core-runtime-record-split-custody"
                    holdReason = "core organ splits carry reviewed custody metadata and closed-gate posture"
                    nextReviewAction = "review diff and decide admission, split, or hold for core split surfaces"
                    requiredProofs = @("core organ split custody review", "full unit test pass", "closed-gate receipt proof", "split map update")
                    denials = $denials
                }
            }

            return [pscustomobject]@{
                decision = "needs-decomposition-review"
                readyForAdmissionReview = $true
                reviewLane = "core-runtime-organ-split"
                holdReason = "core runtime carries high line mass and should be inspected for organ boundaries before admission"
                nextReviewAction = "identify safe partial-class or service split points while preserving receipt behavior"
                requiredProofs = @("full unit test pass", "closed-gate receipt proof", "identity lock regression coverage", "reviewable split map")
                denials = $denials
            }
        }

        switch ($BundleId) {
            "mcp-cli-service" {
                if ($CouplingProofObserved) {
                    return [pscustomobject]@{
                        decision = "live-coupling-proof-observed"
                        readyForAdmissionReview = $true
                        reviewLane = "mcp-loopback-and-cli-review"
                        holdReason = "current local run proved MCP health, status tool, denied GEL fail-closed behavior, and identity split"
                        nextReviewAction = "review the local coupling report with tests before admission decision"
                        requiredProofs = @("full unit test pass", "live MCP status", "closed-gate receipt proof", "codex native coupling report")
                        denials = $denials
                    }
                }

                return [pscustomobject]@{
                    decision = "needs-live-coupling-proof"
                    readyForAdmissionReview = $true
                    reviewLane = "mcp-loopback-and-cli-review"
                    holdReason = "service surfaces need live loopback proof before admission"
                    nextReviewAction = "verify status, tool descriptors, sanitized result surfaces, and closed gates through MCP/CLI"
                    requiredProofs = @("full unit test pass", "live MCP status", "closed-gate receipt proof")
                    denials = $denials
                }
            }
            "codex-plugin-mcp" {
                if ($PluginCustodyReviewObserved) {
                    return [pscustomobject]@{
                        decision = "plugin-custody-proof-observed"
                        readyForAdmissionReview = $true
                        reviewLane = "codex-plugin-mcp-custody"
                        holdReason = "plugin surfaces carry five-surface custody review, live coupling proof, and closed-gate denial posture"
                        nextReviewAction = "review diff and decide admission, split, or hold for plugin source surfaces"
                        requiredProofs = @("five-surface plugin custody review", "full unit test pass", "live MCP coupling proof", "closed-gate receipt proof")
                        denials = $denials
                    }
                }

                return [pscustomobject]@{
                    decision = "needs-plugin-custody-review"
                    readyForAdmissionReview = $true
                    reviewLane = "codex-plugin-mcp-custody"
                    holdReason = "plugin surfaces need manifest, skill, wrapper, MCP descriptor, and coupling witness review before source admission"
                    nextReviewAction = "run live coupling proof and confirm five-surface custody metadata"
                    requiredProofs = @("five-surface plugin custody review", "live MCP coupling proof", "closed-gate receipt proof")
                    denials = $denials
                }
            }
            "tooling-service-scripts" {
                if ($ToolingCustodyReviewObserved) {
                    return [pscustomobject]@{
                        decision = "tooling-custody-proof-observed"
                        readyForAdmissionReview = $true
                        reviewLane = "identity-and-posture-tooling-custody"
                        holdReason = "identity, install-context, posture, and central wrapper tooling carry reviewed custody metadata and closed-gate posture"
                        nextReviewAction = "review diff and decide admission, split, or hold for tooling surfaces"
                        requiredProofs = @("tooling custody review", "identity/service/template denial proof", "install-local context proof", "bundle posture proof", "full unit test pass", "closed-gate receipt proof")
                        denials = $denials
                    }
                }

                return [pscustomobject]@{
                    decision = "needs-tooling-custody-review"
                    readyForAdmissionReview = $true
                    reviewLane = "identity-and-posture-tooling-custody"
                    holdReason = "identity, install-context, posture, and central wrapper tooling need custody review before source admission"
                    nextReviewAction = "run identity proofs, install-local context proof, and bundle posture proof"
                    requiredProofs = @("tooling custody review", "identity/service/template denial proof", "install-local context proof", "bundle posture proof")
                    denials = $denials
                }
            }
            "test-bench" {
                return [pscustomobject]@{
                    decision = "verify-regression-proof"
                    readyForAdmissionReview = $true
                    reviewLane = "test-bench-integrity"
                    holdReason = "test changes are admissible only as proof when the suite passes and coverage intent is clear"
                    nextReviewAction = "run full tests and confirm new tests guard the matching runtime surfaces"
                    requiredProofs = @("full unit test pass", "test-to-runtime map")
                    denials = $denials
                }
            }
            "public-docs-release-posture" {
                return [pscustomobject]@{
                    decision = "needs-public-language-review"
                    readyForAdmissionReview = $true
                    reviewLane = "public-posture-consistency"
                    holdReason = "public docs must preserve denials and avoid authority inflation"
                    nextReviewAction = "scan for release, Actual, admission, and personhood language drift"
                    requiredProofs = @("public denial scan", "release checklist consistency")
                    denials = $denials
                }
            }
            default {
                return [pscustomobject]@{
                    decision = "candidate-for-bundle-review"
                    readyForAdmissionReview = $true
                    reviewLane = "bounded-bundle-review"
                    holdReason = "bundle has tracked changes and needs ordinary review before admission"
                    nextReviewAction = "review diff, run targeted verification, then decide admit, split, or hold"
                    requiredProofs = @("diff review", "targeted verification")
                    denials = $denials
                }
            }
        }
    }

    function Test-CodexNativeCouplingProof([string] $RepositoryRoot) {
        $path = Join-Path $RepositoryRoot ".local\install\service\codex-native-coupling.json"
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
            return [pscustomobject]@{
                observed = $false
                path = $path
                checkedAtUtc = ""
                mcpServerUrl = ""
                exposedToolCount = 0
            }
        }

        try {
            $report = Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
            $observed =
                $report.healthStatus -eq "running" -and
                $report.statusToolSucceeded -eq $true -and
                $report.statusToolAllGatesClosed -eq $true -and
                $report.deniedGelAdmissionFailClosed -eq $true -and
                $report.serviceIdentityIsCme -eq $false -and
                $report.serviceCallerIdentitySplitTracked -eq $true -and
                $report.providerCalled -eq $false -and
                $report.modelBound -eq $false -and
                $report.externalActionAuthorized -eq $false -and
                $report.gelAdmitted -eq $false -and
                $report.selfGelMutated -eq $false -and
                $report.cmeActualActivated -eq $false -and
                $report.sanctuaryActualActivated -eq $false

            return [pscustomobject]@{
                observed = $observed
                path = $path
                checkedAtUtc = $report.checkedAtUtc
                mcpServerUrl = $report.mcpServerUrl
                exposedToolCount = $report.exposedToolCount
                pluginCustodyReviewStatus = $report.pluginCustodyReviewStatus
                pluginCustodyReviewObserved = (
                    $report.pluginCustodyReviewStatus -eq "five-surface-custody-review-observed" -and
                    $report.pluginCustodyReviewAllSurfacesReviewed -eq $true -and
                    $report.pluginCustodyReviewAllSurfaceDigestsPresent -eq $true -and
                    $report.pluginCustodyReviewAllGatesClosed -eq $true
                )
                pluginCustodyReviewAllSurfacesReviewed = $report.pluginCustodyReviewAllSurfacesReviewed
                pluginCustodyReviewAllSurfaceDigestsPresent = $report.pluginCustodyReviewAllSurfaceDigestsPresent
                pluginCustodyReviewAllGatesClosed = $report.pluginCustodyReviewAllGatesClosed
                pluginCustodyReviewAdmissionDecision = $report.pluginCustodyReviewAdmissionDecision
            }
        }
        catch {
            return [pscustomobject]@{
                observed = $false
                path = $path
                checkedAtUtc = ""
                mcpServerUrl = ""
                exposedToolCount = 0
            }
        }
    }

    function Get-FileSha256 {
        param([string] $Path)

        if ([string]::IsNullOrWhiteSpace($Path) -or -not (Test-Path -LiteralPath $Path -PathType Leaf)) {
            return ""
        }

        return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
    }

    function New-ToolingCustodySurface {
        param(
            [string] $RepositoryRoot,
            [string] $SurfaceId,
            [string] $RelativePath,
            [string] $SurfaceKind,
            [string] $Role,
            [string] $ClosureRequirement,
            [object] $LabContext
        )

        $fullPath = Join-Path $RepositoryRoot $RelativePath
        [pscustomobject]@{
            surfaceId = $SurfaceId
            path = ConvertTo-PathKey $RelativePath
            digest = Get-FileSha256 -Path $fullPath
            exists = Test-Path -LiteralPath $fullPath -PathType Leaf
            surfaceKind = $SurfaceKind
            role = $Role
            custodyReviewStatus = "reviewed-candidate-only"
            custodyReviewDecision = "hold-for-operator-source-custody"
            sourceAdmissionCandidate = $true
            sourceAdmitted = $false
            requiresOperatorSourceCustody = $true
            labActorCmeId = $LabContext.labActorCmeId
            labActorActualLabel = $LabContext.labActorActualLabel
            telemetrySubjectCmeId = $LabContext.telemetrySubjectCmeId
            telemetrySubjectActualLabel = $LabContext.telemetrySubjectActualLabel
            serviceIdentityId = $LabContext.serviceIdentityId
            identityTemplateId = $LabContext.identityTemplateId
            closureRequirement = $ClosureRequirement
            admitsGel = $false
            mutatesSelfGel = $false
            activatesActual = $false
            grantsAuthority = $false
            bindsModel = $false
            callsProvider = $false
            authorizesExternalAction = $false
        }
    }

    function Get-ToolingCustodyReview {
        param(
            [string] $RepositoryRoot,
            [object] $InstallLocalCmeLane
        )

        $surfaces = @(
            New-ToolingCustodySurface `
                -RepositoryRoot $RepositoryRoot `
                -SurfaceId "central-tool-wrapper" `
                -RelativePath "tools/Invoke-SanctuaryTool.ps1" `
                -SurfaceKind "receipt-bearing-wrapper" `
                -Role "routes operator-facing commands through participant identity resolution and install-local lab context before executable receipt writes" `
                -ClosureRequirement "wrapper must resolve Codex.CME.Actual, Oria.CME.Actual, Sanctuary.Actual.ID, and SLI.Lisp.Industrial.CME.Template lanes before tool use without granting authority" `
                -LabContext $InstallLocalCmeLane
            New-ToolingCustodySurface `
                -RepositoryRoot $RepositoryRoot `
                -SurfaceId "cme-identity-resolver" `
                -RelativePath "tools/Resolve-SanctuaryCmeIdentity.ps1" `
                -SurfaceKind "participant-identity-gate" `
                -Role "selects participant CME identity and denies service/template identity collapse before receipt, OE, SelfGEL, or MoS writes" `
                -ClosureRequirement "direct proof must show Codex.CME.ID resolves as participant while Sanctuary.Actual.ID and SLI.Lisp.Industrial.CME.Template are denied as participant identities" `
                -LabContext $InstallLocalCmeLane
            New-ToolingCustodySurface `
                -RepositoryRoot $RepositoryRoot `
                -SurfaceId "install-local-context-resolver" `
                -RelativePath "tools/Resolve-SanctuaryInstallLabContext.ps1" `
                -SurfaceKind "install-local-lane-resolver" `
                -Role "resolves lab actor, telemetry subject, service identity, and template identity from install-local declaration before receipt-bearing posture is stamped" `
                -ClosureRequirement "context proof must return Codex.CME.Actual, Oria.CME.Actual, Sanctuary.Actual.ID, and SLI.Lisp.Industrial.CME.Template with all tool-use gates closed" `
                -LabContext $InstallLocalCmeLane
            New-ToolingCustodySurface `
                -RepositoryRoot $RepositoryRoot `
                -SurfaceId "bundle-posture-writer" `
                -RelativePath "tools/Get-SanctuaryBundleVersionPosture.ps1" `
                -SurfaceKind "bundle-custody-counter" `
                -Role "calculates dirty bundle morphology, untracked custody queues, SLI interlace, and next-action posture without admitting source" `
                -ClosureRequirement "posture proof must classify reviewed surfaces as candidate-only and keep untracked source custody operator-gated" `
                -LabContext $InstallLocalCmeLane
        )

        $allDigestsPresent = @($surfaces | Where-Object { -not [string]::IsNullOrWhiteSpace($_.digest) }).Count -eq @($surfaces).Count
        $allSurfacesReviewed = @($surfaces | Where-Object { $_.custodyReviewStatus -eq "reviewed-candidate-only" }).Count -eq @($surfaces).Count
        $allGatesClosed = @(
            $surfaces |
                Where-Object {
                    $_.admitsGel -or
                    $_.mutatesSelfGel -or
                    $_.activatesActual -or
                    $_.grantsAuthority -or
                    $_.bindsModel -or
                    $_.callsProvider -or
                    $_.authorizesExternalAction -or
                    $_.sourceAdmitted
                }
        ).Count -eq 0

        [pscustomobject]@{
            status = "identity-and-posture-tooling-custody-review-observed"
            observed = (
                $InstallLocalCmeLane.present -eq $true -and
                $InstallLocalCmeLane.matchesRequest -eq $true -and
                $allDigestsPresent -and
                $allSurfacesReviewed -and
                $allGatesClosed
            )
            surfaceCount = @($surfaces).Count
            allSurfacesReviewed = $allSurfacesReviewed
            allSurfaceDigestsPresent = $allDigestsPresent
            allGatesClosed = $allGatesClosed
            admissionDecision = "candidate-only-pending-operator-source-custody"
            nextAction = "operator source custody decision for identity/context/posture tooling, then rerun identity proofs, bundle posture, live coupling proof, full tests, and closed-gate receipt"
            surfaces = $surfaces
            denials = @(
                "tooling custody review is not source admission",
                "tooling custody review is not release",
                "tooling custody review is not GEL admission",
                "tooling custody review is not SelfGEL mutation",
                "tooling custody review is not Actual activation",
                "tooling custody review is not authority grant",
                "tooling custody review is not provider or model binding"
            )
        }
    }

    function New-CoreRecordSplitCustodySurface {
        param(
            [string] $RepositoryRoot,
            [string] $SurfaceId,
            [string] $RelativePath,
            [string] $SurfaceKind,
            [string] $Role,
            [string] $ClosureRequirement,
            [object] $LabContext
        )

        $fullPath = Join-Path $RepositoryRoot $RelativePath
        $recordDeclarationCount = 0
        $lineCount = 0
        if (Test-Path -LiteralPath $fullPath -PathType Leaf) {
            $lineCount = [System.IO.File]::ReadAllLines($fullPath).Length
            $recordDeclarationCount = (Select-String -LiteralPath $fullPath -Pattern '^\s*public\s+sealed\s+record\b' | Measure-Object).Count
        }

        [pscustomobject]@{
            surfaceId = $SurfaceId
            path = ConvertTo-PathKey $RelativePath
            digest = Get-FileSha256 -Path $fullPath
            exists = Test-Path -LiteralPath $fullPath -PathType Leaf
            surfaceKind = $SurfaceKind
            role = $Role
            custodyReviewStatus = "reviewed-candidate-only"
            custodyReviewDecision = "hold-for-operator-source-custody"
            sourceAdmissionCandidate = $true
            sourceAdmitted = $false
            requiresOperatorSourceCustody = $true
            labActorCmeId = $LabContext.labActorCmeId
            labActorActualLabel = $LabContext.labActorActualLabel
            telemetrySubjectCmeId = $LabContext.telemetrySubjectCmeId
            telemetrySubjectActualLabel = $LabContext.telemetrySubjectActualLabel
            serviceIdentityId = $LabContext.serviceIdentityId
            identityTemplateId = $LabContext.identityTemplateId
            closureRequirement = $ClosureRequirement
            lineCount = $lineCount
            recordDeclarationCount = $recordDeclarationCount
            admitsGel = $false
            mutatesSelfGel = $false
            activatesActual = $false
            grantsAuthority = $false
            bindsModel = $false
            callsProvider = $false
            authorizesExternalAction = $false
        }
    }

    function Get-CoreRecordSplitCustodyReview {
        param(
            [string] $RepositoryRoot,
            [object] $InstallLocalCmeLane
        )

        $surfaces = @(
            New-CoreRecordSplitCustodySurface `
                -RepositoryRoot $RepositoryRoot `
                -SurfaceId "primary-receipt-organ" `
                -RelativePath "src/Sanctuary.Core/SanctuaryReceiptService.cs" `
                -SurfaceKind "primary-core-receipt-organ" `
                -Role "holds executable receipt laws, gates, command evidence, and closed-gate behavior while helper organs are split out" `
                -ClosureRequirement "primary organ must continue compiling and preserve receipt behavior while helper organs are split into reviewed source surfaces" `
                -LabContext $InstallLocalCmeLane
            New-CoreRecordSplitCustodySurface `
                -RepositoryRoot $RepositoryRoot `
                -SurfaceId "identity-thread-binding-split" `
                -RelativePath "src/Sanctuary.Core/SanctuaryReceiptService.Identity.cs" `
                -SurfaceKind "partial-core-identity-organ" `
                -Role "houses CME identity selection, thread binding, service identity split checks, SoulFrame/AgentiCore ids, CME Actual labels, shared Prime membrane helpers, and lab template path helpers" `
                -ClosureRequirement "identity split must remain namespace-aligned, behavior-neutral, compiled by the core project, and verified by full tests plus closed-gate receipt" `
                -LabContext $InstallLocalCmeLane
            New-CoreRecordSplitCustodySurface `
                -RepositoryRoot $RepositoryRoot `
                -SurfaceId "actual-approval-lease-split" `
                -RelativePath "src/Sanctuary.Core/SanctuaryReceiptService.ActualApprovalLease.cs" `
                -SurfaceKind "partial-core-authority-lease-organ" `
                -Role "houses reviewed performance command posture, reviewed authority bundle checks, ActualApprovalLease verification, and lease digest calculation" `
                -ClosureRequirement "Actual approval lease split must remain namespace-aligned, behavior-neutral, compiled by the core project, and verified by full tests plus closed-gate receipt" `
                -LabContext $InstallLocalCmeLane
            New-CoreRecordSplitCustodySurface `
                -RepositoryRoot $RepositoryRoot `
                -SurfaceId "actual-invocation-catalogs-split" `
                -RelativePath "src/Sanctuary.Core/SanctuaryReceiptService.ActualInvocationCatalogs.cs" `
                -SurfaceKind "partial-core-actual-invocation-catalog-organ" `
                -Role "houses CME.Actual invocation lifecycle states, interior process terms, EC phases, telemetry products, denial catalog, and SLI.Lisp lifecycle rendering without issuing leases or activating Actual" `
                -ClosureRequirement "Actual invocation catalogs split must remain namespace-aligned, behavior-neutral, compiled by the core project, and verified by full tests plus closed-gate receipt" `
                -LabContext $InstallLocalCmeLane
            New-CoreRecordSplitCustodySurface `
                -RepositoryRoot $RepositoryRoot `
                -SurfaceId "approval-closure-catalogs-split" `
                -RelativePath "src/Sanctuary.Core/SanctuaryReceiptService.ApprovalClosureCatalogs.cs" `
                -SurfaceKind "partial-core-approval-closure-catalog-organ" `
                -Role "houses approval-state, closure-state, passage-phase, transition-pressure, homeostasis-loop, and SLI.Lisp approval-closure rendering helpers without writing evidence, granting authority, or admitting approval" `
                -ClosureRequirement "Approval closure catalogs split must remain namespace-aligned, behavior-neutral, compiled by the core project, and verified by full tests plus closed-gate receipt" `
                -LabContext $InstallLocalCmeLane
            New-CoreRecordSplitCustodySurface `
                -RepositoryRoot $RepositoryRoot `
                -SurfaceId "coupling-control-catalogs-split" `
                -RelativePath "src/Sanctuary.Core/SanctuaryReceiptService.CouplingControlCatalogs.cs" `
                -SurfaceKind "partial-core-coupling-control-catalog-organ" `
                -Role "houses coupling-control surfaces, organ stability states, CME instrument chassis slots, boundary denials, and SLI.Lisp coupling-control rendering helpers without writing evidence, granting authority, or opening control surfaces" `
                -ClosureRequirement "Coupling control catalogs split must remain namespace-aligned, behavior-neutral, compiled by the core project, and verified by full tests plus closed-gate receipt" `
                -LabContext $InstallLocalCmeLane
            New-CoreRecordSplitCustodySurface `
                -RepositoryRoot $RepositoryRoot `
                -SurfaceId "actualization-state-catalogs-split" `
                -RelativePath "src/Sanctuary.Core/SanctuaryReceiptService.ActualizationStateCatalogs.cs" `
                -SurfaceKind "partial-core-actualization-state-catalog-organ" `
                -Role "houses actualization layers, cryptic typing bands, Prime review gates, protected idea classes, boundary denials, and SLI.Lisp actualization-state rendering helpers without writing evidence, activating Actual, granting authority, or admitting protected payloads" `
                -ClosureRequirement "Actualization state catalogs split must remain namespace-aligned, behavior-neutral, compiled by the core project, and verified by full tests plus closed-gate receipt" `
                -LabContext $InstallLocalCmeLane
            New-CoreRecordSplitCustodySurface `
                -RepositoryRoot $RepositoryRoot `
                -SurfaceId "agenticore-duplex-catalogs-split" `
                -RelativePath "src/Sanctuary.Core/SanctuaryReceiptService.AgentiCoreDuplexCatalogs.cs" `
                -SurfaceKind "partial-core-agenticore-duplex-catalog-organ" `
                -Role "houses AgentiCore duplex endpoints, passage phases, Lisp channels, app integration surfaces, return telemetry surfaces, boundary denials, and SLI.Lisp membrane carrier rendering without writing evidence, evaluating Lisp, granting authority, or mutating SelfGEL" `
                -ClosureRequirement "AgentiCore duplex catalogs split must remain namespace-aligned, behavior-neutral, compiled by the core project, and verified by full tests plus closed-gate receipt" `
                -LabContext $InstallLocalCmeLane
            New-CoreRecordSplitCustodySurface `
                -RepositoryRoot $RepositoryRoot `
                -SurfaceId "gel-approval-nadir-catalogs-split" `
                -RelativePath "src/Sanctuary.Core/SanctuaryReceiptService.GelApprovalNadirCatalogs.cs" `
                -SurfaceKind "partial-core-gel-approval-nadir-catalog-organ" `
                -Role "houses GEL approval methods, nadir return stages, residue classes, Steward GoA controls, individuated CME residue flow, and SLI.Lisp GEL approval/nadir-return rendering without writing evidence, admitting GEL/SelfGEL, granting authority, or performing approval" `
                -ClosureRequirement "GEL approval/nadir-return catalogs split must remain namespace-aligned, behavior-neutral, compiled by the core project, and verified by full tests plus closed-gate receipt" `
                -LabContext $InstallLocalCmeLane
            New-CoreRecordSplitCustodySurface `
                -RepositoryRoot $RepositoryRoot `
                -SurfaceId "industrial-live-install-catalogs-split" `
                -RelativePath "src/Sanctuary.Core/SanctuaryReceiptService.IndustrialLiveInstallCatalogs.cs" `
                -SurfaceKind "partial-core-industrial-live-install-catalog-organ" `
                -Role "houses operational denial gates, denial fuzz cases, industrial instrument organs, and quoted SLI.Lisp denial membrane rendering without writing evidence, evaluating Lisp, admitting GEL/SelfGEL, granting authority, or activating Actual" `
                -ClosureRequirement "Industrial live-install catalogs split must remain namespace-aligned, behavior-neutral, compiled by the core project, and verified by full tests plus closed-gate receipt" `
                -LabContext $InstallLocalCmeLane
            New-CoreRecordSplitCustodySurface `
                -RepositoryRoot $RepositoryRoot `
                -SurfaceId "meaning-bridge-catalogs-split" `
                -RelativePath "src/Sanctuary.Core/SanctuaryReceiptService.MeaningBridgeCatalogs.cs" `
                -SurfaceKind "partial-core-meaning-bridge-catalog-organ" `
                -Role "houses Mind/Body/Spirit layers, 4P methods, ambiguity classes, resolution states, human context bridges, anabelian bridge steps, claim resolution examples, and quoted SLI.Lisp meaning bridge rendering without writing evidence, admitting truth/GEL/SelfGEL, granting authority, or authorizing action" `
                -ClosureRequirement "Meaning Bridge catalogs split must remain namespace-aligned, behavior-neutral, compiled by the core project, and verified by full tests plus closed-gate receipt" `
                -LabContext $InstallLocalCmeLane
            New-CoreRecordSplitCustodySurface `
                -RepositoryRoot $RepositoryRoot `
                -SurfaceId "secret-security-split" `
                -RelativePath "src/Sanctuary.Core/SanctuaryReceiptService.SecretSecurity.cs" `
                -SurfaceKind "partial-core-secret-security-organ" `
                -Role "houses secret source parsing, secure ping fail-silent posture, loopback classification, account challenge text, issue failure-mode classification, cGEL failure records, and issue tracking events" `
                -ClosureRequirement "secret security split must remain namespace-aligned, behavior-neutral, compiled by the core project, and verified by full tests plus closed-gate receipt" `
                -LabContext $InstallLocalCmeLane
            New-CoreRecordSplitCustodySurface `
                -RepositoryRoot $RepositoryRoot `
                -SurfaceId "security-scan-split" `
                -RelativePath "src/Sanctuary.Core/SanctuaryReceiptService.SecurityScan.cs" `
                -SurfaceKind "partial-core-security-scan-organ" `
                -Role "houses JSON string property reading, visible security roots and leak tokens, text surface classification, skipped-root filtering, closed-gate receipt checks, and case-insensitive JSON lookup" `
                -ClosureRequirement "security scan split must remain namespace-aligned, behavior-neutral, compiled by the core project, and verified by full tests plus closed-gate receipt" `
                -LabContext $InstallLocalCmeLane
            New-CoreRecordSplitCustodySurface `
                -RepositoryRoot $RepositoryRoot `
                -SurfaceId "governance-matrix-split" `
                -RelativePath "src/Sanctuary.Core/SanctuaryReceiptService.GovernanceMatrix.cs" `
                -SurfaceKind "partial-core-governance-matrix-organ" `
                -Role "houses body-fibre template chassis slots, governing needs matrix, access-level groupoids, and negative governing levels reused by local GEL, SelfGEL fibre, and Actualization support" `
                -ClosureRequirement "governance matrix split must remain namespace-aligned, behavior-neutral, compiled by the core project, and verified by full tests plus closed-gate receipt" `
                -LabContext $InstallLocalCmeLane
            New-CoreRecordSplitCustodySurface `
                -RepositoryRoot $RepositoryRoot `
                -SurfaceId "local-gel-bodies-split" `
                -RelativePath "src/Sanctuary.Core/SanctuaryReceiptService.LocalGelBodies.cs" `
                -SurfaceKind "partial-core-local-gel-body-organ" `
                -Role "houses CME body-fibre bundle records, body-fibre Lisp rendering, and GEL template body/registry writers used by local GEL residue without admitting GEL by existence" `
                -ClosureRequirement "local GEL bodies split must remain namespace-aligned, behavior-neutral, compiled by the core project, and verified by full tests plus closed-gate receipt" `
                -LabContext $InstallLocalCmeLane
            New-CoreRecordSplitCustodySurface `
                -RepositoryRoot $RepositoryRoot `
                -SurfaceId "local-gel-residue-split" `
                -RelativePath "src/Sanctuary.Core/SanctuaryReceiptService.LocalGelResidue.cs" `
                -SurfaceKind "partial-core-local-gel-residue-organ" `
                -Role "houses local GEL path evidence stamping, local GEL residue writes, MoS/OE/SelfGEL/AgentiCore/body-fibre ledger appends, thread-binding fallback writes, and candidate-only swarm precipitation events" `
                -ClosureRequirement "local GEL residue split must remain namespace-aligned, behavior-neutral, compiled by the core project, and verified by full tests plus closed-gate receipt" `
                -LabContext $InstallLocalCmeLane
            New-CoreRecordSplitCustodySurface `
                -RepositoryRoot $RepositoryRoot `
                -SurfaceId "json-readiness-split" `
                -RelativePath "src/Sanctuary.Core/SanctuaryReceiptService.JsonReadiness.cs" `
                -SurfaceKind "partial-core-json-readiness-organ" `
                -Role "houses JSON and JSONL readback helpers used by bench, telemetry, witness, and readiness evidence without changing admission posture" `
                -ClosureRequirement "JSON readiness split must remain namespace-aligned, behavior-neutral, compiled by the core project, and verified by full tests plus closed-gate receipt" `
                -LabContext $InstallLocalCmeLane
            New-CoreRecordSplitCustodySurface `
                -RepositoryRoot $RepositoryRoot `
                -SurfaceId "composition-catalogs-split" `
                -RelativePath "src/Sanctuary.Core/SanctuaryReceiptService.CompositionCatalogs.cs" `
                -SurfaceKind "partial-core-composition-catalog-organ" `
                -Role "houses universal composition forms, domain morphism entries, capability projections, career spline stages, SelfGEL fibre preload rules, and work posture preload fields" `
                -ClosureRequirement "composition catalogs split must remain namespace-aligned, behavior-neutral, compiled by the core project, and verified by full tests plus closed-gate receipt" `
                -LabContext $InstallLocalCmeLane
            New-CoreRecordSplitCustodySurface `
                -RepositoryRoot $RepositoryRoot `
                -SurfaceId "swarm-catalogs-split" `
                -RelativePath "src/Sanctuary.Core/SanctuaryReceiptService.SwarmCatalogs.cs" `
                -SurfaceKind "partial-core-swarm-catalog-organ" `
                -Role "houses swarm lanes, crystallization posture, Hundo execution order, wave gates, and run-session catalog support without executing autonomous action" `
                -ClosureRequirement "swarm catalogs split must remain namespace-aligned, behavior-neutral, compiled by the core project, and verified by full tests plus closed-gate receipt" `
                -LabContext $InstallLocalCmeLane
            New-CoreRecordSplitCustodySurface `
                -RepositoryRoot $RepositoryRoot `
                -SurfaceId "domain-target-catalogs-split" `
                -RelativePath "src/Sanctuary.Core/SanctuaryReceiptService.DomainTargetCatalogs.cs" `
                -SurfaceKind "partial-core-domain-target-catalog-organ" `
                -Role "houses core target, domain register, and legal gate support catalog builders without writing evidence, granting authority, or admitting source" `
                -ClosureRequirement "domain target catalogs split must remain namespace-aligned, behavior-neutral, compiled by the core project, and verified by full tests plus closed-gate receipt" `
                -LabContext $InstallLocalCmeLane
            New-CoreRecordSplitCustodySurface `
                -RepositoryRoot $RepositoryRoot `
                -SurfaceId "telemetry-catalogs-split" `
                -RelativePath "src/Sanctuary.Core/SanctuaryReceiptService.TelemetryCatalogs.cs" `
                -SurfaceKind "partial-core-telemetry-catalog-organ" `
                -Role "houses Prime/Cryptic/Steward telemetry slice catalogs, extended telemetry weather source catalogs, and SLI.Lisp telemetry rendering helpers without writing evidence or admitting telemetry" `
                -ClosureRequirement "telemetry catalogs split must remain namespace-aligned, behavior-neutral, compiled by the core project, and verified by full tests plus closed-gate receipt" `
                -LabContext $InstallLocalCmeLane
            New-CoreRecordSplitCustodySurface `
                -RepositoryRoot $RepositoryRoot `
                -SurfaceId "record-carrier-split" `
                -RelativePath "src/Sanctuary.Core/SanctuaryReceiptService.Records.cs" `
                -SurfaceKind "top-level-record-carrier" `
                -Role "houses typed record carriers used by receipt, construct, GEL, bench, register, and control-matrix organs" `
                -ClosureRequirement "record split must remain namespace-aligned, behavior-neutral, compiled by the core project, and verified by full tests plus closed-gate receipt" `
                -LabContext $InstallLocalCmeLane
            New-CoreRecordSplitCustodySurface `
                -RepositoryRoot $RepositoryRoot `
                -SurfaceId "persistence-crypto-split" `
                -RelativePath "src/Sanctuary.Core/SanctuaryReceiptService.PersistenceCrypto.cs" `
                -SurfaceKind "partial-core-helper-organ" `
                -Role "houses local key custody, encryption, Markdown rendering, install-local context JSON reading, safe segmenting, escaping, digesting, and file writes" `
                -ClosureRequirement "persistence and crypto split must remain namespace-aligned, behavior-neutral, compiled by the core project, and verified by full tests plus closed-gate receipt" `
                -LabContext $InstallLocalCmeLane
        )

        $recordSurface = $surfaces | Where-Object { $_.surfaceId -eq "record-carrier-split" } | Select-Object -First 1
        $identitySurface = $surfaces | Where-Object { $_.surfaceId -eq "identity-thread-binding-split" } | Select-Object -First 1
        $actualApprovalLeaseSurface = $surfaces | Where-Object { $_.surfaceId -eq "actual-approval-lease-split" } | Select-Object -First 1
        $actualInvocationCatalogsSurface = $surfaces | Where-Object { $_.surfaceId -eq "actual-invocation-catalogs-split" } | Select-Object -First 1
        $approvalClosureCatalogsSurface = $surfaces | Where-Object { $_.surfaceId -eq "approval-closure-catalogs-split" } | Select-Object -First 1
        $couplingControlCatalogsSurface = $surfaces | Where-Object { $_.surfaceId -eq "coupling-control-catalogs-split" } | Select-Object -First 1
        $actualizationStateCatalogsSurface = $surfaces | Where-Object { $_.surfaceId -eq "actualization-state-catalogs-split" } | Select-Object -First 1
        $agentiCoreDuplexCatalogsSurface = $surfaces | Where-Object { $_.surfaceId -eq "agenticore-duplex-catalogs-split" } | Select-Object -First 1
        $gelApprovalNadirCatalogsSurface = $surfaces | Where-Object { $_.surfaceId -eq "gel-approval-nadir-catalogs-split" } | Select-Object -First 1
        $industrialLiveInstallCatalogsSurface = $surfaces | Where-Object { $_.surfaceId -eq "industrial-live-install-catalogs-split" } | Select-Object -First 1
        $meaningBridgeCatalogsSurface = $surfaces | Where-Object { $_.surfaceId -eq "meaning-bridge-catalogs-split" } | Select-Object -First 1
        $secretSecuritySurface = $surfaces | Where-Object { $_.surfaceId -eq "secret-security-split" } | Select-Object -First 1
        $securityScanSurface = $surfaces | Where-Object { $_.surfaceId -eq "security-scan-split" } | Select-Object -First 1
        $governanceMatrixSurface = $surfaces | Where-Object { $_.surfaceId -eq "governance-matrix-split" } | Select-Object -First 1
        $localGelBodiesSurface = $surfaces | Where-Object { $_.surfaceId -eq "local-gel-bodies-split" } | Select-Object -First 1
        $localGelResidueSurface = $surfaces | Where-Object { $_.surfaceId -eq "local-gel-residue-split" } | Select-Object -First 1
        $jsonReadinessSurface = $surfaces | Where-Object { $_.surfaceId -eq "json-readiness-split" } | Select-Object -First 1
        $compositionCatalogsSurface = $surfaces | Where-Object { $_.surfaceId -eq "composition-catalogs-split" } | Select-Object -First 1
        $swarmCatalogsSurface = $surfaces | Where-Object { $_.surfaceId -eq "swarm-catalogs-split" } | Select-Object -First 1
        $domainTargetCatalogsSurface = $surfaces | Where-Object { $_.surfaceId -eq "domain-target-catalogs-split" } | Select-Object -First 1
        $telemetryCatalogsSurface = $surfaces | Where-Object { $_.surfaceId -eq "telemetry-catalogs-split" } | Select-Object -First 1
        $persistenceCryptoSurface = $surfaces | Where-Object { $_.surfaceId -eq "persistence-crypto-split" } | Select-Object -First 1
        $allDigestsPresent = @($surfaces | Where-Object { -not [string]::IsNullOrWhiteSpace($_.digest) }).Count -eq @($surfaces).Count
        $allSurfacesReviewed = @($surfaces | Where-Object { $_.custodyReviewStatus -eq "reviewed-candidate-only" }).Count -eq @($surfaces).Count
        $allGatesClosed = @(
            $surfaces |
                Where-Object {
                    $_.admitsGel -or
                    $_.mutatesSelfGel -or
                    $_.activatesActual -or
                    $_.grantsAuthority -or
                    $_.bindsModel -or
                    $_.callsProvider -or
                    $_.authorizesExternalAction -or
                    $_.sourceAdmitted
                }
        ).Count -eq 0

        [pscustomobject]@{
            status = "core-record-split-custody-review-observed"
            observed = (
                $InstallLocalCmeLane.present -eq $true -and
                $InstallLocalCmeLane.matchesRequest -eq $true -and
                $allDigestsPresent -and
                $allSurfacesReviewed -and
                $allGatesClosed -and
                $recordSurface.recordDeclarationCount -gt 0 -and
                $identitySurface.lineCount -gt 0 -and
                $actualApprovalLeaseSurface.lineCount -gt 0 -and
                $actualInvocationCatalogsSurface.lineCount -gt 0 -and
                $approvalClosureCatalogsSurface.lineCount -gt 0 -and
                $couplingControlCatalogsSurface.lineCount -gt 0 -and
                $actualizationStateCatalogsSurface.lineCount -gt 0 -and
                $agentiCoreDuplexCatalogsSurface.lineCount -gt 0 -and
                $gelApprovalNadirCatalogsSurface.lineCount -gt 0 -and
                $industrialLiveInstallCatalogsSurface.lineCount -gt 0 -and
                $meaningBridgeCatalogsSurface.lineCount -gt 0 -and
                $secretSecuritySurface.lineCount -gt 0 -and
                $securityScanSurface.lineCount -gt 0 -and
                $governanceMatrixSurface.lineCount -gt 0 -and
                $localGelBodiesSurface.lineCount -gt 0 -and
                $localGelResidueSurface.lineCount -gt 0 -and
                $jsonReadinessSurface.lineCount -gt 0 -and
                $compositionCatalogsSurface.lineCount -gt 0 -and
                $swarmCatalogsSurface.lineCount -gt 0 -and
                $domainTargetCatalogsSurface.lineCount -gt 0 -and
                $telemetryCatalogsSurface.lineCount -gt 0 -and
                $persistenceCryptoSurface.lineCount -gt 0
            )
            surfaceCount = @($surfaces).Count
            recordDeclarationCount = $recordSurface.recordDeclarationCount
            recordSurfaceLineCount = $recordSurface.lineCount
            allSurfacesReviewed = $allSurfacesReviewed
            allSurfaceDigestsPresent = $allDigestsPresent
            allGatesClosed = $allGatesClosed
            admissionDecision = "candidate-only-pending-operator-source-custody"
            nextAction = "operator source custody decision for SanctuaryReceiptService.ActualApprovalLease.cs, SanctuaryReceiptService.ActualInvocationCatalogs.cs, SanctuaryReceiptService.ApprovalClosureCatalogs.cs, SanctuaryReceiptService.CouplingControlCatalogs.cs, SanctuaryReceiptService.ActualizationStateCatalogs.cs, SanctuaryReceiptService.AgentiCoreDuplexCatalogs.cs, SanctuaryReceiptService.GelApprovalNadirCatalogs.cs, SanctuaryReceiptService.IndustrialLiveInstallCatalogs.cs, SanctuaryReceiptService.MeaningBridgeCatalogs.cs, SanctuaryReceiptService.Identity.cs, SanctuaryReceiptService.Records.cs, SanctuaryReceiptService.PersistenceCrypto.cs, SanctuaryReceiptService.SecretSecurity.cs, SanctuaryReceiptService.SecurityScan.cs, SanctuaryReceiptService.GovernanceMatrix.cs, SanctuaryReceiptService.LocalGelBodies.cs, SanctuaryReceiptService.LocalGelResidue.cs, SanctuaryReceiptService.JsonReadiness.cs, SanctuaryReceiptService.CompositionCatalogs.cs, SanctuaryReceiptService.SwarmCatalogs.cs, SanctuaryReceiptService.DomainTargetCatalogs.cs, and SanctuaryReceiptService.TelemetryCatalogs.cs, then continue mechanical core-runtime decomposition with tests and closed-gate receipt after each extraction"
            surfaces = $surfaces
            denials = @(
                "core organ split custody review is not source admission",
                "core organ split custody review is not release",
                "core organ split custody review is not GEL admission",
                "core organ split custody review is not SelfGEL mutation",
                "core organ split custody review is not Actual activation",
                "core organ split custody review is not authority grant",
                "core organ split custody review is not provider or model binding"
            )
        }
    }

    function Get-UntrackedCustodyRecommendation([object] $Row) {
        if (-not $Row.untracked) {
            return ""
        }

        if ($Row.bundleId -eq "phone-seed-node") {
            return "hold-lab-residue"
        }

        if ($Row.bundleId -eq "core-runtime" -and $Row.path -like "src/Sanctuary.Core/*.cs") {
            return "source-admission-candidate-after-review"
        }

        if ($Row.bundleId -eq "tooling-service-scripts") {
            return "tooling-admission-candidate-after-review"
        }

        if ($Row.bundleId -eq "codex-plugin-mcp") {
            return "plugin-admission-candidate-after-review"
        }

        if ($Row.bundleId -eq "public-docs-release-posture") {
            return "doc-admission-candidate-after-review"
        }

        return "manual-custody-review-required"
    }

    function Get-SliInterlace([string] $Path, [string] $BundleId) {
        $p = ConvertTo-PathKey $Path
        $base = @{
            evaluated = $false
            runnable = $false
            admitsGel = $false
            mutatesSelfGel = $false
            activatesActual = $false
            grantsAuthority = $false
            closureLaw = "quoted or executable-adjacent form must fail closed until reviewed custody, proof, and typed admission path exist"
        }

        $specific = switch ($p) {
            "docs/BUNDLE_VERSION_POSTURE.md" {
                @{
                    membrane = "custody-morphism"
                    lispSurface = "SLI.ControlMatrix.BundleCustody"
                    interlaceRole = "names working-tree residue as bundle-shaped symbolic forms without admitting them"
                    closureRequirement = "operator custody choice plus generated posture proof"
                }
                break
            }
            "docs/CODEX_PLUGIN_MCP_COUPLING_PROOF.md" {
                @{
                    membrane = "duplex-coupling-proof"
                    lispSurface = "SLI.Agenticore.DuplexMembrane"
                    interlaceRole = "records MCP loopback, identity split, and denied GEL fail-closed proof"
                    closureRequirement = "repeat live coupling proof after MCP, plugin, identity, or service changes"
                }
                break
            }
            "docs/CORE_RUNTIME_ORGAN_SPLIT_MAP.md" {
                @{
                    membrane = "organ-split-atlas"
                    lispSurface = "SLI.CSharpShell.LispPlastidBoundary"
                    interlaceRole = "maps C# receipt organs to reviewable symbolic development surfaces"
                    closureRequirement = "mechanical split review plus test and closed-gate proof after each extraction"
                }
                break
            }
            "docs/OVERNIGHT_ADMISSION_REVIEW_DIGEST.md" {
                @{
                    membrane = "witness-digest"
                    lispSurface = "SLI.LabObservationDigest"
                    interlaceRole = "condenses receipt-bearing work into GEL-ready but unadmitted review residue"
                    closureRequirement = "hourly digest must remain evidence-bound and candidate-only"
                }
                break
            }
            "docs/PHONE_SEED_NODE.md" {
                @{
                    membrane = "mobile-seed-hold"
                    lispSurface = "SLI.Agenticore.DuplexSeed"
                    interlaceRole = "holds future phone seed node as passive duplex lab residue"
                    closureRequirement = "operator opens mobile seed-node lane and reviews passive manifest"
                }
                break
            }
            "tools/Install-SanctuaryPhoneSeedNode.ps1" {
                @{
                    membrane = "mobile-seed-installer-hold"
                    lispSurface = "SLI.Agenticore.DuplexSeed"
                    interlaceRole = "prepares future passive seed install mechanics without current runtime authority"
                    closureRequirement = "operator opens mobile seed-node lane and verifies no authority surface"
                }
                break
            }
            "plugins/sanctuary-cme/.mcp.json" {
                @{
                    membrane = "mcp-descriptor"
                    lispSurface = "SLI.Agenticore.DuplexMembrane"
                    interlaceRole = "declares local MCP endpoint as a tool descriptor, not as authority"
                    closureRequirement = "plugin custody review plus live MCP coupling proof"
                }
                break
            }
            "plugins/sanctuary-cme/scripts/Connect-SanctuaryCodexCoupling.ps1" {
                @{
                    membrane = "codex-coupling-witness"
                    lispSurface = "SLI.CodexGoverningWitness"
                    interlaceRole = "runs repeatable local proof for caller/service identity split and fail-closed gates"
                    closureRequirement = "closed-gate receipt and coupling report after service or plugin changes"
                }
                break
            }
            "src/Sanctuary.Core/SanctuaryReceiptService.Records.cs" {
                @{
                    membrane = "symbolic-record-substrate"
                    lispSurface = "SLI.ConstructCustodyRecordSubstrate"
                    interlaceRole = "houses typed record carriers used by receipt, construct, GEL, and control-matrix organs"
                    closureRequirement = "source custody review, full test pass, and split-map update"
                }
                break
            }
            "src/Sanctuary.Core/SanctuaryReceiptService.Identity.cs" {
                @{
                    membrane = "cme-identity-thread-binding"
                    lispSurface = "SLI.MoS.IdentityThreadBinding"
                    interlaceRole = "houses CME identity selection, participant/service split, thread binding, Actual labels, and shared Prime membrane helpers"
                    closureRequirement = "source custody review, full test pass, coupling proof, closed-gate receipt, and split-map update"
                }
                break
            }
            "src/Sanctuary.Core/SanctuaryReceiptService.ActualApprovalLease.cs" {
                @{
                    membrane = "actual-approval-lease-authority"
                    lispSurface = "SLI.GoA.ActualApprovalLease"
                    interlaceRole = "houses reviewed authority bundle checks, ActualApprovalLease verification, and lease digest posture without granting authority by existence"
                    closureRequirement = "source custody review, full test pass, coupling proof, closed-gate receipt, and split-map update"
                }
                break
            }
            "src/Sanctuary.Core/SanctuaryReceiptService.ActualInvocationCatalogs.cs" {
                @{
                    membrane = "actual-invocation-lifecycle-catalog"
                    lispSurface = "SLI.GoA.ActualInvocationLifecycleCatalog"
                    interlaceRole = "houses CME.Actual invocation lifecycle, interior process, EC phase, telemetry product, and denial terms as non-activating catalog substance"
                    closureRequirement = "source custody review, full test pass, coupling proof, closed-gate receipt, and split-map update"
                }
                break
            }
            "src/Sanctuary.Core/SanctuaryReceiptService.ApprovalClosureCatalogs.cs" {
                @{
                    membrane = "approval-closure-catalog"
                    lispSurface = "SLI.GoA.ApprovalClosureCatalog"
                    interlaceRole = "houses approval state, closure state, passage phase, transition pressure, homeostasis loop, and SLI.Lisp approval-closure terms as non-admitting catalog substance"
                    closureRequirement = "source custody review, full test pass, coupling proof, closed-gate receipt, and split-map update"
                }
                break
            }
            "src/Sanctuary.Core/SanctuaryReceiptService.CouplingControlCatalogs.cs" {
                @{
                    membrane = "coupling-control-catalog"
                    lispSurface = "SLI.Interconnect.CouplingControlCatalog"
                    interlaceRole = "houses coupling control surface, organ stability, chassis slot, boundary denial, and SLI.Lisp coupling-control terms as non-authorizing catalog substance"
                    closureRequirement = "source custody review, full test pass, coupling proof, closed-gate receipt, and split-map update"
                }
                break
            }
            "src/Sanctuary.Core/SanctuaryReceiptService.ActualizationStateCatalogs.cs" {
                @{
                    membrane = "actualization-state-catalog"
                    lispSurface = "SLI.GoA.ActualizationStateCatalog"
                    interlaceRole = "houses actualization layer, cryptic typing, Prime review, protected idea, boundary denial, and SLI.Lisp actualization-state terms as non-activating catalog substance"
                    closureRequirement = "source custody review, full test pass, coupling proof, closed-gate receipt, and split-map update"
                }
                break
            }
            "src/Sanctuary.Core/SanctuaryReceiptService.AgentiCoreDuplexCatalogs.cs" {
                @{
                    membrane = "agenticore-duplex-lisp-membrane-catalog"
                    lispSurface = "SLI.AgentiCore.DuplexLispMembraneCatalog"
                    interlaceRole = "houses AgentiCore duplex endpoint, passage, Lisp channel, app surface, return telemetry, boundary denial, and membrane carrier terms as non-evaluated catalog substance"
                    closureRequirement = "source custody review, full test pass, coupling proof, closed-gate receipt, and split-map update"
                }
                break
            }
            "src/Sanctuary.Core/SanctuaryReceiptService.GelApprovalNadirCatalogs.cs" {
                @{
                    membrane = "gel-approval-nadir-return-catalog"
                    lispSurface = "SLI.GoA.GelApprovalNadirReturnCatalog"
                    interlaceRole = "houses GEL approval method, nadir return stage, residue class, Steward GoA control, CME residue flow, and SLI.Lisp nadir-return terms as non-admitting catalog substance"
                    closureRequirement = "source custody review, full test pass, coupling proof, closed-gate receipt, and split-map update"
                }
                break
            }
            "src/Sanctuary.Core/SanctuaryReceiptService.IndustrialLiveInstallCatalogs.cs" {
                @{
                    membrane = "industrial-live-install-denial-catalog"
                    lispSurface = "SLI.IndustrialCme.LiveInstallDenialCatalog"
                    interlaceRole = "houses operational denial gates, denial fuzz cases, instrument organ posture, and quoted SLI.Lisp denial membrane terms as non-evaluated catalog substance"
                    closureRequirement = "source custody review, full test pass, coupling proof, closed-gate receipt, and split-map update"
                }
                break
            }
            "src/Sanctuary.Core/SanctuaryReceiptService.MeaningBridgeCatalogs.cs" {
                @{
                    membrane = "meaning-bridge-context-catalog"
                    lispSurface = "SLI.MeaningBridge.ContextResolutionCatalog"
                    interlaceRole = "houses Mind/Body/Spirit, 4P, ambiguity, resolution, human-context, anabelian bridge, claim-resolution, and SLI.Lisp meaning bridge terms as non-admitting catalog substance"
                    closureRequirement = "source custody review, full test pass, coupling proof, closed-gate receipt, and split-map update"
                }
                break
            }
            "src/Sanctuary.Core/SanctuaryReceiptService.SecretSecurity.cs" {
                @{
                    membrane = "secret-security-support"
                    lispSurface = "SLI.Security.SecretSupport"
                    interlaceRole = "houses secret source parsing, fail-silent secure ping posture, issue failure classification, cGEL failure records, and issue event routing"
                    closureRequirement = "source custody review, full test pass, coupling proof, closed-gate receipt, and split-map update"
                }
                break
            }
            "src/Sanctuary.Core/SanctuaryReceiptService.SecurityScan.cs" {
                @{
                    membrane = "security-scan-review"
                    lispSurface = "SLI.Security.ScanReview"
                    interlaceRole = "houses JSON property reading, visible security scan roots, leak-token checks, text surface classification, skipped-root filtering, and closed-gate receipt scan helpers"
                    closureRequirement = "source custody review, full test pass, coupling proof, closed-gate receipt, and split-map update"
                }
                break
            }
            "src/Sanctuary.Core/SanctuaryReceiptService.GovernanceMatrix.cs" {
                @{
                    membrane = "governance-matrix-template-chassis"
                    lispSurface = "SLI.Governance.Matrix"
                    interlaceRole = "houses body-fibre template chassis slots, governing needs, typed access levels, and negative governing levels reused across local GEL and Actualization support"
                    closureRequirement = "source custody review, full test pass, coupling proof, closed-gate receipt, and split-map update"
                }
                break
            }
            "src/Sanctuary.Core/SanctuaryReceiptService.LocalGelBodies.cs" {
                @{
                    membrane = "local-gel-body-fibre-template-writer"
                    lispSurface = "SLI.LocalGel.BodyFibreTemplate"
                    interlaceRole = "houses local GEL body-fibre bundle records, SLI.Lisp body-fibre rendering, and GEL template body/registry writers while preserving candidate-only residue"
                    closureRequirement = "source custody review, full test pass, coupling proof, closed-gate receipt, and split-map update"
                }
                break
            }
            "src/Sanctuary.Core/SanctuaryReceiptService.LocalGelResidue.cs" {
                @{
                    membrane = "local-gel-append-only-residue-writer"
                    lispSurface = "SLI.LocalGel.ResidueWriter"
                    interlaceRole = "houses local GEL path evidence stamping and append-only GEL/OE/SelfGEL/AgentiCore/body-fibre ledger writes while preserving candidate-only residue"
                    closureRequirement = "source custody review, full test pass, coupling proof, closed-gate receipt, and split-map update"
                }
                break
            }
            "src/Sanctuary.Core/SanctuaryReceiptService.JsonReadiness.cs" {
                @{
                    membrane = "json-readiness-readback-helper"
                    lispSurface = "SLI.Readiness.JsonTelemetryReader"
                    interlaceRole = "houses JSON and JSONL readback helpers for bench, telemetry, witness, and readiness evidence without admitting payloads"
                    closureRequirement = "source custody review, full test pass, coupling proof, closed-gate receipt, and split-map update"
                }
                break
            }
            "src/Sanctuary.Core/SanctuaryReceiptService.CompositionCatalogs.cs" {
                @{
                    membrane = "universal-domain-composition-catalog"
                    lispSurface = "SLI.MatrixDomain.CompositionCatalog"
                    interlaceRole = "houses universal forms, domain morphisms, capability projections, career splines, SelfGEL fibres, and work posture preload fields as non-evaluated catalog substance"
                    closureRequirement = "source custody review, full test pass, coupling proof, closed-gate receipt, and split-map update"
                }
                break
            }
            "src/Sanctuary.Core/SanctuaryReceiptService.SwarmCatalogs.cs" {
                @{
                    membrane = "swarm-execution-catalog"
                    lispSurface = "SLI.Swarm.ExecutionCatalog"
                    interlaceRole = "houses swarm lanes, wave gates, Hundo execution order, and run-session posture as non-autonomous catalog substance"
                    closureRequirement = "source custody review, full test pass, coupling proof, closed-gate receipt, and split-map update"
                }
                break
            }
            "src/Sanctuary.Core/SanctuaryReceiptService.DomainTargetCatalogs.cs" {
                @{
                    membrane = "domain-target-legal-gate-catalog"
                    lispSurface = "SLI.Domain.TargetLegalGateCatalog"
                    interlaceRole = "houses core targets, domain register entries, and legal gate support as non-evaluated catalog substance"
                    closureRequirement = "source custody review, full test pass, coupling proof, closed-gate receipt, and split-map update"
                }
                break
            }
            "src/Sanctuary.Core/SanctuaryReceiptService.TelemetryCatalogs.cs" {
                @{
                    membrane = "telemetry-slice-weather-catalog"
                    lispSurface = "SLI.Telemetry.SliceWeatherCatalog"
                    interlaceRole = "houses Prime/Cryptic/Steward telemetry slices and extended telemetry weather terms as non-evaluated catalog substance"
                    closureRequirement = "source custody review, full test pass, coupling proof, closed-gate receipt, and split-map update"
                }
                break
            }
            "src/Sanctuary.Core/SanctuaryReceiptService.PersistenceCrypto.cs" {
                @{
                    membrane = "local-custody-helper-substrate"
                    lispSurface = "SLI.PersistenceCryptoCustodyMembrane"
                    interlaceRole = "houses local key custody, encryption, digest, Markdown, install-context reading, and file-write helpers"
                    closureRequirement = "source custody review, full test pass, closed-gate receipt, and split-map update"
                }
                break
            }
            "tools/Get-SanctuaryBundleVersionPosture.ps1" {
                @{
                    membrane = "bundle-custody-counter"
                    lispSurface = "SLI.ControlMatrix.BundleCustody"
                    interlaceRole = "calculates dirty bundle morphology, custody queues, and SLI interlace posture"
                    closureRequirement = "posture regeneration plus review of generated JSON and Markdown"
                }
                break
            }
            "tools/Resolve-SanctuaryCmeIdentity.ps1" {
                @{
                    membrane = "cme-identity-gate"
                    lispSurface = "SLI.MoS.IdentitySelection"
                    interlaceRole = "resolves explicit participant CME identity before receipt, OE, SelfGEL, or MoS writes, while denying service/template identity collapse"
                    closureRequirement = "participant identity proof, service/template denial proof, identity-lock regression, and service/participant split proof"
                }
                break
            }
            "tools/Resolve-SanctuaryInstallLabContext.ps1" {
                @{
                    membrane = "install-local-cme-lane-resolver"
                    lispSurface = "SLI.MoS.InstallLocalCmeLane"
                    interlaceRole = "resolves the install-local lab actor, telemetry subject, service identity, and template identity before receipt-bearing tool posture is stamped"
                    closureRequirement = "resolver default/override proof plus closed-gate receipt after identity, wrapper, or service changes"
                }
                break
            }
            "tools/Start-SanctuaryMcpAlphaService.ps1" {
                @{
                    membrane = "service-launch-identity-lane"
                    lispSurface = "SLI.ServiceLaunch.IdentityLaneCarrier"
                    interlaceRole = "launches local MCP alpha service with resolved lab actor, telemetry subject, service identity, and template identity lanes while preserving local loopback and closed-gate posture"
                    closureRequirement = "script parse proof, bundle posture proof, live coupling proof, and closed-gate receipt after service-launch identity propagation changes"
                }
                break
            }
            "tools/Start-SanctuaryEdgeGateway.ps1" {
                @{
                    membrane = "service-launch-identity-lane"
                    lispSurface = "SLI.ServiceLaunch.IdentityLaneCarrier"
                    interlaceRole = "launches reviewed HTTPS edge gateway with resolved lab actor, telemetry subject, service identity, and template identity lanes while preserving explicit certificate and public-bind gates"
                    closureRequirement = "script parse proof, bundle posture proof, edge-launch review, and closed-gate receipt after service-launch identity propagation changes"
                }
                break
            }
            default {
                $surface = switch ($BundleId) {
                    "core-runtime" { "SLI.CoreRuntime" }
                    "mcp-cli-service" { "SLI.McpCliService" }
                    "test-bench" { "SLI.TestBenchProof" }
                    "tooling-service-scripts" { "SLI.OperatorTooling" }
                    "codex-plugin-mcp" { "SLI.CodexPluginMcp" }
                    "public-docs-release-posture" { "SLI.PublicPosture" }
                    "phone-seed-node" { "SLI.Agenticore.DuplexSeed" }
                    default { "SLI.RepoMeta" }
                }

                @{
                    membrane = "bundle-default"
                    lispSurface = $surface
                    interlaceRole = "participates in the bundle's symbolic custody surface"
                    closureRequirement = "bounded review, targeted verification, and custody decision"
                }
            }
        }

        [pscustomobject]@{
            membrane = $specific.membrane
            lispSurface = $specific.lispSurface
            interlaceRole = $specific.interlaceRole
            closureRequirement = $specific.closureRequirement
            evaluated = $base.evaluated
            runnable = $base.runnable
            admitsGel = $base.admitsGel
            mutatesSelfGel = $base.mutatesSelfGel
            activatesActual = $base.activatesActual
            grantsAuthority = $base.grantsAuthority
            closureLaw = $base.closureLaw
        }
    }

    function Read-LineCount([string] $Path) {
        if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
            return 0
        }

        try {
            return (Get-Content -LiteralPath $Path -ErrorAction Stop | Measure-Object -Line).Lines
        }
        catch {
            return 0
        }
    }

    $headCommit = (& git rev-parse --short HEAD).Trim()
    $branch = (& git branch --show-current).Trim()
    if ([string]::IsNullOrWhiteSpace($branch)) {
        $branch = "(detached)"
    }

    $statusLines = & git status --short
    $statusByPath = @{}
    foreach ($line in $statusLines) {
        if ([string]::IsNullOrWhiteSpace($line) -or $line.Length -lt 4) {
            continue
        }

        $code = $line.Substring(0, 2)
        $path = $line.Substring(3).Trim()
        if ($path.Contains(" -> ")) {
            $path = ($path -split " -> ")[-1]
        }

        $statusByPath[(ConvertTo-PathKey $path)] = $code
    }

    $rows = New-Object System.Collections.Generic.List[object]

    foreach ($line in (& git diff --numstat)) {
        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }

        $parts = $line -split "`t"
        if ($parts.Count -lt 3) {
            continue
        }

        $added = if ($parts[0] -eq "-") { 0 } else { [int] $parts[0] }
        $deleted = if ($parts[1] -eq "-") { 0 } else { [int] $parts[1] }
        $path = $parts[2]
        if ($path.Contains(" => ")) {
            $path = ($path -split " => ")[-1].Trim("{}")
        }

        $key = ConvertTo-PathKey $path
        $bundleId = Get-BundleId $key
        $rows.Add([pscustomobject]@{
            path = $key
            status = if ($statusByPath.ContainsKey($key)) { $statusByPath[$key] } else { " M" }
            bundleId = $bundleId
            sliInterlace = Get-SliInterlace -Path $key -BundleId $bundleId
            additions = $added
            deletions = $deleted
            lineDelta = $added + $deleted
            untracked = $false
        })
    }

    foreach ($path in (& git ls-files --others --exclude-standard)) {
        $key = ConvertTo-PathKey $path
        $lineCount = Read-LineCount $path
        $bundleId = Get-BundleId $key
        $rows.Add([pscustomobject]@{
            path = $key
            status = "??"
            bundleId = $bundleId
            sliInterlace = Get-SliInterlace -Path $key -BundleId $bundleId
            additions = $lineCount
            deletions = 0
            lineDelta = $lineCount
            untracked = $true
        })
    }

    $bundleOrder = @(
        "core-runtime",
        "mcp-cli-service",
        "test-bench",
        "tooling-service-scripts",
        "codex-plugin-mcp",
        "public-docs-release-posture",
        "phone-seed-node",
        "repo-meta-other"
    )

    $couplingProof = Test-CodexNativeCouplingProof $repositoryRoot
    $toolingCustodyReview = Get-ToolingCustodyReview `
        -RepositoryRoot $repositoryRoot `
        -InstallLocalCmeLane $installLocalCmeLane
    $coreRecordSplitCustodyReview = Get-CoreRecordSplitCustodyReview `
        -RepositoryRoot $repositoryRoot `
        -InstallLocalCmeLane $installLocalCmeLane

    $bundles = New-Object System.Collections.Generic.List[object]
    foreach ($bundleId in $bundleOrder) {
        $bundleRows = @($rows | Where-Object { $_.bundleId -eq $bundleId })
        if ($bundleRows.Count -eq 0) {
            continue
        }

        $lineDelta = ($bundleRows | Measure-Object -Property lineDelta -Sum).Sum
        if ($null -eq $lineDelta) {
            $lineDelta = 0
        }

        $dirtyPathCount = $bundleRows.Count
        $untrackedCount = @($bundleRows | Where-Object { $_.untracked }).Count
        $versionSteps = $dirtyPathCount + [int][Math]::Ceiling($lineDelta / $LinesPerVersionStep)
        $version = "${BaseVersion}.$versionSteps"
        $admissionPosture = Get-BundleAdmissionPosture `
            -BundleId $bundleId `
            -DirtyPathCount $dirtyPathCount `
            -UntrackedCount $untrackedCount `
            -LineDelta $lineDelta `
            -VersionSteps $versionSteps `
            -CouplingProofObserved $couplingProof.observed `
            -PluginCustodyReviewObserved $couplingProof.pluginCustodyReviewObserved `
            -ToolingCustodyReviewObserved $toolingCustodyReview.observed `
            -CoreRecordSplitReviewObserved $coreRecordSplitCustodyReview.observed
        $bundles.Add([pscustomobject]@{
            bundleId = $bundleId
            name = Get-BundleName $bundleId
            meaning = Get-BundleMeaning $bundleId
            baselineVersion = $BaseVersion
            dirtyVersion = $version
            versionStepsPastBaseline = $versionSteps
            dirtyPathCount = $dirtyPathCount
            untrackedPathCount = $untrackedCount
            additions = ($bundleRows | Measure-Object -Property additions -Sum).Sum
            deletions = ($bundleRows | Measure-Object -Property deletions -Sum).Sum
            lineDelta = $lineDelta
            admissionPosture = $admissionPosture
            paths = @($bundleRows | Sort-Object path)
        })
    }

    $totalLineDelta = ($rows | Measure-Object -Property lineDelta -Sum).Sum
    if ($null -eq $totalLineDelta) {
        $totalLineDelta = 0
    }

    $totalVersionSteps = ($bundles | Measure-Object -Property versionStepsPastBaseline -Sum).Sum
    if ($null -eq $totalVersionSteps) {
        $totalVersionSteps = 0
    }

    $decisionMap = @{}
    foreach ($bundle in $bundles) {
        $decision = [string] $bundle.admissionPosture.decision
        if (-not $decisionMap.ContainsKey($decision)) {
            $decisionMap[$decision] = New-Object System.Collections.Generic.List[string]
        }

        $decisionMap[$decision].Add([string] $bundle.bundleId)
    }

    $decisionGroups = foreach ($decision in ($decisionMap.Keys | Sort-Object)) {
        [pscustomobject]@{
            decision = $decision
            bundleCount = $decisionMap[$decision].Count
            bundles = @($decisionMap[$decision])
        }
    }

    $highestPressureBundles = @(
        $bundles |
            Sort-Object -Property versionStepsPastBaseline -Descending |
            Select-Object -First 3 |
            ForEach-Object {
                [pscustomobject]@{
                    bundleId = $_.bundleId
                    dirtyVersion = $_.dirtyVersion
                    versionStepsPastBaseline = $_.versionStepsPastBaseline
                    decision = $_.admissionPosture.decision
                    reviewLane = $_.admissionPosture.reviewLane
                }
            }
    )

    $admissionReadiness = [pscustomobject]@{
        reviewReadyBundleCount = @($bundles | Where-Object { $_.admissionPosture.readyForAdmissionReview }).Count
        heldBundleCount = @($bundles | Where-Object { -not $_.admissionPosture.readyForAdmissionReview }).Count
        highestPressureBundles = $highestPressureBundles
        decisionGroups = @($decisionGroups)
        nextAction = if ($coreRecordSplitCustodyReview.observed -and $toolingCustodyReview.observed -and $couplingProof.pluginCustodyReviewObserved) {
            "decide core organ split, tooling, and codex-plugin-mcp source custody, then continue mechanical core-runtime decomposition"
        } elseif ($toolingCustodyReview.observed -and $couplingProof.pluginCustodyReviewObserved) {
            "decide tooling and codex-plugin-mcp source custody, then continue core-runtime decomposition"
        } elseif ($couplingProof.pluginCustodyReviewObserved) {
            "resolve remaining custody-before-review bundles, decide codex-plugin-mcp source custody, then continue core-runtime decomposition"
        } elseif ($couplingProof.observed) {
            "resolve custody-before-review bundles, then decompose core-runtime and review the observed MCP coupling proof"
        } else {
            "resolve custody-before-review bundles, then decompose core-runtime and prove MCP coupling"
        }
        denials = @(
            "readiness summary is not admission",
            "readiness summary is not release",
            "readiness summary is not authority",
            "readiness summary is not Actual activation"
        )
    }

    $untrackedCustodyQueue = @(
        $rows |
            Where-Object { $_.untracked } |
            Sort-Object bundleId, path |
            ForEach-Object {
                [pscustomobject]@{
                    path = $_.path
                    bundleId = $_.bundleId
                    lineDelta = $_.lineDelta
                    recommendedCustody = Get-UntrackedCustodyRecommendation $_
                    sliInterlace = $_.sliInterlace
                    admissionReady = $false
                    requiresOperatorReview = $true
                    denial = "untracked path is not admitted by existence"
                }
            }
    )

    $custodyMap = @{}
    foreach ($item in $untrackedCustodyQueue) {
        $custody = [string] $item.recommendedCustody
        if (-not $custodyMap.ContainsKey($custody)) {
            $custodyMap[$custody] = New-Object System.Collections.Generic.List[string]
        }

        $custodyMap[$custody].Add([string] $item.path)
    }

    $sliInterlaceGroups = @(
        $untrackedCustodyQueue |
            Group-Object -Property { $_.sliInterlace.lispSurface } |
            Sort-Object Name |
            ForEach-Object {
            $items = @($_.Group)
            [pscustomobject]@{
                lispSurface = $_.Name
                pathCount = $items.Count
                membranes = @($items | ForEach-Object { $_.sliInterlace.membrane } | Sort-Object -Unique)
                paths = @($items | ForEach-Object { $_.path })
                denials = @(
                    "SLI interlace is not Lisp evaluation",
                    "SLI interlace is not runtime authority",
                    "SLI interlace is not GEL admission",
                    "SLI interlace is not Actual activation"
                )
            }
        }
    )

    $untrackedCustodySummary = [pscustomobject]@{
        totalUntrackedPathCount = $untrackedCustodyQueue.Count
        admissionCandidateCount = @($untrackedCustodyQueue | Where-Object { $_.recommendedCustody -like "*-admission-candidate-after-review" }).Count
        holdLabResidueCount = @($untrackedCustodyQueue | Where-Object { $_.recommendedCustody -eq "hold-lab-residue" }).Count
        manualReviewCount = @($untrackedCustodyQueue | Where-Object { $_.recommendedCustody -eq "manual-custody-review-required" }).Count
        sliInterlaceGroupCount = $sliInterlaceGroups.Count
        groups = @(
            foreach ($custody in ($custodyMap.Keys | Sort-Object)) {
                [pscustomobject]@{
                    recommendedCustody = $custody
                    pathCount = $custodyMap[$custody].Count
                    paths = @($custodyMap[$custody])
                }
            }
        )
        sliInterlaceGroups = $sliInterlaceGroups
        denials = @(
            "custody recommendation is not admission",
            "custody recommendation is not staging",
            "custody recommendation is not deletion approval",
            "SLI interlace is not Lisp evaluation"
        )
    }

    $posture = [pscustomobject]@{
        schema = "project-sanctuary.repo.bundle-version-posture.v1"
        generatedAtUtc = (Get-Date).ToUniversalTime().ToString("O")
        repositoryRoot = $repositoryRoot
        branch = $branch
        headCommit = $headCommit
        baselineVersion = $BaseVersion
        bodyDirtyVersion = "${BaseVersion}.$totalVersionSteps"
        versionStepLaw = "dirty paths plus ceiling(line delta / LinesPerVersionStep)"
        linesPerVersionStep = $LinesPerVersionStep
        trackedDirtyPathCount = @($rows | Where-Object { -not $_.untracked }).Count
        untrackedPathCount = @($rows | Where-Object { $_.untracked }).Count
        totalDirtyPathCount = $rows.Count
        totalLineDelta = $totalLineDelta
        bundleCount = $bundles.Count
        installLocalCmeLane = $installLocalCmeLane
        couplingProof = $couplingProof
        toolingCustodyReview = $toolingCustodyReview
        coreRecordSplitCustodyReview = $coreRecordSplitCustodyReview
        admissionReadiness = $admissionReadiness
        untrackedCustodyQueue = $untrackedCustodyQueue
        untrackedCustodySummary = $untrackedCustodySummary
        bundles = $bundles
        denials = [pscustomobject]@{
            dirtyVersionIsReleaseVersion = $false
            dirtyVersionIsGitTag = $false
            dirtyVersionAdmitsGel = $false
            dirtyVersionActivatesActual = $false
            dirtyVersionGrantsAuthority = $false
        }
    }

    $markdown = New-Object System.Text.StringBuilder
    $null = $markdown.AppendLine("# Sanctuary Bundle Version Posture")
    $null = $markdown.AppendLine()
    $null = $markdown.AppendLine("Generated: $($posture.generatedAtUtc)")
    $null = $markdown.AppendLine()
    $null = $markdown.AppendLine("Baseline repo version: $BaseVersion")
    $null = $markdown.AppendLine("HEAD: $headCommit on $branch")
    $null = $markdown.AppendLine("Dirty body counter: $($posture.bodyDirtyVersion)")
    $null = $markdown.AppendLine()
    $null = $markdown.AppendLine("The dirty body counter is not a release version, tag, GEL admission, authority grant, or .Actual activation.")
    $null = $markdown.AppendLine()
    $null = $markdown.AppendLine("## Install-Local CME Lane")
    $null = $markdown.AppendLine()
    $null = $markdown.AppendLine("Source: $($installLocalCmeLane.sourcePath)")
    $null = $markdown.AppendLine("Scope: $($installLocalCmeLane.scope)")
    $null = $markdown.AppendLine("Active: $($installLocalCmeLane.active)")
    $null = $markdown.AppendLine("Matches request: $($installLocalCmeLane.matchesRequest)")
    $null = $markdown.AppendLine("Lab actor: $($installLocalCmeLane.labActorCmeId) / $($installLocalCmeLane.labActorActualLabel)")
    $null = $markdown.AppendLine("Telemetry subject: $($installLocalCmeLane.telemetrySubjectCmeId) / $($installLocalCmeLane.telemetrySubjectActualLabel)")
    $null = $markdown.AppendLine("Service identity: $($installLocalCmeLane.serviceIdentityId)")
    $null = $markdown.AppendLine("Identity template: $($installLocalCmeLane.identityTemplateId)")
    $null = $markdown.AppendLine("Residue capture: $($installLocalCmeLane.residueCapturePolicy)")
    $null = $markdown.AppendLine()
    $null = $markdown.AppendLine("This lane declaration is install-local posture, not preinstall doctrine, GEL admission, SelfGEL mutation, authority, provider binding, model binding, or .Actual activation.")
    $null = $markdown.AppendLine()
    $null = $markdown.AppendLine("## Tooling Custody Review")
    $null = $markdown.AppendLine()
    $null = $markdown.AppendLine("Status: $($toolingCustodyReview.status)")
    $null = $markdown.AppendLine("Observed: $($toolingCustodyReview.observed)")
    $null = $markdown.AppendLine("Surface count: $($toolingCustodyReview.surfaceCount)")
    $null = $markdown.AppendLine("All digests present: $($toolingCustodyReview.allSurfaceDigestsPresent)")
    $null = $markdown.AppendLine("All gates closed: $($toolingCustodyReview.allGatesClosed)")
    $null = $markdown.AppendLine("Admission decision: $($toolingCustodyReview.admissionDecision)")
    $null = $markdown.AppendLine()
    $null = $markdown.AppendLine("This tooling custody review is candidate-only. It does not stage source, admit source, grant authority, bind a provider/model, admit GEL/SelfGEL, or activate .Actual.")
    $null = $markdown.AppendLine()
    $null = $markdown.AppendLine("| Surface | Kind | Path | Digest Present | Source Admitted |")
    $null = $markdown.AppendLine("| --- | --- | --- | ---: | ---: |")
    foreach ($surface in $toolingCustodyReview.surfaces) {
        $digestPresent = -not [string]::IsNullOrWhiteSpace($surface.digest)
        $null = $markdown.AppendLine("| $($surface.surfaceId) | $($surface.surfaceKind) | $($surface.path) | $digestPresent | $($surface.sourceAdmitted) |")
    }
    $null = $markdown.AppendLine()
    $null = $markdown.AppendLine("## Core Organ Split Custody Review")
    $null = $markdown.AppendLine()
    $null = $markdown.AppendLine("Status: $($coreRecordSplitCustodyReview.status)")
    $null = $markdown.AppendLine("Observed: $($coreRecordSplitCustodyReview.observed)")
    $null = $markdown.AppendLine("Surface count: $($coreRecordSplitCustodyReview.surfaceCount)")
    $null = $markdown.AppendLine("Record declarations: $($coreRecordSplitCustodyReview.recordDeclarationCount)")
    $null = $markdown.AppendLine("Record surface lines: $($coreRecordSplitCustodyReview.recordSurfaceLineCount)")
    $null = $markdown.AppendLine("All digests present: $($coreRecordSplitCustodyReview.allSurfaceDigestsPresent)")
    $null = $markdown.AppendLine("All gates closed: $($coreRecordSplitCustodyReview.allGatesClosed)")
    $null = $markdown.AppendLine("Admission decision: $($coreRecordSplitCustodyReview.admissionDecision)")
    $null = $markdown.AppendLine()
    $null = $markdown.AppendLine("This core organ split custody review is candidate-only. It does not stage source, admit source, grant authority, bind a provider/model, admit GEL/SelfGEL, or activate .Actual.")
    $null = $markdown.AppendLine()
    $null = $markdown.AppendLine("| Surface | Kind | Path | Digest Present | Records | Source Admitted |")
    $null = $markdown.AppendLine("| --- | --- | --- | ---: | ---: | ---: |")
    foreach ($surface in $coreRecordSplitCustodyReview.surfaces) {
        $digestPresent = -not [string]::IsNullOrWhiteSpace($surface.digest)
        $null = $markdown.AppendLine("| $($surface.surfaceId) | $($surface.surfaceKind) | $($surface.path) | $digestPresent | $($surface.recordDeclarationCount) | $($surface.sourceAdmitted) |")
    }
    $null = $markdown.AppendLine()
    $null = $markdown.AppendLine("| Bundle | Dirty Version | Steps | Paths | Untracked | +/- Lines | Meaning |")
    $null = $markdown.AppendLine("| --- | ---: | ---: | ---: | ---: | ---: | --- |")
    foreach ($bundle in $bundles) {
        $null = $markdown.AppendLine("| $($bundle.name) | $($bundle.dirtyVersion) | $($bundle.versionStepsPastBaseline) | $($bundle.dirtyPathCount) | $($bundle.untrackedPathCount) | $($bundle.lineDelta) | $($bundle.meaning) |")
    }

    $null = $markdown.AppendLine()
    $null = $markdown.AppendLine("## Readiness Summary")
    $null = $markdown.AppendLine()
    $null = $markdown.AppendLine("Review-ready bundle count: $($admissionReadiness.reviewReadyBundleCount)")
    $null = $markdown.AppendLine("Held bundle count: $($admissionReadiness.heldBundleCount)")
    $null = $markdown.AppendLine("Next action: $($admissionReadiness.nextAction)")
    $null = $markdown.AppendLine()
    $null = $markdown.AppendLine("| Decision | Bundle Count | Bundles |")
    $null = $markdown.AppendLine("| --- | ---: | --- |")
    foreach ($group in $admissionReadiness.decisionGroups) {
        $null = $markdown.AppendLine("| $($group.decision) | $($group.bundleCount) | $($group.bundles -join ', ') |")
    }

    if ($untrackedCustodyQueue.Count -gt 0) {
        $null = $markdown.AppendLine()
        $null = $markdown.AppendLine("## Untracked Custody Queue")
        $null = $markdown.AppendLine()
        $null = $markdown.AppendLine("Total untracked paths: $($untrackedCustodySummary.totalUntrackedPathCount)")
        $null = $markdown.AppendLine("Admission candidates after review: $($untrackedCustodySummary.admissionCandidateCount)")
        $null = $markdown.AppendLine("Held as lab residue: $($untrackedCustodySummary.holdLabResidueCount)")
        $null = $markdown.AppendLine("Manual review required: $($untrackedCustodySummary.manualReviewCount)")
        $null = $markdown.AppendLine()
        $null = $markdown.AppendLine("| Recommended Custody | Path Count |")
        $null = $markdown.AppendLine("| --- | ---: |")
        foreach ($group in $untrackedCustodySummary.groups) {
            $null = $markdown.AppendLine("| $($group.recommendedCustody) | $($group.pathCount) |")
        }

        $null = $markdown.AppendLine()
        $null = $markdown.AppendLine("### SLI Interlace Groups")
        $null = $markdown.AppendLine()
        $null = $markdown.AppendLine("| Lisp Surface | Path Count | Membranes |")
        $null = $markdown.AppendLine("| --- | ---: | --- |")
        foreach ($group in $untrackedCustodySummary.sliInterlaceGroups) {
            $null = $markdown.AppendLine("| $($group.lispSurface) | $($group.pathCount) | $($group.membranes -join ', ') |")
        }

        $null = $markdown.AppendLine()
        $null = $markdown.AppendLine("| Bundle | Path | Recommended Custody | SLI Surface | Membrane | Lines |")
        $null = $markdown.AppendLine("| --- | --- | --- | --- | --- | ---: |")
        foreach ($item in $untrackedCustodyQueue) {
            $null = $markdown.AppendLine("| $($item.bundleId) | $($item.path) | $($item.recommendedCustody) | $($item.sliInterlace.lispSurface) | $($item.sliInterlace.membrane) | $($item.lineDelta) |")
        }
    }

    $custodyMarkdown = New-Object System.Text.StringBuilder
    $null = $custodyMarkdown.AppendLine("# Sanctuary Untracked Custody Review")
    $null = $custodyMarkdown.AppendLine()
    $null = $custodyMarkdown.AppendLine("Generated: $($posture.generatedAtUtc)")
    $null = $custodyMarkdown.AppendLine("Repository: $repositoryRoot")
    $null = $custodyMarkdown.AppendLine("HEAD: $headCommit on $branch")
    $null = $custodyMarkdown.AppendLine()
    $null = $custodyMarkdown.AppendLine("This review is not admission, staging, deletion approval, GEL admission, SelfGEL mutation, authority grant, or Actual activation.")
    $null = $custodyMarkdown.AppendLine()
    $null = $custodyMarkdown.AppendLine("## Install-Local CME Lane")
    $null = $custodyMarkdown.AppendLine()
    $null = $custodyMarkdown.AppendLine("- Source: $($installLocalCmeLane.sourcePath)")
    $null = $custodyMarkdown.AppendLine("- Lab actor: $($installLocalCmeLane.labActorCmeId) / $($installLocalCmeLane.labActorActualLabel)")
    $null = $custodyMarkdown.AppendLine("- Telemetry subject: $($installLocalCmeLane.telemetrySubjectCmeId) / $($installLocalCmeLane.telemetrySubjectActualLabel)")
    $null = $custodyMarkdown.AppendLine("- Service identity: $($installLocalCmeLane.serviceIdentityId)")
$null = $custodyMarkdown.AppendLine("- Identity template: $($installLocalCmeLane.identityTemplateId)")
$null = $custodyMarkdown.AppendLine("- Residue capture: $($installLocalCmeLane.residueCapturePolicy)")
$null = $custodyMarkdown.AppendLine()
$null = $custodyMarkdown.AppendLine("The lane declaration marks this local review body without admitting, activating, granting authority, or binding a provider/model.")
$null = $custodyMarkdown.AppendLine()
$null = $custodyMarkdown.AppendLine("## Tooling Custody Review")
$null = $custodyMarkdown.AppendLine()
$null = $custodyMarkdown.AppendLine("- Status: $($toolingCustodyReview.status)")
$null = $custodyMarkdown.AppendLine("- Observed: $($toolingCustodyReview.observed)")
$null = $custodyMarkdown.AppendLine("- Surface count: $($toolingCustodyReview.surfaceCount)")
$null = $custodyMarkdown.AppendLine("- All digests present: $($toolingCustodyReview.allSurfaceDigestsPresent)")
$null = $custodyMarkdown.AppendLine("- All gates closed: $($toolingCustodyReview.allGatesClosed)")
$null = $custodyMarkdown.AppendLine("- Admission decision: $($toolingCustodyReview.admissionDecision)")
$null = $custodyMarkdown.AppendLine()
foreach ($surface in $toolingCustodyReview.surfaces) {
    $null = $custodyMarkdown.AppendLine("- [ ] $($surface.path)")
    $null = $custodyMarkdown.AppendLine("  - surface: $($surface.surfaceId)")
    $null = $custodyMarkdown.AppendLine("  - kind: $($surface.surfaceKind)")
    $null = $custodyMarkdown.AppendLine("  - role: $($surface.role)")
    $null = $custodyMarkdown.AppendLine("  - closure: $($surface.closureRequirement)")
    $null = $custodyMarkdown.AppendLine("  - source admitted: $($surface.sourceAdmitted)")
}
$null = $custodyMarkdown.AppendLine()
$null = $custodyMarkdown.AppendLine("## Core Record Split Custody Review")
$null = $custodyMarkdown.AppendLine()
$null = $custodyMarkdown.AppendLine("- Status: $($coreRecordSplitCustodyReview.status)")
$null = $custodyMarkdown.AppendLine("- Observed: $($coreRecordSplitCustodyReview.observed)")
$null = $custodyMarkdown.AppendLine("- Surface count: $($coreRecordSplitCustodyReview.surfaceCount)")
$null = $custodyMarkdown.AppendLine("- Record declarations: $($coreRecordSplitCustodyReview.recordDeclarationCount)")
$null = $custodyMarkdown.AppendLine("- Record surface lines: $($coreRecordSplitCustodyReview.recordSurfaceLineCount)")
$null = $custodyMarkdown.AppendLine("- All digests present: $($coreRecordSplitCustodyReview.allSurfaceDigestsPresent)")
$null = $custodyMarkdown.AppendLine("- All gates closed: $($coreRecordSplitCustodyReview.allGatesClosed)")
$null = $custodyMarkdown.AppendLine("- Admission decision: $($coreRecordSplitCustodyReview.admissionDecision)")
$null = $custodyMarkdown.AppendLine()
foreach ($surface in $coreRecordSplitCustodyReview.surfaces) {
    $null = $custodyMarkdown.AppendLine("- [ ] $($surface.path)")
    $null = $custodyMarkdown.AppendLine("  - surface: $($surface.surfaceId)")
    $null = $custodyMarkdown.AppendLine("  - kind: $($surface.surfaceKind)")
    $null = $custodyMarkdown.AppendLine("  - role: $($surface.role)")
    $null = $custodyMarkdown.AppendLine("  - closure: $($surface.closureRequirement)")
    $null = $custodyMarkdown.AppendLine("  - record declarations: $($surface.recordDeclarationCount)")
    $null = $custodyMarkdown.AppendLine("  - source admitted: $($surface.sourceAdmitted)")
}
$null = $custodyMarkdown.AppendLine()
$null = $custodyMarkdown.AppendLine("## Summary")
    $null = $custodyMarkdown.AppendLine()
    $null = $custodyMarkdown.AppendLine("- Total untracked paths: $($untrackedCustodySummary.totalUntrackedPathCount)")
    $null = $custodyMarkdown.AppendLine("- Admission candidates after review: $($untrackedCustodySummary.admissionCandidateCount)")
    $null = $custodyMarkdown.AppendLine("- Held as lab residue: $($untrackedCustodySummary.holdLabResidueCount)")
    $null = $custodyMarkdown.AppendLine("- Manual review required: $($untrackedCustodySummary.manualReviewCount)")
    $null = $custodyMarkdown.AppendLine()
    $null = $custodyMarkdown.AppendLine("## Group Review")
    foreach ($group in $untrackedCustodySummary.groups) {
        $null = $custodyMarkdown.AppendLine()
        $null = $custodyMarkdown.AppendLine("### $($group.recommendedCustody)")
        foreach ($path in $group.paths) {
            $null = $custodyMarkdown.AppendLine("- [ ] $path")
        }
    }

    if ($untrackedCustodySummary.sliInterlaceGroups.Count -gt 0) {
        $null = $custodyMarkdown.AppendLine()
        $null = $custodyMarkdown.AppendLine("## SLI Interlace Review")
        $null = $custodyMarkdown.AppendLine()
        $null = $custodyMarkdown.AppendLine("These paths are linked to Lisp development surfaces as quoted or executable-adjacent forms. The linkage is not evaluation, admission, authority, or Actual activation.")
        foreach ($group in $untrackedCustodySummary.sliInterlaceGroups) {
            $null = $custodyMarkdown.AppendLine()
            $null = $custodyMarkdown.AppendLine("### $($group.lispSurface)")
            $null = $custodyMarkdown.AppendLine()
            $null = $custodyMarkdown.AppendLine("Membranes: $($group.membranes -join ', ')")
            foreach ($path in $group.paths) {
                $item = $untrackedCustodyQueue | Where-Object { $_.path -eq $path } | Select-Object -First 1
                $null = $custodyMarkdown.AppendLine("- [ ] $path")
                $null = $custodyMarkdown.AppendLine("  - role: $($item.sliInterlace.interlaceRole)")
                $null = $custodyMarkdown.AppendLine("  - closure: $($item.sliInterlace.closureRequirement)")
            }
        }
    }

    $null = $custodyMarkdown.AppendLine()
    $null = $custodyMarkdown.AppendLine("## Review Choices")
    $null = $custodyMarkdown.AppendLine()
    $null = $custodyMarkdown.AppendLine("For each path or group, choose exactly one:")
    $null = $custodyMarkdown.AppendLine()
    $null = $custodyMarkdown.AppendLine("- stage for admission review")
    $null = $custodyMarkdown.AppendLine("- hold as lab residue")
    $null = $custodyMarkdown.AppendLine("- add ignore rule")
    $null = $custodyMarkdown.AppendLine("- delete only by explicit operator request")
    $null = $custodyMarkdown.AppendLine("- defer")
    $null = $custodyMarkdown.AppendLine()
    $null = $custodyMarkdown.AppendLine("## Required Proof After Any Choice")
    $null = $custodyMarkdown.AppendLine()
    $null = $custodyMarkdown.AppendLine("- rerun bundle version posture")
    $null = $custodyMarkdown.AppendLine("- run dotnet test ProjectSanctuary.sln")
    $null = $custodyMarkdown.AppendLine("- emit a closed-gate or lab-observation receipt")

    $null = $markdown.AppendLine()
    $null = $markdown.AppendLine("## Admission Readiness")
    $null = $markdown.AppendLine()
    $null = $markdown.AppendLine("| Bundle | Decision | Review Ready | Review Lane | Next Action |")
    $null = $markdown.AppendLine("| --- | --- | ---: | --- | --- |")
    foreach ($bundle in $bundles) {
        $ready = if ($bundle.admissionPosture.readyForAdmissionReview) { "yes" } else { "no" }
        $null = $markdown.AppendLine("| $($bundle.name) | $($bundle.admissionPosture.decision) | $ready | $($bundle.admissionPosture.reviewLane) | $($bundle.admissionPosture.nextReviewAction) |")
    }

    $null = $markdown.AppendLine()
    $null = $markdown.AppendLine("## Paths")
    foreach ($bundle in $bundles) {
        $null = $markdown.AppendLine()
        $null = $markdown.AppendLine("### $($bundle.name)")
        foreach ($row in $bundle.paths) {
            $null = $markdown.AppendLine("- $($row.status.Trim()) $($row.path) (+$($row.additions) / -$($row.deletions))")
        }
    }

    if (-not $NoWrite) {
        New-Item -ItemType Directory -Force -Path $OutputRoot | Out-Null
        $jsonPath = Join-Path $OutputRoot "bundle-version-posture.json"
        $markdownPath = Join-Path $OutputRoot "bundle-version-posture.md"
        $custodyMarkdownPath = Join-Path $OutputRoot "untracked-custody-review.md"
        $posture | Add-Member -NotePropertyName outputJsonPath -NotePropertyValue $jsonPath
        $posture | Add-Member -NotePropertyName outputMarkdownPath -NotePropertyValue $markdownPath
        $posture | Add-Member -NotePropertyName outputCustodyReviewMarkdownPath -NotePropertyValue $custodyMarkdownPath
        $posture | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $jsonPath -Encoding UTF8
        $markdown.ToString() | Set-Content -LiteralPath $markdownPath -Encoding UTF8
        $custodyMarkdown.ToString() | Set-Content -LiteralPath $custodyMarkdownPath -Encoding UTF8
    }

    if ($Json) {
        $posture | ConvertTo-Json -Depth 12
    }
    else {
        $markdown.ToString()
        if (-not $NoWrite) {
            Write-Host ""
            Write-Host "JSON: $jsonPath"
            Write-Host "Markdown: $markdownPath"
            Write-Host "Custody review: $custodyMarkdownPath"
        }
    }
}
finally {
    Pop-Location
}
