using System;

namespace Game.Net.Shared
{
    [Serializable]
    public class UserData
    {
        public string userName;
        public string userAuthID;
        public int teamIndex = -1;
        public GameInfo userGamePreferences = new GameInfo();
    }

    [Serializable]
    public class GameInfo
    {
        public Map map;
        public GameMode gameMode;
        public GameQueue gameQueue;

        public string ToMultiplayQueue()
        {
            return gameQueue switch
            {
                GameQueue.Solo => "solo-queue",
                GameQueue.Team => "team-queue",
                _ => "solo-queue",
            };
        }
    }
    
    public enum Map
    {
        Default,
    }
    
    public enum GameMode
    {
        Default,
    }
    
    public enum GameQueue
    {
        Solo,
        Team,
    }
}