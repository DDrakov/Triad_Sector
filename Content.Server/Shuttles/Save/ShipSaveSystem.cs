using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Content.Shared.Shuttles.Save;
using Content.Shared._NF.Shipyard.Components;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Robust.Shared.Log;

namespace Content.Server.Shuttles.Save
{
    public sealed class ShipSaveSystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;
        [Dependency] private readonly IServerNetManager _netManager = default!;
        
        // Store pending admin requests
        private static readonly Dictionary<string, Action<string>> _pendingAdminRequests = new();
        // Store player ship data for admin commands
        private static readonly Dictionary<string, List<(string filename, string shipName, DateTime timestamp, string checksum)>> _playerShipCache = new();

        public override void Initialize()
        {
            base.Initialize();
            SubscribeNetworkEvent<RequestSaveShipServerMessage>(OnRequestSaveShipServer);
            SubscribeNetworkEvent<RequestLoadShipMessage>(OnRequestLoadShip);
            SubscribeNetworkEvent<RequestAvailableShipsMessage>(OnRequestAvailableShips);
            SubscribeNetworkEvent<AdminSendPlayerShipsMessage>(OnAdminSendPlayerShips);
            SubscribeNetworkEvent<AdminSendShipDataMessage>(OnAdminSendShipData);
        }

        private void OnRequestSaveShipServer(RequestSaveShipServerMessage msg, EntitySessionEventArgs args)
        {
            var playerSession = args.SenderSession;
            if (playerSession == null)
                return;


            var deedUid = new EntityUid((int)msg.DeedUid);
            if (!_entityManager.TryGetComponent<ShuttleDeedComponent>(deedUid, out var deedComponent))
            {
                Logger.Warning($"Player {playerSession.Name} tried to save ship with invalid deed UID: {msg.DeedUid}");
                return;
            }

            if (deedComponent.ShuttleUid == null)
            {
                Logger.Warning($"Player {playerSession.Name} tried to save ship with deed {msg.DeedUid} but no valid shuttle UID found.");
                return;
            }

            if (!_entityManager.TryGetEntity(deedComponent.ShuttleUid.Value, out var shuttleUid))
            {
                Logger.Warning($"Player {playerSession.Name} tried to save ship with deed {msg.DeedUid} but failed to convert NetEntity to EntityUid.");
                return;
            }
            if (!_entityManager.TryGetComponent<MapGridComponent>(shuttleUid.Value, out var grid))
            {
                Logger.Warning($"Player {playerSession.Name} tried to save ship with deed {msg.DeedUid} but no valid shuttle UID found.");
                return;
            }

            // ShipSerializationSystem was removed - ship saving now handled by ShipyardGridSaveSystem
            var shipName = deedComponent.ShuttleName ?? "SavedShip_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // TODO: Integrate with ShipyardGridSaveSystem for ship saving functionality
            Logger.Warning($"Ship saving temporarily disabled - ShipSerializationSystem was removed");
            return;
        }
    public void RequestSaveShip(EntityUid deedUid, ICommonSession? playerSession)
        {
            if (playerSession == null)
            {
                Logger.Warning($"Attempted to save ship for deed {deedUid} without a valid player session.");
                return;
            }

            if (!_entityManager.TryGetComponent<ShuttleDeedComponent>(deedUid, out var deedComponent))
            {
                Logger.Warning($"Player {playerSession.Name} tried to save ship with invalid deed UID: {deedUid}");
                return;
            }

            if (deedComponent.ShuttleUid == null || !_entityManager.TryGetEntity(deedComponent.ShuttleUid.Value, out var shuttleUid) || !_entityManager.TryGetComponent<MapGridComponent>(shuttleUid.Value, out var grid))
            {
                Logger.Warning($"Player {playerSession.Name} tried to save ship with deed {deedUid} but no valid shuttle UID found.");
                return;
            }

            // ShipSerializationSystem was removed - ship saving now handled by ShipyardGridSaveSystem
            var shipName = deedComponent.ShuttleName ?? "SavedShip_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // TODO: Integrate with ShipyardGridSaveSystem for ship saving functionality
            Logger.Warning($"Ship saving temporarily disabled - ShipSerializationSystem was removed");
            return;
        }

