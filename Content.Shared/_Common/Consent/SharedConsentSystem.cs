// SPDX-FileCopyrightText: Copyright (c) 2024-2025 Space Wizards Federation
// SPDX-License-Identifier: MIT

using Content.Shared.Examine;
using Content.Shared.Mind;
using Content.Shared.Verbs;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._Common.Consent;

public abstract partial class SharedConsentSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    [Dependency] private readonly ExamineSystemShared _examineSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ConsentComponent, GetVerbsEvent<ExamineVerb>>(OnGetExamineVerbs);
    }

    public bool HasConsent(Entity<ConsentComponent?> ent, ProtoId<ConsentTogglePrototype> consentId)
    {
        if (!Resolve(ent, ref ent.Comp))
        {
            // Entities that have never been controlled by a player consent to all mechanics.
            return true;
        }

        return ent.Comp.ConsentSettings.Toggles.TryGetValue(consentId, out var val) && val == "on";
    }

    private void OnGetExamineVerbs(Entity<ConsentComponent> ent, ref GetVerbsEvent<ExamineVerb> args)
    {
        var user = args.User;

        args.Verbs.Add(new()
        {
            Text = Loc.GetString("consent-examine-verb"),
            Icon = new SpriteSpecifier.Texture(new ("/Textures/Interface/VerbIcons/settings.svg.192dpi.png")),
            Act = () =>
            {
                var message = GetConsentText(ent.Comp.ConsentSettings);
                _examineSystem.SendExamineTooltip(user, ent, message, getVerbs: false, centerAtCursor: false);
            },
            Category = VerbCategory.Examine,
            CloseMenu = true,
        });
    }

    private FormattedMessage GetConsentText(PlayerConsentSettings consentSettings)
    {
        var text = consentSettings.Freetext;
        if (text == string.Empty)
        {
            text = Loc.GetString("consent-examine-not-set");
        }

        var message = new FormattedMessage();
        message.AddText(text);
        return message;
    }

}
