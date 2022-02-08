using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit {

    public interface ILOSResult {
        Signal OutputSignal { get; }
        float Visibility { get; }
        bool IsVisible { get; }
        List<LOSRayResult> Rays { get; }
    }

    public struct LOSRayResult {
        public Vector3 OriginPoint;
        public Vector3 TargetPoint;
        public Transform TargetTransform;
        public RayHit RayHit;
        public float VisibilityMultiplier;
        public bool IsObstructed => RayHit.IsObstructing;
        public float Visibility => IsObstructed ? 0f : VisibilityMultiplier;
    }

    public enum ScalingMode { Step, LinearDecay, Curve }
    [System.Serializable]
    public struct ScalingFunction {
        public ScalingMode Mode;
        public AnimationCurve Curve;

        public float Evaluate(float t) {
            if (Mode == ScalingMode.Step) {
                return t < 1f ? 1f : 0f;
            } if (Mode == ScalingMode.LinearDecay) {
                return 1f - Mathf.Clamp01(t);
            } else {
                return Curve.Evaluate(Mathf.Clamp01(t));
            }
        }

        public static ScalingFunction Default() =>
            new ScalingFunction() {
                Mode = ScalingMode.Step,
                Curve = new AnimationCurve(new Keyframe(0,1), new Keyframe(.5f,1), new Keyframe(1f,0))
            };
    }

    public abstract class BaseLOSTest : ILOSResult {
        public struct ConfigParams {
            public Signal InputSignal;
            public List<Collider> OwnedColliders;
            public List<Collider2D> OwnedCollider2Ds;

            public Vector3 Origin;
            public ReferenceFrame Frame;

            public float MinimumVisibility;

            public LayerMask BlocksLineOfSight;
            public bool IgnoreTriggerColliders;
            public bool TestLOSTargetsOnly;
            public int NumberOfRays;

            public bool MovingAverageEnabled;
            public int MovingAverageWindowSize;

            public bool LimitDistance;
            public float MaxDistance;
            public ScalingFunction VisibilityByDistance;

            public bool LimitViewAngle;
            public float MaxHorizAngle;
            public ScalingFunction VisibilityByHorizAngle;
            public float MaxVertAngle;
            public ScalingFunction VisibilityByVertAngle;
        }
        public ConfigParams Config { get; private set; }

        public Signal OutputSignal { get; private set; }
        public float Visibility { get; private set; }
        public bool IsVisible { get; private set; } // Possible to have Visibility=0 and still be visible
        public List<LOSRayResult> Rays { get; } = new List<LOSRayResult>();

        Signal prevInputSignal;
        ComponentCache losTargetsCache;
        List<Vector3> generatedPoints = new List<Vector3>();
        MovingAverageFilter avgFilter = new MovingAverageFilter(1);

        public void Reset() {
            avgFilter.Clear();
            generatedPoints.Clear();
            Rays.Clear();
            Visibility = 0f;
        }

        public bool PerformTest(ConfigParams config) {
            Config = config;

            Rays.Clear();

            // Need to initialize output signal so .Object is populated
            OutputSignal = new Signal() {
                Object = Config.InputSignal.Object,
                Shape = Config.InputSignal.Shape,
                Strength = 0f
            };
            IsVisible = false;
            Visibility = 0f;
            if (!ReferenceEquals(Config.InputSignal.Object, prevInputSignal.Object)) {
                avgFilter.Clear();
            }
            prevInputSignal = Config.InputSignal;

            var isUsingGeneratedPoints = false;

            var losTargets = losTargetsCache.GetComponent<LOSTargets>(Config.InputSignal.Object);
            if (losTargets == null || losTargets.Targets == null || losTargets.Targets.Count == 0) {
                if (Config.TestLOSTargetsOnly) {
                    return IsVisible;
                }
                GenerateTestPoints();
                isUsingGeneratedPoints = true;
            }

            if (isUsingGeneratedPoints) {
                foreach (var pt in generatedPoints) {
                    var trans = Config.InputSignal.Object.transform;
                    var result = TestPointInternal(trans.rotation * pt + trans.position);
                    Rays.Add(result);
                }
            } else {
                foreach (var target in losTargets.Targets) {
                    var result = TestTransform(target);
                    Rays.Add(result);
                }
            }

            var rayVisibilitySum = 0f;
            foreach (var ray in Rays) {
                rayVisibilitySum += ray.Visibility;
            }
            Visibility = rayVisibilitySum / Rays.Count;


            if (Config.MovingAverageEnabled) {
                avgFilter.Size = Config.MovingAverageWindowSize;
                avgFilter.AddSample(Visibility);
                Visibility = avgFilter.Value;
            } else {
                avgFilter.Clear();
            }

            IsVisible = Visibility >= Config.MinimumVisibility;

            OutputSignal = new Signal() {
                Object = Config.InputSignal.Object,
                Shape = Config.InputSignal.Shape,
                Strength = IsVisible ? Config.InputSignal.Strength * Visibility : 0f
            };

            return IsVisible;
        }

        LOSRayResult TestTransform(Transform testTransform) {
            var result = TestPointInternal(testTransform.position);
            result.TargetTransform = testTransform;
            return result;
        }

        LOSRayResult TestPointInternal(Vector3 testPoint) {
            var visibilityScale = 1f;

            if (Config.LimitDistance) {
                float distance = (Config.Origin - testPoint).magnitude;
                visibilityScale *= Config.VisibilityByDistance.Evaluate(distance / Config.MaxDistance);
            }

            if (Config.LimitViewAngle) {
                var dir = (testPoint - Config.Origin).normalized;
                var dirProjHoriz = Vector3.ProjectOnPlane(dir, Config.Frame.Up);
                var horizAngle = Vector3.Angle(dirProjHoriz, Config.Frame.Forward);
                var vertAngle = Vector3.Angle(dirProjHoriz, dir);

                visibilityScale *= Config.VisibilityByHorizAngle.Evaluate(horizAngle / Config.MaxHorizAngle)
                    * Config.VisibilityByVertAngle.Evaluate(vertAngle / Config.MaxVertAngle);
            }

            if (visibilityScale <= 0f) {
                return new LOSRayResult() {
                    OriginPoint = Config.Origin, TargetPoint = testPoint, VisibilityMultiplier = 0f
                };
            }

            var result = TestPoint(testPoint);

            result.VisibilityMultiplier *= visibilityScale;
            return result;
        }

        protected abstract LOSRayResult TestPoint(Vector3 testPoint);

        protected abstract Vector3 GenerateRandomTestPoint();

        void GenerateTestPoints() {
            generatedPoints.Clear();
            for (int i = 0; i < Config.NumberOfRays; i++) {
                generatedPoints.Add(GenerateRandomTestPoint());
            }
        }
    }

}
