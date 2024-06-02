using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AppIdentity.DataContext;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Serilog;

namespace AppIdentity
{
    public class SeedData
    {
        public static void  EnsureSeedData(IServiceProvider serviceProvider)
        {
            using(var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {   
                scope.ServiceProvider.GetService<PersistedGrantDbContext>().Database.Migrate();
                var appDbContext = scope.ServiceProvider.GetService<ConfigurationDbContext>();
                appDbContext.Database.Migrate();
                EnsureSeedData(appDbContext);
                EnsureUsers(scope);
            }
        }

        private static void EnsureSeedData(ConfigurationDbContext context)
        {
            Console.WriteLine("Seeding database...");

            if (!context.Clients.Any())
            {
                Console.WriteLine("Clients being populated");
                foreach (var client in InMemoryConfig.Clients.ToList())
                {
                    context.Clients.Add(client.ToEntity());
                }
                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("Clients already populated");
            }

            if (!context.IdentityResources.Any())
            {
                Console.WriteLine("IdentityResources being populated");
                foreach (var resource in InMemoryConfig.IdentityResources.ToList())
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("IdentityResources already populated");
            }

            if (!context.ApiResources.Any())
            {
                Console.WriteLine("ApiResources being populated");
                foreach (var resource in InMemoryConfig.ApiResources.ToList())
                {
                    context.ApiResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("ApiResources already populated");
            }

            if (!context.ApiScopes.Any())
            {
                Console.WriteLine("ApiScopes being populated");
                foreach (var scope in InMemoryConfig.ApiScopes.ToList())
                {
                    context.ApiScopes.Add(scope.ToEntity());
                }
                context.SaveChanges();
            }
            else
            {
                Console.WriteLine("ApiScopes already populated");
            }

            Console.WriteLine("Done seeding database.");
            Console.WriteLine();
        }

        private static void EnsureUsers(IServiceScope scope)
        {
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var alice = userMgr.FindByNameAsync("alice").Result;
            if (alice == null)
            {
            alice = new IdentityUser
            {
                UserName = "alice",
                Email = "AliceSmith@email.com",
                EmailConfirmed = true,
                
            };
            var result = userMgr.CreateAsync(alice, "Pass123$").Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            result = userMgr.AddClaimsAsync(alice, new Claim[]
            {
                new Claim(JwtClaimTypes.Name, "Alice Smith"),
                new Claim(JwtClaimTypes.GivenName, "Alice"),
                new Claim(JwtClaimTypes.FamilyName, "Smith"),
                new Claim(JwtClaimTypes.WebSite, "http://alice.com"),
            }).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            Log.Debug("alice created");
            }
            else
            {
                Log.Debug("alice already exists");
            }

            //bob user
            var bob = userMgr.FindByNameAsync("bob").Result;
            if (bob == null)
            {
            bob = new IdentityUser
            {
                UserName = "bob",
                Email = "BobSmith@email.com",
                EmailConfirmed = true
            };
            var result = userMgr.CreateAsync(bob, "Pass123$").Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            result = userMgr.AddClaimsAsync(bob, new Claim[]
            {
                new Claim(JwtClaimTypes.Name, "Bob Smith"),
                new Claim(JwtClaimTypes.GivenName, "Bob"),
                new Claim(JwtClaimTypes.FamilyName, "Smith"),
                new Claim(JwtClaimTypes.WebSite, "http://bob.com"),
                new Claim("location", "somewhere")
            }).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            Log.Debug("bob created");
            }
            else
            {
            Log.Debug("bob already exists");
            }
        }
    }
}