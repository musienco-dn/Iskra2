using UnityEngine;
using System.Collections.Generic;
using SpawningFramework;
using System.Collections;

namespace SpawningFramework
{
    public class SpawningManager : MonoBehaviour
    {
        #region Variables
        public static SpawningManager Instance;

        #region Delgates
        public delegate void BasicDelegate();
        public BasicDelegate waveGroupFinished, waveFinished, allGroupsFinished;
        #endregion

        public PlayerSkill skillValue;//scales difficulty accordingly if the player is doing better

        #region Spawning
        int randomWaveRunCount;//how many waves have been spawned. Used to stop random waves
        int randomGroupRunCount;//how many groups have been spawned. Stops random groups
        public int MaxEnemies = 100;
        bool spawnedAll = false;//used for determining when a wave is over
        bool inWavePadding = false;//this is time after a block when nothing spawns to let players kill everything
        int currentMaxEnemy;//which enemy is about to be reinitialised

        [HideInInspector]
        public int currentWaveIndex;

        int currentSubInstruction;//which instruction is currently running
        #endregion

        #region Helpers
        SpawnInstruction currentInstruction;//purely to keep code tidy

        float SpawnInterval
        {
            get
            {
                try
                {
                    if(skillValue != null)
                    {
                        float baseValue = K2Maths.Lerp(currentInstruction.minSpawnInterval, currentInstruction.maxSpawnInterval, skillValue.percentageSkill);//calculate the base value

                        return K2Maths.Deviate(baseValue, K2Maths.Lerp(currentInstruction.minSpawnIntervalDeviation, currentInstruction.maxSpawnIntervalDeviation, skillValue.percentageSkill));//and now deviate the value as needed
                    }
                    else
                        return K2Maths.Deviate(currentInstruction.minSpawnInterval, currentInstruction.minSpawnIntervalDeviation);

                }
                catch(System.Exception e)
                {
                    Debug.LogError("Error in spawn interval:\nInstruction: " + currentWaveIndex + " Max: " + groups[currentGroupIndex].waves.Length +
                        "\nSub Instruction: " + currentSubInstruction);
                    return -1;
                }
            }
        }

        public float InstructionDuration
        {
            get
            {
                if(skillValue != null)
                {
                    float baseValue = K2Maths.Lerp(currentInstruction.minInstructionDuration, currentInstruction.maxInstructionDuration, skillValue.percentageSkill);
                    return K2Maths.Deviate(baseValue, K2Maths.Lerp(currentInstruction.minDurationDeviation, currentInstruction.maxDurationDeviation, skillValue.percentageSkill));//and now deviate the value as needed
                }
                else
                    return K2Maths.Deviate(currentInstruction.minInstructionDuration, currentInstruction.minDurationDeviation);
            }
        }

        public float SpawnPercentage
        {
            get
            {
                if(skillValue != null)
                    return K2Maths.Lerp(currentInstruction.minSpawnPercentage.percentage, currentInstruction.maxSpawnPercentage.percentage, skillValue.percentageSkill);
                else
                    return currentInstruction.minSpawnPercentage.percentage;
            }
        }

        public float CustomValue
        {
            get
            {
                if(skillValue != null)
                {
                    float baseValue = K2Maths.Lerp(currentInstruction.minCustomValue, currentInstruction.maxCustomValue, skillValue.percentageSkill);
                    return K2Maths.Deviate(baseValue, K2Maths.Lerp(currentInstruction.minCustomValueDeviation, currentInstruction.maxCustomValueDeviation, skillValue.percentageSkill));//and now deviate the value as needed
                }
                else
                    return K2Maths.Deviate(currentInstruction.minCustomValue, currentInstruction.minCustomValueDeviation);
            }
        }
        #endregion

        [HideInInspector]
        public Enemy[] enemies;

        #region Wave Data
        public RestrictedType.Type type;

        /// <summary>
        /// How many groups are spawned. This only applies using the random spawn mode!
        /// </summary>
        public int groupsToSpawn;

        public WaveGroup[] groups;
        #endregion

        [HideInInspector]
        public int enemiesAlive, currentGroupIndex;

