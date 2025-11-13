using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace JRPGGame.Cutscene
{
    [CreateAssetMenu(fileName = "New Cutscene", menuName = "JRPG/Cutscene/Cutscene")]
    public class CutsceneData : ScriptableObject
    {
        [Header("Cutscene Info")]
        public string cutsceneID;
        public string cutsceneName;
        [TextArea(3, 5)]
        public string description;

        [Header("Timeline")]
        public PlayableAsset timelineAsset;

        [Header("Manual Cutscene")]
        public List<CutsceneAction> cutsceneActions = new List<CutsceneAction>();

        [Header("Settings")]
        public bool canSkip = true;
        public bool playOnce = true;

        [Header("Events")]
        public List<string> endEvents = new List<string>();
    }
}
