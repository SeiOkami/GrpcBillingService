using Billing;

namespace GrpcBillingService.BillingBase
{
    public interface IBillingBase
    {
        public List<UserProfile> ListUsers();
        public void AddUser(string name, int rating);
        public Response CoinsEmission(EmissionAmount amount);
        public Response MoveCoins(MoveCoinsTransaction moveCoinsTransaction);
        public Coin LongestHistoryCoin();
    }
}
