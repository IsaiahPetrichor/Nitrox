using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using NitroxServer.GameLogic.Bases;
using NitroxServer.GameLogic.Entities;
using NitroxServer.GameLogic.Players;

namespace NitroxServer.Serialization.World
{
    [DataContract]
    public class PersistedWorldData
    {
        [DataMember(Order = 1)]
        public WorldData WorldData { get; set; } = new WorldData();

        [DataMember(Order = 2)]
        public PlayerData PlayerData
        {
            get; set;
        }

        [DataMember(Order = 3)]
        public GlobalRootData GlobalRootData
        {
            get; set;
        }

        [DataMember(Order = 4)]
        public EntityData EntityData
        {
            get; set;
        }

        public static PersistedWorldData From(World world)
        {
            PersistedWorldData persistedWorldData = new PersistedWorldData
            {
                WorldData =
                {
                    ParsedBatchCells = world.BatchEntitySpawner.SerializableParsedBatches,
                    GameData = GameData.From(world.GameData.PDAState, world.GameData.StoryGoals, world.ScheduleKeeper, world.StoryManager, world.TimeKeeper),
                    Seed = world.Seed
                },
                PlayerData = PlayerData.From(world.PlayerManager.GetAllPlayers()),
                GlobalRootData = GlobalRootData.From(world.WorldEntityManager.GetGlobalRootEntities(true)),
                EntityData = EntityData.From(world.EntityRegistry.GetAllEntities(true))
            };

            List<Player> players = [.. persistedWorldData.PlayerData.GetPlayers()];

            players.ForEach(player =>
            {
                Log.Info($"{player.Name}'s Equipment: {JsonConvert.SerializeObject(player.GetEquipment())}, Modules: {JsonConvert.SerializeObject(player.GetModules())}");
            });

            return persistedWorldData;
        }

        public bool IsValid()
        {
            return WorldData.IsValid() &&
                   PlayerData != null &&
                   GlobalRootData != null &&
                   EntityData != null;
        }
    }
}
