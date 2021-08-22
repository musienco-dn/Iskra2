using UnityEngine;
using System.Collections;
using UnityEditor;

namespace SpawningFramework
{
    public class SpawningPreferences
    {
        public enum Orientation { Verticle = 0, Horizontal }

        public static Orientation WindowLayout = Orientation.Horizontal;
        public static float WindowSpacing;
        public static bool ShowCustomValues = false, ShowSummariesInWindow = true, ShowSummariesInEditor = false;

        static bool loaded;

        public static void Load()
        {
            if(!loaded)
            {
                WindowLayout = (Orientation)EditorPrefs.GetInt("WindowLayout");

                WindowSpacing = EditorPrefs.GetFloat("WindowSpacing");

                if(WindowSpacing < 150)
                    WindowSpacing = 150;

                ShowCustomValues = EditorPrefs.GetInt("ShowCustomValues") == 1;
                ShowSummariesInWindow = EditorPrefs.GetInt("ShowSummariesInWindow") == 1;
                ShowSummariesInEditor = EditorPrefs.GetInt("ShowSummariesInEditor") == 1;

                loaded = true;
            }
        }

        public static void Save()
        {
            EditorPrefs.SetInt("WindowLayout", (int)WindowLayout);

            EditorPrefs.SetFloat("WindowSpacing", WindowSpacing);

            EditorPrefs.SetInt("ShowCustomValues", ShowCustomValues ? 1 : 0);
            EditorPrefs.SetInt("ShowSummariesInWindow", ShowSummariesInWindow ? 1 : 0);
            EditorPrefs.SetInt("ShowSummariesInEditor", ShowSummariesInEditor ? 1 : 0);
        }
    }
}