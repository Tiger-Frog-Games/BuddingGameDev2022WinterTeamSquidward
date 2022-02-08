using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Micosmo.SensorToolkit.Editors {
    [CustomEditor(typeof(RangeSensor2D))]
    [CanEditMultipleObjects]
    public class RangeSensor2DEditor : BaseSensorEditor<RangeSensor2D> {
        SerializedProperty shape;
        SerializedProperty circle;
        SerializedProperty box;
        SerializedProperty capsule;
        SerializedProperty ignoreList;
        SerializedProperty tagFilterEnabled;
        SerializedProperty tagFilter;
        SerializedProperty detectsOnLayers;
        SerializedProperty pulseMode;
        SerializedProperty pulseInterval;
        SerializedProperty detectionMode;
        SerializedProperty ignoreTriggerColliders;
        SerializedProperty onDetected;
        SerializedProperty onLostDetection;

        bool showEvents = false;

        protected override bool canTest { get { return true; } }

        protected override void OnEnable() {
            base.OnEnable();

            if (serializedObject == null) {
                return;
            }

            shape = serializedObject.FindProperty("Shape");
            circle = serializedObject.FindProperty("Circle");
            box = serializedObject.FindProperty("Box");
            capsule = serializedObject.FindProperty("Capsule");
            ignoreList = serializedObject.FindProperty("signalFilter.IgnoreList");
            tagFilterEnabled = serializedObject.FindProperty("signalFilter.EnableTagFilter");
            tagFilter = serializedObject.FindProperty("signalFilter.AllowedTags");
            detectsOnLayers = serializedObject.FindProperty("DetectsOnLayers");
            pulseMode = serializedObject.FindProperty("pulseRoutine.Mode");
            pulseInterval = serializedObject.FindProperty("pulseRoutine.Interval");
            detectionMode = serializedObject.FindProperty("DetectionMode");
            ignoreTriggerColliders = serializedObject.FindProperty("IgnoreTriggerColliders");
            onDetected = serializedObject.FindProperty("OnDetected");
            onLostDetection = serializedObject.FindProperty("OnLostDetection");
        }

        protected override void InspectorParameters() {
            EditorGUILayout.PropertyField(shape);
            if (sensor.Shape == RangeSensor2D.Shapes.Circle) {
                EditorUtils.InlinePropertyField(circle);
            } else if (sensor.Shape == RangeSensor2D.Shapes.Box) {
                EditorUtils.InlinePropertyField(box);
            } else if (sensor.Shape == RangeSensor2D.Shapes.Capsule) {
                EditorUtils.InlinePropertyField(capsule);
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(ignoreList, true);
            EditorGUILayout.PropertyField(tagFilterEnabled);
            if (tagFilterEnabled.boolValue) {
                EditorGUILayout.PropertyField(tagFilter, true);
            }
            EditorGUILayout.PropertyField(detectsOnLayers);
            EditorGUILayout.PropertyField(detectionMode);
            EditorGUILayout.PropertyField(ignoreTriggerColliders);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(pulseMode, new GUIContent("Pulse Mode"));
            if (sensor.PulseMode == PulseRoutine.Modes.FixedInterval) {
                EditorGUILayout.PropertyField(pulseInterval, new GUIContent("Pulse Interval"));
            }

            EditorGUILayout.Space();

            if (showEvents = EditorGUILayout.Foldout(showEvents, "Events")) {
                EditorGUILayout.PropertyField(onDetected);
                EditorGUILayout.PropertyField(onLostDetection);
            }

            EditorGUILayout.Space();

            BufferSizeInfo(sensor.CurrentBufferSize);
        }
    }
}