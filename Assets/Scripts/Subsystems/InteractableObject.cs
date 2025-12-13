using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private GameObject interactionLinkedObject;
    [SerializeField] private string interactionPrompt = "Use";
    [SerializeField] private InteractableType interactionType = InteractableType.Generic;
    [SerializeField, Tooltip("Check only if you using for a box lid")] private bool isBoxLid = false;
    //[SerializeField] private AudioSource audioSource;

    [Header("Interaction/Door Settings")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float rotationDoorSpeed = 90f;
    [SerializeField] private Vector3 rotationAxis = Vector3.up;

    private bool isOpen = false;
    private bool isMoving = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool isInitialized = false;

    [Header("Interaction/Drawer Settings")]
    [SerializeField] private float slideDistance = 0.5f; // Default drawer slide distance
    [SerializeField] private float moveingDrawerSpeed = 1f;
    [SerializeField] private Vector3 slideDirection = Vector3.forward; // Usually forward for drawers
    // [SerializeField] private AudioClip drawerOpenSound;
    // [SerializeField] private AudioClip drawerCloseSound;

    private Vector3 closedPosition;
    private Vector3 openPosition;

    [Header("Interaction/Note Settings")]
    [SerializeField] private List<Image> notesImgs = new List<Image>();//move to another script later
    [SerializeField] private GameObject notesPanel;
    [SerializeField] private int noteIndex = 0;
    //[SerializeField] private AudioClip noteOpenSound;
    //[SerializeField] private AudioClip noteCloseSound;

    [Header("Interaction/Puzzle Settings")]
    [SerializeField] private GameObject puzzlePanel;
    //[SerializeField] private AudioClip puzzleOpenSound;
    //[SerializeField] private AudioClip puzzleCloseSound;

    [Header("UI References")]
    [SerializeField] private TMP_Text interactionUIText;

    [Header("Additional References")]
    private PlayerInteract playerInteract;
    private DialogueTrigger dialogueTrigger;

    [Header("Objective Properties")]
    public bool IsInteracted;

    public enum InteractableType { Door, Drawer, Note, Puzzle, NPC, Generic }//tell what type of interaction this object has

    private void Start()
    {
        playerInteract = GameObject.FindWithTag("Player").GetComponent<PlayerInteract>();
        dialogueTrigger = GetComponent<DialogueTrigger>();

        InitializeUI();
        InitializeDoor();
        InitializeDrawer();
    }
    
    private void Update()
    {
        if (isMoving)
        {
            switch (interactionType)
            {
                case InteractableType.Door:
                    HandleSwingingDoor();
                    break;
                case InteractableType.Drawer:
                    HandleSlidingDrawer();
                    break;
            }
        }
    }

    private void InitializeUI() //set the interaction prompt text on UI
    {
        if (interactionUIText != null)
        {
            interactionUIText.text = GetInteractionPrompt();
            interactionUIText.gameObject.SetActive(false);
        }
    }

    private void InitializeDoor()
    {
        if (interactionType == InteractableType.Door && interactionLinkedObject != null)
        {
            // Store the initial closed rotation
            closedRotation = interactionLinkedObject.transform.localRotation;

            // Calculate the open rotation
            openRotation = closedRotation * Quaternion.Euler(rotationAxis * openAngle);

            Debug.Log($"Door initialized. Closed: {closedRotation.eulerAngles}, Open: {openRotation.eulerAngles}");
            isInitialized = true;
        }
    }
    
    private void InitializeDrawer()
    {
        if (interactionType == InteractableType.Drawer && interactionLinkedObject != null)
        {
            // Store the initial closed position
            closedPosition = interactionLinkedObject.transform.localPosition;
            
            // Calculate the open position
            // Transform the direction from local to world space, then back to local
            Vector3 localSlideDirection = interactionLinkedObject.transform.TransformDirection(slideDirection.normalized);
            localSlideDirection = interactionLinkedObject.transform.InverseTransformDirection(localSlideDirection);
            openPosition = closedPosition + localSlideDirection * slideDistance;
            
            Debug.Log($"Drawer initialized. Closed: {closedPosition}, Open: {openPosition}");
            Debug.Log($"Slide direction (world): {interactionLinkedObject.transform.TransformDirection(slideDirection.normalized)}");
            isInitialized = true;
        }
        else if (interactionType == InteractableType.Drawer && interactionLinkedObject == null)
        {
            Debug.LogError("No linked object assigned for drawer! Assign the drawer GameObject.");
        }
    }

    public string GetInteractionPrompt() 
    {
        switch (interactionType)
        {
            case InteractableType.Door:
                return isOpen ? "Close" : "Open";
            case InteractableType.Drawer:
                return isOpen ? "Close" : "Open";
            case InteractableType.Note:
                return "Read";
            case InteractableType.Puzzle:
                return "Solve";
            default:
                return interactionPrompt;
        }
    }

    public void Interact()
    {
        Debug.Log($"Interacted with {gameObject.name} ({interactionType})");

        switch (interactionType)
        {
            case InteractableType.Door:
                HandleDoorInteraction();
                if (isBoxLid)
                {
                    GetComponent<Collider>().enabled = false;
                    playerInteract.ClearCurrentInteractable();
                }
                break;
            case InteractableType.Drawer:
                HandleDrawerInteraction();
                if (isBoxLid)
                {
                    GetComponent<Collider>().enabled = false;
                    playerInteract.ClearCurrentInteractable();
                }
                break;
            case InteractableType.Note:
                HandleNoteInteraction();
                break;
            case InteractableType.Puzzle:
                HandlePuzzleInteraction();
                break;
            case InteractableType.Generic:
                HandleGenericInteraction();
                break;
            case InteractableType.NPC:
                // Trigger dialogue or NPC interaction here
                dialogueTrigger?.TriggerDialogue();
                break;
        }

        if (!IsInteracted)
        {
            IsInteracted = true;
            // Notify objective manager or other systems about the interaction
        }
    }


    //* Specific interaction handlers
    /// <summary>
    /// Add dialogue or sound effects later and get the function to play sound and show dialogue for their respective
    /// Manager scripts.
    /// </summary>

    /// <summary>
    /// Starts door interaction (toggles open/close state)
    /// </summary>
    private void HandleDoorInteraction()
    {
        if (!isInitialized)
        {
            InitializeDoor();
        }
        
        if (interactionLinkedObject == null)
        {
            Debug.LogError("No linked object assigned for door!");
            return;
        }

        if (!isMoving)
        {
            isOpen = !isOpen;
            isMoving = true;
            Debug.Log($"Door {(isOpen ? "opening" : "closing")}");
        }
    }

    /// <summary>
    /// Handles the actual door rotation animation
    /// </summary>
    private void HandleSwingingDoor()
    {
        if (interactionLinkedObject == null) return;

        if (isOpen)
        {
            // Opening door - rotate towards open position
            interactionLinkedObject.transform.localRotation = Quaternion.RotateTowards(
                interactionLinkedObject.transform.localRotation,
                openRotation,
                rotationDoorSpeed * Time.deltaTime
            );

            // Check if we've reached the open position
            if (Quaternion.Angle(interactionLinkedObject.transform.localRotation, openRotation) < 1f)
            {
                interactionLinkedObject.transform.localRotation = openRotation;
                isMoving = false;
                //PlaySound(closeSound);
            }
        }
        else if (!isBoxLid)
        {
            // Closing door - rotate towards closed position
            interactionLinkedObject.transform.localRotation = Quaternion.RotateTowards(
                interactionLinkedObject.transform.localRotation,
                closedRotation,
                rotationDoorSpeed * Time.deltaTime
            );

            // Check if we've reached the closed position
            if (Quaternion.Angle(interactionLinkedObject.transform.localRotation, closedRotation) < 1f)
            {
                interactionLinkedObject.transform.localRotation = closedRotation;
                isMoving = false;
                //PlaySound(openSound);
            }
        }
    }

    private void HandleDrawerInteraction()
    {
        if (!isInitialized)
        {
            InitializeDrawer();
        }
        
        if (interactionLinkedObject == null)
        {
            Debug.LogError("No linked object assigned for drawer!");
            return;
        }

        if (!isMoving)
        {
            isOpen = !isOpen;
            isMoving = true;
            Debug.Log($"Drawer {(isOpen ? "opening" : "closing")}");
        }
    }

    private void HandleSlidingDrawer()
    {
        if (interactionLinkedObject == null) return;

        if (isOpen)
        {
            // Opening drawer - move towards open position
            interactionLinkedObject.transform.localPosition = Vector3.MoveTowards(
                interactionLinkedObject.transform.localPosition, 
                openPosition, 
                moveingDrawerSpeed * 0.1f * Time.deltaTime
            );
            
            if (Vector3.Distance(interactionLinkedObject.transform.localPosition, openPosition) < 0.001f)
            {
                interactionLinkedObject.transform.localPosition = openPosition;
                isMoving = false;
                //PlaySound(drawerOpenSound);
                Debug.Log("Drawer fully opened");
            }
        }
        else if (!isBoxLid)
        {
            // Closing drawer - move towards closed position
            interactionLinkedObject.transform.localPosition = Vector3.MoveTowards(
                interactionLinkedObject.transform.localPosition, 
                closedPosition, 
                moveingDrawerSpeed * 0.1f * Time.deltaTime
            );
            
            if (Vector3.Distance(interactionLinkedObject.transform.localPosition, closedPosition) < 0.001f)
            {
                interactionLinkedObject.transform.localPosition = closedPosition;
                isMoving = false;
                //PlaySound(drawerCloseSound);
                Debug.Log("Drawer fully closed");
            }
        }
    }

    private void HandleNoteInteraction()
    {
        // Add note-specific logic here
        notesPanel.SetActive(true);
        //PlaySound(noteOpenSound);
        OpenNote();
    }
    
    private void OpenNote()
    {
        DisableAllNotes();
        for (int i = 0; i < notesImgs.Count; i++)
        {
            notesImgs[i].gameObject.SetActive(i == noteIndex);
        }
    }
    
    private void DisableAllNotes()
    {
        foreach (var img in notesImgs)
        {
            img.gameObject.SetActive(false);
        }
    }
    
    public void CloseNoteInteraction()//move to another script later (UI manager)
    {
        DisableAllNotes();
        notesPanel.SetActive(false);
        //PlaySound(noteCloseSound);
    }

    private void HandlePuzzleInteraction()
    {
        // Add puzzle-specific logic here
        puzzlePanel.SetActive(true);
        //PlaySound(puzzleOpenSound);
    }
    
    public void ClosePuzzleInteraction()//move to another script later (UI manager)
    {
        puzzlePanel.SetActive(false);
        //PlaySound(puzzleCloseSound);
    }

    private void HandleGenericInteraction()
    {
        // Default interaction behavior
        Debug.Log($"Interacted with {gameObject.name}");
    }

    // Optional: Helper methods for specific interactions
    public void SetInteractionPrompt(string newPrompt)
    {
        interactionPrompt = newPrompt;
        if (interactionUIText != null)
            interactionUIText.text = newPrompt;
    }

    // Debug visualization in Scene view
    private void OnDrawGizmosSelected()
    {
        if (interactionLinkedObject != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(interactionLinkedObject.transform.position, Vector3.one * 0.1f);
            
            if (interactionType == InteractableType.Door)
            {
                // Draw rotation arc for doors
                Gizmos.color = Color.green;
                Vector3 startDirection = interactionLinkedObject.transform.forward;
                Vector3 endDirection = Quaternion.Euler(rotationAxis * openAngle) * startDirection;
                Gizmos.DrawRay(interactionLinkedObject.transform.position, startDirection * 0.5f);
                Gizmos.DrawRay(interactionLinkedObject.transform.position, endDirection * 0.5f);
            }
            else if (interactionType == InteractableType.Drawer)
            {
                // Draw slide path for drawers
                Gizmos.color = Color.blue;
                Vector3 worldSlideDirection = interactionLinkedObject.transform.TransformDirection(slideDirection.normalized);
                Vector3 slideEnd = interactionLinkedObject.transform.position + worldSlideDirection * slideDistance;
                Gizmos.DrawLine(interactionLinkedObject.transform.position, slideEnd);
                Gizmos.DrawWireCube(slideEnd, Vector3.one * 0.05f);
            }
        }
    }

    // private void PlaySound(AudioClip clip)
    // {
    //     if (audioSource != null && clip != null)
    //     {
    //         audioSource.PlayOneShot(clip);
    //     }
    // }
}