using JobBoard.API.Data;
using Microsoft.EntityFrameworkCore;

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ১. ডাটাবেজ কানেকশন কনফিগারেশন
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MyConn")));

// ২. .NET 10 প্রোডাকশন স্ট্যান্ডার্ড CORS কনফিগারেশন
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJS", policy =>
    {
        policy.SetIsOriginAllowed(origin => true) // Vercel-এর সব ডাইনামিক প্রিভিউ ও প্রোডাকশন লিংক এলাউ করবে
              .AllowAnyMethod()                   // GET, POST, PUT, DELETE, OPTIONS সব এলাউড
              .AllowAnyHeader()                   // Content-Type, Authorization সহ সব হেডার এলাউড
              .AllowCredentials();                // ফ্রন্টএন্ড টোকেন/কুকি আদান-প্রদানের জন্য আবশ্যক
    });
});

// ৩. JWT Authentication কনফিগারেশন
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

builder.Services.AddOpenApi(); // .NET 10 API ডক্স

var app = builder.Build();

// 🚨 সর্ব প্রথম এবং পাইপলাইনের সবার উপরেই CORS মিডলওয়্যার থাকবে
app.UseCors("AllowNextJS");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// ৪. বাকি সিকিউরিটি মিডলওয়্যারগুলোর সঠিক সিকোয়েন্স
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// রুট ইউআরএল টেস্ট করার জন্য এন্ডপয়েন্ট
app.MapGet("/", () => "Remote Job Board API is Running Successfully!");

app.Run();