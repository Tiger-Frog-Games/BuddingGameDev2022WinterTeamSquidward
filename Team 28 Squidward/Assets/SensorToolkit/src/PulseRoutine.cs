using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Micosmo.SensorToolkit {

    public interface IPulseRoutine {
        PulseRoutine.Modes PulseMode { get; set; }
        float PulseInterval { get; set; }
    }

    [Serializable]
    public class PulseRoutine {
        public enum Modes { Manual, FixedInterval, EachFrame }

        [Serializable]
        public class ObservableMode : Observable<Modes> { }

        public ObservableMode Mode = new ObservableMode() { Value = Modes.EachFrame };

        public ObservableFloat Interval = new ObservableFloat() { Value = 1f };

        public float dt {
            get {
                if (Mode.Value == Modes.EachFrame) {
                    return Time.deltaTime;
                } else if (Mode.Value == Modes.FixedInterval) {
                    return Interval.Value;
                }
                return 0;
            }
        }

        BasePulsableSensor pulsable;
        float steppedPulseDelay;
        Coroutine pulseRoutine;

        public void Awake(BasePulsableSensor pulsable) {
            this.pulsable = pulsable;

            if (Mode == null) {
                Mode = new ObservableMode();
            }

            if (Interval == null) {
                Interval = new ObservableFloat();
            }

            steppedPulseDelay = UnityEngine.Random.Range(0f, 1f);
        }

        public void OnEnable() {
            Mode.OnChanged += PulseModeChangedHandler;
            Interval.OnChanged += PulseModeChangedHandler;

            PulseModeChangedHandler();
        }

        public void OnDisable() {
            Mode.OnChanged -= PulseModeChangedHandler;
            Interval.OnChanged -= PulseModeChangedHandler;
        }

        public void OnValidate() {
            Mode?.OnValidate();
            Interval?.OnValidate();
        }

        void PulseModeChangedHandler() {
            if (!Application.isPlaying) {
                return;
            }
            RunPulseMode(Mode.Value, Interval.Value);
        }

        void RunPulseMode(Modes mode, float interval = 0) {
            if (pulseRoutine != null) {
                pulsable.StopCoroutine(pulseRoutine);
                pulseRoutine = null;
            }
            if (mode == Modes.EachFrame) {
                pulseRoutine = pulsable.StartCoroutine(PulseEachFrameRoutine());
            } else if (mode == Modes.FixedInterval) {
                pulseRoutine = pulsable.StartCoroutine(PulseFixedIntervalRoutine(interval));
            }
        }

        WaitForSecondsCache initialDelayCache;
        WaitForSecondsCache intervalDelayCache;
        IEnumerator PulseFixedIntervalRoutine(float interval) {
            var initialDelay = initialDelayCache.WaitForSeconds(steppedPulseDelay * interval);
            var intervalDelay = intervalDelayCache.WaitForSeconds(interval);

            yield return initialDelay;

            while (true) {
                pulsable.Pulse();
                yield return intervalDelay;
            }
        }

        IEnumerator PulseEachFrameRoutine() {
            while (true) {
                yield return null;
                pulsable.Pulse();
            }
        }
    }

}