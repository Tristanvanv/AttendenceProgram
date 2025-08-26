using AttendenceProgram.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

builder.Services.AddControllers();
builder.Services.AddSingleton<PresenceStore>();

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI();

app.UseCors("Pages");

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
