using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditor.AnimatedValues;
using System.Text;

namespace SpawningFramework
{
    [CustomEditor(typeof(SpawnInstruction))]

    public class SpawnInstructionEditor : Editor
    {
        #region Variables
        SerializedProperty enemyType,

         minSpawnPercentage, maxSpawnPercentage,

         minSpawnInterval, maxSpawnInterval,
         minSpawnIntervalDeviation, maxSpawnIntervalDeviation,

         minCustomValue, maxCustomValue,
         minCustomValueDeviation, maxCustomValueDeviation,

         minInstructionDuration, maxInstructionDuration,
         minDurationDeviation, maxDurationDeviation,

         noRepeatInterval;

        bool searchedForManager;
        SpawningManager linkedManager;//which manager this instruction belongs to
        Wave linkedWave;//which wave is this instruction part of. Important for determining if we show min or max chances

        public AnimBool customValueAnimation, spawnIntervalAnimation, spawnPercentageAnimation, instructionDurationAnimation, noRepeatAnimation, enemyTypeAnimation, animateAll;//toggle groups
        public bool expandAll = true;
        bool usingPlayerSkill;

        GUIStyle centeredLabel, rightJustified;

        bool instructionDestroyed;

        public float spacing;
        public SpawningPreferences.Orientation layout;
        bool showSummary;
        #endregion

        void OnEnable()
        {
            try
            {
                enemyType = serializedObject.FindProperty("type");

                minInstructionDuration = serializedObject.FindProperty("minInstructionDuration");
                maxInstructionDuration = serializedObject.FindProperty("maxInstructionDuration");
                minDurationDeviation = serializedObject.FindProperty("minDurationDeviation");
                maxDurationDeviation = serializedObject.FindProperty("maxDurationDeviation");

                minSpawnPercentage = serializedObject.FindProperty("minSpawnPercentage");
                maxSpawnPercentage = serializedObject.FindProperty("maxSpawnPercentage");


                minSpawnInterval = serializedObject.FindProperty("minSpawnInterval");
                maxSpawnInterval = serializedObject.FindProperty("maxSpawnInterval");
                minSpawnIntervalDeviation = serializedObject.FindProperty("minSpawnIntervalDeviation");
                maxSpawnIntervalDeviation = serializedObject.FindProperty("maxSpawnIntervalDeviation");


                minCustomValue = serializedObject.FindProperty("minCustomValue");
                maxCustomValue = serializedObject.FindProperty("maxCustomValue");
                minCustomValueDeviation = serializedObject.FindProperty("minCustomValueDeviation");
                maxCustomValueDeviation = serializedObject.FindProperty("maxCustomValueDeviation");

                noRepeatInterval = serializedObject.FindProperty("noRepeatInterval");

                FindLinkedWave();

                customValueAnimation = new AnimBool(true);
                spawnIntervalAnimation = new AnimBool(true);
                spawnPercentageAnimation = new AnimBool(true);
                instructionDurationAnimation = new AnimBool(true);
                animateAll = new AnimBool(true);
                noRepeatAnimation = new AnimBool(true);
                enemyTypeAnimation = new AnimBool(true);

                SpawningPreferences.Load();
            }
            catch(System.Exception e)
            {
                //prevents an annoying error from appearing in the inspector
            }
        }

