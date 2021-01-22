using System.IO;
using DafnyRefactor;
using NUnit.Framework;

namespace DafnyRefactorTests
{
    [TestFixture]
    public class MoveToAssociatedTests
    {
        private string _testDir;
        private string _testFileDir;
        private int _testNumber;
        private string _methodPos;
        private string _fieldPos;
        private string TestFilePath => $"{_testFileDir}\\source.dfy";
        private string TestOutputPath => $"{_testFileDir}\\test{_testNumber}.out";
        private string TestExpectedPath => $"{_testFileDir}\\test{_testNumber}.expected";

        [SetUp]
        protected void SetUp()
        {
            var testRunnerDir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            _testDir = $"{testRunnerDir.Parent?.Parent?.Parent?.FullName}\\Tests";
        }

        private string[] Args => new[]
        {
            "apply-move-method-to-associated", "-f", TestFilePath, "-s", _methodPos, "-t", _fieldPos, "-o",
            TestOutputPath
        };

        [Test]
        public void ClassWithFieldT1()
        {
            _testFileDir = $"{_testDir}\\class_with_field";
            _testNumber = 1;
            _methodPos = "19:15";
            _fieldPos = "13:11";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }
    }
}