        public List<SimplifiedInstruction> instructions = new List<SimplifiedInstruction>();//when to spawn enemies

        #region Garbage Sinks
        int[] chances;
        List<int> possibleValues = new List<int>();
        #endregion
        #endregion

        #region Methods
        public void Awake()
        {
            Instance = this;
            enemies = new Enemy[MaxEnemies];

            spawnedAll = false;//start spawning
            currentWaveIndex = 0;
        }

#if(UNITY_EDITOR)
        /// <summary>
        /// Runs a series of checks to ensure the spawner is setup correctly. Only runs in the editor so has no performance cost for final builds
        /// </summary>
        void Start()
        {
            CheckValues();
        }

        protected void CheckValues()
        {
            if(groups == null || groups.Length == 0)
                Debug.LogError("No groups found: " + gameObject.name);

            for(int i = 0; i < groups.Length; i++)
            {
                if(groups[i] == null)
                    Debug.LogError("Null group found: " + gameObject.name);

                if(groups[i].wavesToSpawn == 0 && groups[i].type == RestrictedType.Type.SpawnRandomly)
                    Debug.LogError("0 waves count for instruction: " + groups[i].gameObject.name);

                if(groups[i].waves == null)
                    Debug.LogError("null waves found: " + groups[i].gameObject.name);

                for(int ii = 0; ii < groups[i].waves.Length; ii++)
                {
                    if(groups[i].waves[ii] == null)
                        Debug.LogError("null wave found: " + groups[i].gameObject.name);

                    if(groups[i].waves[ii].totalWaveTime == 0 && groups[i].waves[ii].type != WaveType.Type.SpawnSequentially)
                        Debug.LogError("No wave time set for: " + groups[i].waves[ii].gameObject.name);

                    if(groups[i].waves[ii].spawnInstructions == null || groups[i].waves[ii].spawnInstructions.Length == 0)
                        Debug.LogError("No spawn instructions found: " + groups[i].waves[ii].gameObject.name);

                    for(int iii = 0; iii < groups[i].waves[ii].spawnInstructions.Length; iii++)
                    {
                        if(groups[i].waves[ii].spawnInstructions[iii] == null)
                            Debug.LogError("null spawn instruction found: " + groups[i].waves[ii].gameObject.name);

                        if(groups[i].waves[ii].spawnInstructions[iii].minInstructionDuration == 0)
                            Debug.LogError("Spawn instruction with no duration found. They will not spawn and cause errors: " + groups[i].waves[ii].gameObject.name);

                        if(groups[i].waves[ii].type == WaveType.Type.SpawnRandomly && groups[i].waves[ii].spawnInstructions[iii].minSpawnPercentage.percentage == 0 && groups[i].waves[ii].spawnInstructions[iii].maxSpawnPercentage.percentage == 0)
                            Debug.LogError("Instruction " + i + " on " + groups[i].waves[ii].spawnInstructions[iii].gameObject.name + " has no chance of spawning!");

                        if(groups[i].waves[ii].spawnInstructions[iii].minSpawnInterval > groups[i].waves[ii].spawnInstructions[iii].minInstructionDuration || groups[i].waves[ii].spawnInstructions[iii].maxSpawnInterval > groups[i].waves[ii].spawnInstructions[iii].maxInstructionDuration)
                            Debug.LogError("Your spawn interval is higher than your duration! This will cause errors. Wave: " + groups[i].waves[ii].gameObject.name + " Instruction: " + iii);
                    }
                }
            }
        }
#endif

        #region Initialises
        /// <summary>
        /// Basically called when run for the very first time
        /// </summary>
        public void InitialiseNewSector()
        {
            randomGroupRunCount = groupsToSpawn;
            randomWaveRunCount = groups[0].wavesToSpawn;
            currentGroupIndex = 0;//remember to reset the wave for new sectors!
            enemiesAlive = 0;

            if(Stats.Instance != null)
                Stats.Instance.Reset();

            for(int i = 0; i < enemies.Length; i++)
                if(enemies[i] != null)
                    GameObject.Destroy(enemies[i].gameObject);

            for(int i = 0; i < groups.Length; i++)
                if(groups[i] != null)
                {
                    for(int ii = 0; ii < groups[i].waves.Length; ii++)
                        if(groups[i].waves[ii] != null)
                            groups[i].waves[ii].nextRunTime = 0;//let all waves run again
                        else
                            Debug.LogError("Found null wave: " + groups[i].gameObject.name);
                }
                else
                    Debug.LogError("Found null group: " + gameObject.name);

            InitisliseWaveGroup();
        }

