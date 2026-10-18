using System.Linq;
using Content.Client._Triad.ShipPreview.UI;
using Content.Shared._NF.Shipyard.Prototypes;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Client._Triad.ShipPreview;

/// <summary>
/// Opens the standalone ship preview window for a vessel prototype. Client-side only, no permissions needed.
/// </summary>
public sealed partial class ShipPreviewCommand : LocalizedCommands
{
    [Dependency] private IPrototypeManager _prototypeManager = default!;

    public override string Command => "triad_shippreview";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number-need-specific", ("properAmount", 1), ("currentAmount", args.Length)));
            return;
        }

        if (!_prototypeManager.TryIndex<VesselPrototype>(args[0], out var vessel))
        {
            shell.WriteError(Loc.GetString("cmd-triad_shippreview-unknown", ("id", args[0])));
            return;
        }

        ShipPreviewWindow.Open(vessel);
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length != 1)
            return CompletionResult.Empty;

        var vessels = _prototypeManager.EnumeratePrototypes<VesselPrototype>()
            .Where(vessel => !vessel.Abstract)
            .OrderBy(vessel => vessel.ID)
            .Select(vessel => new CompletionOption(vessel.ID, vessel.Name));

        return CompletionResult.FromHintOptions(vessels, Loc.GetString("cmd-triad_shippreview-hint-vessel"));
    }
}
