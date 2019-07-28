using NUnit.Framework;

namespace MockQueryable.Sample
{
    [SetUpFixture]
    public class TestsSetup
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            MyService.Initialize();
        }
    }
}