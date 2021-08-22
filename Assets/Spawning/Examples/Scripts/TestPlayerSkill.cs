using UnityEngine;
using System.Collections;
using SpawningFramework;

namespace SpawningFrameworkExamples
{
    /// <summary>
    /// This class was a hack to test waves for the video
    /// </summary>
    public class TestPlayerSkill : PlayerSkill
    {
        public int testSkill;

        public string label;//for the GUI

        public YourSpawnManager spawnManager;

        public static bool runFinished;

        public GameObject[] disableList, enableList;//for jumping between scenes in the example

        void Start()
        {
            percentageSkill = 0;
        }

        void Update()
        {
            if(testSkill != skillValue)
            {
                skillValue = testSkill;
                percentageSkill = Mathf.Min(1, K2Maths.Percentage((float)skillValue, maxSkillValue));
            }
        }

        void OnGUI()
        {
            GUILayout.Label(label);

            GUILayout.Label("Player Skill %");
            percentageSkill = GUILayout.HorizontalSlider(percentageSkill, 0, 1, GUILayout.Width(50));

            GUILayout.Space(10);

            GUILayout.Label("Note that the current wave has to\nfinish before the skill change can be seen!");

            if(runFinished)
                if(GUILayout.Button("Restart"))
                {
                    spawnManager.InitialiseNewSector();
                    runFinished = false;
                }

            GUILayout.Space(10);

            if(GUILayout.Button("Next Example"))
            {
                for(int i = 0; i < disableList.Length; i++)
                    disableList[i].SetActive(false);

                for(int i = 0; i < enableList.Length; i++)
                    enableList[i].SetActive(true);
            }
        }
    }
}
