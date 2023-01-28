using UnityEngine;

public class RaftSail : MonoBehaviour, IInteractable
{
    private Animator animator;

    public string InteractionInfo => open ? "close sail" : "open sail";
    public bool open { get; private set; }

    void Start()
    {
        animator = GetComponentInParent<Animator>();
    }

    public void Interact()
    {
        open = !open;
        if (open)
            animator.Play("OpenSail");
        else
            animator.Play("CloseSail");
    }
}
