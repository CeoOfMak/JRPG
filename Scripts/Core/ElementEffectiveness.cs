using System.Collections.Generic;

namespace JRPGGame.Core
{
    public static class ElementEffectiveness
    {
        private static Dictionary<ElementType, Dictionary<ElementType, float>> effectivenessChart;

        static ElementEffectiveness()
        {
            InitializeEffectivenessChart();
        }

        private static void InitializeEffectivenessChart()
        {
            effectivenessChart = new Dictionary<ElementType, Dictionary<ElementType, float>>();

            // Fire effectiveness
            effectivenessChart[ElementType.Fire] = new Dictionary<ElementType, float>
            {
                { ElementType.None, GameConstants.ELEMENT_NORMAL },
                { ElementType.Fire, GameConstants.ELEMENT_NOT_EFFECTIVE },
                { ElementType.Water, GameConstants.ELEMENT_NOT_EFFECTIVE },
                { ElementType.Earth, GameConstants.ELEMENT_SUPER_EFFECTIVE },
                { ElementType.Wind, GameConstants.ELEMENT_NORMAL },
                { ElementType.Lightning, GameConstants.ELEMENT_NORMAL },
                { ElementType.Ice, GameConstants.ELEMENT_SUPER_EFFECTIVE },
                { ElementType.Light, GameConstants.ELEMENT_NORMAL },
                { ElementType.Dark, GameConstants.ELEMENT_NORMAL }
            };

            // Water effectiveness
            effectivenessChart[ElementType.Water] = new Dictionary<ElementType, float>
            {
                { ElementType.None, GameConstants.ELEMENT_NORMAL },
                { ElementType.Fire, GameConstants.ELEMENT_SUPER_EFFECTIVE },
                { ElementType.Water, GameConstants.ELEMENT_NOT_EFFECTIVE },
                { ElementType.Earth, GameConstants.ELEMENT_NOT_EFFECTIVE },
                { ElementType.Wind, GameConstants.ELEMENT_NORMAL },
                { ElementType.Lightning, GameConstants.ELEMENT_NOT_EFFECTIVE },
                { ElementType.Ice, GameConstants.ELEMENT_NORMAL },
                { ElementType.Light, GameConstants.ELEMENT_NORMAL },
                { ElementType.Dark, GameConstants.ELEMENT_NORMAL }
            };

            // Earth effectiveness
            effectivenessChart[ElementType.Earth] = new Dictionary<ElementType, float>
            {
                { ElementType.None, GameConstants.ELEMENT_NORMAL },
                { ElementType.Fire, GameConstants.ELEMENT_NOT_EFFECTIVE },
                { ElementType.Water, GameConstants.ELEMENT_SUPER_EFFECTIVE },
                { ElementType.Earth, GameConstants.ELEMENT_NOT_EFFECTIVE },
                { ElementType.Wind, GameConstants.ELEMENT_NOT_EFFECTIVE },
                { ElementType.Lightning, GameConstants.ELEMENT_SUPER_EFFECTIVE },
                { ElementType.Ice, GameConstants.ELEMENT_NORMAL },
                { ElementType.Light, GameConstants.ELEMENT_NORMAL },
                { ElementType.Dark, GameConstants.ELEMENT_NORMAL }
            };

            // Wind effectiveness
            effectivenessChart[ElementType.Wind] = new Dictionary<ElementType, float>
            {
                { ElementType.None, GameConstants.ELEMENT_NORMAL },
                { ElementType.Fire, GameConstants.ELEMENT_NORMAL },
                { ElementType.Water, GameConstants.ELEMENT_NORMAL },
                { ElementType.Earth, GameConstants.ELEMENT_SUPER_EFFECTIVE },
                { ElementType.Wind, GameConstants.ELEMENT_NOT_EFFECTIVE },
                { ElementType.Lightning, GameConstants.ELEMENT_NORMAL },
                { ElementType.Ice, GameConstants.ELEMENT_NORMAL },
                { ElementType.Light, GameConstants.ELEMENT_NORMAL },
                { ElementType.Dark, GameConstants.ELEMENT_NORMAL }
            };

            // Lightning effectiveness
            effectivenessChart[ElementType.Lightning] = new Dictionary<ElementType, float>
            {
                { ElementType.None, GameConstants.ELEMENT_NORMAL },
                { ElementType.Fire, GameConstants.ELEMENT_NORMAL },
                { ElementType.Water, GameConstants.ELEMENT_SUPER_EFFECTIVE },
                { ElementType.Earth, GameConstants.ELEMENT_NOT_EFFECTIVE },
                { ElementType.Wind, GameConstants.ELEMENT_NORMAL },
                { ElementType.Lightning, GameConstants.ELEMENT_NOT_EFFECTIVE },
                { ElementType.Ice, GameConstants.ELEMENT_NORMAL },
                { ElementType.Light, GameConstants.ELEMENT_NORMAL },
                { ElementType.Dark, GameConstants.ELEMENT_NORMAL }
            };

            // Ice effectiveness
            effectivenessChart[ElementType.Ice] = new Dictionary<ElementType, float>
            {
                { ElementType.None, GameConstants.ELEMENT_NORMAL },
                { ElementType.Fire, GameConstants.ELEMENT_NOT_EFFECTIVE },
                { ElementType.Water, GameConstants.ELEMENT_NORMAL },
                { ElementType.Earth, GameConstants.ELEMENT_NORMAL },
                { ElementType.Wind, GameConstants.ELEMENT_SUPER_EFFECTIVE },
                { ElementType.Lightning, GameConstants.ELEMENT_NORMAL },
                { ElementType.Ice, GameConstants.ELEMENT_NOT_EFFECTIVE },
                { ElementType.Light, GameConstants.ELEMENT_NORMAL },
                { ElementType.Dark, GameConstants.ELEMENT_NORMAL }
            };

            // Light effectiveness
            effectivenessChart[ElementType.Light] = new Dictionary<ElementType, float>
            {
                { ElementType.None, GameConstants.ELEMENT_NORMAL },
                { ElementType.Fire, GameConstants.ELEMENT_NORMAL },
                { ElementType.Water, GameConstants.ELEMENT_NORMAL },
                { ElementType.Earth, GameConstants.ELEMENT_NORMAL },
                { ElementType.Wind, GameConstants.ELEMENT_NORMAL },
                { ElementType.Lightning, GameConstants.ELEMENT_NORMAL },
                { ElementType.Ice, GameConstants.ELEMENT_NORMAL },
                { ElementType.Light, GameConstants.ELEMENT_NOT_EFFECTIVE },
                { ElementType.Dark, GameConstants.ELEMENT_SUPER_EFFECTIVE }
            };

            // Dark effectiveness
            effectivenessChart[ElementType.Dark] = new Dictionary<ElementType, float>
            {
                { ElementType.None, GameConstants.ELEMENT_NORMAL },
                { ElementType.Fire, GameConstants.ELEMENT_NORMAL },
                { ElementType.Water, GameConstants.ELEMENT_NORMAL },
                { ElementType.Earth, GameConstants.ELEMENT_NORMAL },
                { ElementType.Wind, GameConstants.ELEMENT_NORMAL },
                { ElementType.Lightning, GameConstants.ELEMENT_NORMAL },
                { ElementType.Ice, GameConstants.ELEMENT_NORMAL },
                { ElementType.Light, GameConstants.ELEMENT_SUPER_EFFECTIVE },
                { ElementType.Dark, GameConstants.ELEMENT_NOT_EFFECTIVE }
            };

            // None effectiveness (neutral to everything)
            effectivenessChart[ElementType.None] = new Dictionary<ElementType, float>
            {
                { ElementType.None, GameConstants.ELEMENT_NORMAL },
                { ElementType.Fire, GameConstants.ELEMENT_NORMAL },
                { ElementType.Water, GameConstants.ELEMENT_NORMAL },
                { ElementType.Earth, GameConstants.ELEMENT_NORMAL },
                { ElementType.Wind, GameConstants.ELEMENT_NORMAL },
                { ElementType.Lightning, GameConstants.ELEMENT_NORMAL },
                { ElementType.Ice, GameConstants.ELEMENT_NORMAL },
                { ElementType.Light, GameConstants.ELEMENT_NORMAL },
                { ElementType.Dark, GameConstants.ELEMENT_NORMAL }
            };
        }

        public static float GetEffectiveness(ElementType attackElement, ElementType defenseElement)
        {
            if (effectivenessChart.ContainsKey(attackElement) &&
                effectivenessChart[attackElement].ContainsKey(defenseElement))
            {
                return effectivenessChart[attackElement][defenseElement];
            }

            return GameConstants.ELEMENT_NORMAL;
        }

        public static string GetEffectivenessText(float effectiveness)
        {
            if (effectiveness >= GameConstants.ELEMENT_SUPER_EFFECTIVE)
                return "Super Effective!";
            else if (effectiveness <= GameConstants.ELEMENT_NOT_EFFECTIVE && effectiveness > GameConstants.ELEMENT_IMMUNE)
                return "Not Very Effective...";
            else if (effectiveness == GameConstants.ELEMENT_IMMUNE)
                return "No Effect!";
            else
                return "";
        }
    }
}