        public override void OnInspectorGUI()
        {
            #region Styles and Layouts
            centeredLabel = EditorStyles.boldLabel;
            centeredLabel.alignment = TextAnchor.UpperCenter;

            rightJustified = GUI.skin.label;
            rightJustified.alignment = TextAnchor.UpperRight;

            spacing = SpawningPreferences.WindowSpacing;

            if(WaveEditorWindow.drawingWindow)
            {
                showSummary = SpawningPreferences.ShowSummariesInWindow;
                layout = SpawningPreferences.WindowLayout;
            }
            else
            {
                showSummary = SpawningPreferences.ShowSummariesInEditor;
                layout = SpawningPreferences.Orientation.Verticle;//force the editors to be verticle
            }

            spacing = Mathf.Clamp(spacing, 150, 300);
            #endregion

            usingPlayerSkill = UsingPlayerSkill();

            Repaint();//forces the editor to repaint every frame to give nice animations for drop down objects. 

            serializedObject.Update();

            FindLinkedWave();

            if(EditorGUILayout.BeginFadeGroup(animateAll.faded))//allows everything to expand and collapse as a group
            {
                #region Verticle
                if(layout == SpawningPreferences.Orientation.Verticle)
                {
                    DrawSummary();

                    using(new FixedWidthLabel(new GUIContent("Enemy Type", "Which enemy to spawn")))
                        EditorGUILayout.PropertyField(enemyType, GUIContent.none);

                    if(enemyType.intValue != (int)EnemyTypes.Type.Pause)//basically dont display anything for pauses
                    {
                        if(enemyType.intValue == (int)EnemyTypes.Type.Wait)
                        {
                            EditorGUILayout.PropertyField(minSpawnInterval, new GUIContent("Wait Time", "How long to wait before spawning the next block"));
                            minInstructionDuration.floatValue = minSpawnInterval.floatValue + 1;//basically ensure the block lasts as long as the instruction inside it. the wait will skip this time anyway

                            instructionDestroyed = WaveEditorWindow.RemoveInstruction(this);
                        }
                        else
                        {
                            if(enemyType.intValue > (int)EnemyTypes.Type.None)
                            {
                                if(linkedWave != null)
                                {
                                    if(usingPlayerSkill)
                                        DisplayPlayerSkillBasedInstructions();
                                    else
                                        DisplayStandardInstructions();
                                }
                                else//if there was no wave to read instruction types from
                                {
                                    GUILayout.Label("No wave found!\nMake sure Wave.cs is attached to either this gameobject or its parent\nAlso ensure the gameobject containing this wave is active\nIf you are using multiple Spawn Managers it might be easier to disable this script");

                                    DisplayPlayerSkillBasedInstructions();//then let the user edit everything anyway
                                }
                            }
                            else
                                #region Remove Instruction
                                instructionDestroyed = WaveEditorWindow.RemoveInstruction(this);
                                #endregion
                        }
                    }
                    else
                        instructionDestroyed = WaveEditorWindow.RemoveInstruction(this);

                    GUILayout.FlexibleSpace();
                }
                #endregion

                #region Horizontal
                else
                {
                    EditorGUILayout.BeginHorizontal();

                    DrawSummary();

                    #region Type
                    using(new Vertical(GUILayout.Width(spacing)))
                    {

                        DrawToggle(enemyTypeAnimation, new GUIContent(enemyTypeAnimation.value ? "Enemy Type" : ((EnemyTypes.Type)(enemyType.enumValueIndex)).ToString(), "Which enemy to spawn"));

                        if(EditorGUILayout.BeginFadeGroup(enemyTypeAnimation.faded))
                        {
                            EditorGUILayout.PropertyField(enemyType, GUIContent.none, GUILayout.Width(spacing));

                            ShowExpandCollapseAll();
                        }

                        EditorGUILayout.EndFadeGroup();
                    }
                    #endregion

                    if(enemyType.intValue != (int)EnemyTypes.Type.Pause)//basically dont display anything for pauses
                    {
                        #region Wait
                        if(enemyType.intValue == (int)EnemyTypes.Type.Wait)
                        {
                            EditorGUILayout.BeginVertical();

                            EditorGUILayout.LabelField(new GUIContent("Wait Time", "How long to wait before spawning the next block"), centeredLabel);

                            EditorGUILayout.PropertyField(minSpawnInterval, GUIContent.none);
                            minInstructionDuration.floatValue = minSpawnInterval.floatValue + 1;//basically ensure the block lasts as long as the instruction inside it. the wait will skip this time anyway

                            EditorGUILayout.EndVertical();

                            instructionDestroyed = WaveEditorWindow.RemoveInstruction(this);
                        }
                        #endregion

                        else
                        {
                            if(enemyType.intValue > (int)EnemyTypes.Type.None)
                            {
                                if(linkedWave != null)
                                {
                                    if(usingPlayerSkill)
                                        DisplayPlayerSkillBasedInstructions();
                                    else
                                        DisplayStandardInstructions();
                                }
                                else//if there was no wave to read instruction types from
                                {
                                    GUILayout.Label("No wave found!\nMake sure Wave.cs is attached to either this gameobject or its parent\nAlso ensure the gameobject containing this wave is active\nIf you are using multiple Spawn Managers it might be easier to disable this script");

                                    DisplayPlayerSkillBasedInstructions();//then let the user edit everything anyway
                                }
                            }
                            else
                                #region Remove Instruction
                                instructionDestroyed = WaveEditorWindow.RemoveInstruction(this);
                                #endregion
                        }
                    }
                    else
                        instructionDestroyed = WaveEditorWindow.RemoveInstruction(this);

                    EditorGUILayout.EndHorizontal();
                }
                #endregion
            }

            EditorGUILayout.EndFadeGroup();

            if(!instructionDestroyed)//when this is true then trying to run this line will result in an error on the destroyed object
                serializedObject.ApplyModifiedProperties();// Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
        }

