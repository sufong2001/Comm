using Sufong2001.Comm.Tests.Base;
using Sufong2001.Test.AzureFunctions;
using Xunit;
using Xunit.Abstractions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Sufong2001.Comm.Tests.AzureFunctions
{
    public class DeliveryMessageTests : IClassFixture<ApplicationBaseFixture>
    {
        private readonly ILogger _logger = TestFactory.CreateLogger();

        private readonly ITestOutputHelper _output;
        private readonly ApplicationBaseFixture _app;

        public DeliveryMessageTests(ITestOutputHelper output, ApplicationBaseFixture app)
        {
            _output = output;
            _app = app;
        }

        //[Fact]
        //public async void Should_DispatchMessageActivity_Return_14_Schedules()
        //{
        //    var scheduleTable = _app.Repository.GetTable(TableNames.CommSchedule);

        //    var id = _app.IdGenerator;
        //    var app = _app.App;
        //    var dispatchDate = app.DateTimeNow.AddDays(-1).ToUniversalTime();

        //    var result = await DispatchScheduleActivity(
        //        dispatchDate,
        //        scheduleTable,
        //        id,
        //        app,
        //        _logger);

        //    Assert.Equal(14, result.Count());

        //    _output.WriteLine(result.ToJson(Formatting.Indented));
        //}
    }
}