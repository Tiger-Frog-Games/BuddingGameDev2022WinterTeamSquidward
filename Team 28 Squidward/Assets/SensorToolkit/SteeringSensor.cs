using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Micosmo.SensorToolkit {

    /**
     * The Steering Sensor is a unique sensor for implementing steering behaviour. It's an implementation of 
     * Context Based Steering as described here. The sensor can operate in a spherical mode suitable for flying 
     * agents, or a planar mode for ground-based agents.
     */
    [AddComponentMenu("Sensors/Steering Sensor")]
    public class SteeringSensor : BasePulsableSensor, IPulseRoutine {

        #region Configurations
        [SerializeField]
        [Tooltip("Steering Vectors are 3D when this is true and they are planar when this is false.")]
        ObservableBool isSpherical;

        [SerializeField]
        [Tooltip("Determines the number of discrete buckets that directions around the sensor are boken up into.")]
        ObservableInt resolution = new ObservableInt() { Value = 3 };

        [Tooltip("Speed that the sensor interpolates it's state.")]
        public float InterpolationSpeed = 8f;

        // Configurations struct for the Seek behaviour.
        public SteerSeek Seek = new SteerSeek();

        // Configurations struct for the Avoid behaviour.
        public SteerAvoid Avoid = new SteerAvoid();

        [SerializeField]
        PulseRoutine pulseRoutine;

        [Tooltip("Enables the built-in locomotion if this is any value other then None.")]
        public LocomotionMode LocomotionMode;

        [Tooltip("The RigidBody to control with built-in locomotion.")]
        public Rigidbody RigidBody;

        [Tooltip("The CharacterController to control with built-in locomotion.")]
        public CharacterController CharacterController;

        // Configurations struct for the built-in locomotion behaviours.
        public LocomotionSystem Locomotion;
        #endregion

        #region Events
        public override event System.Action OnPulsed;
        #endregion

        #region Public
        // Change IsSpherical value at runtime
        public bool IsSpherical {
            get => isSpherical.Value;
            set => isSpherical.Value = value;
        }

        // Change Resolution at runtime
        public int Resolution {
            get => Mathf.Abs(resolution.Value);
            set => resolution.Value = value;
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

        // The Vector3 position we're currently seeking. Maps to Seek.Destination.
        public Vector3 Destination {
            get => Seek.Destination;
            set => Seek.Destination = value;
        }

        // The Transform that we're currently seeking. Maps to Seek.DestinationTransform.
        public Transform DestinationTransform {
            get => Seek.DestinationTransform;
            set => Seek.DestinationTransform = value;
        }

        // Is true when we are within the desired range from the target seek position.
        public bool IsDestinationReached => Seek.GetIsDestinationReached(gameObject);

        // Is true when we have not yet reached the destination.
        public bool IsSeeking => !IsDestinationReached;

        // Returns a vector that the agent should move towards. It's length will be roughly the distance to the target position.
        public Vector3 GetSteeringVector() => interpolatedMap?.GetMaxContinuous() ?? Vector3.zero;

        // Calculates a new steering vector.
        public override void Pulse() {
            if (!isActiveAndEnabled) {
                return;
            }

            if (!Application.isPlaying) {
                GridConfigChangeHandler();
                Avoid.PulseSensors();
            }

            CalculateMaps();

            OnPulsed?.Invoke();
        }
        #endregion

        #region Internals
        bool isControlling => LocomotionMode != LocomotionMode.None;
        
        ObservableEffect gridConfigEffect;

        DirectionalGrid interestMap;
        DirectionalGrid avoidMap;
        DirectionalGrid mergedMap;
        DirectionalGrid interpolatedMap;
        
        void CalculateMaps() {
            Seek.SetInterest(gameObject, interestMap);
            Avoid.SetAvoid(avoidMap);

            mergedMap.Copy(interestMap);

            var nCells = mergedMap.CellCount;
            for (int i = 0; i < nCells; i++) {
                var seekDistance = interestMap.GetCell(i).Value;
                var avoid = avoidMap.GetCell(i).Value;
                if (avoid == 0) {
                    continue;
                }
                var obstacleDistance = (Avoid.DesiredDistance - avoid);
                if (seekDistance < obstacleDistance) {
                    avoidMap.GetCell(i).SetValue(0);
                }
            }

            var minAvoid = avoidMap.GetMinCell().VectorValue;
            mergedMap.MaskUnder(avoidMap, minAvoid.magnitude);

            if (Application.isPlaying && PulseMode != PulseRoutine.Modes.Manual) {
                interpolatedMap.InterpolateTo(mergedMap, pulseRoutine.dt * InterpolationSpeed);
            } else {
                interpolatedMap.Copy(mergedMap);
            }
        }

        void Awake() {
            if (isSpherical == null) {
                isSpherical = new ObservableBool();
            }
            if (resolution == null) {
                resolution = new ObservableInt() { Value = 3 };
            }
            gridConfigEffect = ObservableEffect.Create(GridConfigChangeHandler, new ObservableBase[] { isSpherical, resolution });

            if (pulseRoutine == null) {
                pulseRoutine = new PulseRoutine();
            }
            pulseRoutine.Awake(this);
        }

        void OnEnable() {
            pulseRoutine.OnEnable();
        }

        void OnDisable() {
            pulseRoutine.OnDisable();
        }

        void OnDestroy() {
            gridConfigEffect.Dispose();
        }

        void OnValidate() {
            isSpherical?.OnValidate();
            resolution?.OnValidate();
            pulseRoutine?.OnValidate();
        }

        void Update() {
            if (LocomotionMode == LocomotionMode.UnityCharacterController) {
                Locomotion.CharacterSeek(CharacterController, transform.position + GetSteeringVector(), Vector3.up);
            }
        }

        void FixedUpdate() {
            if (LocomotionMode == LocomotionMode.RigidBodyFlying) {
                Locomotion.FlyableSeek(RigidBody, transform.position + GetSteeringVector());
            } else if (LocomotionMode == LocomotionMode.RigidBodyCharacter) {
                Locomotion.CharacterSeek(RigidBody, transform.position + GetSteeringVector(), Vector3.up);
            }
        }

        void GridConfigChangeHandler() {
            if (IsSpherical) {
                interestMap = new SphereGrid(Resolution);
                avoidMap = new SphereGrid(Resolution);
                mergedMap = new SphereGrid(Resolution);
                interpolatedMap = new SphereGrid(Resolution);
            } else {
                interestMap = new CircleGrid(Vector3.up, Resolution * 4);
                avoidMap = new CircleGrid(Vector3.up, Resolution * 4);
                mergedMap = new CircleGrid(Vector3.up, Resolution * 4);
                interpolatedMap = new CircleGrid(Vector3.up, Resolution * 4);
            }
        }

        void OnDrawGizmosSelected() {
            if (!ShowDetectionGizmos) {
                return;
            }

            Gizmos.color = Color.yellow;
            interpolatedMap?.DrawGizmos(transform.position, 1, 1f / (transform.position - Seek.Destination).magnitude);

            Gizmos.color = Color.red;
            avoidMap?.DrawGizmos(transform.position, 2, 1f / Avoid.DesiredDistance);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + GetSteeringVector());

            Avoid.DrawGizmos();
        }
        #endregion
    }
}
 