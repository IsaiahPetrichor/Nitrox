using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace NitroxServer.GameLogic.Players
{
    [DataContract]
    public class PlayerData
    {
        [DataMember(Order = 1)]
        public List<PersistedPlayerData> Players = new();

        public List<Player> GetPlayers()
        {
            return Players.ConvertAll(playerData => playerData.ToPlayer());
        }

        public static PlayerData From(IEnumerable<Player> players)
        {
            List<PersistedPlayerData> persistedPlayers = players.Select(PersistedPlayerData.FromPlayer).ToList();

            return new PlayerData { Players = persistedPlayers };
        }
    }
}
