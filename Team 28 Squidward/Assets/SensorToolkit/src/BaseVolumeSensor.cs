using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit {
    /*
     * Common functionality for sensors that detect colliders within a volume such as the Range and Trigger Sensors.
     */
    public abstract class BaseVolumeSensor : Sensor {

        #region Configurations
        [SerializeField]
        DefaultSignalFilter signalFilter = new DefaultSignalFilter();

        [Tooltip("In Collider mode the sensor detects GameObjects attached to colliders. In RigidBody mode it detects the RigidBody GameObject attached to colliders.")]
        public DetectionModes DetectionMode;
        #endregion

        #region Public
        // Edit the IgnoreList at runtime. Anything in the list will not be detected
        public List<GameObject> IgnoreList => signalFilter.IgnoreList;

        // Enable/Disable the tag filtering at runtime
        public bool EnableTagFilter {
            get => signalFilter.EnableTagFilter;
            set => signalFilter.EnableTagFilter = value;
        }

        // Change the allowed tags at runtime
        public string[] AllowedTags {
            get => signalFilter.AllowedTags;
            set => signalFilter.AllowedTags = value;
        }
        #endregion

        #region Protected
        protected override List<Collider> GetInputColliders(GameObject inputObject, List<Collider> storeIn) {
            List<Collider> clist;
            if (gameObjectColliders.TryGetValue(inputObject, out clist)) {
                foreach (var c in clist) {
                    storeIn.Add(c);
                }
            }
            return storeIn;
        }

        protected override void InitialiseSignalProcessors() {
            base.InitialiseSignalProcessors();
            mapToRB.Sensor = this;
            signalFilter.Sensor = this;
            SignalProcessors.Add(mapToRB);
            SignalProcessors.Add(signalFilter);
        }

        protected void UpdateAllSignals() {
            workList.Clear();

            foreach (var cols in gameObjectColliders) {
                workList.Add(CalculateSignal(cols.Value));
            }
            mapToRB.IsRigidBodyMode = DetectionMode == DetectionModes.RigidBodies;
            UpdateAllSignals(workList);
        }

        protected void AddCollider(Collider c, bool updateSignal) {
            var cols = AddColliderToMap(c, c.gameObject, gameObjectColliders);

            if (!updateSignal) {
                return;
            }
            mapToRB.IsRigidBodyMode = DetectionMode == DetectionModes.RigidBodies;
            UpdateSignalImmediate(CalculateSignal(cols));
        }

        protected void RemoveCollider(Collider c, bool updateSignal) {
            if (c == null) {
                ClearDestroyedGameObjects();
                return;
            }

            var cols = RemoveColliderFromMap(c, c.gameObject, gameObjectColliders);

            if (!updateSignal) {
                return;
            }
            mapToRB.IsRigidBodyMode = DetectionMode == DetectionModes.RigidBodies;
            if (cols == null) {
                LostSignalImmediate(c.gameObject);
            } else {
                UpdateSignalImmediate(CalculateSignal(cols));
            }
        }

        protected void ClearColliders() {
            foreach (var set in gameObjectColliders) {
                colliderListCache.Dispose(set.Value);
            }
            gameObjectColliders.Clear();
        }

        List<Collider> colliderList = new List<Collider>();
        protected List<Collider> GetColliders() {
            colliderList.Clear();
            foreach (var set in gameObjectColliders) {
                colliderList.AddRange(set.Value);
            }
            return colliderList;
        }
        #endregion

        #region Internals
        // Maps a GameObject to a list of it's colliders that have been detected.
        Dictionary<GameObject, List<Collider>> gameObjectColliders = new Dictionary<GameObject, List<Collider>>();

        // List of temporary values for modifying collections
        List<GameObject> gameObjectList = new List<GameObject>();
        List<Signal> workList = new List<Signal>();
        MapToRigidBodyFilter mapToRB = new MapToRigidBodyFilter();

        static ListCache<Collider> colliderListCache = new ListCache<Collider>();

        void ClearDestroyedGameObjects() {
            gameObjectList.Clear();
            foreach (var set in gameObjectColliders) {
                if (set.Key == null) {
                    gameObjectList.Add(set.Key);
                }
            }
            foreach (var go in gameObjectList) {
                colliderListCache.Dispose(gameObjectColliders[go]);
                gameObjectColliders.Remove(go);
            }
        }

        List<Collider> AddColliderToMap(Collider c, GameObject go, Dictionary<GameObject, List<Collider>> dict) {
            List<Collider> colliderList;
            if (!dict.TryGetValue(go, out colliderList)) {
                colliderList = colliderListCache.Get();
                dict[go] = colliderList;
            }
            if (!colliderList.Contains(c)) {
                colliderList.Add(c);
            }
            return colliderList;
        }

        List<Collider> RemoveColliderFromMap(Collider c, GameObject go, Dictionary<GameObject, List<Collider>> dict) {
            List<Collider> colliderList = null;
            if (dict.TryGetValue(go, out colliderList)) {
                colliderList.Remove(c);
                if (colliderList.Count == 0) {
                    dict.Remove(go);
                    colliderListCache.Dispose(colliderList);
                    colliderList = null;
                }
            }
            return colliderList;
        }

        Signal CalculateSignal(List<Collider> colliders) {
            var signal = new Signal();

            Bounds bounds = new Bounds();
            bool isFirst = true;
            foreach (var c in colliders) {
                if (isFirst) {
                    bounds = c.bounds;
                    isFirst = false;
                } else {
                    bounds.Encapsulate(c.bounds);
                }
            }

            signal.Object = colliders[0].gameObject;
            signal.Bounds = bounds;
            signal.Strength = 1f;

            return signal;
        }
        #endregion
    }
}