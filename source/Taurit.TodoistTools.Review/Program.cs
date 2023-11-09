namespace Taurit.TodoistTools.Review;

public class Program
{
    public static void Main(String[] args)
    {
        Program.CreateHostBuilder(args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(String[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>().UseUrls("http://*:8888"); })
        .UseWindowsService();
    }
}
