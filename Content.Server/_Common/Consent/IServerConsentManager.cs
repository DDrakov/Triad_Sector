// SPDX-FileCopyrightText: Copyright (c) 2024-2025 Space Wizards Federation
// SPDX-License-Identifier: MIT

using Content.Shared._Common.Consent;
using Robust.Shared.Network;
using Robust.Shared.Player;
using System.Threading;
using System.Threading.Tasks;

namespace Content.Server._Common.Consent;

public interface IServerConsentManager
{
    event Action<ICommonSession, PlayerConsentSettings>? OnConsentUpdated;

    void Initialize();

    Task LoadData(ICommonSession session, CancellationToken cancel);
    void OnClientDisconnected(ICommonSession session);

    /// <summary>
    /// Get a player's consent settings from their user id.
    /// </summary>
    PlayerConsentSettings GetPlayerConsentSettings(NetUserId userId);
}
