using JobBoard.API.Data;
using Microsoft.EntityFrameworkCore;

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ১. ডাটাবেজ কানেকশন কনফিগারেশন
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MyConn")));

// ২. CORS পলিসি কনফিগারেশন (যেকোনো ডাইনামিক বা প্রিভিউ ডোমেইনকে অটোমেটিক অ্যালাউ করার জন্য)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJS", policy =>
    {
        policy.SetIsOriginAllowed(origin => true) // 👈 Vercel এর সব সাবডোমেইন ও প্রিভিউ লিংক হ্যান্ডেল করবে
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // টোকেন ও কুকি সিকিউরিটির জন্য
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

builder.Services.AddOpenApi(); // .NET 10 এর ডিফল্ট API ডকুমেন্টেশন

var app = builder.Build();

// 🚨 ক্রিটিক্যাল ফিক্স ১: UseCors পাইপলাইনের একদম শুরুতে থাকবে
app.UseCors("AllowNextJS");

// 🚨 ক্রিটিক্যাল ফিক্স ২: প্রি-ফ্লাইট (OPTIONS) রিকোয়েস্ট ডাইনামিকালি পাস করানোর মিডলওয়্যার
app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        var origin = context.Request.Headers["Origin"].ToString();
        context.Response.Headers.Append("Access-Control-Allow-Origin", string.IsNullOrEmpty(origin) ? "*" : origin);
        context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
        context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization");
        context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
        context.Response.StatusCode = 200;
        await context.Response.CompleteAsync();
        return;
    }
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// ৪. সিকিউরিটি মিডলওয়্যারগুলোর সিকোয়েন্স
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// রুট ইউআরএল টেস্ট করার জন্য এন্ডপয়েন্ট
app.MapGet("/", () => "Remote Job Board API is Running Successfully!");

app.Run();