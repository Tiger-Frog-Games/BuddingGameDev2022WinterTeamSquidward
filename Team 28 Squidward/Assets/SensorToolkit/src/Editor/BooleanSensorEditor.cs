using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Micosmo.SensorToolkit.Editors {

    [CustomEditor(typeof(BooleanSensor))]
    [CanEditMultipleObjects]
    public class BooleanSensorEditor : BaseSensorEditor<BooleanSensor> {
        SerializedProperty inputSensors;
        SerializedProperty operation;
        SerializedProperty onDetected;
        SerializedProperty onLostDetection;

        bool showEvents = false;

        protected override bool canTest { get { return true; } }

        protected override void OnEnable() {
            base.OnEnable();

            if (serializedObject == null) {
                return;
            }

            inputSensors = serializedObject.FindProperty("InputSensors");
            operation = serializedObject.FindProperty("operation");
            onDetected = serializedObject.FindProperty("OnDetected");
            onLostDetection = serializedObject.FindProperty("OnLostDetection");
        }

        protected override void InspectorParameters() {
            EditorGUILayout.PropertyField(inputSensors);
            EditorGUILayout.PropertyField(operation);

            EditorGUILayout.Space();

            if (showEvents = EditorGUILayout.Foldout(showEvents, "Events")) {
                EditorGUILayout.PropertyField(onDetected);
                EditorGUILayout.PropertyField(onLostDetection);
            }
        }
    }

}