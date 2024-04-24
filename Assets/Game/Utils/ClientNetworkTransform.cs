using Unity.Netcode.Components;

namespace Game.Utils
{
    public class ClientNetworkTransform : NetworkTransform
    {
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            CanCommitToTransform = IsOwner;
        }

        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }

        private void TryCommitTransformToServer()
        {
            if (NetworkManager)
            {
                if (NetworkManager.IsConnectedClient || NetworkManager.IsListening)
                {
                    if (CanCommitToTransform)
                    {
                        TryCommitTransformToServer(transform, NetworkManager.LocalTime.Time);
                    }
                }
            }
        }
        
        protected override void Update()
        {
            CanCommitToTransform = IsOwner;

            base.Update();

            TryCommitTransformToServer();
        }
    }
}