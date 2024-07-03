using CategoryProcessor.Models;
using Microsoft.AspNetCore.Mvc;
using OpenAI_API;
using OpenAI_API.Completions;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddSingleton(new OpenAIAPI(Environment.GetEnvironmentVariable("OPENAI_API_KEY")));
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



app.MapPost("/ProcessCategories", async (HttpContext context, [FromBody] List<Category> categories) =>
{
    
     var openAiApi = context.RequestServices.GetRequiredService<OpenAIAPI>();
    var results = new List<object>();

    foreach (var category in categories)
    {
        foreach (var subCategory in category.SubCategories)
        {
            // Get popular attributes for each subcategory
            var attributes = await GetPopularAttributes( openAiApi,subCategory.CategoryId, subCategory.CategoryName);
            results.Add(new
            {
                CategoryId = subCategory.CategoryId,
                Attributes = attributes
            });
        }
    }

    return results;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();




async Task<List<string>> GetPopularAttributes(OpenAIAPI openAiApi,int categoryId, string categoryName)
{
    var completionRequest = new CompletionRequest
    {
        Prompt = $"List the three most popular attributes for the category '{categoryName}' with ID {categoryId}.",
        MaxTokens = 50
    };
      
    var completionResult = await openAiApi.Completions.CreateCompletionAsync(completionRequest);

    if (completionResult.Completions.Any())
    {
        var attributes = completionResult.Completions.First().Text.Trim().Split('\n').Select(attr => attr.Trim()).ToList();
        return attributes.Count >= 3 ? attributes.Take(3).ToList() : attributes;
    }
    else
    {
        return new List<string> { "attribute1", "attribute2", "attribute3" };
    }
}

