using CardActionMicroservice.Infrastructure;
using CardActionMicroservice.Validators;
using CardActionService.Extensions;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssemblyContaining<CardDetailsValidator>();
builder.Services.AddFluentValidationAutoValidation();
var configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Config", "ActionsConfiguration.json");
var jsonContent = File.ReadAllText(configFilePath, Encoding.UTF8);
builder.Services.AddSingleton<IRuleLoader>(_ => new JsonRuleLoader(jsonContent));
builder.Services.AddBusinessServices();

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

app.UseAuthorization();

app.MapControllers();

app.Run();


