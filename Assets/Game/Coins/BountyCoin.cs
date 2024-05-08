namespace Game.Coins
{
    public class BountyCoin : Coin
    {
        public override int Collect()
        {
            if (IsServer)
            {
                if (AlreadyCollected) return 0;
                
                AlreadyCollected = true;
                
                Destroy(gameObject);
                
                return CoinValue;
            }
            
            Show(false);
            
            return 0;
        }
    }
}