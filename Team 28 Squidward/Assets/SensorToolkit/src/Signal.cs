using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit {

    [Serializable]
    public struct Signal : IEquatable<Signal> {
        public GameObject Object;
        public float Strength;
        public Bounds Shape;
        public Signal(GameObject obj, float strength, Bounds shape) {
            Object = obj;
            Strength = strength;
            Shape = shape;
        }
        public Signal(GameObject obj) {
            Object = obj;
            Strength = 1f;
            Shape = new Bounds();
        }
        public Signal(Signal other) {
            Object = other.Object;
            Strength = other.Strength;
            Shape = other.Shape;
        }
        public void Expand(Collider c) {
            var b = c.bounds;
            var lb = new Bounds(b.center - Object.transform.position, b.size);
            Shape.Encapsulate(lb);
        }
        public void Combine(Signal signal) {
            Strength = Mathf.Max(Strength, signal.Strength);
            var bounds = Bounds;
            bounds.Encapsulate(signal.Bounds);
            Bounds = bounds;
        }
        public Bounds Bounds {
            get => new Bounds(Shape.center + Object.transform.position, Shape.size);
            set => Shape = new Bounds(value.center - Object.transform.position, value.size);
        }
        public bool Equals(Signal other) {
            return ReferenceEquals(Object, other.Object) && Strength == other.Strength && Shape == other.Shape;
        }
        public float DistanceTo(Vector3 point) {
            return (Bounds.center - point).magnitude;
        }
    }

}