        /// <summary>
        /// Called when a wave has finished to spawn a new one
        /// </summary>
        public void InitialiseNewWave()
        {
            switch(groups[currentGroupIndex].type)
            {
                #region Spawn Random waves
                case WaveGroup.Type.SpawnRandomly:
                    SelectRandomWave();
                    break;
                #endregion

                //Note that we don't change anything for the seqential waves since its already increased the wave index
            }

            spawnedAll = false;
            inWavePadding = false;

            CalculateInstructions();
        }

        /// <summary>
        /// Called when a group of waves have finished running
        /// </summary>
        public void InitisliseWaveGroup()
        {
            switch(type)
            {
                #region Spawn Random waves
                case RestrictedType.Type.SpawnRandomly:
                    currentGroupIndex = SelectRandomGroupIndex();//select a new group randomly whilst preventing stack overflows
                    break;
                #endregion

                //again the index has already ben increased for sequential waves
            }

            randomWaveRunCount = groups[currentGroupIndex].wavesToSpawn;
            currentWaveIndex = 0;
            InitialiseNewWave();
        }

        /// <summary>
        /// A helper to avoid stack overflows whilst attempting to select from a random number range
        /// </summary>
        public int SelectRandomGroupIndex()
        {
            List<int> possibleValues = new List<int>();
            int returnValue;

            for(int i = 0; i < groups.Length; i++)
                if(Time.realtimeSinceStartup > groups[currentGroupIndex].nextRunTime)//if this instruction can run
                    possibleValues.Add(i);//show we can select this value

            if(possibleValues.Count == 0)
            {
                Debug.LogError("No possible values could be selected! Try lowering your no repeat intervals on: " + groups[currentGroupIndex].gameObject + "\nSpawning the first group as a fallback");
                return 0;
            }

            returnValue = possibleValues[Random.Range(0, possibleValues.Count)];
            groups[returnValue].nextRunTime = Time.realtimeSinceStartup + groups[returnValue].noRepeatInterval;//record this area has been selected

            return returnValue;
        }

        /// <summary>
        /// Selects a random wave. called when a wave has finished and a new group has started
        /// </summary>
        void SelectRandomWave()
        {
            chances = new int[groups[currentGroupIndex].waves.Length];

            for(int i = 0; i < chances.Length; i++)
                chances[i] = groups[currentGroupIndex].waves[i].spawnChance;//populate the list of chances

            chances = TrimChancesList(chances);//basically remove the indexes of waves that cannot be spawned yet to avoid stack overflows

            currentWaveIndex = RandomFromList(chances);//select a new wave randomly. Returns 0 if nothing can spawn

            groups[currentGroupIndex].waves[currentWaveIndex].nextRunTime = Time.realtimeSinceStartup + groups[currentGroupIndex].waves[currentWaveIndex].noRepeatInterval;//show the next time this instruction can run
        }

        /// <summary>
        /// A helper to avoid stack overflows whilst attempting to select from a random number range
        /// </summary>
        public int[] TrimChancesList(int[] waveChances)
        {
            possibleValues.Clear();

            for(int i = 0; i < waveChances.Length; i++)
                if(Time.realtimeSinceStartup > groups[currentGroupIndex].waves[i].nextRunTime)//if this instruction can run
                    possibleValues.Add(i);//show we can select this value

            if(possibleValues.Count == 0)
                Debug.LogError("No possible values could be selected! Try lowering your no repeat intervals on: " + groups[currentGroupIndex].gameObject + "\nSpawning the first group as a fallback");

            return possibleValues.ToArray();
        }
        #endregion

