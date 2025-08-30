using Content.IntegrationTests;
using Content.Server._NF.Shipyard.Systems;
using Content.Server.Maps;
using Content.Shared.Shuttles.Save;
using Content.Shared.VendingMachines;
using NUnit.Framework;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.Utility;
using System.Threading.Tasks;
using System.Linq;

namespace Content.IntegrationTests.Tests._NF.Shipyard
{
    [TestFixture]
    public sealed class ShipyardGridSaveTest
    {
        [Test]
        public async Task TestAmbitionShipSave()
        {
            await using var pair = await PoolManager.GetServerClient();
            var server = pair.Server;

            var entityManager = server.ResolveDependency<IEntityManager>();
            var mapManager = server.ResolveDependency<IMapManager>();
            var mapLoader = server.ResolveDependency<IEntitySystemManager>().GetEntitySystem<MapLoaderSystem>();
            var shipyardGridSaveSystem = server.ResolveDependency<IEntitySystemManager>().GetEntitySystem<ShipyardGridSaveSystem>();

            await server.WaitPost(() =>
            {
                // Create a test map
                var mapId = mapManager.CreateMap();
                var mapUid = mapManager.GetMapEntityId(mapId);

                // Load the ambition ship
                var mapLoaded = mapLoader.TryLoadGrid(mapId, new ResPath("/Maps/_NF/Shuttles/Expedition/ambition.yml"), out var gridUid);

                Assert.That(mapLoaded, Is.True, "Should successfully load the ambition ship");
                Assert.That(gridUid, Is.Not.Null, "Should get a valid grid UID");

                // Test that the grid can be cleaned for saving without errors
                if (gridUid != null)
                    shipyardGridSaveSystem.CleanGridForSaving(gridUid.Value);

                // Check that vending machines have been deleted
                var vendingMachineQuery = entityManager.EntityQueryEnumerator<VendingMachineComponent>();
                var foundVendingMachine = false;

                while (vendingMachineQuery.MoveNext(out var vendingUid, out var vendingComp))
                {
                    var transform = entityManager.GetComponent<TransformComponent>(vendingUid);
                    if (gridUid != null && transform.GridUid == gridUid.Value)
                    {
                        foundVendingMachine = true;
                        break;
                    }
                }

                Assert.That(foundVendingMachine, Is.False, "No vending machines should remain in cleaned grid");

                // Clean up
                mapManager.DeleteMap(mapId);
            });

            await pair.CleanReturnAsync();
        }
    }
}
