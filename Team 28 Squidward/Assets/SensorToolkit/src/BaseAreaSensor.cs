using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit
{
    /*
     * Common functionality for 2D sensors that detect colliders within an area such as the Range2D and Trigger2D Sensors.
     */
    public abstract class BaseAreaSensor : Sensor {

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
        protected override List<Collider2D> GetInputColliders(GameObject inputObject, List<Collider2D> storeIn) {
            List<Collider2D> clist;
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

        protected void AddCollider(Collider2D c, bool updateSignal) {
            var cols = AddColliderToMap(c, c.gameObject, gameObjectColliders);

            if (!updateSignal) {
                return;
            }
            mapToRB.IsRigidBodyMode = DetectionMode == DetectionModes.RigidBodies;
            UpdateSignalImmediate(CalculateSignal(cols));
        }

        protected void RemoveCollider(Collider2D c, bool updateSignal) {
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

        List<Collider2D> colliderList = new List<Collider2D>();
        protected List<Collider2D> GetColliders() {
            colliderList.Clear();
            foreach (var set in gameObjectColliders) {
                colliderList.AddRange(set.Value);
            }
            return colliderList;
        }
        #endregion

        #region Internals
        // Maps a GameObject to a list of it's colliders that have been detected.
        Dictionary<GameObject, List<Collider2D>> gameObjectColliders = new Dictionary<GameObject, List<Collider2D>>();

        // List of temporary values for modifying collections
        List<GameObject> gameObjectList = new List<GameObject>();
        List<Signal> workList = new List<Signal>();
        MapToRigidBodyFilter mapToRB = new MapToRigidBodyFilter();

        static ListCache<Collider2D> colliderListCache = new ListCache<Collider2D>();

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

        List<Collider2D> AddColliderToMap(Collider2D c, GameObject go, Dictionary<GameObject, List<Collider2D>> dict) {
            List<Collider2D> colliderList;
            if (!dict.TryGetValue(go, out colliderList)) {
                colliderList = colliderListCache.Get();
                dict[go] = colliderList;
            }
            if (!colliderList.Contains(c)) {
                colliderList.Add(c);
            }
            return colliderList;
        }

        List<Collider2D> RemoveColliderFromMap(Collider2D c, GameObject go, Dictionary<GameObject, List<Collider2D>> dict) {
            List<Collider2D> colliderList = null;
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

        Signal CalculateSignal(List<Collider2D> colliders) {
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
