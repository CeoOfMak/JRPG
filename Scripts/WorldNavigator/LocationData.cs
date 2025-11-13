using System.Collections.Generic;
using UnityEngine;
using JRPGGame.Core;

namespace JRPGGame.WorldNavigator
{
    [CreateAssetMenu(fileName = "New Location", menuName = "JRPG/World/Location")]
    public class LocationData : ScriptableObject
    {
        [Header("Location Info")]
        public string locationID;
        public string locationName;
        [TextArea(3, 5)]
        public string description;

        [Header("Scene")]
        public string sceneName;
        public LocationType locationType;

        [Header("Encounters")]
        public bool hasRandomEncounters = true;
        [Range(0f, 2f)]
        public float encounterRate = 1f;
        public List<EnemyGroup> possibleEncounters = new List<EnemyGroup>();

        [Header("Music")]
        public AudioClip backgroundMusic;

        public EnemyGroup GetRandomEnemyGroup()
        {
            if (possibleEncounters == null || possibleEncounters.Count == 0)
            {
                return null;
            }

            return possibleEncounters[Random.Range(0, possibleEncounters.Count)];
        }
    }

    [System.Serializable]
    public class EnemyGroup
    {
        public string groupName;
        public List<EnemyData> enemies = new List<EnemyData>();
    }

    [System.Serializable]
    public class EnemyData
    {
        public string enemyID;
        public GameObject enemyPrefab;
        public int level = 1;
    }
}
