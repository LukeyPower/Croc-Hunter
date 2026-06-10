using UnityEngine;

namespace CrocHunter
{
    [CreateAssetMenu(menuName = "CrocHunter/World Scroll Settings")]
    public class WorldScrollSettings : ScriptableObject
    {
        [SerializeField] private float scrollSpeed = 6f;
        public float ScrollSpeed => scrollSpeed;
    }
}
