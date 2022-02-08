using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Micosmo.SensorToolkit.Editors {

    [CustomEditor(typeof(SteeringSensor))]
    [CanEditMultipleObjects]
    public class SteeringSensorEditor : BasePulsableEditor<SteeringSensor> {
        SerializedProperty isSpherical;
        SerializedProperty resolution;
        SerializedProperty interpolationSpeed;
        SerializedProperty seek;
        SerializedProperty avoid;
        SerializedProperty pulseMode;
        SerializedProperty pulseInterval;
        SerializedProperty locomotionMode;
        SerializedProperty rigidBody;
        SerializedProperty characterController;
        SerializedProperty locomotion;

        protected SteeringSensor sensor;

        protected override bool canTest => true;

        protected override void OnEnable() {
            base.OnEnable();

            if (serializedObject == null) return;
            sensor = serializedObject.targetObject as SteeringSensor;

            isSpherical = serializedObject.FindProperty("isSpherical");
            resolution = serializedObject.FindProperty("resolution");
            interpolationSpeed = serializedObject.FindProperty("InterpolationSpeed");
            seek = serializedObject.FindProperty("Seek");
            avoid = serializedObject.FindProperty("Avoid");
            pulseMode = serializedObject.FindProperty("pulseRoutine.Mode");
            pulseInterval = serializedObject.FindProperty("pulseRoutine.Interval");
            locomotionMode = serializedObject.FindProperty("LocomotionMode");
            rigidBody = serializedObject.FindProperty("RigidBody");
            characterController = serializedObject.FindProperty("CharacterController");
            locomotion = serializedObject.FindProperty("Locomotion");
        }

        protected override void OnPulsableGUI() {
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(isSpherical);
            EditorGUILayout.PropertyField(resolution);
            EditorGUILayout.PropertyField(pulseMode, new GUIContent("Pulse Mode"));
            if (sensor.PulseMode == PulseRoutine.Modes.FixedInterval) {
                EditorGUILayout.PropertyField(pulseInterval, new GUIContent("Pulse Interval"));
            }
            if (sensor.PulseMode != PulseRoutine.Modes.Manual) {
                EditorGUILayout.PropertyField(interpolationSpeed);
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(seek, true);
            EditorGUILayout.PropertyField(avoid, true);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(locomotionMode);

            var locmode = (LocomotionMode)locomotionMode.enumValueIndex;

            if (locmode != LocomotionMode.None) {
                if (locmode == LocomotionMode.RigidBodyFlying || locmode == LocomotionMode.RigidBodyCharacter) {
                    EditorGUILayout.PropertyField(rigidBody);
                } else {
                    EditorGUILayout.PropertyField(characterController);
                }
                EditorGUILayout.PropertyField(locomotion, true);
            }

            if (EditorGUI.EndChangeCheck()) {
                EditorState.StopAllTesting();
            }

            displayErrors();

            serializedObject.ApplyModifiedProperties();
        }

        void displayErrors() {
            
        }
    }

}