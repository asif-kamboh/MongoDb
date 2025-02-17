// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDb.Example.Repositories;
using ScientificBit.MongoDb.Extensions;

var builder = Host.CreateApplicationBuilder();

builder.Services.AddMongoDb(opts =>
{
    opts.DatabaseName = "CustomersDb";
    opts.ConnectionString =
        "mongodb://myuser:mystrongpass@localhost:27017,localhost:27018,localhost:27019/?replicaSet=rs1&authSource=admin";
});

builder.Services.AddScoped<IUsersRepository, UsersRepository>();

var app = builder.Build();

await app.StartAsync();
await app.WaitForShutdownAsync();
