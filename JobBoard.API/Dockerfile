# .NET 10 SDK দিয়ে প্রজেক্ট বিল্ড করা
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# .csproj ফাইল কপি এবং রিস্টোর করা
COPY ["JobBoard.API.csproj", "./"]
RUN dotnet restore "./JobBoard.API.csproj"

# বাকি সব কোড কপি করে পাবলিশ করা
COPY . .
RUN dotnet publish "JobBoard.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ফাইনাল রানটাইম ইমেজ তৈরি করা
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# রেন্ডার বা যেকোনো ক্লাউডের পোর্টের জন্য এনভায়রনমেন্ট সেটআপ
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "JobBoard.API.dll"]