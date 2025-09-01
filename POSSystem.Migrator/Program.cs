using DbUp;
using System.Reflection;

namespace POSSystem.Migrator;

internal class Program
{
    public static int Main(string[] args)
    {
        Console.WriteLine("POS System Database Migrator");
        Console.WriteLine("================================");

        var connectionString = args.Length > 0 
            ? args[0] 
            : "Server=(localdb)\\mssqllocaldb;Database=POSSystemDB;Trusted_Connection=true;MultipleActiveResultSets=true";

        Console.WriteLine($"Connection: {connectionString.Substring(0, Math.Min(50, connectionString.Length))}...");

        try
        {
            EnsureDatabase.For.SqlDatabase(connectionString);
            Console.WriteLine("Database ensured");

            var upgrader = DeployChanges.To
                .SqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .LogToConsole()
                .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Migration failed!");
                Console.WriteLine(result.Error);
                Console.ResetColor();
                return -1;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Database migration completed successfully!");
            Console.ResetColor();
            return 0;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Migration error: {ex.Message}");
            Console.ResetColor();
            return -1;
        }
    }
}
