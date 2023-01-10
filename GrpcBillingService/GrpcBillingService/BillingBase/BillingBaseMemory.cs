using Billing;
using GrpcBillingService.BillingBase;

public class BillingBaseMemory : IBillingBase
{
    #region Fields

    private List<UserProfile> users = new();
    private Dictionary<Coin, UserProfile> coinsOwners = new();
    private List<CoinHistoryRecord> historyCoins = new();
    private List<UserRatingRecord> usersRatings = new();
    private int lastCoinID = 0;

    #endregion

    #region Structs

    struct CoinHistoryRecord
    {
        public Coin Coin { get; }
        public UserProfile User { get; }
        public CoinHistoryRecord(Coin coin, UserProfile user)
        {
            Coin = coin;
            User = user;
        }
    }

    struct UserRatingRecord
    {
        public UserProfile User { get; set; }
        public long Rating { get; set; }

        public UserRatingRecord(UserProfile user, int rating)
        {
            User = user;
            Rating = rating;
        }
    }

    #endregion

    #region Constructor

    public BillingBaseMemory()
    {
        AddUser("boris", 5000);
        AddUser("maria", 1000);
        AddUser("oleg", 800);
    }

    #endregion

    #region Public

    public void AddUser(string name, int rating)
    {
        var user = new UserProfile() { Name = name };
        users.Add(user);
        usersRatings.Add(new(user, rating));
    }

    public Response CoinsEmission(EmissionAmount amount)
    {
        if (amount.Amount < users.Count)
            return new Response
            {
                Status = Response.Types.Status.Failed,
                Comment = "Not enough coins"
            };

        var parts = PartsCoinsEmission(amount);

        string comment = "Users received coins. ";

        foreach (var item in parts)
        {
            AddNewCoins(item.Key, item.Value);
            comment += $"{item.Key.Name}: {item.Value}, ";
        }

        return new Response
        {
            Status = Response.Types.Status.Ok,
            Comment = comment
        };
    }

    public List<UserProfile> ListUsers() => users;

    public Coin LongestHistoryCoin()
    {
        var select = historyCoins.GroupBy(rec => rec.Coin)
                        .Select(group => new
                        {
                            Coin = group.Key,
                            Count = group.Count(),
                            History = group.Aggregate("Users:",
                                (first, next) => $"{first} → {next.User.Name}")
                        })
                        .OrderByDescending(x => x.Count)
                        .FirstOrDefault();

        return new Coin()
        {
            Id = select?.Coin?.Id ?? 0,
            History = select?.History ?? "",
        };
    }

    public Response MoveCoins(MoveCoinsTransaction request)
    {
        try
        {
            MoveCoinsBetweenUsers(request);
            return new Response() 
            { 
                Status = Response.Types.Status.Ok 
            };
        }
        catch (Exception ex)
        {
            return new Response()
            {
                Comment = ex.Message,
            };
        }

    }

    #endregion

    #region Private

    private void AddNewCoins(UserProfile user, long amount)
    {
        for (long i = 0; i < amount; i++)
            AddCoin(user);
    }
    private void AddCoin(UserProfile user)
    {
        var coin = new Coin();
        coin.Id = ++lastCoinID;
        AddCoin(user, coin);
    }

    private void AddCoin(UserProfile user, Coin coin)
    {
        var owner = coinsOwners.GetValueOrDefault(coin);
        if (owner != null)
            owner.Amount--;

        coinsOwners[coin] = user;
        historyCoins.Add(new CoinHistoryRecord(coin, user));
        user.Amount++;
    }

    private void MoveCoinsBetweenUsers(MoveCoinsTransaction request)
    {
        var userSrc = UserByName(request.SrcUser);
        var userDst = UserByName(request.DstUser);

        if (userSrc.Amount < request.Amount)
            throw new Exception("Not enough coins");

        var coins = (from rec in coinsOwners
                     where rec.Value == userSrc
                     select rec.Key);

        long amount = request.Amount;
        foreach (var coin in coins)
        {
            AddCoin(userDst, coin);
            amount--;
            if (amount == 0)
                break;
        }
    }

    private UserProfile UserByName(string name)
    {
        var user = users.FirstOrDefault(u => u.Name == name);
        if (user == null)
            throw new Exception($"User '{name}' not found!");
        return user;
    }

    private Dictionary<UserProfile, long> PartsCoinsEmission(EmissionAmount amount)
    {
        var ratings = (from r in usersRatings
                       orderby r.Rating descending
                       select r).ToList();

        var coins = new long[usersRatings.Count];
        var cents = new decimal[usersRatings.Count];

        decimal coefficient = (decimal)ratings.Sum(x => x.Rating) / (decimal)amount.Amount;

        for (int i = 0; i < coins.Length; i++)
        {
            var thisRating = ratings[i].Rating / coefficient;
            var countCoins = (long)Math.Truncate(thisRating);
            cents[i] = thisRating - countCoins;
            coins[i] = countCoins < 1 ? 1 : countCoins;
        }

        var remainder = amount.Amount - coins.Sum();
        if (remainder != 0)
        {
            int index = (remainder < 0) ? 0 :
                Array.IndexOf(cents, cents.Max());
            coins[index] += remainder;
        } 

        var result = new Dictionary<UserProfile, long>();
        for (int i = 0; i < ratings.Count; i++)
            result.Add(ratings[i].User, coins[i]);

        return result;

    }

    #endregion

}
