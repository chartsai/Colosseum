using UnityEngine;
using System.Collections;


public class FootstepVoice : StateMachineBehaviour {

	public AudioSource footStep;

//	private AudioSource source;
//	private float volLowRange = .5f;
//	private float volHighRange = 1.0f;

	// OnStateEnter is called before OnStateEnter is called on any state inside this state machine
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//	float vol = Random.Range (volLowRange, volHighRange);
	//	gameObject.SetActive(false);
		footStep = Instantiate (footStep);
		if (footStep.isPlaying) {
		//	footStep.Stop ();
			footStep.Play ();
			footStep.PlayDelayed(0.5f);
		} else {
			footStep.Play ();
		}
			
		Debug.Log ("food Step Playing voice");	
	}

	// OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		footStep = Instantiate (footStep);
		if (footStep.isPlaying) {
			//footStep.Stop ();
			///			footStep.Play ();
			footStep.PlayDelayed(1f);
		} else {
			//footStep.Play ();
		}
	}

	// OnStateExit is called before OnStateExit is called on any state inside this state machine
	//override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateMove is called before OnStateMove is called on any state inside this state machine
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateIK is called before OnStateIK is called on any state inside this state machine
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateMachineEnter is called when entering a statemachine via its Entry Node
	//override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash){
	//
	//}

	// OnStateMachineExit is called when exiting a statemachine via its Exit Node
	override public void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
		if (footStep.isPlaying) {
			footStep.Stop ();
		}
		while (!footStep.isPlaying) {
			footStep.Stop ();
		}
			
		Debug.Log ("food Step Stop voice");	
	}
}