        void SelectNewWave()
        {
            switch(groups[currentGroupIndex].type)
            {
                #region Spawn Randomly
                case WaveGroup.Type.SpawnRandomly:
                    if(randomWaveRunCount != WaveType.InfiniteWaves && randomWaveRunCount > 0)
                        randomWaveRunCount--;

                    if(randomWaveRunCount == WaveType.InfiniteWaves || randomWaveRunCount > 0)//if we can spawn more waves do so
                    {
                        InitialiseNewWave();

                        if(waveFinished != null)//a wave has just finished so call the delegate if one exists
                            waveFinished();
                    }
                    else
                        SelectNewGroup();
                    break;
                #endregion

                #region Spawn Sequentially
                case WaveGroup.Type.SpawnSequentially:
                    currentWaveIndex++;

                    if(currentWaveIndex < groups[currentGroupIndex].waves.Length)//if there are more waves, keep spawning them
                    {
                        InitialiseNewWave();//select a new wave of instructions

                        if(waveFinished != null)//a wave has just finished so call the delegate if one exists
                            waveFinished();

                        return;//prevents spawnedAll from becoming true
                    }
                    else//there are no more waves
                        SelectNewGroup();
                    break;
                #endregion
            }
        }

        /// <summary>
        /// Called when a group of waves has finished running
        /// </summary>
        void SelectNewGroup()
        {
            switch(type)
            {
                case RestrictedType.Type.SpawnSequentially:
                    currentGroupIndex++;

                    if(currentGroupIndex < groups.Length)
                    {
                        InitisliseWaveGroup();

                        if(waveGroupFinished != null)
                            waveGroupFinished();//Calls any delegate you have assign to run
                    }
                    else
                    {
                        spawnedAll = true;

                        if(allGroupsFinished != null)
                            allGroupsFinished();//Calls any delegate you have assign to run

                        StopAllCoroutines();
                        return;
                    }
                    break;

                case RestrictedType.Type.SpawnRandomly:
                    if(randomGroupRunCount != WaveType.InfiniteWaves)
                        randomGroupRunCount--;

                    if(randomGroupRunCount == WaveType.InfiniteWaves || randomGroupRunCount > 0)//if we can spawn more waves do so
                    {
                        InitisliseWaveGroup();

                        if(waveGroupFinished != null)
                            waveGroupFinished();//Calls any delegate you have assign to run
                    }
                    else
                    {
                        spawnedAll = true;

                        if(allGroupsFinished != null)
                            allGroupsFinished();//Calls any delegate you have assign to run

                        StopAllCoroutines();
                        return;
                    }
                    break;
            }

            if(waveFinished != null)//since we are selecting a new group then a wave has just been finished
                waveFinished();//so call the delegate if one exists
        }

        void CheckIfSectorComplete()
        {
            if(!spawnedAll)//if there are still spawn instructions
            {
                #region Block Paddings
                if(inWavePadding)
                {
                    inWavePadding = false;
                    SelectNewWave();
                }
                #endregion
            }
            else if(enemiesAlive == 0)
            {
                if(waveGroupFinished != null)
                    waveGroupFinished();//runs your delegate to show all waves have finished
            }
        }

