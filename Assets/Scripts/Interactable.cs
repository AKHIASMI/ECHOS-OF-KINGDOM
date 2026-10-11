using UnityEngine;

// Base class for anything the player can use with the E key.
public abstract class Interactable : MonoBehaviour
{
    [SerializeField] protected string promptText = "Interact";

    public virtual bool CanInteract => true;
    public virtual string GetPrompt() => "Press E - " + promptText;
    public abstract void Interact();
}
