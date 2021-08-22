using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.AnimatedValues;

namespace SpawningFramework
{
    [CustomEditor(typeof(Wave))]

    public class WaveEditorWindow : EditorWindow
    {
        #region Variables
        public static bool drawingWindow;

        WaveGroup linkedGroup;
        static Wave current;

        Vector2 instructionScrolling;

        bool hideAll = true, hideType = true, hideDuration = true, hideDifficulty = true, hideInterval = true, hideSpawnPercentage = true, hideCustom = true, hideNoRepeat = true;

        GUIStyle centeredLabel, heading;

        Editor instructionEditor;

        float inspectorSpacing = 200;

        static Editor[] editors;

        static AnimBool propertiesDropDown, controlsDropDown, preferencesDropDown;
        #endregion

        [MenuItem("Window/Wave Editor")]
        static void Init()
        {
            WaveEditorWindow window = (WaveEditorWindow)EditorWindow.GetWindow(typeof(WaveEditorWindow));
            window.Show();
        }

        public void OnGUI()
        {
            drawingWindow = true;

            #region Properties
            if(propertiesDropDown == null)
            {
                propertiesDropDown = new AnimBool();
                controlsDropDown = new AnimBool();
                preferencesDropDown = new AnimBool();
            }
            #endregion

            #region Styles
            //if (centeredLabel == null || heading == null)
            //{
            heading = EditorStyles.boldLabel;
            heading.alignment = TextAnchor.UpperCenter;
            //heading.fontSize = 32;//this seems to break the editor

            if(EditorGUIUtility.isProSkin)
                heading.normal.textColor = new Color(205, 205, 205);

            centeredLabel = EditorStyles.boldLabel;
            centeredLabel.alignment = TextAnchor.UpperCenter;
            //centeredLabel.fontSize = 8;

            if(EditorGUIUtility.isProSkin)
                centeredLabel.normal.textColor = new Color(205, 205, 205);
            //}
            #endregion

            #region Find Wave
            if(Selection.activeGameObject != null)
            {
                Wave temp = Selection.activeGameObject.GetComponent<Wave>();

                if(temp != current || current == null)
                {
                    current = temp;
                    editors = null;//force new editors for spawn instructions as well
                }
            }

            if(current == null)
            {
                GUILayout.Label("Couldn't find wave! Please select one to edit its properties", centeredLabel);
                return;
            }

            if(editors == null)
            {
                if(current.spawnInstructions == null)
                    current.spawnInstructions = new SpawnInstruction[0];

                editors = new Editor[current.spawnInstructions.Length];

                for(int i = 0; i < editors.Length; i++)
                    if(current.spawnInstructions[i] != null)
                        editors[i] = Editor.CreateEditor(current.spawnInstructions[i], typeof(SpawnInstructionEditor));
            }
            #endregion

            #region Heading
            EditorGUILayout.LabelField(current.gameObject.name, heading);
            #endregion

            #region Properties
            FindGroup();

            if(linkedGroup == null)
            {
                EditorGUILayout.LabelField("Failed to find WaveGroup!");
                EditorGUILayout.LabelField("Make sure the componenet is attached to either this object or its parent");
            }

            EditorGUILayout.BeginHorizontal();

            using(new Vertical(GUILayout.Width(position.width / 3)))
            {
                //EditorGUILayout.LabelField("Properties", centeredLabel, GUILayout.Width(position.width / 3));

                using(new Horizontal(GUILayout.Width(position.width / 3)))
                {
                    GUILayout.FlexibleSpace();
                    DrawToggle(propertiesDropDown, new GUIContent("Properties"));
                    GUILayout.FlexibleSpace();
                }

                if(EditorGUILayout.BeginFadeGroup(propertiesDropDown.faded))
                {
                    using(new FixedWidthLabel(new GUIContent("Wave Spawn Type", "Determines if waves should spawn randomly or in sequence"), GUILayout.Width(position.width / 3)))
                        current.type = (WaveType.Type)EditorGUILayout.EnumPopup(current.type);

                    if(current.type == WaveType.Type.SpawnRandomly || current.type == WaveType.Type.SpawnSimultaneously)
                    {
                        if(current.totalWaveTime == float.MaxValue)
                            current.totalWaveTime = 0;//just for neatness. Looks messy displaying the max value

                        current.totalWaveTime = EditorGUILayout.FloatField(new GUIContent("Total Run Time", "How long to run this wave for"), current.totalWaveTime);
                    }
                    else
                        current.totalWaveTime = float.MaxValue;//basically for sequential waves run them until told otherwise

                    if(linkedGroup != null)
                    {
                        if(linkedGroup.type == RestrictedType.Type.SpawnRandomly)//only show this option if the group is spawning waves randomly
                            current.noRepeatInterval = EditorGUILayout.FloatField(new GUIContent("No Repeat Interval", "Determines if waves should spawn randomly or in sequence"), current.noRepeatInterval);
                    }

                    current.paddingTime = EditorGUILayout.FloatField(new GUIContent("Padding Time", "This time is used to create a pause in game play before spawning the next block.\nE.G it can be useful if you have spawned a lot of enemies to have some padding so the player has time to kill them"), current.paddingTime);

                    if(linkedGroup.type == RestrictedType.Type.SpawnRandomly)
                        current.spawnChance = EditorGUILayout.IntField(new GUIContent("Spawn Chance", "What chance this wave has of being spawned compared to other waves"), current.spawnChance);
                }

                EditorGUILayout.EndFadeGroup();
            }
            #endregion

            GUILayout.FlexibleSpace();

            #region Controls Panel
            using(new Vertical(GUILayout.Width(position.width / 3)))
            {
                using(new Horizontal(GUILayout.Width(position.width / 3)))
                {
                    GUILayout.FlexibleSpace();
                    DrawToggle(controlsDropDown, new GUIContent("Controls"));
                    GUILayout.FlexibleSpace();
                }

                if(EditorGUILayout.BeginFadeGroup(controlsDropDown.faded))
                {
                    #region All
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    if(GUILayout.Button(new GUIContent(hideAll ? "Collapse All" : "Expand All", "Filters the spawn instructions to view this value or not"), GUILayout.Width(position.width / 4)))
                    {
                        hideAll = !hideAll;

                        SpawnInstructionEditor currentEditor;

                        for(int i = 0; i < editors.Length; i++)
                        {
                            currentEditor = (SpawnInstructionEditor)editors[i];

                            currentEditor.instructionDurationAnimation.target = hideAll;
                            currentEditor.spawnIntervalAnimation.target = hideAll;
                            currentEditor.customValueAnimation.target = hideAll;
                            currentEditor.spawnPercentageAnimation.target = hideAll;
                            currentEditor.noRepeatAnimation.target = hideAll;
                            currentEditor.enemyTypeAnimation.target = hideAll;
                            currentEditor.expandAll = hideAll;
                        }

                        hideType = hideAll;
                        hideDuration = hideAll;
                        hideDifficulty = hideAll;
                        hideInterval = hideAll;
                        hideSpawnPercentage = hideAll;
                    }

                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    #endregion

                    #region Type
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    if(GUILayout.Button(new GUIContent(hideType ? "Collapse Type" : "Expand Type", "Filters the spawn instructions to view this value or not"), GUILayout.Width(position.width / 4)))
                    {
                        hideType = !hideType;

                        for(int i = 0; i < editors.Length; i++)
                            ((SpawnInstructionEditor)editors[i]).enemyTypeAnimation.target = hideType;
                    }

                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    #endregion

                    #region Duration
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    if(current.type != WaveType.Type.SpawnSimultaneously)//simulataneous instructions dont care about the duration
                    {
                        if(GUILayout.Button(new GUIContent(hideDuration ? "Collapse Duration" : "Expand Duration", "Filters the spawn instructions to view this value or not"), GUILayout.Width(position.width / 4)))
                        {
                            hideDuration = !hideDuration;

                            for(int i = 0; i < editors.Length; i++)
                                ((SpawnInstructionEditor)editors[i]).instructionDurationAnimation.target = hideDuration;
                        }
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    #endregion

                    #region Interval
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    if(GUILayout.Button(new GUIContent(hideInterval ? "Collapse Interval" : "Expand Interval", "Filters the spawn instructions to view this value or not"), GUILayout.Width(position.width / 4)))
                    {
                        hideInterval = !hideInterval;

                        for(int i = 0; i < editors.Length; i++)
                            ((SpawnInstructionEditor)editors[i]).spawnIntervalAnimation.target = hideInterval;
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    #endregion

                    #region Spawn Percentage
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    if(current.type == WaveType.Type.SpawnRandomly)
                        if(GUILayout.Button(new GUIContent(hideDifficulty ? "Collapse Spawn Percentage" : "Expand Spawn Percentage", "Filters the spawn instructions to view this value or not"), GUILayout.Width(position.width / 4)))
                        {
                            hideSpawnPercentage = !hideSpawnPercentage;

                            for(int i = 0; i < editors.Length; i++)
                                ((SpawnInstructionEditor)editors[i]).spawnPercentageAnimation.target = hideSpawnPercentage;
                        }

                    GUILayout.FlexibleSpace();//this helps center objects
                    GUILayout.EndHorizontal();
                    #endregion

                    #region Custom
                    if(SpawningPreferences.ShowCustomValues)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();

                        if(GUILayout.Button(new GUIContent(hideCustom ? "Collapse Custom" : "Expand Custom", "Filters the spawn instructions to view this value or not"), GUILayout.Width(position.width / 4)))
                        {
                            hideCustom = !hideCustom;

                            for(int i = 0; i < editors.Length; i++)
                                ((SpawnInstructionEditor)editors[i]).customValueAnimation.target = hideCustom;
                        }

                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                    #endregion

                    #region No Repeat Interval
                    if(current.type == WaveType.Type.SpawnRandomly)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();

                        if(GUILayout.Button(new GUIContent(hideNoRepeat ? "Collapse No Repeat" : "Expand No Repeat", "Filters the spawn instructions to view this value or not"), GUILayout.Width(position.width / 4)))
                        {
                            hideNoRepeat = !hideNoRepeat;

                            for(int i = 0; i < editors.Length; i++)
                                ((SpawnInstructionEditor)editors[i]).noRepeatAnimation.target = hideNoRepeat;
                        }

                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                    #endregion
                }

                EditorGUILayout.EndFadeGroup();
            }
            #endregion

            #region Preferences Panel
            SpawningPreferences.Load();

            using(new Vertical(GUILayout.Width(position.width / 3)))//this is basically just EditorGUILayout.BeginVerticle but using braces {} instead. Makes things easier to understand
            {
                using(new Horizontal(GUILayout.Width(position.width / 3)))
                {
                    GUILayout.FlexibleSpace();
                    DrawToggle(preferencesDropDown, new GUIContent("Preferences"));
                    GUILayout.FlexibleSpace();
                }

                if(EditorGUILayout.BeginFadeGroup(preferencesDropDown.faded))
                {
                    using(new Horizontal())
                    using(new FixedWidthLabel(new GUIContent("Window Layout", "How should the window be organised")))
                        SpawningPreferences.WindowLayout = (SpawningPreferences.Orientation)EditorGUILayout.EnumPopup(SpawningPreferences.WindowLayout, GUILayout.Width(position.width / 3 - (GUI.skin.label.CalcSize(new GUIContent("Window Layout")).x + 25)));

                    #region Window Spacing
                    using(new Horizontal())
                    {
                        EditorGUILayout.LabelField(new GUIContent("Window Spacing", "The spacing for each SpawnInstruction displayed below"), GUILayout.Width(GUI.skin.label.CalcSize(new GUIContent("Window Spacing")).x));

                        inspectorSpacing = SpawningPreferences.WindowSpacing;//important to read this incase of a load
                        inspectorSpacing = GUILayout.HorizontalSlider(inspectorSpacing, 150, 300, GUILayout.Width(position.width / 3 - (GUI.skin.label.CalcSize(new GUIContent("Window Spacing")).x + 25)));
                        SpawningPreferences.WindowSpacing = inspectorSpacing;

                        GUILayout.Space(25);
                    }
                    #endregion

                    #region Show Custom Values
                    using(new Horizontal(GUILayout.Width(position.width / 3)))
                    {
                        GUILayout.Label(new GUIContent("Show Custom Values", "Should custom values be displayed or not"));
                        GUILayout.FlexibleSpace();
                        SpawningPreferences.ShowCustomValues = GUILayout.Toggle(SpawningPreferences.ShowCustomValues, GUIContent.none);
                        GUILayout.FlexibleSpace();
                    }
                    #endregion

                    #region Show Summaries
                    using(new Horizontal(GUILayout.Width(position.width / 3)))
                    {
                        GUILayout.Label(new GUIContent("Show Summaries:", "Should instruction summaries be displayed or not"));
                        GUILayout.FlexibleSpace();
                    }

                    using(new Horizontal(GUILayout.Width(position.width / 3)))
                    {
                        using(new FixedWidthLabel(new GUIContent("In Window")))
                            SpawningPreferences.ShowSummariesInWindow = GUILayout.Toggle(SpawningPreferences.ShowSummariesInWindow, GUIContent.none);

                        using(new FixedWidthLabel(new GUIContent("In Editor")))
                            SpawningPreferences.ShowSummariesInEditor = GUILayout.Toggle(SpawningPreferences.ShowSummariesInEditor, GUIContent.none);

                        GUILayout.FlexibleSpace();
                    }
                    #endregion
                }

                EditorGUILayout.EndFadeGroup();
            }

            SpawningPreferences.Save();
            GUILayout.FlexibleSpace();
            #endregion

            EditorGUILayout.EndHorizontal();//Top layer

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Spawn Instructions", centeredLabel);

            #region Spawn Instructions
            instructionScrolling = EditorGUILayout.BeginScrollView(instructionScrolling);

            #region Verticle
            if(SpawningPreferences.WindowLayout == SpawningPreferences.Orientation.Verticle)
            {
                //GUILayout.FlexibleSpace();

                EditorGUILayout.BeginHorizontal();

                for(int i = 0; i < current.spawnInstructions.Length; i++)
                {
                    GUILayout.Space(5);

                    EditorGUILayout.BeginVertical(GUILayout.Width(SpawningPreferences.WindowSpacing));

                    if(editors != null && editors[i] != null)
                        editors[i].OnInspectorGUI();
                    else
                        EditorGUILayout.LabelField("null editor found!!");

                    EditorGUILayout.EndVertical();
                }

                #region Add New Instruction
                using(new Vertical(GUILayout.Width(SpawningPreferences.WindowSpacing)))
                {
                    if(GUILayout.Button(new GUIContent("Add Instruction", "Remove a spawn instruction from this waves list of spawn instructions"), GUILayout.Width(SpawningPreferences.WindowSpacing)))
                    {
                        SpawnInstruction temp = current.gameObject.AddComponent<SpawnInstruction>();

                        System.Array.Resize<SpawnInstruction>(ref current.spawnInstructions, current.spawnInstructions.Length + 1);
                        System.Array.Resize<Editor>(ref editors, editors.Length + 1);

                        current.spawnInstructions[current.spawnInstructions.Length - 1] = temp;//the array has been expanded but we need a new instance of the actual instructions
                        editors[editors.Length - 1] = Editor.CreateEditor(temp, typeof(SpawnInstructionEditor));
                    }
                }
                #endregion

                EditorGUILayout.EndHorizontal();
            }
            #endregion

            else

            #region Horizontal
            {
                EditorGUILayout.BeginVertical(GUILayout.Height(SpawningPreferences.WindowSpacing));

                for(int i = 0; i < current.spawnInstructions.Length; i++)
                {
                    GUILayout.Space(10);

                    if(editors != null && editors[i] != null)
                        editors[i].OnInspectorGUI();
                    else
                        EditorGUILayout.LabelField("null editor found!!");
                }

                #region Add New Instruction
                GUILayout.Space(10);

                using(new Vertical(GUILayout.Width(SpawningPreferences.WindowSpacing)))
                {
                    if(GUILayout.Button(new GUIContent("Add Instruction", "Remove a spawn instruction from this waves list of spawn instructions"), GUILayout.Width(SpawningPreferences.WindowSpacing)))
                    {
                        SpawnInstruction temp = current.gameObject.AddComponent<SpawnInstruction>();

                        System.Array.Resize<SpawnInstruction>(ref current.spawnInstructions, current.spawnInstructions.Length + 1);
                        System.Array.Resize<Editor>(ref editors, editors.Length + 1);

                        current.spawnInstructions[current.spawnInstructions.Length - 1] = temp;//the array has been expanded but we need a new instance of the actual instructions
                        editors[editors.Length - 1] = Editor.CreateEditor(temp, typeof(SpawnInstructionEditor));
                    }
                }
                #endregion
                EditorGUILayout.EndVertical();
            }
            #endregion

            EditorGUILayout.EndScrollView();

            Undo.RecordObject(current, "Wave Proprties");//let users undo changes as needed
            #endregion

            if(GUI.changed)
                EditorUtility.SetDirty(current);

            drawingWindow = false;
        }

        /// <summary>
        /// Attempts to find a wave group compenent either on ths same gameobject or its parent. Edit this is your hierarchy requires it to be somewhere else, or delete this script all together
        /// </summary>
        void FindGroup()
        {
            if(linkedGroup == null)
            {
                linkedGroup = current.gameObject.GetComponent<WaveGroup>();

                if(linkedGroup == null && current.transform.parent != null)
                    linkedGroup = current.gameObject.transform.parent.GetComponent<WaveGroup>();
            }
        }

        void Update()
        {
            Repaint();//repaint each frame to help with expanding and collapsing animations
        }

        public static bool RemoveInstruction(SpawnInstructionEditor editor)
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            if(GUILayout.Button(new GUIContent("Remove Instruction", "Remove a spawn instruction from this waves list of spawn instructions"), GUILayout.Width(editor.spacing)))
            {
                for(int i = 0; i < editors.Length; i++)
                    if(editor.Equals(editors[i]))
                    {
                        DestroyImmediate(editor.target);//destroy the actual object in the scene

                        if(i < editors.Length - 1)//if this isnt the last instruction
                        {
                            current.spawnInstructions[i] = current.spawnInstructions[current.spawnInstructions.Length - 1];//swap with the last instruction
                            editors[i] = editors[editors.Length - 1];//and the editor
                            editors[i].serializedObject.Update();
                            break;
                        }
                    }

                System.Array.Resize<SpawnInstruction>(ref current.spawnInstructions, current.spawnInstructions.Length - 1);//now resize the array to match
                System.Array.Resize<Editor>(ref editors, editors.Length - 1);

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                return true;
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            return false;
        }

        void DrawToggle(AnimBool animation, GUIContent content)
        {
            GUI.contentColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.85f);//change the colour of the heading to get it to stand out

            if(!GUILayout.Toggle(true, content, "PreToolbar2", GUILayout.MinWidth(20f)))
                animation.target = !animation.target;//invert

            GUI.contentColor = Color.white;
        }
    }
}