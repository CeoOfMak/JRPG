using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using JRPGGame.Core;
using JRPGGame.BattleSystem;

namespace JRPGGame.WorldNavigator
{
    public class WorldNavigator : MonoBehaviour
    {
        private static WorldNavigator instance;
        public static WorldNavigator Instance => instance;

        [Header("Current Location")]
        [SerializeField] private LocationData currentLocation;
        [SerializeField] private LocationType currentLocationType;

        [Header("Encounter Settings")]
        [SerializeField] private float encounterCheckInterval = 1f;
        [SerializeField] private float baseEncounterRate = 0.05f;
        [SerializeField] private int stepsSinceLastEncounter = 0;

        [Header("Events")]
        public UnityEvent<LocationData> OnLocationChanged;
        public UnityEvent<EncounterType> OnEncounterTriggered;
        public UnityEvent OnSceneTransitionStart;
        public UnityEvent OnSceneTransitionEnd;

        private bool isTransitioning = false;
        private float encounterTimer = 0f;

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
        }

        private void Update()
        {
            if (currentLocationType == LocationType.Overworld || currentLocationType == LocationType.Dungeon)
            {
                CheckRandomEncounters();
            }
        }

        // Location Management
        public void TravelToLocation(LocationData location)
        {
            if (location == null)
            {
                Debug.LogWarning("Cannot travel to null location!");
                return;
            }

            if (isTransitioning)
            {
                Debug.LogWarning("Already transitioning to another location!");
                return;
            }

            StartCoroutine(TransitionToLocation(location));
        }

        private IEnumerator TransitionToLocation(LocationData location)
        {
            isTransitioning = true;
            OnSceneTransitionStart?.Invoke();

            // Fade out or show loading screen
            yield return new WaitForSeconds(0.5f);

            // Load the scene
            if (!string.IsNullOrEmpty(location.sceneName))
            {
                AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(location.sceneName);
                while (!asyncLoad.isDone)
                {
                    yield return null;
                }
            }

            currentLocation = location;
            currentLocationType = location.locationType;

            OnLocationChanged?.Invoke(location);
            OnSceneTransitionEnd?.Invoke();

            isTransitioning = false;

            // Reset encounter counter
            stepsSinceLastEncounter = 0;
        }

        public void TravelToLocationByID(string locationID)
        {
            // You would load the location from a database or resources
            Debug.Log($"Travel to location: {locationID}");
        }

        // Encounter System
        private void CheckRandomEncounters()
        {
            if (currentLocation == null || !currentLocation.hasRandomEncounters)
            {
                return;
            }

            // Check if player is moving
            if (PlayerController.PlayerController.Instance != null &&
                PlayerController.PlayerController.Instance.IsMoving)
            {
                encounterTimer += Time.deltaTime;

                if (encounterTimer >= encounterCheckInterval)
                {
                    encounterTimer = 0f;
                    stepsSinceLastEncounter++;

                    float encounterChance = CalculateEncounterChance();

                    if (UnityEngine.Random.value < encounterChance)
                    {
                        TriggerRandomEncounter();
                    }
                }
            }
        }

        private float CalculateEncounterChance()
        {
            // Increase chance with each step
            float chance = baseEncounterRate * currentLocation.encounterRate;
            chance += (stepsSinceLastEncounter * 0.01f);
            return Mathf.Clamp(chance, 0f, 0.5f);
        }

        private void TriggerRandomEncounter()
        {
            stepsSinceLastEncounter = 0;
            OnEncounterTriggered?.Invoke(EncounterType.Random);

            // Start battle with random enemies from this location
            StartBattle(currentLocation.GetRandomEnemyGroup());
        }

        public void TriggerScriptedEncounter(EnemyGroup enemyGroup)
        {
            OnEncounterTriggered?.Invoke(EncounterType.Scripted);
            StartBattle(enemyGroup);
        }

        public void TriggerBossEncounter(EnemyGroup bossGroup)
        {
            OnEncounterTriggered?.Invoke(EncounterType.Boss);
            StartBattle(bossGroup);
        }

        private void StartBattle(EnemyGroup enemyGroup)
        {
            if (enemyGroup == null)
            {
                Debug.LogWarning("No enemy group to battle!");
                return;
            }

            // Transition to battle scene
            StartCoroutine(TransitionToBattle(enemyGroup));
        }

        private IEnumerator TransitionToBattle(EnemyGroup enemyGroup)
        {
            isTransitioning = true;
            OnSceneTransitionStart?.Invoke();

            yield return new WaitForSeconds(0.5f);

            // Load battle scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("BattleScene");
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // Initialize battle
            // This would be handled by BattleManager
            Debug.Log($"Starting battle with: {enemyGroup.groupName}");

            OnSceneTransitionEnd?.Invoke();
            isTransitioning = false;
        }

        // Teleportation
        public void Teleport(Vector3 position)
        {
            if (PlayerController.PlayerController.Instance != null)
            {
                PlayerController.PlayerController.Instance.transform.position = position;
            }
        }

        public void TeleportToWaypoint(string waypointID)
        {
            // Find waypoint by ID and teleport
            GameObject waypoint = GameObject.Find(waypointID);
            if (waypoint != null)
            {
                Teleport(waypoint.transform.position);
            }
        }

        // Getters
        public LocationData GetCurrentLocation()
        {
            return currentLocation;
        }

        public LocationType GetCurrentLocationType()
        {
            return currentLocationType;
        }

        public bool IsInSafeZone()
        {
            return currentLocationType == LocationType.Town;
        }
    }
}
