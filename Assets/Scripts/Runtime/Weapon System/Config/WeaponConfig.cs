using UnityEngine;

namespace Game
{
    public class WeaponConfig : ScriptableObject
    {
        public string id;
        public string displayName;
        public Sprite icon;
        public WeaponCategory category;

        [Header("Common")]
        public float equipTime = 0.3f;
    }
}
