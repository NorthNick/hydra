using System;
using System.Reactive.Concurrency;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shastra.Hydra.Messaging.Utils;

namespace Shastra.Hydra.Tests
{
    [TestClass]
    public class ObservableGeneratorTest : ReactiveTest
    {
        private TestScheduler _scheduler;
        private DateTime _startDate;
        private ObservableGenerator<int> _generator;
        
        [TestInitialize]
        public void Initialize()
        {
            _scheduler = new TestScheduler();
            // Any time after 1/1/1970 will do for startDate. CouchIds go wrong before that date as they are microseconds since 1/1/1970.
            _startDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            // Dispose of the generator after 10 minutes. (res disposes of its subscription to generator, but generator itself also has to be disposed of otherwise the test never terminates.)
            _scheduler.Schedule(new DateTimeOffset(_startDate.AddMinutes(10)), () => _generator.Dispose());
        }

        [TestMethod]
        public void TestBasicFunctionality()
        {
            var res = _scheduler.Start(() => {
                    _generator = new ObservableGenerator<int>(1000, () => 1, _scheduler);
                    return _generator;
                },
                _startDate.Ticks, _startDate.Ticks, _startDate.AddSeconds(30.5).Ticks);
            Assert.AreEqual(30, res.Messages.Count, "Should receive 30 messages.");
        }
    }
}
