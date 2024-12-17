using CardActionMicroservice.Infrastructure;
using CardActionMicroservice.Models;
using CardActionMicroservice.Validators;
using CardActionService.Extensions;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Diagnostics;
using System.Text;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssemblyContaining<CardDetailsValidator>();
builder.Services.AddFluentValidationAutoValidation();
var configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Config", "ActionsConfiguration.json");
var jsonContent = File.ReadAllText(configFilePath, Encoding.UTF8);
builder.Services.AddSingleton<IRuleLoader>(_=>{
    Debug.WriteLine("Registering JsonRuleLoader...");
    return new JsonRuleLoader(jsonContent);
});

//Automatic request validation - through binding
builder.Services.AddValidatorsFromAssemblyContaining<CardRequestValidator>();
//Provider data validation - controler body
builder.Services.AddScoped<IValidator<CardDetails>, CardDetailsValidator>();

builder.Services.AddBusinessServices();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


