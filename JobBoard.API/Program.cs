using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// 💡 তোমার প্রজেক্টের ডাটাবেজ ফোল্ডারের সঠিক নেমস্পেসটি এখানে নিশ্চিত করো
using JobBoard.API.Data; 

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// ১. ডাটাবেজ কানেকশন কনফিগারেশন
// ==========================================
var connectionString = builder.Configuration.GetConnectionString("MyConn") 
                       ?? builder.Configuration["ConnectionStrings:MyConn"]
                       ?? builder.Configuration["ConnectionStrings__MyConn"];

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'MyConn' not found.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// ==========================================
// ২. CORS পলিসি কনফিগারেশন
// ==========================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "https://remote-job-board-pi.vercel.app",
                "https://remote-job-board-zdtu.vercel.app",
                "http://localhost:5173"
               )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ==========================================
// ৩. JWT অথেনটিকেশন কনফিগারেশন
// ==========================================
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Key"] ?? builder.Configuration["Jwt:Key"] ?? builder.Configuration["Jwt__Key"] ?? "YourSuperSecretBackupKey1234567890!";

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
        ValidIssuer = jwtSettings["Issuer"] ?? "JobBoardAPI",
        ValidAudience = jwtSettings["Audience"] ?? "JobBoardFrontend",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


var app = builder.Build();

// ==========================================
// ৪. মিডলওয়্যার পাইপলাইন
// ==========================================


app.UseRouting();

// 🚨 CORS পলিসি অ্যাক্টিভেট করা
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();