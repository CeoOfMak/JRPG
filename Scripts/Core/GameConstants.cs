namespace JRPGGame.Core
{
    public static class GameConstants
    {
        // Party limits
        public const int MAX_PLAYER_PARTY_SIZE = 4;
        public const int MAX_ENEMY_PARTY_SIZE = 6;

        // Equipment slots
        public const int MAX_ACCESSORY_SLOTS = 4;

        // Battle constants
        public const float BREAK_STATE_DAMAGE_MULTIPLIER = 1.1f;
        public const int BREAK_STATE_DURATION_TURNS = 1;

        // Save system
        public const string SAVE_FILE_PREFIX = "SaveSlot_";
        public const string AUTOSAVE_FILE_NAME = "AutoSave";
        public const string SAVE_FILE_EXTENSION = ".sav";
        public const int MAX_SAVE_SLOTS = 10;

        // Auto-save intervals (in seconds)
        public const float AUTOSAVE_INTERVAL = 300f; // 5 minutes

        // Level constants
        public const int MAX_LEVEL = 99;
        public const int MIN_LEVEL = 1;

        // Economy
        public const int SELL_PRICE_PERCENTAGE = 50; // Items sell for 50% of purchase price

        // Element effectiveness multipliers
        public const float ELEMENT_SUPER_EFFECTIVE = 1.5f;
        public const float ELEMENT_NOT_EFFECTIVE = 0.5f;
        public const float ELEMENT_IMMUNE = 0f;
        public const float ELEMENT_NORMAL = 1f;
    }
}
