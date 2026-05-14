using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shopping_Mobile.Configurations;
using Shopping_Mobile.Data;
using Shopping_Mobile.Interfaces;
using Shopping_Mobile.Repositories; 
using Shopping_Mobile.Repositories.Implementations;
using Shopping_Mobile.Repositories.Interfaces; 
using Shopping_Mobile.Services.Implementations;
using Shopping_Mobile.Services.Interfaces;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Db
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// DI

//DI Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenProvider, TokenProvider>();
builder.Services.AddScoped<ICartService, CartService>();


//DI Repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
//builder.Services.AddScoped<IGenericRepository, GenericRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Mapper
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<AutoMapperConfig>());

builder.Services.AddControllers()
    .AddJsonOptions(options => {
        
       
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Mobile Shopping API", Version = "v1" });

    // Định nghĩa chuẩn bảo mật JWT
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Vui lòng nhập Token theo định dạng: Bearer {your_token}",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    // Áp dụng chuẩn bảo mật này cho toàn bộ API trong Swagger
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});
// CORS
builder.Services.AddCors(options => {
    options.AddPolicy("AllowReact", policy => {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); 
    });
});

// Authentication & JWT
var jwtSecret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrEmpty(jwtSecret))
{
    throw new Exception("JWT Secret Key is missing in appsettings.json!");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),

            //sau khi hết token ngắt connect
            ClockSkew = TimeSpan.Zero
        };
        
    });

var app = builder.Build();

// Middleware Pipeline 

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowReact");

app.UseAuthentication(); 
app.UseAuthorization(); 

app.MapControllers();

app.Run();