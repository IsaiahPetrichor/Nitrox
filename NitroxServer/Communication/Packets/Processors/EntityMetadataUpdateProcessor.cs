using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors
{
    public class EntityMetadataUpdateProcessor : AuthenticatedPacketProcessor<EntityMetadataUpdate>
    {
        private readonly PlayerManager playerManager;
        private readonly EntityRegistry entityRegistry;

        public EntityMetadataUpdateProcessor(PlayerManager playerManager, EntityRegistry entityRegistry)
        {
            this.playerManager = playerManager;
            this.entityRegistry = entityRegistry;
        }

        public override void Process(EntityMetadataUpdate packet, Player sendingPlayer)
        {
            Optional<Entity> entity = entityRegistry.GetEntityById(packet.Id);

            if (entity.HasValue)
            {
                if (entity.Value.Metadata.GetType() == typeof(PlayerMetadata))
                {
                    PlayerMetadata playerMetadata = (PlayerMetadata)packet.NewValue;

                    Log.Info($"Player {sendingPlayer.Name}'s equipped items: {JsonConvert.SerializeObject(playerMetadata.EquippedItems)}");
                    playerMetadata.EquippedItems.ForEach(equippedItem =>
                    {
                        EquippedItemData itemData = new(null, equippedItem.Id, null, equippedItem.Slot, equippedItem.TechType);

                        sendingPlayer.GetEquipment().ForEach(equippedItem =>
                        {
                            if (!equippedItem.Equals(itemData))
                            {
                                Log.Info($"{equippedItem.ToString()} does not equal {itemData.ToString()}");
                                sendingPlayer.AddEquipment((itemData));
                            }
                            else
                            {
                                Log.Info($"De-sync, player already has that equipped.");
                            }
                        });
                    });
                }

                entity.Value.Metadata = packet.NewValue;
                SendUpdateToVisiblePlayers(packet, sendingPlayer, entity.Value);
            }
            else
            {
                Log.Error($"Entity metadata updated on an entity unknown to the server {packet.Id} {packet.NewValue.GetType()} ");
            }
        }

        private void SendUpdateToVisiblePlayers(EntityMetadataUpdate packet, Player sendingPlayer, Entity entity)
        {
            foreach (Player player in playerManager.GetConnectedPlayers())
            {
                bool updateVisibleToPlayer = player.CanSee(entity);

                if (player != sendingPlayer && updateVisibleToPlayer)
                {
                    player.SendPacket(packet);
                }
            }
        }
    }
}
