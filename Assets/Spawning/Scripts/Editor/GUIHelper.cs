using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

namespace SpawningFramework
{
    public class GUIHelper
    {

        public static void ArrayGUI(SerializedObject instance, string name)
        {
            ArrayGUI(instance, instance.FindProperty(name));
        }

        public static void ArrayGUI(SerializedObject instance, SerializedProperty array)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(array, true);

            if(EditorGUI.EndChangeCheck())
                instance.ApplyModifiedProperties();

            EditorGUIUtility.LookLikeControls();
        }
    }

    /// <summary>
    /// A helper to make horizontal groups easier
    /// </summary>
    public class Horizontal : IDisposable
    {
        public Horizontal()
        {
            EditorGUILayout.BeginHorizontal();
        }

        public Horizontal(params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal(options);
        }

        public void Dispose()
        {
            EditorGUILayout.EndHorizontal();
        }
    }

    /// <summary>
    /// A helper to make vertical groups easier
    /// </summary>
    public class Vertical : IDisposable
    {
        public Vertical()
        {
            EditorGUILayout.BeginVertical();
        }

        public Vertical(params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(options);
        }

        public void Dispose()
        {
            EditorGUILayout.EndVertical();
        }
    }

    //FixedWidthLabel class. Extends IDisposable, so that it can be used with the "using" keyword.
    public class FixedWidthLabel : IDisposable
    {
        private readonly ZeroIndent indentReset; //helper class to reset and restore indentation

        public FixedWidthLabel(GUIContent label, params GUILayoutOption[] options)//	constructor.
        {//						state changes are applied here.
            EditorGUILayout.BeginHorizontal(options);// create a new horizontal group

            EditorGUILayout.LabelField(label,
                GUILayout.Width(GUI.skin.label.CalcSize(label).x +// actual label width
                    9 * EditorGUI.indentLevel));//indentation from the left side. It's 9 pixels per indent level

            indentReset = new ZeroIndent();//helper class to have no indentation after the label
        }

        public FixedWidthLabel(string label)
            : this(new GUIContent(label))//alternative constructor, if we don't want to deal with GUIContents
        {
        }

        public void Dispose() //restore GUI state
        {
            indentReset.Dispose();//restore indentation
            EditorGUILayout.EndHorizontal();//finish horizontal group
        }
    }

    class ZeroIndent : IDisposable //helper class to clear indentation
    {
        private readonly int originalIndent;//the original indentation value before we change the GUI state
        public ZeroIndent()
        {
            originalIndent = EditorGUI.indentLevel;//save original indentation
            EditorGUI.indentLevel = 0;//clear indentation
        }

        public void Dispose()
        {
            EditorGUI.indentLevel = originalIndent;//restore original indentation
        }
    }
}
