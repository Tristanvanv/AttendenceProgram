using AttendenceProgram.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var origins = (builder.Configuration["CORS_ALLOWED_ORIGINS"] ?? "")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

//CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("Pages", policy =>
        policy.WithOrigins("https://tristanvanv.github.io")   
              .AllowAnyHeader()
              .AllowAnyMethod()    
    );
});

builder.Services.AddControllers();
builder.Services.AddSingleton<PresenceStore>();

var app = builder.Build();



app.UseSwagger();

app.UseSwaggerUI();

app.UseCors("Pages");

app.MapGet("/health", (AttendenceProgram.Services.PresenceStore s) =>
Results.Json(new { ok = true, mode = s.Mode })
);

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
