using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Navigation
{
    public class BumbleBloomFieldNavigationData : NavigationData
    {
        [SerializeField] NavigationUnitConfig _bumbleBloomNavigationConfig;
        [SerializeField] float _availableRestPositionRadius;

        public Vector3 GetRandomRestPosition()
        {
            var randomPos = Random.insideUnitCircle * _availableRestPositionRadius;
            return transform.position + new Vector3(randomPos.x, 0, randomPos.y);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, _availableRestPositionRadius);
        }
    }
}
