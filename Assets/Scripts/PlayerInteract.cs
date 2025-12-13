using TMPro;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private GameObject interactionUIBtn;
    [SerializeField] private TMP_Text interactionUIText;
    
    private IInteractable currentInteractable;

    private void Start()
    {
        HideInteractionUI();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Other has Collider");
        if (other.TryGetComponent(out IInteractable interactable))
        {
            SetCurrentInteractable(interactable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable) && interactable == currentInteractable)
        {
            ClearCurrentInteractable();
        }
    }

    private void Update()
    {
        // Optional: Add keyboard interaction support
        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        {
            PerformInteraction();
        }
    }

    private void SetCurrentInteractable(IInteractable interactable)
    {
        currentInteractable = interactable;
        ShowInteractionUI(interactable.GetInteractionPrompt());
    }

    public void ClearCurrentInteractable()
    {
        currentInteractable = null;
        HideInteractionUI();
    }

    private void ShowInteractionUI(string prompt)
    {
        if (interactionUIBtn != null)
            interactionUIBtn.SetActive(true);
            
        if (interactionUIText != null)
        {
            interactionUIText.text = prompt;
            interactionUIText.gameObject.SetActive(true);
        }
    }

    private void HideInteractionUI()
    {
        if (interactionUIBtn != null)
            interactionUIBtn.SetActive(false);
            
        if (interactionUIText != null)
            interactionUIText.gameObject.SetActive(false);
    }

    public void OnMobileInteractButtonPressed()
    {
        if (currentInteractable != null)
        {
            PerformInteraction();
            HideInteractionUI();
        }
    }

    private void PerformInteraction()
    {
        Debug.Log($"Player interacted with {currentInteractable}");
        currentInteractable.Interact();
    }
}