        #region Spawning
        /// <summary>
        /// Calculates an entire waves of instructions
        /// </summary>
        void CalculateInstructions()
        {
            float duration = 0, currentTime = 0, interval = 0;

            instructions.Clear();

            switch(groups[currentGroupIndex].waves[currentWaveIndex].type)
            {
                #region Random Waves
                case WaveType.Type.SpawnRandomly:
                    duration = groups[currentGroupIndex].waves[currentWaveIndex].totalWaveTime;//first grab the total time this wave will run for

                    float totalTime = 0;

                    do
                    {
                        SelectNewSubInstruction();//selects a correct currentSubInstruction randomly
                        currentInstruction = groups[currentGroupIndex].waves[currentWaveIndex].spawnInstructions[currentSubInstruction];

                        GenerateInstructions(interval, ref totalTime, duration);
                    }
                    while(totalTime < duration);//loop until enough time has been filled
                    break;
                #endregion

                #region Sequential Waves
                case WaveType.Type.SpawnSequentially:
                    for(int i = 0; i < groups[currentGroupIndex].waves[currentWaveIndex].spawnInstructions.Length; i++)//loop for all instructions
                    {
                        currentInstruction = groups[currentGroupIndex].waves[currentWaveIndex].spawnInstructions[i];

                        GenerateInstructions(interval, ref duration, float.MaxValue);
                    }
                    break;
                #endregion

                #region Simulataneous Data
                case WaveType.Type.SpawnSimultaneously:
                    #region Populate Data
                    for(int i = 0; i < groups[currentGroupIndex].waves[currentWaveIndex].spawnInstructions.Length; i++)//loop for all instructions
                    {
                        currentInstruction = groups[currentGroupIndex].waves[currentWaveIndex].spawnInstructions[i];
                        duration = InstructionDuration;//first calculate the duration, this includes player skill and deviations
                        currentTime = 0;

                        #region Deviating Instructions
                        if(currentInstruction.minSpawnIntervalDeviation != 0 || currentInstruction.maxSpawnIntervalDeviation != 0)//if there is any deviation do things one instruction at a time
                        {
                            do
                            {
                                interval = SpawnInterval;//calculate the spawn interval, including skill and deviation
                                currentTime += interval;//reduce the timer

                                if(currentTime < duration)//if there is time left to spawn this enemy
                                    instructions.Add(new SimplifiedInstruction(currentInstruction.type, currentTime, 1));//show the enemy needs spawned. This stores total time and not duration

                            }
                            while(currentTime < duration);
                        }
                        #endregion

                        #region Static Instructions
                        else
                        {
                            interval = SpawnInterval;
                            int count = Mathf.FloorToInt(groups[currentGroupIndex].waves[currentWaveIndex].totalWaveTime / interval);

                            for(int ii = 0; ii < count; ii++)
                            {
                                currentTime += interval;
                                instructions.Add(new SimplifiedInstruction(currentInstruction.type, currentTime, 1));//basically spawn the enemy X times in a row
                            }
                        }
                        #endregion
                    }
                    #endregion//At this stage all data is stored in the Instructions list with the time they should be spawned, but they are not sorted

                    #region Sort Data
                    instructions.Sort();//now sort the data. The simplified instruction CompareTo method tells it how to sort

                    for(int i = instructions.Count - 1; i > 0; i--)
                        instructions[i].time = instructions[i].time - instructions[i - 1].time;//now fix the time to be relative to each other. E.G 0.5, 0.5, 0.5 rather than 0.5, 1, 1.5
                    #endregion
                    break;
                #endregion
            }

            if(groups[currentGroupIndex].waves[currentWaveIndex].paddingTime > 0)//if there should be a padding between these waves
                instructions[0].time += groups[currentGroupIndex].waves[currentWaveIndex].paddingTime;//wait until the padding is finished before spawning enemies

            StartCoroutine(SpawnNextEnemy());

            if(groups[currentGroupIndex].waves[currentWaveIndex].paddingTime > 0)//if there should be a padding between these waves
                instructions[0].time -= groups[currentGroupIndex].waves[currentWaveIndex].paddingTime;//then reduce the padding again, otherwise all enemies will have padding between spawning
        }