        void DisplayPlayerSkillBasedInstructions()
        {
            #region Expand/Collapse All Button
            if(layout == SpawningPreferences.Orientation.Verticle)//only show in verticle view. Horizontal displays elsewhere
            {
                using(new Horizontal())
                {
                    GUILayout.FlexibleSpace();
                    ShowExpandCollapseAll();
                    GUILayout.FlexibleSpace();
                }
            }
            #endregion

            #region Duration
            if(linkedWave.type != WaveType.Type.SpawnSimultaneously)
            {
                #region Cleaning
                if(minInstructionDuration.floatValue == float.MaxValue)
                    minInstructionDuration.floatValue = 0;

                if(maxInstructionDuration.floatValue == float.MaxValue)
                    maxInstructionDuration.floatValue = 0;

                if(minDurationDeviation.floatValue == float.MaxValue)
                    minDurationDeviation.floatValue = 0;

                if(maxDurationDeviation.floatValue == float.MaxValue)
                    maxDurationDeviation.floatValue = 0;
                #endregion

                DrawPlayerSkillGroup("Instruction Duration", "How long to run this instruction for at 0 skill level", "How long to run this instruction for at max skill level",
                  minInstructionDuration, maxInstructionDuration, minDurationDeviation, maxDurationDeviation, instructionDurationAnimation);
            }
            else
            {
                minInstructionDuration.floatValue = linkedWave.totalWaveTime;//for simultaneous instructions the duration doesnt matter
                maxInstructionDuration.floatValue = linkedWave.totalWaveTime;

                minDurationDeviation.floatValue = 0;//also make sure there is no deviation
                maxDurationDeviation.floatValue = 0;
            }
            #endregion

            #region Spawn Percentage
            if(linkedWave.type == WaveType.Type.SpawnRandomly)
                DrawPlayerSkillGroup("Spawn Percentage", "The chance of this instruction being run with a 0 skill level", "The chance of this instruction being run at max skill level",
                     minSpawnPercentage, maxSpawnPercentage, null, null, spawnPercentageAnimation, null);
            #endregion

            #region Spawn Interval
            DrawPlayerSkillGroup("Instruction Interval", "How often this enemy spawns, in seconds, at 0 skill level", "How often this enemy spawns, in seconds, at max skill level",
                 minSpawnInterval, maxSpawnInterval, minSpawnIntervalDeviation, maxSpawnIntervalDeviation, spawnIntervalAnimation);
            #endregion

            #region Custom Value
            if(SpawningPreferences.ShowCustomValues)
                DrawPlayerSkillGroup("Custom Value", "The custom value at 0 skill level.\nThis is usually just 1", "The custom value at max skill level",
                     minCustomValue, maxCustomValue, minCustomValueDeviation, maxCustomValueDeviation, customValueAnimation);
            #endregion

            #region No Repeat Interval
            if(linkedWave.type == WaveType.Type.SpawnRandomly)
                if(layout == SpawningPreferences.Orientation.Verticle)
                {
                    DrawCenteredToggle(noRepeatAnimation, new GUIContent("No Repeat Interval", "How much much time has to pass before this instruction can be used again.\nE.G once you spawn a rare enemy it could be a good idea to wait at least a few seconds before it is even possible to spawn another"));

                    if(EditorGUILayout.BeginFadeGroup(noRepeatAnimation.faded))
                        EditorGUILayout.PropertyField(noRepeatInterval, GUIContent.none);

                    EditorGUILayout.EndFadeGroup();
                }
                else
                {
                    using(new Vertical())
                    {
                        DrawCenteredToggle(noRepeatAnimation, new GUIContent("No Repeat Interval", "How much much time has to pass before this instruction can be used again.\nE.G once you spawn a rare enemy it could be a good idea to wait at least a few seconds before it is even possible to spawn another"));

                        if(EditorGUILayout.BeginFadeGroup(noRepeatAnimation.faded))
                            EditorGUILayout.PropertyField(noRepeatInterval, GUIContent.none);

                        EditorGUILayout.EndFadeGroup();
                    }
                }
            #endregion

            #region Remove Instruction
            instructionDestroyed = WaveEditorWindow.RemoveInstruction(this);
            #endregion

            GUILayout.FlexibleSpace();
        }

