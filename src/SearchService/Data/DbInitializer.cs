using System;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Features;
using MongoDB.Driver;
using MongoDB.Entities;

namespace SearchService.Data;

public class DbInitializer
{
    public static async Task InitDb(WebApplication app) {

        await DB.InitAsync("SearchDb", MongoClientSettings
            .FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

        await DB.Index<Item>()
            .Key(x => x.Make, KeyType.Text)
            .Key(x => x.Model, KeyType.Text)
            .Key(x => x.Color, KeyType.Text)
            .CreateAsync();

        var count = await DB.CountAsync<Item>();

        if (count == 0) {
            using var scope = app.Services.CreateScope();
            var httpClient = scope.ServiceProvider.GetRequiredService<AuctionSvcHttpClient>();
            var items = await httpClient.GetItemsForSerachDb();
            Console.WriteLine(items.Count + " returned from the auction service");
            if (items.Count > 0) await DB.SaveAsync(items);
        }
    }
}
