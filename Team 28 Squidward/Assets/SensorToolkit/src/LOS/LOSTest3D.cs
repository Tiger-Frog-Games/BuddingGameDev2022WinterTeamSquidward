using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit {

    public class LOSTest3D : BaseLOSTest {

        QueryTriggerInteraction queryTriggerInteraction => Config.IgnoreTriggerColliders 
            ? QueryTriggerInteraction.Ignore 
            : QueryTriggerInteraction.Collide;

        protected override LOSRayResult TestPoint(Vector3 testPoint) {
            var delta = testPoint - Config.Origin;

            var ray = new Ray(Config.Origin, delta.normalized);
            RaycastHit hitInfo;
            var result = new LOSRayResult() { OriginPoint = ray.origin, TargetPoint = testPoint, VisibilityMultiplier = 1f };
            if (Physics.Raycast(ray, out hitInfo, delta.magnitude, Config.BlocksLineOfSight, queryTriggerInteraction)) {
                // Ray hit something, check that it was the target.
                var isTarget = (hitInfo.rigidbody != null && hitInfo.rigidbody.gameObject == Config.InputSignal.Object) 
                    || hitInfo.collider.gameObject == Config.InputSignal.Object;

                isTarget = isTarget || (Config.OwnedColliders?.Contains(hitInfo.collider) ?? false);

                if (!isTarget) {
                    result.RayHit = new RayHit() {
                        IsObstructing = true,
                        Point = hitInfo.point,
                        Normal = hitInfo.normal,
                        Distance = hitInfo.distance,
                        DistanceFraction = hitInfo.distance / delta.magnitude,
                        Collider = hitInfo.collider
                    };
                }
            }
            return result;
        }

        protected override Vector3 GenerateRandomTestPoint() {
            var colliders = Config.OwnedColliders;
            if (colliders == null || colliders.Count == 0) {
                return RandomPointInBounds(Config.InputSignal.Shape);
            }
            // Choose a random collider weighted by its volume
            Collider rc = colliders[0];
            var totalVolume = 0f;
            for (int i = 0; i < colliders.Count; i++) {
                var c = colliders[i];
                totalVolume += c.bounds.size.x * c.bounds.size.y * c.bounds.size.z;
            }

            var r = Random.Range(0f, 1f);
            for (int i = 0; i < colliders.Count; i++) {
                var c = colliders[i];
                rc = c;
                var v = c.bounds.size.x * c.bounds.size.y * c.bounds.size.z;
                r -= v / totalVolume;
                if (r <= 0f) break;
            }

            // Now choose a random point within that random collider and return it
            var goRoot = Config.InputSignal.Object;
            var rp = new Vector3(Random.Range(-.5f, .5f), Random.Range(-.5f, .5f), Random.Range(-.5f, .5f));
            rp.Scale(rc.bounds.size);
            rp += rc.bounds.center - goRoot.transform.position;
            return rp;
        }

        Vector3 RandomPointInBounds(Bounds bounds) {
            var r = .75f;
            var rp = new Vector3(Random.Range(-r, r), Random.Range(-r, r), Random.Range(-r, r));
            return Vector3.Scale(rp, bounds.extents) + bounds.center;
        }
    }

}