using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SettleDown.CRUDs;
using SettleDown.Data;
using SettleDown.Models;
using SettleDown.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SettleDownContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SettleDownContext")
                         ?? throw new InvalidOperationException(
                             "Connection string 'SettleDownContext' not found."));
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtIssuer"],
            ValidAudience = builder.Configuration["JwtAudience"],
            IssuerSigningKey = 
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                    Environment.GetEnvironmentVariable("JwtIssuerKey") ??
                    throw new KeyNotFoundException("No 'JwtIssuerKey' found in env. variables!")))
        };
    });

builder.Services.AddScoped<ITokenService, SettleJwtService>();
builder.Services.AddScoped<ICrudService<SettleDownUser>, UserCrudService>();
builder.Services.AddScoped<ICrudService<SettleDownCredential>, CredentialCrudService>();
builder.Services.AddScoped<ICrudService<SettleDownMember>, MemberCrudService>();
builder.Services.AddScoped<ICrudService<SettleDownDebt>, DebtCrudService>();
builder.Services.AddScoped<IDebtManagementService, SettleDownDebtManagementService>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.SwaggerDoc("v1", new OpenApiInfo {
        Title = "JWTToken_Auth_API", Version = "v1"
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme() {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
                      "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                      // ReSharper disable once StringLiteralTypo
                      "Example: \"Bearer 1safsfsdfdfd\"",
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
