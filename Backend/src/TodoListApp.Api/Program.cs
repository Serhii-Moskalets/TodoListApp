using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.PostgreSQL;
using TodoListApp.Api.Extensions;
using TodoListApp.Application.Extensions;
using TodoListApp.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");

var columnWriters = new Dictionary<string, ColumnWriterBase>
{
    { "Timestamp", new TimestampColumnWriter() },
    { "Level", new LevelColumnWriter(true, NpgsqlTypes.NpgsqlDbType.Varchar) },
    { "Message", new RenderedMessageColumnWriter() },
    { "Exception", new ExceptionColumnWriter() },
    { "Context", new PropertiesColumnWriter(NpgsqlTypes.NpgsqlDbType.Text, null) },
};

// Serilog
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.PostgreSQL(
        connectionString: connectionString,
        tableName: "Logs",
        columnOptions: columnWriters,
        needAutoCreateTable: true)
    .CreateLogger();

builder.Logging.ClearProviders();

builder.Host.UseSerilog();

// DI
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddApplicationServices();
builder.Services.AddPersistence();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
