namespace Sanctuary.Core;

public sealed record SanctuaryGates
{
    public bool DataAdmitted { get; init; }
    public bool CarrierAdmitted { get; init; }
    public bool GelAdmitted { get; init; }
    public bool MemoryAdmitted { get; init; }
    public bool SelfGelMutated { get; init; }
    public bool ContinuityAdmitted { get; init; }
    public bool AuthorityGranted { get; init; }
    public bool ActionAuthorized { get; init; }
    public bool RuntimeActionAllowed { get; init; }
    public bool ExternalActionAuthorized { get; init; }
    public bool ModelBound { get; init; }
    public bool ProviderCalled { get; init; }
    public bool CmeActualActivated { get; init; }
    public bool SanctuaryActualActivated { get; init; }
    public bool PersonhoodClaimed { get; init; }
    public bool SovereigntyClaimed { get; init; }

    public static SanctuaryGates Closed { get; } = new();

    public bool AllClosed =>
        !DataAdmitted &&
        !CarrierAdmitted &&
        !GelAdmitted &&
        !MemoryAdmitted &&
        !SelfGelMutated &&
        !ContinuityAdmitted &&
        !AuthorityGranted &&
        !ActionAuthorized &&
        !RuntimeActionAllowed &&
        !ExternalActionAuthorized &&
        !ModelBound &&
        !ProviderCalled &&
        !CmeActualActivated &&
        !SanctuaryActualActivated &&
        !PersonhoodClaimed &&
        !SovereigntyClaimed;
}
