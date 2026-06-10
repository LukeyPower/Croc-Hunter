using UnityEngine;

namespace CrocHunter
{
    public class CrocHitbox : MonoBehaviour
    {
        [SerializeField] private CrocController controller;
        [SerializeField] private bool isHead;

        public bool IsHead => isHead;

        public void TakeHit()
        {
            if (isHead)
                controller.HitHead();
            else
                controller.HitBody();
        }
    }
}