        void ShowExpandCollapseAll()
        {
            GUI.contentColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0, 0, 1f, 1f);//change the colour of the heading to get it to stand out

            if(!GUILayout.Toggle(true, expandAll ? "Collapse All" : "Expand All", "PreToolbar2", GUILayout.MinWidth(20f)))
            {
                expandAll = !expandAll;

                instructionDurationAnimation.target = expandAll;
                spawnPercentageAnimation.target = expandAll;
                spawnIntervalAnimation.target = expandAll;
                customValueAnimation.target = expandAll;
                noRepeatAnimation.target = expandAll;
                enemyTypeAnimation.target = expandAll;
            }

            GUI.contentColor = Color.white;//restore to normal
        }

        /// <summary>
        /// A helper class to draw a group of data with a drop box list
        /// </summary>
        void DrawPlayerSkillGroup(string title, string minToolTip, string maxToolTip, SerializedProperty minValue, SerializedProperty maxValue,
            SerializedProperty minDeviation, SerializedProperty maxDeviation, AnimBool fadeAnimation, SpawningFramework.SpawningManager.BasicDelegate extraFieldDelegate = null)
        {
            GUILayout.Space(10);

            #region Heading
            if(layout == SpawningPreferences.Orientation.Horizontal)
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(spacing));

                EditorGUILayout.BeginHorizontal();
                //GUILayout.FlexibleSpace();

                DrawToggle(fadeAnimation, new GUIContent(title, minToolTip));

