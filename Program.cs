using AppIdentity;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;

var builder = WebApplication.CreateBuilder(args);
// Identity server config
builder.Services.AddIdentityServer(options =>{
    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;
    options.EmitStaticAudienceClaim = true;

})
.AddTestUsers(InMemoryConfig.Users)
.AddInMemoryClients(InMemoryConfig.Clients)
.AddInMemoryApiResources(InMemoryConfig.ApiResources)
.AddInMemoryApiScopes(InMemoryConfig.ApiScopes)
.AddInMemoryIdentityResources(InMemoryConfig.IdentityResources);

var app = builder.Build();

app.UseIdentityServer();
app.MapGet("/", () => "Good evening");

app.Run();
