using BaXoai;
using UnityEngine;

namespace Game
{
    public class Player : MonoSingleton<Player>
    {
        public PlayerControl pControl;
        public PlayerCamera pCamera;
        public Character character;
    }
}
