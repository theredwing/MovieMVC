using Microsoft.EntityFrameworkCore;
using MovieMVC.Data;
using MovieMVC.Repositories;
using MovieMVC.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
// use SQL Server database named 'Movies' (connection string uses localdb by default)
var connectionString = builder.Configuration.GetConnectionString("Movies")
                       ?? "Server=(localdb)\\mssqllocaldb;Database=Movies;Trusted_Connection=True;MultipleActiveResultSets=true;Connect Timeout=60";
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connectionString, sql => sql.CommandTimeout(60)));
builder.Services.AddScoped<IHomeRepository, HomeRepository>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IGraphRepository, GraphRepository>();
builder.Services.AddScoped<IGraphService, GraphService>();

var app = builder.Build();

// after building app, create scope and run a simple seeder
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment ())
{
    app.UseExceptionHandler ("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts ();
}

app.UseHttpsRedirection ();
app.UseRouting ();

app.UseAuthorization ();

app.MapStaticAssets ();

app.MapControllerRoute (
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets ();


app.Run ();
