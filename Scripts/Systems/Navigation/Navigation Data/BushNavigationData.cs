using UnityEngine;

namespace Navigation
{
    public class BushNavigationData : NavigationData
    {
        [SerializeField] SphereCollider _capacityArea;

        public Vector3 WorldPosition => transform.position + _capacityArea.center * transform.lossyScale.x;

        float _capacity;

        // Fudge capacity to account for rectangular areas fitting side by side
        const float CAPACITY_FUDGE = 0.15f;

        private void Awake()
        {
            float area = Mathf.PI * Mathf.Pow(_capacityArea.radius * transform.lossyScale.x, 2);
            _capacity = area * CAPACITY_FUDGE;
        }

        public bool ClaimCapacity(NavigationUnitConfig unitConfig)
        {
            float unitArea = GetUnitArea(unitConfig);

            float postCapacity = _capacity - unitArea;
            if (postCapacity < 0)
            { 
                return false;
            }

            _capacity = postCapacity;
            return true;
        }

        public void ReleaseCapacity(NavigationUnitConfig unitConfig)
        {
            float unitArea = GetUnitArea(unitConfig);
            _capacity += unitArea;
        }

        float GetUnitArea(NavigationUnitConfig unitConfig)
        {
            var s = unitConfig.UnitSize;
            return s.x * s.z;
        }
    }
}
