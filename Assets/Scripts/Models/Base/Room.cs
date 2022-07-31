using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Model
{
    public class Room : Singleton<Room>
    {
        public bool IsSingle { get; set; } = true;
        // public Mode mode;
        public List<Player> players;

        public void InitPlayers(StartGameJson json)
        {
            players = new List<Player>();
            RandomGeneral = json.generals;
            Debug.Log(json.generals[0]);
            foreach (string username in json.players)
            {
                Player player = new Player();
                player.Username = username;
                players.Add(player);
            }
        }

        public List<int> RandomGeneral { get; set; }
    }
}