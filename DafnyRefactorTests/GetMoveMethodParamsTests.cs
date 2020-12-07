using System.IO;
using DafnyRefactor;
using NUnit.Framework;

namespace DafnyRefactorTests
{
    [TestFixture]
    public class GetMoveMethodParamsTests
    {
        [SetUp]
        protected void SetUp()
        {
            var testRunnerDir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            _testDir = $"{testRunnerDir.Parent?.Parent?.Parent?.FullName}\\Tests";
        }

        private string _testDir;
        private string _testFileDir;
        private int _testNumber;
        private string _methodPosition;

        private string TestFilePath => $"{_testFileDir}\\source.dfy";
        private string TestOutputPath => $"{_testFileDir}\\test{_testNumber}.out";
        private string TestExpectedPath => $"{_testFileDir}\\test{_testNumber}.expected";

        private string[] Args => new[]
        {
            "get-move-method-params", "-f", TestFilePath, "-p", _methodPosition, "-o", TestOutputPath
        };

        [Test]
        public void ComplexClassesT1()
        {
            _testFileDir = $"{_testDir}\\simple_classes";
            _testNumber = 1;
            _methodPosition = "14:12";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
        }
    }
}