using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.PostgreSQL;
using TodoListApp.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found");
}

var columnWriters = new Dictionary<string, ColumnWriterBase>
{
    { "Timestamp", new TimestampColumnWriter() },
    { "Level", new LevelColumnWriter(true, NpgsqlTypes.NpgsqlDbType.Varchar) },
    { "Message", new RenderedMessageColumnWriter() },
    { "Exception", new ExceptionColumnWriter() },
    { "Context", new PropertiesColumnWriter(NpgsqlTypes.NpgsqlDbType.Text, null) },
};

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.PostgreSQL(
        connectionString: connectionString,
        tableName: "Logs",
        columnOptions: columnWriters,
        needAutoCreateTable: true)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddInfrastructure(connectionString);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

await app.RunAsync();
