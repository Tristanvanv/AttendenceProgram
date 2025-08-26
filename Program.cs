using AttendenceProgram.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("Pages", policy =>
    {
        var origins = (builder.Configuration["CORS_ALLOWED_ORIGINS"] ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (origins.Length > 0)
        {
            policy.WithOrigins(origins)
                  .WithMethods("GET", "POST", "OPTIONS")
                  .AllowAnyHeader();
        }
        else
        {
            
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
    });
});

builder.Services.AddControllers();
builder.Services.AddSingleton<PresenceStore>();

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI();

app.UseCors("Pages");

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