        void GenerateInstructions(float interval, ref float total, float maxDuration)
        {
            #region Waits
            if(currentInstruction.type == EnemyTypes.Type.Wait)
            {
                instructions.Add(new SimplifiedInstruction(EnemyTypes.Type.Wait, currentInstruction.minSpawnInterval, 1));
                return;
            }
            #endregion

            #region Pauses
            if(currentInstruction.type == EnemyTypes.Type.Pause)
            {
                instructions.Add(new SimplifiedInstruction(EnemyTypes.Type.Pause, 0, 1));
                return;
            }
            #endregion

            float currentTime = InstructionDuration;//first calculate the duration, this includes player skill and deviations

            total += currentTime;

            if(total > maxDuration)//if the instruction has ran on too long (as random ones will likely do)
                currentTime -= total - maxDuration;//subtract the difference so this instruction only runs as needed

            #region Deviating Instructions
            if(currentInstruction.minSpawnIntervalDeviation != 0 || currentInstruction.maxSpawnIntervalDeviation != 0)//if there is any deviation do things one instruction at a time
            {
                do
                {
                    interval = SpawnInterval;//calculate the spawn interval, including skill and deviation
                    currentTime -= interval;//reduce the timer

                    if(currentTime > 0)//if there is time left to spawn this enemy
                        instructions.Add(new SimplifiedInstruction(currentInstruction.type, interval, 1));//show the enemy needs spawned

                }
                while(currentTime > 0);
            }
            #endregion

            #region Static Instructions
            else
            {
                interval = SpawnInterval;
                instructions.Add(new SimplifiedInstruction(currentInstruction.type, interval, Mathf.FloorToInt(currentTime / interval)));//basically spawn the enemy X times in a row
            }
            #endregion

#if(UNITY_EDITOR)
            if(instructions.Count == 0)
                Debug.LogError("No instructions added! Check your spawn intervals are always less than the instruction duration. Wave: " + groups[currentGroupIndex].waves[currentWaveIndex].gameObject.name);
#endif
        }

        int NextActiveEnemy()
        {
            for(int i = 0; i < currentMaxEnemy; i++)//loop
                if(enemies[i] == null || enemies[i].gameObject.activeSelf)//if inactive
                {
                    enemiesAlive++;
                    return i;
                }

            if(currentMaxEnemy < enemies.Length)
            {
                currentMaxEnemy++;
                enemiesAlive++;
            }
            else
            {
                Debug.Log("Too many enemies spawned!!");
                return -1;
            }

            return currentMaxEnemy - 1;
        }

        /// <summary>
        /// Called to spawn enemies as they are required
        /// </summary>
        /// <returns></returns>
        IEnumerator SpawnNextEnemy()
        {
            yield return new WaitForSeconds(instructions[0].time);//wait until the enemy is needed

            if(instructions[0].type != EnemyTypes.Type.Wait && instructions[0].type != EnemyTypes.Type.Pause)//do nothing when spawning waits or pauses. This is important!!
                Spawn(instructions[0].type);//spawn the enemy

            if(instructions[0].type == EnemyTypes.Type.Pause)//basically for pauses do nothing
                instructions.RemoveAt(0);//basically for pauses do nothing
            else
            {
                instructions[0].count--;

                if(instructions[0].count <= 0)//if all enemies have been spawned
                    instructions.RemoveAt(0);//then remove the instruction

                if(instructions.Count > 0)//if more enemies need spawned
                    StartCoroutine(SpawnNextEnemy());//then keep spawning
                else
                    SelectNewWave();
            }
        }

        /// <summary>
        /// Spawns an enemy, also used to spawn bonus enemies when needed
        /// </summary>
        /// <param name="type"></param>
        protected void Spawn(EnemyTypes.Type type)
        {
            int index = NextActiveEnemy();

            SpawnObject(type, index);

            enemies[index].Initialise();
        }

        /// <summary>
        /// Spawns an enemy
        /// </summary>
        protected virtual void SpawnObject(EnemyTypes.Type type, int index)
        {
            //GameObject enemy = null;//Example code!

            //switch(type)
            //{
            //    case EnemyTypes.EnemyType.Normal:
            //        enemy = (GameObject)GameObject.Instantiate(normalEnemyObject);
            //        break;

            //    default:
            //        Debug.LogError("Enemy Model not assigned: " + type);
            //        break;
            //}

            //enemies[index] = (Enemy)enemy.GetComponent(typeof(Enemy));
        }

        public void RegisterEnemy(Enemy enemy)
        {
            enemies[NextActiveEnemy()] = enemy;
        }

