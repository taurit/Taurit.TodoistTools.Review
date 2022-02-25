namespace Taurit.TodoistTools.Review;

public class Startup
{
    private const Int32 StaticResourceCacheTimeInDays = 180;
    private const Int32 StaticResourceCacheTimeInSeconds = 60 * 60 * 24 * StaticResourceCacheTimeInDays;

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddApplicationInsightsTelemetry();
        services.Configure<CookiePolicyOptions>(options =>
        {
            // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            options.CheckConsentNeeded = context => false;
            options.MinimumSameSitePolicy = SameSiteMode.None;
        });

        services.AddResponseCompression();

        services.AddMvc();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app)
    {
        app.UseDeveloperExceptionPage();
        app.UseResponseCompression();

        app.UseHttpsRedirection();
        app.UseStaticFiles(GetStaticFileOptions());
        app.UseCookiePolicy();

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapControllerRoute(
                "default",
                "{controller=Home}/{action=Index}/{id?}");
        });
    }

    private static StaticFileOptions GetStaticFileOptions()
    {
        StaticFileOptions options = new StaticFileOptions
        {
            OnPrepareResponse = ctx =>
            {
                ctx.Context.Response.Headers.Append("Cache-Control",
                    $"public, max-age={StaticResourceCacheTimeInSeconds}");
            }
        };
        return options;
    }
}
