using AppIdentity;
using AppIdentity.DataContext;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("DefaultConnection");
var migrationAssembly  = typeof(InMemoryConfig).Assembly.GetName().Name;

builder.Services.AddRazorPages();
builder.Services.AddDbContext<ApplicationDbContext>(options =>{
    options.UseSqlite(connectionString, sqliteOptions => sqliteOptions.MigrationsAssembly(migrationAssembly));
});

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();


// Identity server config
builder.Services.AddIdentityServer(options =>{
    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;
    options.EmitStaticAudienceClaim = true;

})
.AddConfigurationStore(options => options.ConfigureDbContext = b => b.UseSqlite(connectionString, opt => opt.MigrationsAssembly(migrationAssembly)))
.AddOperationalStore(options => options.ConfigureDbContext = b => b.UseSqlite(connectionString, opt => opt.MigrationsAssembly(migrationAssembly)))
.AddAspNetIdentity<IdentityUser>();


var app = builder.Build();

app.UseIdentityServer();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages().RequireAuthorization();


//FOR SEEDING THE DATABASE

try
{
    Console.Title = "ACA Identity System";

    // var host = CreateHostBuilder(args).Build();
    using (var scope = app.Services.CreateScope())
    {
        var config = app.Services.GetRequiredService<IConfiguration>();
        var serviceProvider = scope.ServiceProvider;
        try
        {
            Log.Information("Seeding PUBLIC database...");
            SeedData.EnsureSeedData(serviceProvider);
            Log.Information("Done seeding database.");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An error occurred Initializing the DB.");
        }
    }

    Log.Information("Starting host...");
}
    catch (Exception ex)
    {
        Log.Fatal(ex, "Host terminated unexpectedly.");
    }
    finally
    {
        Log.CloseAndFlush();
    }
//END SEED

app.Run();