        private void OnRequestLoadShip(RequestLoadShipMessage msg, EntitySessionEventArgs args)
        {
            var playerSession = args.SenderSession;
            if (playerSession == null)
                return;

            Logger.Info($"Player {playerSession.Name} requested to load ship from YAML data");
            
            // TODO: Implement ship loading from saved files
            // This would involve deserializing the ship data and spawning it in the game world
            // For now, we just log the request
        }

        private void OnRequestAvailableShips(RequestAvailableShipsMessage msg, EntitySessionEventArgs args)
        {
            var playerSession = args.SenderSession;
            if (playerSession == null)
                return;

            // Client handles available ships from local user data
            Logger.Info($"Player {playerSession.Name} requested available ships - client handles this locally");
        }

        private void OnAdminSendPlayerShips(AdminSendPlayerShipsMessage msg, EntitySessionEventArgs args)
        {
            var key = $"player_ships_{msg.AdminName}";
            if (_pendingAdminRequests.TryGetValue(key, out var callback))
            {
                // Cache the ship data for later blacklist commands
                _playerShipCache[key] = msg.Ships;
                
                var result = $"=== Ships for player ===\n\n";
                for (int i = 0; i < msg.Ships.Count; i++)
                {
                    var (filename, shipName, timestamp, checksum) = msg.Ships[i];
                    var serverBinding = Content.Server.Administration.Commands.ShipBlacklistService.ExtractServerBinding(checksum) ?? "Unknown";
                    var isBlacklisted = Content.Server.Administration.Commands.ShipBlacklistService.IsBlacklisted(checksum);
                    result += $"[{i + 1}] {shipName} ({filename})\n";
                    result += $"    Saved: {timestamp:yyyy-MM-dd HH:mm:ss}\n";
                    result += $"    Ship ID: {serverBinding}\n";
                    result += $"    Status: {(isBlacklisted ? "BLACKLISTED" : "OK")}\n";
                    result += "\n";
                }
                result += "Use: shipsave_blacklist [player] [ship_id] [reason]\n";
                result += "Use: shipsave_unblacklist [player] [ship_id]";
                callback(result);
                _pendingAdminRequests.Remove(key);
            }
        }
        
        public static string? FindPlayerShipByBinding(string adminName, string playerName, string shipId)
        {
            var key = $"player_ships_{adminName}";
            if (_playerShipCache.TryGetValue(key, out var ships))
            {
                var ship = ships.FirstOrDefault(s => 
                    Content.Server.Administration.Commands.ShipBlacklistService.ExtractServerBinding(s.checksum) == shipId);
                return ship.checksum;
            }
            return null;
        }

        private void OnAdminSendShipData(AdminSendShipDataMessage msg, EntitySessionEventArgs args)
        {
            var key = $"ship_data_{msg.AdminName}_{msg.ShipFilename}";
            if (_pendingAdminRequests.TryGetValue(key, out var callback))
            {
                callback(msg.ShipData);
                _pendingAdminRequests.Remove(key);
            }
        }

        public static void RegisterAdminRequest(string key, Action<string> callback)
        {
            _pendingAdminRequests[key] = callback;
        }

        public void SendAdminRequestPlayerShips(Guid playerId, string adminName, ICommonSession targetSession)
        {
            RaiseNetworkEvent(new AdminRequestPlayerShipsMessage(playerId, adminName), targetSession);
        }

        public void SendAdminRequestShipData(string filename, string adminName, ICommonSession targetSession)
        {
            RaiseNetworkEvent(new AdminRequestShipDataMessage(filename, adminName), targetSession);
        }
    }
}
