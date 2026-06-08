using JobBoard.API.Data;
using Microsoft.EntityFrameworkCore;

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ১. ডাটাবেজ কানেকশন কনফিগারেশন
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MyConn")));

// ২. CORS পলিসি কনফিগারেশন (সম্পূর্ণ ওপেন প্রোডাকশন এনভায়রনমেন্ট)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJS", policy =>
    {
        policy.AllowAnyOrigin()   // সব ডোমেইন অ্যালাউড
              .AllowAnyMethod()   // GET, POST, PUT, DELETE, OPTIONS সব অ্যালাউড
              .AllowAnyHeader();  // Authorization, Content-Type সহ সব হেডার অ্যালাউড
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

// 🚨 ক্রিটিক্যাল ফিক্স ১: UseCors-কে একদম পাইপলাইনের শুরুতে রাখা হয়েছে
app.UseCors("AllowNextJS");

// 🚨 ক্রিটিক্যাল ফিক্স ২: প্রি-ফ্লাইট (OPTIONS) রিকোয়েস্ট অটো-পাস করানোর মিডলওয়্যার ট্রিক
app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
        context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization");
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

// ৪. বাকি সিকিউরিটি মিডলওয়্যারগুলোর সিকোয়েন্স
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// রুট ইউআরএল টেস্ট করার জন্য এন্ডপয়েন্ট
app.MapGet("/", () => "Remote Job Board API is Running Successfully!");

app.Run();