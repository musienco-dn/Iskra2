namespace GameCreator.Core
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
	using GameCreator.Core;
	using GameCreator.Variables;
	using GameCreator.Characters;

	#if UNITY_EDITOR
	using UnityEditor;
	#endif

	[AddComponentMenu("")]
	public class MoveToRandoMarkerAction : IAction
	{
		public enum MOVE_TO
		{
			Object
		}
		public TargetCharacter target = new TargetCharacter();
		public HelperGetListVariable listVariables = new HelperGetListVariable();

		public MOVE_TO moveTo = MOVE_TO.Object;
		public bool waitUntilArrives = true;
		
		public NavigationMarker marker;
		
		public GameObject gameObjectToCheck;
		
		public bool cancelable = false;
		public float cancelDelay = 1.0f;

		[Range(0.0f, 5.0f)]
		[Tooltip("Threshold distance from the target that is considered as reached")]
		public float stopThreshold = 0.0f;

		private Character character = null;
		private bool forceStop = false;

		private bool wasControllable = false;
		private bool isCharacterMoving = false;
		
		private int lastmarker = -1;
		
		

        // EXECUTABLE: ----------------------------------------------------------------------------		

		public override bool InstantExecute(GameObject target, IAction[] actions, int index)
		{		
					 
			if (this.waitUntilArrives) return false;
			this.character = this.target.GetCharacter(target);

			Vector3 cPosition = Vector3.zero;
			ILocomotionSystem.TargetRotation cRotation = null;
			float cStopThresh = 0f;

			this.GetTarget(this.character, target, ref cPosition, ref cRotation, ref cStopThresh);
			cStopThresh = Mathf.Max(cStopThresh, this.stopThreshold);

			this.character.characterLocomotion.SetTarget(cPosition, cRotation, cStopThresh);
			return true;
		}
		
		public override IEnumerator Execute(GameObject target, IAction[] actions, int index)
		{							
			this.forceStop = false;
			this.character = this.target.GetCharacter(target);

			Vector3 cPosition  = Vector3.zero;
			ILocomotionSystem.TargetRotation cRotation = null;
			float cStopThresh = this.stopThreshold;

			this.GetTarget(character, target, ref cPosition, ref cRotation, ref cStopThresh);
			cStopThresh = Mathf.Max(cStopThresh, this.stopThreshold);

			this.isCharacterMoving = true;
			this.wasControllable = this.character.characterLocomotion.isControllable;
			this.character.characterLocomotion.SetIsControllable(false);

			this.character.characterLocomotion.SetTarget(cPosition, cRotation, cStopThresh, this.CharacterArrivedCallback);

			bool canceled = false;
			float initTime = Time.time;

			while (this.isCharacterMoving && !canceled && !forceStop)
			{
				if (this.cancelable && (Time.time - initTime) >= this.cancelDelay)
				{
					canceled = Input.anyKey;
				}

				yield return null;
			}

			this.character.characterLocomotion.SetIsControllable(this.wasControllable);

			if (canceled) yield return 999999;
			else yield return 0;
		
		}
		
		public override void Stop()
		{
			this.forceStop = true;
			if (this.character == null) return;

			this.character.characterLocomotion.SetIsControllable(this.wasControllable);
			this.character.characterLocomotion.Stop();
		}

		public void CharacterArrivedCallback()
		{
			this.isCharacterMoving = false;
		}

		private void GetTarget(Character targetCharacter, GameObject invoker, 
			ref Vector3 cPosition, ref ILocomotionSystem.TargetRotation cRotation, ref float cStopThresh)
		{							
			cStopThresh = 0.0f;
			switch (this.moveTo)
			{				
				case MOVE_TO.Object:
						
					ListVariables list = this.gameObjectToCheck.GetComponent<ListVariables>();
					//Debug.Log(gameObjectToCheck.name);
					//Debug.Log(list);
					
					if (list == null || list.variables.Count == 0) break;
					//Debug.Log("Count:"+list.variables.Count);									

					int rn = Random.Range(0, list.variables.Count);
					
					Debug.Log("Random:"+rn);
					Debug.Log("Last:"+lastmarker);
					
					if(lastmarker != rn){
						lastmarker = rn;
						
						GameObject marker = list.variables[rn].Get() as GameObject;	
						NavigationMarker varMarker = marker.GetComponent<NavigationMarker>();
						
						Debug.Log("Marker:"+varMarker);					
						
						if (varMarker != null)
						{
							cPosition = varMarker.transform.position;
							cRotation = new ILocomotionSystem.TargetRotation(true, varMarker.transform.forward);
							cStopThresh = varMarker.stopThreshold;
							
						}else cPosition = gameObjectToCheck.transform.position; 											
													
					} else cPosition = this.character.transform.position;			
					
					//Get the markers in the list
					/*for (int i = 0; i < list.variables.Count; ++i)
					{
						GameObject item = list.variables[i].Get() as GameObject;
						if (item == null) continue;
						Debug.Log(item.name);
						
					}*/				 				
					 
				break;
			}
		}
		
        // +--------------------------------------------------------------------------------------+
        // | EDITOR                                                                               |
        // +--------------------------------------------------------------------------------------+

       		#if UNITY_EDITOR

		public static new string NAME = "Razo/Move Character To Random Marker";
		private const string NODE_TITLE = "Move {0} to Random Marker in {1} {2}";

		private static readonly GUIContent GC_CANCEL = new GUIContent("Cancelable");

		// PROPERTIES: ----------------------------------------------------------------------------

		private SerializedProperty spTarget;
		private SerializedProperty spMoveTo;
		private SerializedProperty spWaitUntilArrives;
		private SerializedProperty spObject;

		private SerializedProperty spStopThreshold;
		private SerializedProperty spCancelable;
		private SerializedProperty spCancelDelay;

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle()
		{
			string value = "none";
			switch (this.moveTo)
			{				
				case MOVE_TO.Object :
					value = (this.marker == null ? "nothing" : this.marker.gameObject.name);
					break;
			}

			return string.Format(
				NODE_TITLE, 
				this.target,
				this.moveTo, 
				value
			);
		}

		protected override void OnEnableEditorChild ()
		{
			this.spTarget = this.serializedObject.FindProperty("target");
			this.spMoveTo = this.serializedObject.FindProperty("moveTo");
			this.spWaitUntilArrives = this.serializedObject.FindProperty("waitUntilArrives");
			this.spObject = this.serializedObject.FindProperty("gameObjectToCheck");
			this.spStopThreshold = this.serializedObject.FindProperty("stopThreshold");
			this.spCancelable = this.serializedObject.FindProperty("cancelable");
			this.spCancelDelay = this.serializedObject.FindProperty("cancelDelay");
		}

		protected override void OnDisableEditorChild ()
		{
			return;
		}

		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();

			EditorGUILayout.PropertyField(this.spTarget);
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(this.spMoveTo);

			switch ((MOVE_TO)this.spMoveTo.intValue)
			{
				case MOVE_TO.Object    : EditorGUILayout.PropertyField(this.spObject);    break;
			}

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(this.spStopThreshold);
			EditorGUILayout.PropertyField(this.spWaitUntilArrives);
			if (this.spWaitUntilArrives.boolValue)
			{
				Rect rect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.textField);
				Rect rectLabel = new Rect(
					rect.x,
					rect.y,
					EditorGUIUtility.labelWidth,
					rect.height
				);
				Rect rectCancenable = new Rect(
					rectLabel.x + rectLabel.width,
					rectLabel.y,
					20f,
					rectLabel.height
				);
				Rect rectDelay = new Rect(
					rectCancenable.x + rectCancenable.width,
					rectCancenable.y,
					rect.width - (rectLabel.width + rectCancenable.width),
					rectCancenable.height
				);

				EditorGUI.LabelField(rectLabel, GC_CANCEL);
				EditorGUI.PropertyField(rectCancenable, this.spCancelable, GUIContent.none);

				EditorGUI.BeginDisabledGroup(!this.spCancelable.boolValue);
				EditorGUI.PropertyField(rectDelay, this.spCancelDelay, GUIContent.none);
				EditorGUI.EndDisabledGroup();
			}

			this.serializedObject.ApplyModifiedProperties();
		}

		#endif
	}
}
