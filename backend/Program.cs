using PrivateDoc.Api.Services;
using PrivateDoc.Api.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register application services as singletons/scoped as appropriate
builder.Services.AddSingleton<ISessionService, SessionService>();
builder.Services.AddSingleton<IFileStorageService, FileStorageService>();
builder.Services.AddSingleton<ITextExtractionService, TextExtractionService>();
builder.Services.AddSingleton<IQuestionAnsweringService, QuestionAnsweringService>();

// Allow CORS for local frontend development
builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalDev", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("LocalDev");
app.UseAuthorization();
app.MapControllers();

app.Run();
