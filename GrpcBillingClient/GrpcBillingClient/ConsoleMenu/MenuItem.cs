namespace ConsoleMenu;

public class MenuItem
{
    public delegate void MethodDelegate();
    public delegate Task MethodDelegateAsync();

    public MethodDelegate? Method { get; set; }
    public MethodDelegateAsync? MethodAsync { get; set; }

    public string Description { get; set; } = String.Empty;

    public bool ClearBeforeExecution { get; set; } = true;

    public MenuItem(MethodDelegate method, string description)
    {
        this.Description = description;
        this.Method = method;
    }

    public MenuItem(MethodDelegateAsync method, string description)
    {
        this.Description = description;
        this.MethodAsync = method;
    }

    public async Task ExecuteAsync()
    {
        if (ClearBeforeExecution == true)
            Console.Clear();

        try
        {
            if (MethodAsync == null)
                Method?.Invoke();
            else
                await MethodAsync.Invoke();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        Console.Write("Press any key to close this window . . .");
        Console.ReadKey(true);

    }

    public override string ToString() => Description;
}
