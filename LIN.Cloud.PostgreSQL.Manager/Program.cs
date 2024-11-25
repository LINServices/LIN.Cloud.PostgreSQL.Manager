using LIN.Cloud.PostgreSQL.Manager.Services;
using LIN.Cloud.PostgreSQL.Manager.Services.Data;
using LIN.Cloud.PostgreSQL.Manager.Services.Local;
using Npgsql;
using Http.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddLINHttp();

LIN.Access.Developer.Build.Init();

builder.Services.AddScoped(t =>
{
    var r = new NpgsqlConnection(builder.Configuration.GetConnectionString("postgre"));
    r.Open();
    return r;
});

builder.Services.AddScoped<ManagementService>();
builder.Services.AddScoped<DatabasesManager>();
builder.Services.AddScoped<UsersManager>();
builder.Services.AddScoped<DatabaseConector>();


var app = builder.Build();


app.UseLINHttp();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Llave de LIN Cloud Developers.
LIN.Cloud.Identity.Utilities.Build.Init(builder.Configuration["identity:key"] ?? string.Empty);

app.Run();