                //GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                //using(new Horizontal(GUILayout.Width(spacing)))
                DrawCenteredToggle(fadeAnimation, new GUIContent(title, minToolTip));
            }
            #endregion

            if(EditorGUILayout.BeginFadeGroup(fadeAnimation.faded))
            {
                EditorGUILayout.BeginVertical("AS TextArea", GUILayout.MinHeight(10f));

                GUILayout.Space(2);
                EditorGUILayout.LabelField(new GUIContent("Base Value", "This is the actual value that is influenced by PlayerSkill"), centeredLabel);
                GUILayout.Space(2);

                #region Spawn Percentages
                if(minDeviation == null && maxDeviation == null)//this si when we are displaying the spawn percentage
                {
                    using(new FixedWidthLabel(new GUIContent("At Min", minToolTip)))//these create nice nested visuals for the min and max text boxes
                        EditorGUILayout.PropertyField(minValue, GUIContent.none);

                    using(new FixedWidthLabel(new GUIContent("At Max", maxToolTip)))
                        EditorGUILayout.PropertyField(maxValue, GUIContent.none);
                }
                #endregion
                else
                    #region Standard
                    using(new FixedWidthLabel(new GUIContent("At Min", minToolTip)))//these create nice nested visuals for the min and max text boxes
                    {
                        EditorGUILayout.PropertyField(minValue, GUIContent.none);

                        using(new FixedWidthLabel(new GUIContent("At Max", maxToolTip)))
                            EditorGUILayout.PropertyField(maxValue, GUIContent.none);
                    }
                    #endregion

                GUILayout.Space(2);

                if(minDeviation != null && maxDeviation != null)
                {
                    EditorGUILayout.LabelField(new GUIContent("Deviation Value", "This value is a deviation also influenced by player skill\nE.G If the min base value is 1 and the min deviation is 0.5 then at 0 PlayerSkill the value will be between 0.5 and 1.5"), EditorStyles.boldLabel);
                    GUILayout.Space(2);

                    using(new FixedWidthLabel(new GUIContent("At Min", "How much to deviate the base value by at 0 PlayerSkill\n\nE.G If the min base value is 1 and the min deviation is 0.5 then at 0 PlayerSkill the value will be between 0.5 and 1.5")))//these create nice nested visuals for the min and max text boxes
                    {
                        EditorGUILayout.PropertyField(minDeviation, GUIContent.none);

                        using(new FixedWidthLabel(new GUIContent("At Max", "How much to deviate the base value by at max PlayerSkill\n\nE.G If the max base value is 10 and the max deviation is 2 then at max PlayerSkill the value will be between 8 and 12")))
                            EditorGUILayout.PropertyField(maxDeviation, GUIContent.none);
                    }
                }


                if(extraFieldDelegate != null)
                    extraFieldDelegate();

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFadeGroup();

            if(layout == SpawningPreferences.Orientation.Horizontal)
                EditorGUILayout.EndVertical();
        }

        void DrawStandardGroup(string title, string toolTip, SerializedProperty minValue, SerializedProperty minDeviation, AnimBool fadeAnimation, SpawningFramework.SpawningManager.BasicDelegate extraFieldDelegate = null)
        {
            GUILayout.Space(10);

            GUI.contentColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.85f);//change the colour of the heading to get it to stand out

            #region Heading
            if(layout == SpawningPreferences.Orientation.Horizontal)
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(spacing));

                EditorGUILayout.LabelField(title, centeredLabel, GUILayout.Width(spacing));
            }
            else
                DrawToggle(fadeAnimation, new GUIContent(title));
            #endregion

            GUI.contentColor = Color.white;

            if(EditorGUILayout.BeginFadeGroup(fadeAnimation.faded))
            {
                EditorGUILayout.BeginVertical("AS TextArea", GUILayout.MinHeight(10f), GUILayout.Width(spacing));

                using(new FixedWidthLabel(new GUIContent("Base Value", toolTip)))
                    EditorGUILayout.PropertyField(minValue, GUIContent.none);

                if(minDeviation != null)
                    using(new FixedWidthLabel(new GUIContent("Deviation", "This value is how much to randomly flucate the base value\nE.G If the base value is 1 and the deviation is 0.5 then the value will be between 0.5 and 1.5")))
                        EditorGUILayout.PropertyField(minDeviation, GUIContent.none);

                if(extraFieldDelegate != null)
                    extraFieldDelegate();

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFadeGroup();

            if(layout == SpawningPreferences.Orientation.Horizontal)
                EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Displays an extra field of data for the spawn percentage variables
        /// </summary>
        void ExtraSpawnPercentageField()
        {
            float totalSpawnChance = 0;

            for(int i = 0; i < linkedWave.spawnInstructions.Length; i++)
                totalSpawnChance += linkedWave.spawnInstructions[i].minSpawnPercentage.percentage;

            using(new FixedWidthLabel(new GUIContent("Chance: " + ((minSpawnPercentage.floatValue / (float)totalSpawnChance) * 100).ToString("N0") + "%")))
            {
            }
        }

        void DisplayStandardInstructions()
        {
            #region Duration
            if(linkedWave.type != WaveType.Type.SpawnSimultaneously)
            {
                #region Cleaning
                if(minInstructionDuration.floatValue == float.MaxValue)
                    minInstructionDuration.floatValue = 0;

                if(minDurationDeviation.floatValue == float.MaxValue)
                    minDurationDeviation.floatValue = 0;
                #endregion

                DrawStandardGroup("Instruction Duration", "How long to run this instruction for", minInstructionDuration, minDurationDeviation, instructionDurationAnimation);
            }
            else
            {
                minInstructionDuration.floatValue = linkedWave.totalWaveTime;//for simultaneous instructions the duration doesnt matter
                maxInstructionDuration.floatValue = linkedWave.totalWaveTime;

                minDurationDeviation.floatValue = 0;//also make sure there is no deviation
                maxDurationDeviation.floatValue = 0;
            }
            #endregion

            DrawStandardGroup("Spawn Interval", "How often this enemy spawns", minSpawnInterval, minSpawnIntervalDeviation, spawnIntervalAnimation);

            if(linkedWave.type == WaveType.Type.SpawnRandomly)
                DrawStandardGroup("Spawn Percentage", "The chance of this instruction being run", minSpawnPercentage, null, spawnPercentageAnimation, ExtraSpawnPercentageField);

            if(SpawningPreferences.ShowCustomValues)
                DrawStandardGroup("Custom", "A custom value", minCustomValue, minCustomValueDeviation, customValueAnimation);

            #region No Repeat Interval
            GUILayout.Space(10);

            if(linkedWave.type == WaveType.Type.SpawnRandomly)
                if(layout == SpawningPreferences.Orientation.Verticle)
                {
                    DrawToggle(noRepeatAnimation, new GUIContent("No Repeat Interval", "How much much time has to pass before this instruction can be used again.\nE.G once you spawn a rare enemy it could be a good idea to wait at least a few seconds before it is even possible to spawn another"));

                    if(EditorGUILayout.BeginFadeGroup(noRepeatAnimation.faded))
                        EditorGUILayout.PropertyField(noRepeatInterval, GUIContent.none);

                    EditorGUILayout.EndFadeGroup();
                }
                else
                {
                    using(new Vertical(GUILayout.Width(spacing)))
                    {
                        DrawToggle(noRepeatAnimation, new GUIContent("No Repeat Interval", "How much much time has to pass before this instruction can be used again.\nE.G once you spawn a rare enemy it could be a good idea to wait at least a few seconds before it is even possible to spawn another"));

                        if(EditorGUILayout.BeginFadeGroup(noRepeatAnimation.faded))
                            EditorGUILayout.PropertyField(noRepeatInterval, GUIContent.none, GUILayout.Width(spacing));

                        EditorGUILayout.EndFadeGroup();
                    }
                }
            #endregion

            #region Remove Instruction
            instructionDestroyed = WaveEditorWindow.RemoveInstruction(this);
            #endregion

            GUILayout.FlexibleSpace();
        }

        /// <summary>
        /// This searches for the Wave.cs component either on the same object or its parent. Edit this if you use a different hierarchy
        /// </summary>
        void FindLinkedWave()
        {
            if(linkedWave == null)
            {
                SpawnInstruction current = (SpawnInstruction)target;

                linkedWave = current.gameObject.GetComponent<Wave>();//find the wave attached to the same gameobject

                if(linkedWave == null && current.transform.parent != null)//if there was no wave attached then check the parent instead
                    linkedWave = current.transform.parent.gameObject.GetComponent<Wave>();
            }
        }

        /// <summary>
        /// Searches for a player skill component to determine if the difficulty percentages need displayed 
        /// </summary>
        bool UsingPlayerSkill()
        {
            #region First Run
            if(linkedManager == null && !searchedForManager)
            {
                SpawnInstruction current = (SpawnInstruction)target;
                SpawningManager[] managers = FindObjectsOfType<SpawningManager>();//find all spawning managers

                for(int i = 0; i < managers.Length; i++)//This has the potential to be expensive so having many managers in the scene at once will really slow down your editor BUT this 
                    for(int ii = 0; ii < managers[i].groups.Length; ii++)//has 0 impact on performance during run time.
                        if(managers[i].groups[ii] != null)
                            for(int iii = 0; iii < managers[i].groups[ii].waves.Length; iii++)
                                if(managers[i].groups[ii].waves[iii] != null)
                                    for(int iv = 0; iv < managers[i].groups[ii].waves[iii].spawnInstructions.Length; iv++)
                                        if(managers[i].groups[ii].waves[iii].spawnInstructions[iv] != null)
                                            if(managers[i].groups[ii].waves[iii].spawnInstructions[iv].Equals(current))//if we have found this instruction, and thus the parent spawn manager
                                            {
                                                linkedManager = managers[i];
                                                searchedForManager = true;

                                                //Debug.LogError(current.gameObject.name + " linked with: " + linkedManager.gameObject.name);//Uncomment this if you are having issues

                                                return linkedManager.skillValue != null;//dont bother looping through the rest of the hierarchy!
                                            }


                searchedForManager = true;
                Debug.LogError(current.gameObject.name + " Failed to find a valid SpawnManager!");
            }
            #endregion

            if(linkedManager != null)
                return linkedManager.skillValue != null;//return true if there is a player skill component
            else
                return false;
        }

        void DrawToggle(AnimBool animation, GUIContent content)
        {
            GUI.contentColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.85f);//change the colour of the heading to get it to stand out

            if(!GUILayout.Toggle(true, content, "PreToolbar2", GUILayout.MinWidth(20f)))
                animation.target = !animation.target;//invert

            GUI.contentColor = Color.white;
        }

        void DrawCenteredToggle(AnimBool animation, GUIContent content)
        {
            using(new Horizontal())
            {
                GUILayout.FlexibleSpace();
                DrawToggle(animation, content);
                GUILayout.FlexibleSpace();
            }
        }

        /// <summary>
        /// Draws a summary of this waves instructions if its needed
        /// </summary>
        void DrawSummary()
        {
            GUIStyle richText = new GUIStyle();
            richText.richText = true;

            if(EditorGUIUtility.isProSkin)
                richText.normal.textColor = new Color(225, 225, 225);

            if(showSummary)
                if(layout == SpawningPreferences.Orientation.Verticle)
                {
                    EditorGUILayout.TextArea(GetSummary(), richText);
                    GUILayout.Space(10);
                }
                else if(!usingPlayerSkill || (usingPlayerSkill && (instructionDurationAnimation.target || spawnIntervalAnimation.target || spawnPercentageAnimation.target)))//basically if you are using player skill and have collapsed the tallest objects then dont display this since its big and overlaps
                    using(new Horizontal(GUILayout.Width(spacing)))
                        EditorGUILayout.LabelField(GetSummary(), richText, GUILayout.Width(spacing));
        }

        string GetSummary()
        {
            StringBuilder output = new StringBuilder();
            SpawnInstruction current = (SpawnInstruction)target;

            #region Pauses and Waits
            if(current.type == EnemyTypes.Type.Pause)
                return "<b>Pause</b>";
            else if(current.type == EnemyTypes.Type.Wait)
                return "<b>Wait:</b> " + current.minSpawnInterval;
            #endregion


            float minInterval, maxInterval, minDuration, maxDuration;

            if(usingPlayerSkill)
            {
                #region Min Skill
                minInterval = current.minSpawnInterval + current.minSpawnIntervalDeviation;
                maxInterval = current.minSpawnInterval - current.minSpawnIntervalDeviation;

                minDuration = current.minInstructionDuration - current.minDurationDeviation;
                maxDuration = current.minInstructionDuration + current.minDurationDeviation;

                if(linkedWave.type == WaveType.Type.SpawnSimultaneously)
                {
                    minDuration = linkedWave.totalWaveTime;
                    maxDuration = minDuration;
                }

                int minCount = -1;

                if(minInterval > 0)
                    minCount = (int)(minDuration / minInterval);

                int maxCount = -1;

                if(maxInterval > 0)
                    maxCount = (int)(maxDuration / maxInterval);

                if(minCount != maxCount)
                    output.Append("<b>Spawns:</b>\nMin Skill: " + minCount + " to " + maxCount);
                else
                    output.Append("<b>Spawns:</b>\nMin Skill: " + minCount);

                #region Time
                if(minDuration != maxDuration)
                    output.Append("\n<b>Between:</b> " + minDuration + "s and " + maxDuration + "s");
                else
                    output.Append("\n<b>Over:</b> " + minDuration + "s");
                #endregion
                #endregion

                #region Max Skill
                output.Append("\n");

                minInterval = current.maxSpawnInterval + current.maxSpawnIntervalDeviation;
                maxInterval = current.maxSpawnInterval - current.maxSpawnIntervalDeviation;

                minDuration = current.maxInstructionDuration - current.maxDurationDeviation;
                maxDuration = current.maxInstructionDuration + current.maxDurationDeviation;

                if(linkedWave.type == WaveType.Type.SpawnSimultaneously)
                {
                    minDuration = linkedWave.totalWaveTime;
                    maxDuration = minDuration;
                }

                minCount = -1;

                if(minInterval > 0)
                    minCount = (int)(minDuration / minInterval);

                maxCount = -1;

                if(maxInterval > 0)
                    maxCount = (int)(maxDuration / maxInterval);

                if(minCount != maxCount)
                    output.Append("\nMax Skill: " + minCount + " to " + maxCount);
                else
                    output.Append("\nMax Skill: " + minCount);

                #region Time
                if(minDuration != maxDuration)
                    output.Append("\n<b>Between:</b> " + minDuration + "s and " + maxDuration + "s");
                else
                    output.Append("\n<b>Over:</b> " + minDuration + "s");
                #endregion
                #endregion
            }
            else
            {
                #region No Skill
                minInterval = current.minSpawnInterval + current.minSpawnIntervalDeviation;
                maxInterval = current.minSpawnInterval - current.minSpawnIntervalDeviation;

                minDuration = current.minInstructionDuration - current.minDurationDeviation;
                maxDuration = current.minInstructionDuration + current.minDurationDeviation;

                if(linkedWave.type == WaveType.Type.SpawnSimultaneously)
                {
                    minDuration = linkedWave.totalWaveTime;
                    maxDuration = minDuration;
                }

                int minCount = -1;

                if(minInterval > 0)
                    minCount = (int)(minDuration / minInterval);

                int maxCount = -1;

                if(maxInterval > 0)
                    maxCount = (int)(maxDuration / maxInterval);

                if(minCount != maxCount)
                    output.Append("<b>Spawns:</b> " + minCount + " to " + maxCount);
                else
                    output.Append("<b>Spawns:</b> " + minCount);

                #region Time
                if(minDuration != maxDuration)
                    output.Append("\n<b>Between:</b> " + minDuration + "s and " + maxDuration + "s");
                else
                    output.Append("\n<b>Over:</b> " + minDuration + "s");
                #endregion
                #endregion
            }

            return output.ToString();
        }
    }
}