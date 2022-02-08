using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit {

    public class LOSTest2D : BaseLOSTest {
        protected override LOSRayResult TestPoint(Vector3 testPoint) {
            var saveQHT = Physics2D.queriesHitTriggers;
            Physics2D.queriesHitTriggers = !Config.IgnoreTriggerColliders;
            var result = DoTest(testPoint);
            Physics2D.queriesHitTriggers = saveQHT;
            return result;
        }

        LOSRayResult DoTest(Vector3 testPoint) {
            var delta = (Vector2)testPoint - (Vector2)Config.Origin;

            var ray = new Ray(Config.Origin, delta.normalized);
            var result = new LOSRayResult() { OriginPoint = ray.origin, TargetPoint = testPoint, VisibilityMultiplier = 1f };
            var hitInfo = Physics2D.Raycast(ray.origin, ray.direction, delta.magnitude, Config.BlocksLineOfSight);
            if (hitInfo.collider != null) {
                // Ray hit something, check that it was the target.
                var isTarget = (hitInfo.rigidbody != null && hitInfo.rigidbody.gameObject == Config.InputSignal.Object) 
                    || hitInfo.collider.gameObject == Config.InputSignal.Object;

                isTarget = isTarget || Config.OwnedCollider2Ds.Contains(hitInfo.collider);

                if (!isTarget) {
                    result.RayHit = new RayHit() {
                        IsObstructing = true,
                        Point = hitInfo.point,
                        Normal = hitInfo.normal,
                        Distance = hitInfo.distance,
                        DistanceFraction = hitInfo.distance / delta.magnitude,
                        Collider2D = hitInfo.collider
                    };
                }
            }
            return result;
        }

        protected override Vector3 GenerateRandomTestPoint() {
            var colliders = Config.OwnedCollider2Ds;
            if (colliders == null || colliders.Count == 0) {
                return RandomPointInBounds(Config.InputSignal.Shape);
            }
            // Choose a random collider weighted by its volume
            Collider2D rc = colliders[0];
            var totalArea = 0f;
            for (int i = 0; i < colliders.Count; i++) {
                var c = colliders[i];
                totalArea += c.bounds.size.x * c.bounds.size.y;
            }

            var r = Random.Range(0f, 1f);
            for (int i = 0; i < colliders.Count; i++) {
                var c = colliders[i];
                rc = c;
                var v = c.bounds.size.x * c.bounds.size.y;
                r -= v / totalArea;
                if (r <= 0f) break;
            }

            // Now choose a random point within that random collider and return it
            var goRoot = Config.InputSignal.Object;
            var rp = new Vector2(Random.Range(-.5f, .5f), Random.Range(-.5f, .5f));
            rp.Scale(rc.bounds.size);
            rp += (Vector2)(rc.bounds.center - goRoot.transform.position);
            return rp;
        }

        Vector3 RandomPointInBounds(Bounds bounds) {
            var r = .75f;
            var rp = new Vector3(Random.Range(-r, r), Random.Range(-r, r), Random.Range(-r, r));
            return Vector3.Scale(rp, bounds.extents) + bounds.center;
        }
    }

}