        void SelectNewSubInstruction()
        {
            if(groups[currentGroupIndex].waves[currentWaveIndex].type == WaveType.Type.SpawnRandomly)
            #region Random Instructions
            {
                float totalChance = 0;
                float selectedInstruction;
                float[] chances = new float[groups[currentGroupIndex].waves[currentWaveIndex].spawnInstructions.Length];

                for(currentSubInstruction = 0; currentSubInstruction < groups[currentGroupIndex].waves[currentWaveIndex].spawnInstructions.Length; currentSubInstruction++)//loop for all sub instructions. Needs to current sub to be the index for calculating the right percentage!!
                {
                    currentInstruction = groups[currentGroupIndex].waves[currentWaveIndex].spawnInstructions[currentSubInstruction];
                    chances[currentSubInstruction] = SpawnPercentage;//calculates the percentage based on player skill
                    totalChance += chances[currentSubInstruction];//determine the total
                }

                selectedInstruction = Random.Range(0, totalChance);//now grab a random number in this range
                totalChance = 0;//reset to use as an index

                for(int i = 0; i < chances.Length; i++)//loop again
                    if(selectedInstruction <= chances[i])//if within the range
                    {
                        currentSubInstruction = i;//we have our instruction

                        currentInstruction = groups[currentGroupIndex].waves[currentWaveIndex].spawnInstructions[currentSubInstruction];//store the current instruction to make future code more readable
                        return;//so stop
                    }
                    else
                        selectedInstruction -= chances[i];//move onto the next check (and make the number relative to 0 again rather than the entire range)

                currentSubInstruction = 0;//shouldnt happen..
                Debug.Log("Set sub to 0");
            }
            #endregion
            //else
            //#region Sequential Instructions
            //{
            //    currentSubInstruction++;

            //    if(currentSubInstruction < groups[currentGroupIndex].waves[currentWaveIndex].spawnInstructions.Length)//if there are more instructions
            //    {
            //        currentInstruction = groups[currentGroupIndex].waves[currentWaveIndex].spawnInstructions[currentSubInstruction];//store the current instruction to make future code more readable

            //        currentSubInstructionTime = InstructionDuration;//read the instruction interval. takes into consideration player skill
            //    }
            //    else
            //        currentWaveTime = -1;//There are no more instructions, so end the block
            //}
            //#endregion
        }
        #endregion

        #region Misc
        /// <summary>
        /// Call once the wave has been paused. Important not to call whilst already running or you will get multiple waves spawning at once!
        /// </summary>
        public void Resume()
        {
            if(instructions.Count > 0)//if more enemies need spawned
                StartCoroutine(SpawnNextEnemy());//then keep spawning
            else
                SelectNewWave();
        }

        public void RegisterGameOver()
        {
            enabled = false;//stop spawning enemies

            #region Delegates
            if(waveGroupFinished != null)
                waveGroupFinished();//runs your delegate
            #endregion

            for(int i = 0; i < enemies.Length; i++)
                if(enemies[i] != null)
                    enemies[i].OnGameOver();
        }

        public void RecordDeath()
        {
            enemiesAlive--;

            CheckIfSectorComplete();
        }

        /// <summary>
        /// Returns an index of something to spawn from an array of chances
        /// </summary>
        int RandomFromList(int[] chances)
        {
            int total = 0;

            for(int i = 0; i < chances.Length; i++)
                total += chances[i];

            total = Random.Range(0, total);

            for(int i = 0; i < chances.Length; i++)
            {
                if(total < chances[i])
                    return i;
                else
                    total -= chances[i];
            }

            return 0;
        }
        #endregion

        #region Enemy Specific
        public Vector3 GetPosition(int index)
        {
            if(enemies[index].enabled)
                return enemies[index].transform.position;
            else
                return Vector3.zero;
        }

        public Transform GetWorld(int index)
        {
            if(enemies[index].enabled)
                return enemies[index].transform;
            else
                return null;
        }

        public Enemy FindEnemy(int index)
        {
            if(enemies[index].dying)
                return null;
            else
                return enemies[index];
        }
        #endregion

        #region Damage
        /// <summary>
        /// Hurts enemies in a given area
        /// </summary>
        public void HurtAtArea(Vector3 position, float radiusSquared, float power)
        {
            for(int i = 0; i < currentMaxEnemy; i++)
                if(enemies[i] != null)
                    enemies[i].HurtWithExplosion(position, radiusSquared, power);
        }

