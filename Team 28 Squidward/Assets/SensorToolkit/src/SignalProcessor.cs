using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit {

    public interface ISignalProcessor {
        bool ProcessOutput(ref Signal signal);
    }

    public class TagSelectorAttribute : PropertyAttribute { }

    [Serializable]
    public class DefaultSignalFilter : ISignalProcessor {

        [Tooltip("Any GameObject in this list will not be detected by this sensor.")]
        public List<GameObject> IgnoreList = new List<GameObject>();

        [Tooltip("When set to true the sensor will only detect objects whose tags are in the 'withTag' array.")]
        public bool EnableTagFilter;

        [Tooltip("Array of tags that will be detected by the sensor.")]
        [TagSelector]
        public string[] AllowedTags;

        public Sensor Sensor;

        List<Collider> c3ds = new List<Collider>();
        List<Collider2D> c2ds = new List<Collider2D>();

        public bool IsNull() {
            foreach (var go in IgnoreList) {
                if (go != null) {
                    return false;
                }
            }
            if (AllowedTags == null) {
                return true;
            }
            foreach (var tag in AllowedTags) {
                if (tag != null) {
                    return false;
                }
            }
            return true;
        }

        public bool ShouldIgnoreObstruction(RaycastHit hit) =>
            IgnoreList.Contains(hit.collider.gameObject)
            || IgnoreList.Contains(hit.collider.attachedRigidbody?.gameObject ?? hit.collider.gameObject);

        public bool ShouldIgnoreObstruction(RaycastHit2D hit) =>
            IgnoreList.Contains(hit.collider.gameObject)
            || IgnoreList.Contains(hit.collider.attachedRigidbody?.gameObject ?? hit.collider.gameObject);

        public bool ProcessOutput(ref Signal signal) {
            return DoFilter(ref signal);
        }

        bool DoFilter(ref Signal signal) {
            if (!IsPassingIgnoreList(signal.Object)) {
                return false;
            }
            var passesTagFilter = IsPassingTagFilter(signal.Object);

            c3ds.Clear(); Sensor.GetDetectedColliders(signal.Object, c3ds);
            c2ds.Clear(); Sensor.GetDetectedColliders(signal.Object, c2ds);
            var rbGo = c3ds.Count > 0 ? c3ds[0].attachedRigidbody?.gameObject
                : c2ds.Count > 0 ? c2ds[0].attachedRigidbody?.gameObject
                : null;

            if (rbGo == null) {
                return passesTagFilter;
            }

            if (!IsPassingIgnoreList(rbGo)) {
                return false;
            }

            return passesTagFilter || IsPassingTagFilter(rbGo);
        }

        bool IsPassingTagFilter(GameObject go) {
            if (EnableTagFilter) {
                var tagFound = false;
                for (int i = 0; i < AllowedTags.Length; i++) {
                    if (AllowedTags[i] != "" && go != null && go.CompareTag(AllowedTags[i])) {
                        tagFound = true;
                        break;
                    }
                }
                if (!tagFound) {
                    return false;
                }
            }
            return true;
        }

        bool IsPassingIgnoreList(GameObject go) {
            for (int i = 0; i < IgnoreList.Count; i++) {
                if (ReferenceEquals(IgnoreList[i], go)) {
                    return false;
                }
            }
            return true;
        }
    }

    [Serializable]
    public class MapToRigidBodyFilter : ISignalProcessor {
        public Sensor Sensor;
        public bool IsRigidBodyMode;

        List<Collider> c3ds = new List<Collider>();
        List<Collider2D> c2ds = new List<Collider2D>();

        public bool ProcessOutput(ref Signal signal) {
            if (!IsRigidBodyMode) {
                return true;
            }
            c3ds.Clear(); Sensor.GetDetectedColliders(signal.Object, c3ds);
            c2ds.Clear(); Sensor.GetDetectedColliders(signal.Object, c2ds);
            var rbGo = c3ds.Count > 0 ? c3ds[0].attachedRigidbody?.gameObject
                : c2ds.Count > 0 ? c2ds[0].attachedRigidbody?.gameObject
                : null;
            if (rbGo != null) {
                signal.Shape = new Bounds(signal.Shape.center - (rbGo.transform.position - signal.Object.transform.position), signal.Shape.size);
                signal.Object = rbGo;
                return true;
            }
            return false;
        }
    }
}