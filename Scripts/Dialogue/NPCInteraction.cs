using UnityEngine;
using UnityEngine.Events;
using JRPGGame.Quest;

namespace JRPGGame.Dialogue
{
    public class NPCInteraction : MonoBehaviour
    {
        [Header("NPC Info")]
        [SerializeField] private string npcID;
        [SerializeField] private string npcName;
        [SerializeField] private Sprite npcPortrait;

        [Header("Dialogue")]
        [SerializeField] private DialogueData defaultDialogue;
        [SerializeField] private DialogueData questDialogue;
        [SerializeField] private DialogueData completionDialogue;

        [Header("Quest")]
        [SerializeField] private string associatedQuestID;

        [Header("Interaction")]
        [SerializeField] private float interactionRange = 2f;
        [SerializeField] private KeyCode interactionKey = KeyCode.E;
        [SerializeField] private bool canInteract = true;

        [Header("Events")]
        public UnityEvent OnInteractionStart;
        public UnityEvent OnInteractionEnd;

        private bool playerInRange = false;
        private Transform playerTransform;

        private void Update()
        {
            if (playerInRange && canInteract && Input.GetKeyDown(interactionKey))
            {
                Interact();
            }
        }

        public void Interact()
        {
            if (!canInteract) return;

            OnInteractionStart?.Invoke();

            // Notify quest manager that NPC was talked to
            if (QuestManager.Instance != null && !string.IsNullOrEmpty(npcID))
            {
                QuestManager.Instance.OnNPCTalkedTo(npcID);
            }

            // Determine which dialogue to show
            DialogueData dialogueToShow = GetAppropriateDialogue();

            if (dialogueToShow != null && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(dialogueToShow);
                DialogueManager.Instance.OnDialogueEnded += OnDialogueCompleted;
            }
            else
            {
                Debug.LogWarning($"No dialogue found for NPC: {npcName}");
            }
        }

        private DialogueData GetAppropriateDialogue()
        {
            if (!string.IsNullOrEmpty(associatedQuestID) && QuestManager.Instance != null)
            {
                // Check if quest is completed
                if (QuestManager.Instance.IsQuestCompleted(associatedQuestID) && completionDialogue != null)
                {
                    return completionDialogue;
                }

                // Check if quest is active
                if (QuestManager.Instance.IsQuestActive(associatedQuestID) && questDialogue != null)
                {
                    return questDialogue;
                }

                // Quest not started yet - show default or quest start dialogue
                if (questDialogue != null)
                {
                    return questDialogue;
                }
            }

            return defaultDialogue;
        }

        private void OnDialogueCompleted()
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnDialogueEnded -= OnDialogueCompleted;
            }

            OnInteractionEnd?.Invoke();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = true;
                playerTransform = other.transform;
                ShowInteractionPrompt(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
                playerTransform = null;
                ShowInteractionPrompt(false);
            }
        }

        private void ShowInteractionPrompt(bool show)
        {
            // This would show/hide UI prompt for interaction
            // e.g., "Press E to talk"
        }

        public void SetCanInteract(bool value)
        {
            canInteract = value;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}
