using Billing;
using ConsoleMenu;
using Grpc.Net.Client;
using GrpcBillingClient.Settings;

namespace GrpcBillingClient;

internal class Program
{
    #region Main

    static MenuManager menu = new MenuManager();
    
    static void Main(string[] args)
    {
        menu.Header = "Welcome to The Billing Service";
        menu.AddCommand(ShowListUsers, "ListUsers()");
        menu.AddCommand(CoinsEmission, "CoinsEmission()");
        menu.AddCommand(MoveCoins, "MoveCoins()");
        menu.AddCommand(LongestHistoryCoin, "LongestHistoryCoin()");
        menu.AddCommand(Exit, "Exit");
        menu.Start();
    }

    #endregion

    #region MenuComands

    static async Task ShowListUsers()
    {
        var users = await ListUsers();
        ShowUsers(users);
    }
    
    static async Task CoinsEmission()
    {
        Console.Write("Input amount coins: ");

        long amount = 0;

        string? userInput = Console.ReadLine();
        if (userInput == null)
        {
            return;
        }
        if (!long.TryParse(userInput, out amount))
        {
            Console.WriteLine("Invalid number");
            return;
        }

        var client = BillingClient();

        var response = await client.CoinsEmissionAsync(
            new EmissionAmount() { Amount = amount });

        ConsoleWriteStatus(response);
    }

    static async Task MoveCoins()
    {
        var request = await ReadMoveCoinsTransaction();

        var client = BillingClient();

        var response = await client.MoveCoinsAsync(request);

        ConsoleWriteStatus(response);
    }

    static void LongestHistoryCoin()
    {
        var client = BillingClient();
        var coin = client.LongestHistoryCoin(new None());
        Console.WriteLine($"Id: {coin.Id}; History: {coin.History}");
    }

    static void Exit()
    {
        menu.Stop();
    }

    #endregion

    #region Others

    static void ShowUsers(List<UserProfile> users, bool showNumbers = false)
    {
        for (int i = 0; i < users.Count; i++)
        {
            var user = users[i];

            if (showNumbers)
                Console.Write($"{i + 1}. ");

            Console.WriteLine($"Name: {user.Name}; Amount: {user.Amount}");
        }
    }

    static void ConsoleWriteStatus(Response response)
    {
        Console.WriteLine($"Status: {response.Status}; Comment: {response.Comment}");
    }

    static async Task<MoveCoinsTransaction> ReadMoveCoinsTransaction()
    {
        var users = await ListUsers();

        ShowUsers(users, true);

        var result = new MoveCoinsTransaction();

        Console.WriteLine("Input data: <User number 1> <User number 2> <Amount coins>");
        Console.WriteLine("For example: 1 2 3");

        string inputCommand = Console.ReadLine() ?? "";

        var commandParts = inputCommand.Split(' ');
        result.SrcUser = users[int.Parse(commandParts[0]) - 1].Name;
        result.DstUser = users[int.Parse(commandParts[1]) - 1].Name;
        result.Amount = long.Parse(commandParts[2]);

        return result;
    }

    static Billing.Billing.BillingClient BillingClient()
    {
        var channel = GrpcChannel.ForAddress(SettingsManager.Settings.GRPC);

        var client = new Billing.Billing.BillingClient(channel);

        return client;
    }

    static async Task<List<UserProfile>> ListUsers()
    {
        var result = new List<UserProfile>();
        var client = BillingClient();

        var serverData = client.ListUsers(new None());
        var serverStream = serverData.ResponseStream;

        while (await serverStream.MoveNext(new CancellationToken()))
            result.Add(serverStream.Current);

        return result;
    }

    #endregion
}
