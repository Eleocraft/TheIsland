using UnityEngine;

public class HarvestingHitCallback : StateMachineBehaviour
{
    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       PlayerData.GameObject.GetComponent<HarvestingActionController>().EndPlaying();
    }
}
