using SCP.Api.ConfigureWebApi;
using SCP.Application.Common.Configuration;
using SCP.Application.Common.PipeLine;
using Swashbuckle.AspNetCore.SwaggerUI;
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
                RequestInterceptorFunction = "function (req) { req.headers['Authorization'] = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiI3YTkyMWMxNy0zY2E1LTQxY2QtYjZjYS00YTg0ZDQ1ODNhYTciLCJyb2xlIjoi0J_QvtC70YzQt9C-0LLQsNGC0LXQu9GMIiwibmJmIjoxNzAwNzUxNTEyLCJleHAiOjE3MDA4Mzc5MTIsImlhdCI6MTcwMDc1MTUxMiwiaXNzIjoiQk9TIn0.f9m4oNdw3Z3HxnbTqz_iFbNXiREIye0OJWjkuMuznwU'; return req; }"
            };
        });
}

app.UseMiddleware<ExceptionHandlerMiddleware>();

//app.UseHttpsRedirection();

app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

//custom jwt auth middleware
app.UseMiddleware<JwtMiddleware>();
app.UseAuthentication();
app.UseAuthorization();


app.UseRateLimiter();

app.MapControllers();

app.MapGet("ping", () => "pong");

//Seeding test users
if (true)
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
