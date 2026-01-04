using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class InteractableObject : NetworkBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private GameObject interactionLinkedObject;
    [SerializeField] private string interactionPrompt = "Use";
    [SerializeField] private InteractableType interactionType = InteractableType.Generic;
    [SerializeField, Tooltip("Check only if you using for a box lid")] private bool isBoxLid = false;
    
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
    [SerializeField] private float slideDistance = 0.5f;
    [SerializeField] private float moveingDrawerSpeed = 1f;
    [SerializeField] private Vector3 slideDirection = Vector3.forward;

    private Vector3 closedPosition;
    private Vector3 openPosition;

    [Header("Interaction/Note Settings")]
    [SerializeField] private List<Image> notesImgs = new List<Image>();
    [SerializeField] private GameObject notesPanel;
    [SerializeField] private int noteIndex = 0;

    [Header("Interaction/Puzzle Settings")]
    [SerializeField] private GameObject puzzlePanel;
    private NetworkVariable<bool> isPuzzleActive = new NetworkVariable<bool>(false);

    [Header("UI References")]
    [SerializeField] private TMP_Text interactionUIText;

    [Header("Additional References")]
    private PlayerInteract playerInteract;
    private DialogueTrigger dialogueTrigger;

    [Header("Objective Properties")]
    public bool IsInteracted;

    public enum InteractableType { Door, Drawer, Note, Puzzle, NPC, Generic }

    private void Start()
    {
        playerInteract = GameObject.FindWithTag("Player").GetComponent<PlayerInteract>();
        dialogueTrigger = GetComponent<DialogueTrigger>();

        InitializeUI();
        InitializeDoor();
        InitializeDrawer();

        isPuzzleActive.OnValueChanged += OnPuzzleStateChanged;
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

    private void InitializeUI()
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
            closedRotation = interactionLinkedObject.transform.localRotation;
            openRotation = closedRotation * Quaternion.Euler(rotationAxis * openAngle);
            isInitialized = true;
        }
    }
    
    private void InitializeDrawer()
    {
        if (interactionType == InteractableType.Drawer && interactionLinkedObject != null)
        {
            closedPosition = interactionLinkedObject.transform.localPosition;
            Vector3 localSlideDirection = interactionLinkedObject.transform.TransformDirection(slideDirection.normalized);
            localSlideDirection = interactionLinkedObject.transform.InverseTransformDirection(localSlideDirection);
            openPosition = closedPosition + localSlideDirection * slideDistance;
            isInitialized = true;
        }
        else if (interactionType == InteractableType.Drawer && interactionLinkedObject == null)
        {
            Debug.LogError("No linked object assigned for drawer!");
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
                if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
                {
                    // Client requests to open puzzle
                    RequestShowPuzzleServerRpc();
                }
                else if (NetworkManager.Singleton.IsHost)
                {
                    // Host can directly control
                    ShowPuzzleForClient();
                }
                break;
            case InteractableType.Generic:
                HandleGenericInteraction();
                break;
            case InteractableType.NPC:
                dialogueTrigger?.TriggerDialogue();
                break;
        }

        if (!IsInteracted && InteractableType.Door != interactionType && InteractableType.Drawer != interactionType)
        {
            IsInteracted = true;
            if (ChapterController.Instance != null)
                ChapterController.Instance.AdvanceObjective();
        }
    }

    private void OnPuzzleStateChanged(bool oldValue, bool newValue)
    {
        if (puzzlePanel != null)
        {
            puzzlePanel.SetActive(newValue);
            Debug.Log($"Puzzle panel state changed: {newValue} (IsClient: {NetworkManager.Singleton.IsClient}, IsHost: {NetworkManager.Singleton.IsHost})");
        }
    }

    // ===== HOST BUTTON METHODS =====
    // Call this from the host's UI button
    public void HostOpenPuzzleForClient()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            Debug.Log("❌ Only host can call this!");
            return;
        }
        
        Debug.Log("👑 Host button clicked - showing puzzle for client");
        ShowPuzzleForClient();
    }

    private void ShowPuzzleForClient()
    {
        if (IsServer)
        {
            isPuzzleActive.Value = true;
        }
    }

    private void HidePuzzleForClient()
    {
        if (IsServer)
        {
            isPuzzleActive.Value = false;
        }
    }

    // Client requests host to show puzzle
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestShowPuzzleServerRpc()
    {
        Debug.Log($"📡 Client requested puzzle");
        isPuzzleActive.Value = true;
    }

    // Client requests host to hide puzzle
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestHidePuzzleServerRpc()
    {
        isPuzzleActive.Value = false;
    }

    // Call this from host's UI button to close puzzle
    public void HostClosePuzzleOnClient()
    {
        if (!NetworkManager.Singleton.IsHost) return;
        
        Debug.Log("👑 Host hiding puzzle from client");
        HidePuzzleForClient();
    }

    // ===== EXISTING METHODS (UPDATED) =====

    public void ClosePuzzleInteraction()
    {
        if (interactionType == InteractableType.Puzzle)
        {
            if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
            {
                // Client requests host to hide puzzle
                RequestHidePuzzleServerRpc();
            }
            else if (NetworkManager.Singleton.IsHost)
            {
                // Host can directly hide
                HidePuzzleForClient();
            }
        }
    }

    private void HandleDoorInteraction()
    {
        if (!isInitialized) InitializeDoor();
        if (interactionLinkedObject == null) return;
        
        if (!isMoving)
        {
            isOpen = !isOpen;
            isMoving = true;
            Debug.Log($"Door {(isOpen ? "opening" : "closing")}");
        }
    }

    private void HandleSwingingDoor()
    {
        if (interactionLinkedObject == null) return;

        if (isOpen)
        {
            interactionLinkedObject.transform.localRotation = Quaternion.RotateTowards(
                interactionLinkedObject.transform.localRotation,
                openRotation,
                rotationDoorSpeed * Time.deltaTime
            );

            if (Quaternion.Angle(interactionLinkedObject.transform.localRotation, openRotation) < 1f)
            {
                interactionLinkedObject.transform.localRotation = openRotation;
                isMoving = false;
            }
        }
        else if (!isBoxLid)
        {
            interactionLinkedObject.transform.localRotation = Quaternion.RotateTowards(
                interactionLinkedObject.transform.localRotation,
                closedRotation,
                rotationDoorSpeed * Time.deltaTime
            );

            if (Quaternion.Angle(interactionLinkedObject.transform.localRotation, closedRotation) < 1f)
            {
                interactionLinkedObject.transform.localRotation = closedRotation;
                isMoving = false;
            }
        }
    }

    private void HandleDrawerInteraction()
    {
        if (!isInitialized) InitializeDrawer();
        if (interactionLinkedObject == null) return;
        
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
            interactionLinkedObject.transform.localPosition = Vector3.MoveTowards(
                interactionLinkedObject.transform.localPosition, 
                openPosition, 
                moveingDrawerSpeed * 0.1f * Time.deltaTime
            );
            
            if (Vector3.Distance(interactionLinkedObject.transform.localPosition, openPosition) < 0.001f)
            {
                interactionLinkedObject.transform.localPosition = openPosition;
                isMoving = false;
                Debug.Log("Drawer fully opened");
            }
        }
        else if (!isBoxLid)
        {
            interactionLinkedObject.transform.localPosition = Vector3.MoveTowards(
                interactionLinkedObject.transform.localPosition, 
                closedPosition, 
                moveingDrawerSpeed * 0.1f * Time.deltaTime
            );
            
            if (Vector3.Distance(interactionLinkedObject.transform.localPosition, closedPosition) < 0.001f)
            {
                interactionLinkedObject.transform.localPosition = closedPosition;
                isMoving = false;
                Debug.Log("Drawer fully closed");
            }
        }
    }

    private void HandleNoteInteraction()
    {
        notesPanel.SetActive(true);
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
    
    public void CloseNoteInteraction()
    {
        DisableAllNotes();
        notesPanel.SetActive(false);
    }

    private void HandleGenericInteraction()
    {
        Debug.Log($"Interacted with {gameObject.name}");
    }

    public void SetInteractionPrompt(string newPrompt)
    {
        interactionPrompt = newPrompt;
        if (interactionUIText != null)
            interactionUIText.text = newPrompt;
    }

    private void OnDrawGizmosSelected()
    {
        if (interactionLinkedObject != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(interactionLinkedObject.transform.position, Vector3.one * 0.1f);
            
            if (interactionType == InteractableType.Door)
            {
                Gizmos.color = Color.green;
                Vector3 startDirection = interactionLinkedObject.transform.forward;
                Vector3 endDirection = Quaternion.Euler(rotationAxis * openAngle) * startDirection;
                Gizmos.DrawRay(interactionLinkedObject.transform.position, startDirection * 0.5f);
                Gizmos.DrawRay(interactionLinkedObject.transform.position, endDirection * 0.5f);
            }
            else if (interactionType == InteractableType.Drawer)
            {
                Gizmos.color = Color.blue;
                Vector3 worldSlideDirection = interactionLinkedObject.transform.TransformDirection(slideDirection.normalized);
                Vector3 slideEnd = interactionLinkedObject.transform.position + worldSlideDirection * slideDistance;
                Gizmos.DrawLine(interactionLinkedObject.transform.position, slideEnd);
                Gizmos.DrawWireCube(slideEnd, Vector3.one * 0.05f);
            }
        }
    }
}