        /// <summary>
        /// Finds the nearby enemies without clearing the list of tracked enemies
        /// </summary>
        public void FindNearbyEnemies(Vector3 position, ref List<Transform> trackedEnemies, float trackingRange)
        {
            Vector3 testedPosition;

            for(int i = 0; i < currentMaxEnemy; i++)//loop for all possibly active enemies
                if(enemies[i] != null)
                {
                    testedPosition = enemies[i].transform.position;

                    if(Vector3.SqrMagnitude(position - testedPosition) < trackingRange)//if in range
                    {
                        bool found = false;

                        for(int j = 0; j < trackedEnemies.Count; j++)
                            if(trackedEnemies[j].Equals(enemies[i].transform))
                            {
                                found = true;
                                break;//end this sub loop
                            }

                        if(found)
                            continue;//check the next enemy, this one has already been registered

                        trackedEnemies.Add(enemies[i].transform);
                    }
                }
        }

        /// <summary>
        /// returns a list of enemy indexes within a given position. This is useful for things like tracking missiles.
        /// 
        /// E.G call this at a set interval, say every 0.5 seconds, and store the result. An object can then read that list each frame and track object[0] (the closest one)
        /// </summary>
        /// <param name="missilePosition">The tracking objects current position</param>
        /// <param name="trackingRange">The squared range to track enemies. This is important to be squared!! E.G if you are tracking range is 10 units then pass this 10 * 10</param>
        /// <param name="trackedEnemies">The referenced list for that object to track from. Don't forget the 'ref' keyword!</param>
        /// <returns></returns>
        public Transform FindEnemiesToTrack(Vector3 missilePosition, float trackingRange, ref List<int> trackedEnemies)
        {
            float closestDistance = float.MaxValue;//start with a huge value
            int closestIndex = -1;
            trackedEnemies.Clear();

            for(int i = 0; i < currentMaxEnemy; i++)//loop for all possibly active enemies
                if(enemies[i] != null && enemies[i].enabled)
                {
                    float distance = Vector3.SqrMagnitude(missilePosition - enemies[i].transform.position);//more efficient than distance

                    if(distance < trackingRange)//first check to see if its in tracking range at the current position
                    {
                        trackedEnemies.Add(i);//store the index of the new tracked enemy

                        if(distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestIndex = i;
                        }
                    }
                }

            if(trackedEnemies.Count > 1 && trackedEnemies[0] != closestIndex)//if the first member isnt the closest one to the missile
                trackedEnemies[0] = closestIndex;//make it the closest one 

            if(trackedEnemies.Count > 0)
                return enemies[trackedEnemies[0]].transform;
            else
                return null;
        }
        #endregion

        #region Manual Collisions
        /// <summary>
        /// Useful if you ever want to manually scan just the enemies and ignore their colliders
        /// </summary>
        /// <param name="missilePosition"></param>
        /// <param name="collisionRange"></param>
        /// <returns>-1 for no collisions. The enemy index when there is</returns>
        public int CheckForCollisionsPerFrame(Vector3 missilePosition, float collisionRange)
        {
            for(short i = 0; i < currentMaxEnemy; i++)//loop for all possibly active enemies
                if(enemies[i] != null && enemies[i].enabled)
                {
                    float distance = Vector3.SqrMagnitude(missilePosition - enemies[i].transform.position);

                    if(distance < collisionRange)//first check to see if its in tracking range at the current position
                        return i;
                }

            return -1;
        }

        /// <summary>
        /// This will need tweaked per project but a starting point to check for any collisions
        /// </summary>
        /// <param name="bullet"></param>
        /// <returns>Returns -1 for no collisions and the distance to the collision otherwise</returns>
        public Transform CheckForCollisionsWithRay(Transform bullet, ref float lifeSpan)
        {
            RaycastHit data;

            if(Physics.CapsuleCast(bullet.transform.position - new Vector3(5, 0, 0), bullet.transform.position + new Vector3(5, 0, 0), 10, bullet.transform.forward, out data))
            {
                lifeSpan = data.distance;//return the distance 
                return data.collider.transform;
            }

            return null;
        }
        #endregion
        #endregion
    }
}