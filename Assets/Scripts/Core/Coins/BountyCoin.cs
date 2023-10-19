public class BountyCoin : Coin{
    override public int Collect(){
        if (!IsServer){
            Show(false);
            return 0;
        }
        if (alreadyCollected) return 0;
        alreadyCollected = true;
        Destroy(gameObject);
        return coinValue;
    }
}