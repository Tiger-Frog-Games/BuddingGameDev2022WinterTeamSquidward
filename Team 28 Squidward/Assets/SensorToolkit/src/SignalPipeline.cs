using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit {

    public class SignalPipeline {

        ObjectsEnumerable detectionsEnumerable;
        public ObjectsEnumerable OutputObjects {
            get {
                if (detectionsEnumerable == null) {
                    detectionsEnumerable = new ObjectsEnumerable(this);
                }
                return detectionsEnumerable;
            }
        }

        SignalsEnumerable signalsEnumerable;
        public SignalsEnumerable OutputSignals {
            get {
                if (signalsEnumerable == null) {
                    signalsEnumerable = new SignalsEnumerable(this);
                }
                return signalsEnumerable;
            }
        }

        public event Action<Signal> OnSignalAdded;
        public event Action<Signal> OnSignalChanged;
        public event Action<Signal> OnSignalRemoved;

        public List<ISignalProcessor> SignalProcessors { get { return signalProcessors; } }
        List<ISignalProcessor> signalProcessors = new List<ISignalProcessor>();

        Dictionary<GameObject, SignalMap> inputToMap = new Dictionary<GameObject, SignalMap>();
        Dictionary<GameObject, SignalMap> outputToMap = new Dictionary<GameObject, SignalMap>();

        HashSet<GameObject> toRemove = new HashSet<GameObject>();
        List<SignalMap> added = new List<SignalMap>();
        List<SignalMap> changed = new List<SignalMap>();
        List<Signal> removed = new List<Signal>();

        public Signal GetSignal(GameObject go) {
            return outputToMap[go].CombinedSignal;
        }

        public bool TryGetSignal(GameObject go, out Signal signal) {
            SignalMap map;
            if (outputToMap.TryGetValue(go, out map)) {
                signal = map.CombinedSignal;
                return true;
            }
            signal = new Signal();
            return false;
        }

        public List<GameObject> GetInputObjects(GameObject go, List<GameObject> storeIn) {
            SignalMap map;
            if (outputToMap.TryGetValue(go, out map)) {
                foreach (var input in map.InputSignals) {
                    storeIn.Add(input.InputObject);
                }
            }
            return storeIn;
        }

        public bool ContainsSignal(GameObject go) {
            return outputToMap.ContainsKey(go);
        }

        public void UpdateAllInputSignals(List<Signal> nextSignals) {
            toRemove.Clear();
            foreach (var input in inputToMap) {
                toRemove.Add(input.Key);
            }

            foreach (var signal in nextSignals) {
                toRemove.Remove(signal.Object);
                UpdateInputSignalInternal(signal);
            }

            foreach (var remaining in toRemove) {
                RemoveInputSignalInternal(remaining);
            }

            PlayEvents();
        }

        public void UpdateInputSignal(Signal signal) {
            UpdateInputSignalInternal(signal);
            PlayEvents();
        }

        public void RemoveInputSignal(GameObject forObject) {
            RemoveInputSignalInternal(forObject);
            PlayEvents();
        }

        void UpdateInputSignalInternal(Signal signal) {
            if (ReferenceEquals(signal.Object, null)) {
                return;
            }
            var processed = new ProcessedSignal();
            if (ProcessSignal(in signal, out processed)) {
                UpdateProcessedSignal(processed);
            } else {
                RemoveInputSignalInternal(signal.Object);
            }
        }

        void RemoveInputSignalInternal(GameObject forObject) {
            SignalMap prevSignalMap = null;
            if (inputToMap.TryGetValue(forObject, out prevSignalMap)) {
                RemoveSignalFromMap(forObject, prevSignalMap);
                inputToMap.Remove(forObject);
            }
        }

        void UpdateProcessedSignal(ProcessedSignal proc) {
            SignalMap prevSignalMap = null;
            if (inputToMap.TryGetValue(proc.InputObject, out prevSignalMap)) {
                if (ReferenceEquals(prevSignalMap.OutputObject, proc.Signal.Object)) {
                    prevSignalMap.UpdateSignal(proc);
                } else {
                    RemoveSignalFromMap(proc.InputObject, prevSignalMap);
                    NewProcessedSignal(proc);
                }
            } else {
                NewProcessedSignal(proc);
            }
        }

        void NewProcessedSignal(ProcessedSignal proc) {
            SignalMap map;
            if (!outputToMap.TryGetValue(proc.Signal.Object, out map)) {
                map = signalMapCache.Get();
                map.Spawn(this, proc.Signal.Object);
                outputToMap[proc.Signal.Object] = map;
                OnAddedEvent(map);
            }
            inputToMap[proc.InputObject] = map;
            map.UpdateSignal(proc);
        }

        void RemoveSignalFromMap(GameObject inputObject, SignalMap map) {
            if (map.RemoveByInputObject(inputObject)) {
                outputToMap.Remove(map.OutputObject);
                signalMapCache.Dispose(map);
            }
        }

        void OnAddedEvent(SignalMap signal) {
            added.Add(signal);
        }

        void OnChangedEvent(SignalMap signal) {
            changed.Add(signal);
        }

        void OnRemovedEvent(Signal signal) {
            removed.Add(signal);
        }

        int timestamp = 0;
        void PlayEvents() {
            foreach (var change in changed) {
                if (change.Timestamp != timestamp) {
                    OnSignalChanged?.Invoke(change.CombinedSignal);
                }
            }

            foreach (var remove in removed) {
                OnSignalRemoved?.Invoke(remove);
            }

            foreach (var add in added) {
                OnSignalAdded?.Invoke(add.CombinedSignal);
            }

            added.Clear();
            changed.Clear();
            removed.Clear();

            timestamp += 1;
        }

        bool ProcessSignal(in Signal signal, out ProcessedSignal processed) {
            var processedSignal = signal;

            foreach (var processor in signalProcessors) {
                if (!processor.ProcessOutput(ref processedSignal) || ReferenceEquals(processedSignal.Object, null)) {
                    processed = new ProcessedSignal();
                    return false;
                }
            }
            processed = new ProcessedSignal() { InputObject = signal.Object, Signal = processedSignal };
            return true;
        }

        static SignalMapCache signalMapCache = new SignalMapCache();

        struct ProcessedSignal : IEquatable<ProcessedSignal> {
            public GameObject InputObject;
            public Signal Signal;
            public bool Equals(ProcessedSignal other) {
                return ReferenceEquals(InputObject, other.InputObject) && Signal.Equals(other.Signal);
            }
        }

        class SignalMapCache : ObjectCache<SignalMap> {
            public override void Dispose(SignalMap obj) {
                obj.Dispose();
                base.Dispose(obj);
            }
        }

        class SignalMap {

            Signal combinedSignal;
            public Signal CombinedSignal {
                get {
                    if (isChanged) {
                        CombineSignals();
                    }
                    return combinedSignal;
                }
            }
            public int Timestamp { get; private set; }
            public GameObject OutputObject { get; private set; }

            SignalPipeline pipeline;
            List<ProcessedSignal> inputSignals = new List<ProcessedSignal>();
            public List<ProcessedSignal> InputSignals => inputSignals;
            bool isChanged = false;

            public void UpdateSignal(ProcessedSignal updatedSignal) {
                for (int i = 0; i < inputSignals.Count; i++) {
                    var s = inputSignals[i];
                    if (ReferenceEquals(s.InputObject, updatedSignal.InputObject)) {
                        if (s.Equals(updatedSignal)) {
                            return;
                        }
                        inputSignals[i] = updatedSignal;
                        MarkChanged();
                        return;
                    }
                }
                inputSignals.Add(updatedSignal);
                MarkChanged();
                return;
            }

            public bool RemoveByInputObject(GameObject toRemove) {
                for (int i = 0; i < inputSignals.Count; i++) {
                    var s = inputSignals[i];
                    if (ReferenceEquals(s.InputObject, toRemove)) {
                        inputSignals.RemoveAt(i);
                        if (inputSignals.Count == 0) {
                            pipeline.OnRemovedEvent(combinedSignal);
                            return true;
                        }
                        MarkChanged();
                        return false;
                    }
                }
                return false;
            }

            public void Spawn(SignalPipeline container, GameObject outputObject) {
                this.pipeline = container;
                OutputObject = outputObject;
                Timestamp = container.timestamp;
            }

            public void Dispose() {
                Timestamp = pipeline.timestamp;
                pipeline = null;
                OutputObject = null;
                isChanged = false;
                inputSignals.Clear();
                combinedSignal = new Signal();
            }

            void MarkChanged() {
                if (!isChanged) {
                    pipeline.OnChangedEvent(this);
                    isChanged = true;
                }
            }

            public void CombineSignals() {
                bool isFirst = true;

                foreach (var proc in inputSignals) {
                    if (isFirst) {
                        combinedSignal = proc.Signal;
                        isFirst = false;
                    } else {
                        combinedSignal.Combine(proc.Signal);
                    }
                }

                isChanged = false;
            }
        }

        public class SignalsEnumerable : IEnumerable<Signal>, IEnumerable {
            SignalPipeline source;
            public SignalsEnumerable(SignalPipeline source) { this.source = source; }
            public Enumerator GetEnumerator() { return new Enumerator(source); }
            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
            IEnumerator<Signal> IEnumerable<Signal>.GetEnumerator() { return GetEnumerator(); }

            public int Count { get { return source.outputToMap.Count; } }

            public struct Enumerator : IEnumerator<Signal>, IEnumerator {
                SignalPipeline source;
                Dictionary<GameObject, SignalMap>.Enumerator sourceEnumerator;
                public Enumerator(SignalPipeline source) {
                    this.source = source;
                    sourceEnumerator = source.outputToMap.GetEnumerator();
                }
                public Signal Current { get { return sourceEnumerator.Current.Value.CombinedSignal; } }
                object IEnumerator.Current => throw new NotImplementedException();
                public void Dispose() { sourceEnumerator.Dispose(); }
                public bool MoveNext() {
                    var result = sourceEnumerator.MoveNext();
                    if (result && Current.Object == null) {
                        return MoveNext();
                    }
                    return result;
                }
                public void Reset() { sourceEnumerator = source.outputToMap.GetEnumerator(); }
            }
        }

        public class ObjectsEnumerable : IEnumerable<GameObject>, IEnumerable {
            SignalPipeline source;
            public ObjectsEnumerable(SignalPipeline source) { this.source = source; }
            public Enumerator GetEnumerator() { return new Enumerator(source); }
            IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
            IEnumerator<GameObject> IEnumerable<GameObject>.GetEnumerator() { return GetEnumerator(); }

            public int Count { get { return source.outputToMap.Count; } }

            public struct Enumerator : IEnumerator<GameObject>, IEnumerator {
                SignalPipeline source;
                Dictionary<GameObject, SignalMap>.Enumerator sourceEnumerator;
                public Enumerator(SignalPipeline source) {
                    this.source = source;
                    sourceEnumerator = source.outputToMap.GetEnumerator();
                }
                public GameObject Current { get { return sourceEnumerator.Current.Value.OutputObject; } }
                object IEnumerator.Current => throw new NotImplementedException();
                public void Dispose() { sourceEnumerator.Dispose(); }
                public bool MoveNext() {
                    var result = sourceEnumerator.MoveNext();
                    if (result && Current == null) {
                        return MoveNext();
                    }
                    return result;
                }
                public void Reset() { sourceEnumerator = source.outputToMap.GetEnumerator(); }
            }
        }
    }

}