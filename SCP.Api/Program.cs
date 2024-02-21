using SCP.Api.ConfigureWebApi;
using SCP.Api.Middleware;
using SCP.Application.Common.Configuration;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Text;
using System.Text.Json.Serialization;



//TODO: rabbit - log - elastick, share secret, telegram\discord\email notify, report table
//TODO: deploy, CI\CD github - jenkins, db replicate, load balancer 

Console.OutputEncoding = Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
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
                RequestInterceptorFunction = "function (req) { req.headers['Authorization'] = 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiJiODhjNzhkMi1kOTM2LTQ0YmQtYTZiYi0xOWU4NjM3MzM3Y2QiLCJyb2xlIjoiVXNlciIsIm5iZiI6MTcwODUyNjY2OCwiZXhwIjoxNzExMTE4NjY4LCJpYXQiOjE3MDg1MjY2NjgsImlzcyI6IkJPUyJ9.6PqcEhsiIWrLdapkRrPQGrI4Hi0-7ef3WswQitUXjb8'; return req; }"
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
