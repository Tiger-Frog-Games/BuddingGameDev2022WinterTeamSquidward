using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit {
    /**
     * The LOS Sensor detects objects that have a visibility percentage above some threshold. It determines visibility by 
     * casting rays at the object and calculating the ratio that are obstructed. It is a Compound Sensor, meaning it 
     * requires another sensor as input. The Signals from the input sensor are each tested for line of sight.
     */
    [AddComponentMenu("Sensors/2D LOS Sensor")]
    public class LOSSensor2D : Sensor, IPulseRoutine {

        #region Configurations
        [SerializeField]
        [Tooltip("A Sensor whose Signals will be tested for line of sight.")]
        ObservableSensor inputSensor;

        [SerializeField]
        PulseRoutine pulseRoutine;

        [Tooltip("Layermask for which physics layers block line of sight rays.")]
        public LayerMask BlocksLineOfSight;

        [Tooltip("The line of sight will not be blocked by Trigger Colliders when this is true.")]
        public bool IgnoreTriggerColliders;

        [Tooltip("If this is true the sensor will only attempt line of sight tests on objects that have a LOSTargets component.")]
        public bool TestLOSTargetsOnly;

        [Range(1, 20), Tooltip("The number of randomly generated raycast targets to test on each object. Does nothing if that object has a LOSTargets component.")]
        public int NumberOfRays = 1;

        [Range(0f, 1f), Tooltip("The ratio of unobstructed raycasts must exceed this value for the object to be detected by this sensor.")]
        public float MinimumVisibility = 0.5f;

        [Tooltip("Enables smoothing of visibility values over multiple frames.")]
        public bool MovingAverageEnabled = false;

        [Tooltip("The number of frames to smooth visibility values over.")]
        public int MovingAverageWindowSize = 10;

        [Tooltip("Enables the Distance Limits feature")]
        public bool LimitDistance = false;

        [Tooltip("When LimitDistance is true an object must be within this distance for it to be detected.")]
        public float MaxDistance = 1f;

        // A struct specifying how visibility is scaled as a function of distance. Choices are Step, Linear Decay or a Curve.
        public ScalingFunction VisibilityByDistance = ScalingFunction.Default();

        [Tooltip("Enables the Angle Limits feature when this is true.")]
        public bool LimitViewAngle;

        [Tooltip("When LimitViewAngle is true an object must be within this horizontal view angle to be detected.")]
        public float MaxViewAngle = 45;

        // A struct specifying how visibility is scaled as a function of horizontal view angle. Choices are Step, Linear Decay or a Curve.
        public ScalingFunction VisibilityByViewAngle = ScalingFunction.Default();
        #endregion

        #region Events
        public override event System.Action OnPulsed;
        #endregion

        #region Public
        // Change the input sensor at runtime
        public Sensor InputSensor {
            get => inputSensor.Value;
            set => inputSensor.Value = value;
        }

        // Change the pulse mode at runtime
        public PulseRoutine.Modes PulseMode {
            get => pulseRoutine.Mode.Value;
            set => pulseRoutine.Mode.Value = value;
        }

        // Change the pulse interval at runtime
        public float PulseInterval {
            get => pulseRoutine.Interval.Value;
            set => pulseRoutine.Interval.Value = value;
        }

        // Displays the results of line of sight tests during OnDrawGizmosSelected for objects in this set.
        // Used by the editor classes. You shouldn't need to touch this.
        public HashSet<GameObject> ShowRayCastDebug;

        /**
         * Returns a data-object with details about the line of sight test for a given GameObject. The ILOSResult instances 
         * are cached by the sensor and reused each time it pulses. Don't hold onto this reference for long, it will 
         * be invalid after the next pulse.
         */
        public ILOSResult GetResult(GameObject forObject) {
            return losTests[forObject];
        }

        List<ILOSResult> resultsList = new List<ILOSResult>();
        // Returns a list of ILOSResult data-objects with line of sight results for all of the objects tested.
        public List<ILOSResult> GetAllResults() {
            resultsList.Clear();
            foreach (var tester in losTests) {
                resultsList.Add(tester.Value);
            }
            return resultsList;
        }

        // Recalculates line of sight visibility for each Signal in the input sensor.
        public override void Pulse() {
            if (!isActiveAndEnabled) {
                return;
            }

            if (!Application.isPlaying && InputSensor != null) {
                InputSensor.Pulse();
            }

            SwapLosTestBuffers();

            workList.Clear();

            if (InputSensor != null) {
                foreach (var signal in InputSensor.Signals) {
                    var result = TestSignal(signal);
                    if (result.IsVisible) {
                        workList.Add(result.OutputSignal);
                    }
                }
            }

            UpdateAllSignals(workList);
            OnPulsed?.Invoke();
        }

        List<Collider2D> clist = new List<Collider2D>();
        /**
         * Immediately test line of sight for a given signal and return the results. This gives you full control to test 
         * line of sight when ever you need. Just keep in mind that the ILOSResult is stored until the next Pulse and 
         * then returned to a cache.
         */
        public ILOSResult TestSignal(Signal inputSignal) {
            clist.Clear();
            var tester = getLOSTest(inputSignal.Object);
            var testConfig = new BaseLOSTest.ConfigParams {
                InputSignal = inputSignal,
                OwnedCollider2Ds = GetInputColliders(inputSignal.Object, clist),
                Origin = transform.position,
                Frame = frame,
                MinimumVisibility = MinimumVisibility,
                BlocksLineOfSight = BlocksLineOfSight,
                IgnoreTriggerColliders = IgnoreTriggerColliders,
                TestLOSTargetsOnly = TestLOSTargetsOnly,
                NumberOfRays = NumberOfRays,
                MovingAverageEnabled = MovingAverageEnabled,
                MovingAverageWindowSize = MovingAverageWindowSize,
                LimitDistance = LimitDistance,
                MaxDistance = MaxDistance,
                VisibilityByDistance = VisibilityByDistance,
                LimitViewAngle = LimitViewAngle,
                MaxHorizAngle = MaxViewAngle,
                VisibilityByHorizAngle = VisibilityByViewAngle,
                MaxVertAngle = 360,
                VisibilityByVertAngle = new ScalingFunction() { Mode = ScalingMode.Step }
            };
            tester.PerformTest(testConfig);
            return tester;
        }
        #endregion

        #region Internals
        ReferenceFrame frame => new ReferenceFrame(transform.up, transform.right, -transform.forward);

        // Maps a GameObject to a list of raycast target positions for calculating line of sight
        Dictionary<GameObject, LOSTest2D> losTests = new Dictionary<GameObject, LOSTest2D>();

        Dictionary<GameObject, LOSTest2D> prevLosTests = new Dictionary<GameObject, LOSTest2D>();

        List<Signal> workList = new List<Signal>();

        static ObjectCache<LOSTest2D> losTestCache = new ObjectCache<LOSTest2D>();

        protected override List<Collider2D> GetInputColliders(GameObject inputObject, List<Collider2D> storeIn)
            => InputSensor.GetDetectedColliders(inputObject, storeIn);

        protected override void Awake() {
            base.Awake();

            if (inputSensor == null) {
                inputSensor = new ObservableSensor();
            }

            if (pulseRoutine == null) {
                pulseRoutine = new PulseRoutine();
            }
            pulseRoutine.Awake(this);
        }

        protected virtual void OnEnable() {
            inputSensor.OnChangedValues += InputSensorChangedHandler;
            InputSensorChangedHandler(null, InputSensor);
            pulseRoutine.OnEnable();
        }

        protected virtual void OnDisable() {
            inputSensor.OnChangedValues -= InputSensorChangedHandler;
            pulseRoutine.OnDisable();
        }

        protected virtual void OnValidate() {
            inputSensor?.OnValidate();
            pulseRoutine?.OnValidate();
        }

        void InputSensorChangedHandler(Sensor prev, Sensor next) {
            if (prev != null) {
                prev.OnDetected.RemoveListener(InputOnDetectedHandler);
                prev.OnLostDetection.RemoveListener(InputLostDetectionHandler);
            }
            if (next != null) {
                next.OnDetected.AddListener(InputOnDetectedHandler);
                next.OnLostDetection.AddListener(InputLostDetectionHandler);
            }
        }

        void InputOnDetectedHandler(GameObject detection, Sensor sensor) {
            var signal = sensor.GetSignal(detection);
            var results = TestSignal(signal);
            if (results.IsVisible) {
                UpdateSignalImmediate(results.OutputSignal);
            }
        }

        void InputLostDetectionHandler(GameObject lost, Sensor sensor) {
            DisposeLOSTest(lost);
            LostSignalImmediate(lost);
        }

        void SwapLosTestBuffers() {
            var temp = prevLosTests;
            prevLosTests = losTests;
            losTests = temp;
            DisposeAllLOSTests();
        }

        void DisposeLOSTest(GameObject go) {
            LOSTest2D losTest;
            if (losTests.TryGetValue(go, out losTest)) {
                losTestCache.Dispose(losTest);
                losTests.Remove(go);
            }
        }

        void DisposeAllLOSTests() {
            var losTestEnumerator = losTests.GetEnumerator();
            while (losTestEnumerator.MoveNext()) {
                var losTest = losTestEnumerator.Current.Value;
                losTestCache.Dispose(losTest);
            }
            losTests.Clear();
        }

        LOSTest2D getLOSTest(GameObject go) {
            LOSTest2D losTest;
            if (prevLosTests.TryGetValue(go, out losTest)) {
                losTests[go] = losTest;
                prevLosTests.Remove(go);
                return losTest;
            } else if (losTests.TryGetValue(go, out losTest)) {
                return losTest;
            } else {
                losTest = losTestCache.Get();
                losTest.Reset();
                losTests[go] = losTest;
            }
            return losTest;
        }

        protected override void OnDrawGizmosSelected() {
            base.OnDrawGizmosSelected();

            if (LimitDistance && !LimitViewAngle) {
                SensorGizmos.PushColor(Color.yellow);
                SensorGizmos.SphereGizmo(transform.position, MaxDistance);
                SensorGizmos.PopColor();
            }

            if (LimitViewAngle) {
                SensorGizmos.FOVGizmo(frame, transform.position, LimitDistance ? MaxDistance : 1f, MaxViewAngle, 0);
            }

            if (!ShowDetectionGizmos || ShowRayCastDebug == null) {
                return;
            }

            foreach (var debug in ShowRayCastDebug) {
                LOSTest2D test;
                if (losTests.TryGetValue(debug, out test)) {
                    foreach (var result in test.Rays) {
                        Gizmos.color = SensorGizmos.LerpColour(SensorGizmos.VisibilityGradientColours, 1f - result.VisibilityMultiplier);
                        if (result.IsObstructed) {
                            Gizmos.DrawLine(result.OriginPoint, result.RayHit.Point);
                            Gizmos.color = SensorGizmos.GizmoBlockedColor;
                            Gizmos.DrawLine(result.RayHit.Point, result.TargetPoint);
                            Gizmos.DrawCube(result.TargetPoint, Vector3.one * 0.02f);
                        } else {
                            Gizmos.DrawLine(result.OriginPoint, result.TargetPoint);
                            Gizmos.DrawCube(result.TargetPoint, Vector3.one * 0.02f);
                        }
                    }
                }
            }
        }
        #endregion
    }
}