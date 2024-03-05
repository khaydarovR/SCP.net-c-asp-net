using Grpc.Core;
using GrpcTgBot;

namespace GrpcTgBot.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }

        public override Task<LoginResponse> GetToken(LoginRequest request, ServerCallContext context)
        {
            var res = request.Name.GetHashCode() + request.Pwd.GetHashCode();

            return Task.FromResult(new LoginResponse
            {
                Jwt = res.ToString(),
            });
        }
    }
}
