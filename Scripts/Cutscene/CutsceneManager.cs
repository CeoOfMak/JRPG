using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace JRPGGame.Cutscene
{
    public class CutsceneManager : MonoBehaviour
    {
        private static CutsceneManager instance;
        public static CutsceneManager Instance => instance;

        [Header("Current Cutscene")]
        [SerializeField] private CutsceneData currentCutscene;
        [SerializeField] private bool isCutscenePlaying;

        [Header("Playable Director")]
        [SerializeField] private PlayableDirector playableDirector;

        [Header("Events")]
        public UnityEvent<CutsceneData> OnCutsceneStarted;
        public UnityEvent OnCutsceneEnded;
        public UnityEvent<string> OnCutsceneEvent;

        public bool IsCutscenePlaying => isCutscenePlaying;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            if (playableDirector == null)
            {
                playableDirector = GetComponent<PlayableDirector>();
            }
        }

        public void PlayCutscene(CutsceneData cutscene)
        {
            if (cutscene == null)
            {
                Debug.LogWarning("Cannot play null cutscene!");
                return;
            }

            if (isCutscenePlaying)
            {
                Debug.LogWarning("A cutscene is already playing!");
                return;
            }

            currentCutscene = cutscene;
            StartCoroutine(PlayCutsceneCoroutine(cutscene));
        }

        private IEnumerator PlayCutsceneCoroutine(CutsceneData cutscene)
        {
            isCutscenePlaying = true;
            OnCutsceneStarted?.Invoke(cutscene);

            // Disable player controls
            if (PlayerController.PlayerController.Instance != null)
            {
                PlayerController.PlayerController.Instance.SetControlsEnabled(false);
            }

            // Play Timeline if available
            if (cutscene.timelineAsset != null && playableDirector != null)
            {
                playableDirector.playableAsset = cutscene.timelineAsset;
                playableDirector.Play();

                // Wait for timeline to finish
                while (playableDirector.state == PlayState.Playing)
                {
                    yield return null;
                }
            }
            else
            {
                // Manual cutscene playback
                yield return StartCoroutine(PlayManualCutscene(cutscene));
            }

            // Execute end events
            foreach (var eventTrigger in cutscene.endEvents)
            {
                OnCutsceneEvent?.Invoke(eventTrigger);
            }

            EndCutscene();
        }

        private IEnumerator PlayManualCutscene(CutsceneData cutscene)
        {
            // Simple camera movement and dialogue display
            foreach (var action in cutscene.cutsceneActions)
            {
                yield return StartCoroutine(ExecuteCutsceneAction(action));
            }
        }

        private IEnumerator ExecuteCutsceneAction(CutsceneAction action)
        {
            switch (action.actionType)
            {
                case CutsceneActionType.Dialogue:
                    // Show dialogue
                    Debug.Log($"Dialogue: {action.dialogueText}");
                    yield return new WaitForSeconds(action.duration);
                    break;

                case CutsceneActionType.MoveCamera:
                    // Move camera to target
                    Debug.Log($"Move camera to: {action.targetPosition}");
                    yield return new WaitForSeconds(action.duration);
                    break;

                case CutsceneActionType.SpawnObject:
                    // Spawn object
                    if (action.objectPrefab != null)
                    {
                        Instantiate(action.objectPrefab, action.targetPosition, Quaternion.identity);
                    }
                    yield return new WaitForSeconds(action.duration);
                    break;

                case CutsceneActionType.PlayAnimation:
                    // Play animation on target
                    Debug.Log($"Play animation: {action.animationName}");
                    yield return new WaitForSeconds(action.duration);
                    break;

                case CutsceneActionType.Wait:
                    yield return new WaitForSeconds(action.duration);
                    break;

                case CutsceneActionType.FadeOut:
                    // Fade screen to black
                    yield return new WaitForSeconds(action.duration);
                    break;

                case CutsceneActionType.FadeIn:
                    // Fade in from black
                    yield return new WaitForSeconds(action.duration);
                    break;
            }
        }

        public void SkipCutscene()
        {
            if (!isCutscenePlaying || !currentCutscene.canSkip)
            {
                return;
            }

            if (playableDirector != null && playableDirector.state == PlayState.Playing)
            {
                playableDirector.Stop();
            }

            StopAllCoroutines();
            EndCutscene();
        }

        private void EndCutscene()
        {
            isCutscenePlaying = false;

            // Re-enable player controls
            if (PlayerController.PlayerController.Instance != null)
            {
                PlayerController.PlayerController.Instance.SetControlsEnabled(true);
            }

            OnCutsceneEnded?.Invoke();
            currentCutscene = null;
        }
    }

    [Serializable]
    public class CutsceneAction
    {
        public CutsceneActionType actionType;
        public float duration = 1f;
        public string dialogueText;
        public string animationName;
        public Vector3 targetPosition;
        public GameObject objectPrefab;
    }

    public enum CutsceneActionType
    {
        Dialogue,
        MoveCamera,
        SpawnObject,
        PlayAnimation,
        Wait,
        FadeOut,
        FadeIn
    }
}
