/**
Unity Generic Serial Controller

Author: Brandon Matthews
2018
 */

#if UNITY_EDITOR && UNITY_STANDALONE_WIN
using UnityEngine;
using System.Collections;

using UnityEditor;

namespace Connectivity
{
    [CustomEditor(typeof(SerialConnection))]
    public class SerialConnectionEditor : Editor
    {
        public override void OnInspectorGUI()
        {

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            SerialConnection serialConn = (SerialConnection)target;

            if (serialConn.isOpen)
            {
                if (GUILayout.Button("Close Port"))
                {
                    serialConn.Close();
                }
            }
            else
            {
                if (GUILayout.Button("Open Port"))
                {
                    serialConn.Open();
                }

            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            DrawDefaultInspector();


        }
    }
}
#endif