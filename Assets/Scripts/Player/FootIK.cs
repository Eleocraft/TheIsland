using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FootIK : MonoBehaviour
{
    private Animator animator;
    [SerializeField] [Range(0, 0.2f)] private float distanceToGround;
    [SerializeField] private float additionalRaycastRange;
    [SerializeField] [Range(0, 1)] private float checkheightAboveFoot;
    [SerializeField] private float ikWeightDistance;
    [SerializeField] private LayerMask groundLayer;
    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    private void OnAnimatorIK(int layerIndex)
    {
        SetIK(AvatarIKGoal.LeftFoot);
        SetIK(AvatarIKGoal.RightFoot);
    }
    void SetIK(AvatarIKGoal goal)
    {
        if (Physics.Raycast(animator.GetIKPosition(goal) + Vector3.up * checkheightAboveFoot, Vector3.down, out RaycastHit hit, distanceToGround + checkheightAboveFoot + additionalRaycastRange, groundLayer))
        {
            float weight = Mathf.Clamp01(1f / Mathf.Max(animator.GetIKPosition(goal).y - (hit.point.y + distanceToGround), 0) / ikWeightDistance);
            animator.SetIKRotationWeight(goal, weight);
            animator.SetIKPositionWeight(goal, weight);

            Vector3 footPos = hit.point + Vector3.up * distanceToGround;
            animator.SetIKPosition(goal, footPos);
            animator.SetIKRotation(goal, Quaternion.FromToRotation(Vector3.up, hit.normal) * Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0));
        }
    }
}
