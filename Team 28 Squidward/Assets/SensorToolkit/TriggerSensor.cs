using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Micosmo.SensorToolkit
{
    /*
     * The Trigger Sensor detects objects that intersect a Trigger Collider. It works by listening 
     * for the events OnTriggerEnter and OnTriggerExit. The sensor has a similar role as the 
     * Range Sensor, with some unique advantages. The downside is that its more difficult to configure. 
     * There are some subtle complexities to Trigger Colliders in Unity that must be considered when 
     * using this sensor.
     */
    [AddComponentMenu("Sensors/Trigger Sensor")]
    public class TriggerSensor : BaseVolumeSensor {

        #region Configurations
        [SerializeField]
        ObservableBool runInSafeMode;
        #endregion

        #region Events
        public override event Action OnPulsed;
        #endregion

        #region Public
        // Change RunInSafeMode at runtime
        public bool RunInSafeMode {
            get => runInSafeMode.Value;
            set => runInSafeMode.Value = value;
        }

        // Not necessary to all Pulse on the TriggerSensor.
        public override void Pulse() {
            UpdateAllSignals();
        }
        #endregion

        #region Internals
        Safety safety;

        protected override void Awake() {
            base.Awake();

            if (runInSafeMode == null) {
                runInSafeMode = new ObservableBool();
            }

            runInSafeMode.OnChanged += RunInSafeModeChangedHandler;
            RunInSafeModeChangedHandler();
        }

        void OnDestroy() {
            runInSafeMode.OnChanged -= RunInSafeModeChangedHandler;
            if (safety != null) {
                Destroy(safety);
            }
        }

        void OnValidate() {
            if (runInSafeMode != null) {
                runInSafeMode.OnValidate();
            }
        }

        void RunInSafeModeChangedHandler() {
            if (RunInSafeMode && safety == null) {
                safety = gameObject.AddComponent<Safety>();
                safety.TriggerSensor = this;
            } else if (!RunInSafeMode && safety != null) {
                Destroy(safety);
                safety = null;
            }
        }

        void OnTriggerEnter(Collider other) {
            AddCollider(other, true);
        }

        void OnTriggerExit(Collider other) {
            RemoveCollider(other, true);
        }
        #endregion

        #region Safety Implementation
        public class Safety : MonoBehaviour {
            TriggerSensor triggerSensor;
            public TriggerSensor TriggerSensor {
                set {
                    if (!ReferenceEquals(triggerSensor, value)) {
                        triggerSensor = value;
                        triggerStayTests.Clear();
                        if (triggerSensor != null) {
                            foreach (var collider in triggerSensor.GetColliders()) {
                                triggerStayTests[collider] = 1;
                            }
                        }
                    }
                } get {
                    return triggerSensor;
                }
            }

            Dictionary<Collider, int> triggerStayTests = new Dictionary<Collider, int>();
            List<Collider> tempColliderList = new List<Collider>();

            void FixedUpdate() {
                tempColliderList.Clear();
                foreach (var test in triggerStayTests) {
                    tempColliderList.Add(test.Key);
                }
                foreach (var c in tempColliderList) {
                    triggerStayTests[c] = 0;
                }
            }

            void OnTriggerStay(Collider other) {
                if (!triggerStayTests.ContainsKey(other)) { triggerSensor.AddCollider(other, true); }
                triggerStayTests[other] = 1;
            }

            void Update() {
                tempColliderList.Clear();
                var colliderStayLagEnumerator = triggerStayTests.GetEnumerator();
                foreach (var test in triggerStayTests) {
                    var c = test.Key;
                    int currentCount;
                    if (triggerStayTests.TryGetValue(c, out currentCount) && currentCount == 0) {
                        tempColliderList.Add(c);
                    }
                }

                foreach (var c in tempColliderList) {
                    triggerSensor.RemoveCollider(c, true);
                    triggerStayTests.Remove(c);
                }
            }
        }
        #endregion
    }
}