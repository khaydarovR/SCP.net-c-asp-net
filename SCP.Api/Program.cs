using Microsoft.AspNetCore.Authorization;
using SCP.Api.ConfigureWebApi;
using SCP.Api.Middleware;
using SCP.Application.Common.Configuration;
using SCP.Application.Common.PipeLine;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;

Console.OutputEncoding = Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    });

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
                RequestInterceptorFunction = "function (req) { req.headers['Authorization'] = 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiI4ODIzYTNiNi0zZDlkLTQxMjgtOTFmZi01ZDgxZmY5MTBiMTYiLCJyb2xlIjoi0J_QvtC70YzQt9C-0LLQsNGC0LXQu9GMIiwibmJmIjoxNzA4MzI0NDE5LCJleHAiOjE3MTA5MTY0MTksImlhdCI6MTcwODMyNDQxOSwiaXNzIjoiQk9TIn0.iUVIaXi-Jc8qP9nTfe0dUstToyJMfMgdBJRu7yKf_PE'; return req; }"
            };
        });
}

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());


#if !DEBUG
app.UseMiddleware<WhiteIPMiddleware>();
#endif

app.UseAuthentication();
app.UseAuthorization();


app.UseRateLimiter();
app.MapControllers();

app.MapGet("ping", () => "pong");

//Seeding test users
if (false)
{
    using (var scope = app.Services.CreateScope())
    {
        var seedingService = scope.ServiceProvider.GetRequiredService<SystemEntitySeeding>();
        await seedingService.InitTUsersWithSafe(7);
        //await seedingService.ClearUser("razil_khayka@mail.ru");
        //await seedingService.ClearUser("razil.khayka@gmail.com");
    }
}


app.Run();
