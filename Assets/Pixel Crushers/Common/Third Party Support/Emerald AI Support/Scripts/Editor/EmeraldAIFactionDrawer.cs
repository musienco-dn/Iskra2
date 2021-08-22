// Copyright © Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PixelCrushers.EmeraldAISupport
{

    [CustomPropertyDrawer(typeof(EmeraldAIFactionAttribute))]
    public class EmeraldAIFactionDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (EmeraldAI.EmeraldAISystem.StringFactionList == null) EmeraldAI.EmeraldAISystem.StringFactionList = new List<string>();
            if (EmeraldAI.EmeraldAISystem.StringFactionList.Count == 0) RefreshFactionList();

            var refreshWidth = 56f;
            var popupRect = new Rect(position.x, position.y, position.width - refreshWidth, position.height);
            var refreshRect = new Rect(position.x + position.width - refreshWidth, position.y, refreshWidth, position.height);
            property.intValue = EditorGUI.Popup(popupRect, label.text, property.intValue, EmeraldAI.EmeraldAISystem.StringFactionList.ToArray());
            if (GUI.Button(refreshRect, "Refresh")) RefreshFactionList();
        }

        private void RefreshFactionList()
        {
            var factionData = Resources.Load("EmeraldAIFactions") as TextAsset;
            if (factionData != null)
            {
                foreach (var s in factionData.text.Split(','))
                {
                    if (!EmeraldAI.EmeraldAISystem.StringFactionList.Contains(s) && !string.IsNullOrEmpty(s))
                    {
                        EmeraldAI.EmeraldAISystem.StringFactionList.Add(s);
                    }
                }
            }
        }
    }
}