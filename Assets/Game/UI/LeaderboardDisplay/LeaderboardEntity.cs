using System;
using Unity.Collections;
using Unity.Netcode;

namespace Game.UI.LeaderboardDisplay
{
    [Serializable]
    public struct LeaderboardEntity : INetworkSerializable, IEquatable<LeaderboardEntity>
    {
        public ulong clientId;
        public int teamIndex;
        public FixedString32Bytes playerName;
        public int score;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref clientId);
            serializer.SerializeValue(ref teamIndex);
            serializer.SerializeValue(ref playerName);
            serializer.SerializeValue(ref score);
        }

        public bool Equals(LeaderboardEntity other)
        {
            return clientId == other.clientId &&
                   teamIndex == other.teamIndex &&
                   playerName.Equals(other.playerName) &&
                   score == other.score;
        }
    }
}