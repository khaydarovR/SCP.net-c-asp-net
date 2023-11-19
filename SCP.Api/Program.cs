using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SCP.Api.ConfigureWebApi;
using SCP.Application.Common.Configuration;
using SCP.Application.Common.PipeLine;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.ConfigureWebApi(builder.Configuration);
builder.Services.ConfigureApplication(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
        {
            c.Interceptors = new InterceptorFunctions
            {
                RequestInterceptorFunction = "function (req) { req.headers['Authorization'] = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiI3Y2MwNjY3Ni00NmFhLTRmNzgtODNkNS0yZjJjYzBhYTBkZWUiLCJyb2xlIjoi0J_QvtC70YzQt9C-0LLQsNGC0LXQu9GMIiwibmJmIjoxNzAwMzMzNDUwLCJleHAiOjE3MDA0MTk4NTAsImlhdCI6MTcwMDMzMzQ1MCwiaXNzIjoiQk9TIn0.YKoTsBq4cjXeKxiRD5AI2wvCyV88U-_-X1Pij2hqpVI'; return req; }"
            };
        });
}

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

// custom jwt auth middleware
app.UseMiddleware<JwtMiddleware>();

app.MapControllers();

app.MapGet("ping", () => "pong");

app.Run();
