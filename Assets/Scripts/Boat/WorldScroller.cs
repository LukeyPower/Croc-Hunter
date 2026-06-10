using UnityEngine;

namespace CrocHunter
{
    // Moves the environment in -Z and resets it when it has scrolled a full tile length,
    // creating a seamless loop.
    //
    // IMPORTANT — your environment geometry must be long enough that the reset is invisible:
    //   Tile Z scale should be at least (tileLength + camera far clip) ≈ 400 units.
    //   Set local Z position of each child to tileLength / 2 (≈ 195) so the geometry
    //   extends from local Z ≈ -5 all the way to Z ≈ 395.
    //   Default tileLength = 200 matches a 400-unit-long tile with far clip = 200.
    public class WorldScroller : MonoBehaviour
    {
        [SerializeField] private WorldScrollSettings settings;
        [SerializeField] private float tileLength = 200f;

        private float _originZ;

        void Start()
        {
            _originZ = transform.position.z;
        }

        void Update()
        {
            transform.position += Vector3.back * settings.ScrollSpeed * Time.deltaTime;

            if (_originZ - transform.position.z >= tileLength)
                transform.position = new Vector3(transform.position.x, transform.position.y, _originZ);
        }
    }
}
