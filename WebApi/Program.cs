var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Define your API routes here
app.MapGet("/hello", () => "Hello, World!").WithName("GetHello")
    .WithSummary("Returns a greeting message")
    .WithDescription("This endpoint returns a simple 'Hello, World!' message.")
    .WithTags("Greeting")
   .WithOpenApi();
app.Run();

