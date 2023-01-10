namespace ConsoleMenu;

public class MenuManager
{
    private bool launched = false;
    private List<MenuItem> commands = new();

    public string Header { get; set; } = "";
    
    public MenuItem AddCommand(MenuItem.MethodDelegate method, string description)
    {
        MenuItem menuItem = new MenuItem(method, description);
        commands.Add(menuItem);
        return menuItem;
    }

    public MenuItem AddCommand(MenuItem.MethodDelegateAsync method, string description)
    {
        MenuItem menuItem = new MenuItem(method, description);
        commands.Add(menuItem);
        return menuItem;
    }

    public void Start()
    {
        launched = true;
        while (launched)
        {
            ShowMenu();

            ExecuteCommand();
        }
    }

    private void ShowMenu()
    {
        Console.Clear();
        Console.WriteLine(Header);
    }

    private void ExecuteCommand()
    {
        var selected = (MenuItem)SelectParamUser(commands.ToList<Object>());
        selected.ExecuteAsync().Wait();        
    }

    public static object SelectParamUser(List<object> items, string message = "")
    {
        if (!string.IsNullOrEmpty(message))
            Console.WriteLine(message);
        
        for (int i = 0; i < items.Count; i++)
            Console.WriteLine($"    {i + 1} - {items[i].ToString()}");

        object? selected = null;
        while (selected == null)
        {
            var key = Console.ReadKey(true).KeyChar;
            if (char.IsDigit(key))
            {
                var itemNumber = Byte.Parse(key.ToString());
                if (items.Count >= itemNumber)
                    selected = items[itemNumber - 1];
            }
        }
        return selected;

    }

    public void Stop()
    {
        launched = false;
    }

}
