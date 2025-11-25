using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Game
{
    public class RangedWeaponConfig : WeaponConfig
    {
        public FireMode fireMode = FireMode.Single;
        public RangedWeaponProgression progression;

        public bool useRaycast = true;
        public GameObject projectilePrefab;
        public float projectileSpeed = 30f;

#if UNITY_EDITOR
        [Button("Rename File To displayName_config")]
        public void RenameFile()
        {
            if (string.IsNullOrEmpty(displayName))
            {
                Debug.LogWarning("[RangedWeaponConfig] displayName is empty → không thể rename.");
                return;
            }

            string cleanName = displayName.Replace(" ", "");
            string newFileName = cleanName + "_Config";
            string path = AssetDatabase.GetAssetPath(this);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("[RangedWeaponConfig] Không tìm thấy asset path.");
                return;
            }

            AssetDatabase.RenameAsset(path, newFileName);
            AssetDatabase.SaveAssets();

            Debug.Log($"[RangedWeaponConfig] Đã rename file thành: {newFileName}");
        }
#endif
    }
}