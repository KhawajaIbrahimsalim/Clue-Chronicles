public interface IInteractable
{
    /// <summary>
    /// Returns the prompt text to display on the UI
    /// </summary>
    string GetInteractionPrompt();

    /// <summary>
    /// Called when the player interacts with this object
    /// </summary>
    void Interact();
}