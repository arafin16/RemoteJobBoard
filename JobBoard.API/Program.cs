using JobBoard.API.Data;
using Microsoft.EntityFrameworkCore;

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ডাটাবেজ কানেকশন কনফিগারেশন
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MyConn")));

// CORS পলিসি কনফিগারেশন (ক্রিডেনশিয়াল এবং নির্দিষ্ট ডোমেইন সাপোর্টসহ)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJS", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",                               // লোকাল ডেভেলপমেন্ট এনভায়রনমেন্ট
                "https://remote-job-board-zdtu.vercel.app",           // আপনার প্রথম ভার্সেল ডোমেইন
                "https://remote-job-board-pi.vercel.app"              // কনসোলে পাওয়া বর্তমান একটিভ ভার্সেল ডোমেইন
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowAnyOrigin(); // কুকি বা অথেনটিকেশন টোকেন আদান-প্রদান করার জন্য বাধ্যতামূলক
    });
});

// JWT Authentication কনফিগারেশন
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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// মিডলওয়্যার অ্যাপ্লাই করার সঠিক ক্রমানুসারে সাজানো
app.UseCors("AllowNextJS"); // সবার আগে CORS রিকোয়েস্ট হ্যান্ডেল করবে

app.UseAuthentication();    // ওপরে CORS থাকার কারণে অথেনটিকেশন এররও ব্লক হবে না
app.UseAuthorization();

app.MapControllers();

// রুট ইউআরএল টেস্ট করার জন্য এন্ডপয়েন্ট
app.MapGet("/", () => "Remote Job Board API is Running Successfully!");

app.Run();