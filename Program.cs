using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using GuestbookServer;
using Microsoft.FSharp.Data.UnitSystems.SI.UnitNames;
using NBomber.Configuration;
using NBomber.Contracts;
using NBomber.CSharp;
using Serilog;
using System;
using System.Threading.Tasks;

namespace NBoomberTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Task.Delay(500);
            DateTimeOffset value = DateTimeOffset.Now;
            //long successCode = 400;
            var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new GuestbookProtoBuf.GuestbookProtoBufClient(channel);
            var step = Step.Create("GuestbookGRPCStep", timeout: TimeSpan.FromSeconds(5), execute:async context =>
            {
                await Task.Run(() =>
                {
                    var result = client.GetGuestbookProtobufAsync(new GetGuestbookProtobufRequest { Query = "Reading" }).ResponseAsync;
                    if (result.IsCompleted)
                    {
                        return Response.Ok(400);
                    }
                    else
                    {
                        return Response.Fail();
                    }
                    
                });
                return Response.Ok(400);
            });
            var scenario = ScenarioBuilder.CreateScenario("guestbook", step)
                .WithLoadSimulations(new[] {
                    Simulation.RampConstant(50,TimeSpan.FromSeconds(50))
                })
                .WithWarmUpDuration(TimeSpan.FromSeconds(5));

            NBomberRunner
                .RegisterScenarios(scenario)
                .WithLoggerConfig(() =>
                    new LoggerConfiguration().MinimumLevel.Verbose())
                .WithReportFileName("loadtest_guestbookGRPC")
                .WithReportFolder("loadtest")
                .WithReportFormats(ReportFormat.Html)
                .Run();
        }
    }
}
