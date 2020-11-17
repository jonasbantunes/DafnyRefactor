using System.IO;
using DafnyRefactor;
using NUnit.Framework;

namespace DafnyRefactorTests
{
    [TestFixture]
    public class MoveMethodTests
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
        private string _instancePosition;

        private string TestFilePath => $"{_testFileDir}\\source.dfy";
        private string TestOutputPath => $"{_testFileDir}\\test{_testNumber}.out";
        private string TestExpectedPath => $"{_testFileDir}\\test{_testNumber}.expected";

        private string[] Args => new[]
        {
            "apply-move-method", "-f", TestFilePath, "-i", _instancePosition, "-o", TestOutputPath
        };

        [Test]
        public void ComplexClassesT1()
        {
            _testFileDir = $"{_testDir}\\complex_classes";
            _testNumber = 1;
            _instancePosition = "24:23";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void ComplexClassesT2()
        {
            _testFileDir = $"{_testDir}\\complex_classes";
            _testNumber = 2;
            _instancePosition = "7:16";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }

        [Test]
        public void ComplexClassesT3()
        {
            _testFileDir = $"{_testDir}\\complex_classes";
            _testNumber = 3;
            _instancePosition = "32:17";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }

        [Test]
        public void EqualMethodsT1()
        {
            _testFileDir = $"{_testDir}\\equal_methods";
            _testNumber = 1;
            _instancePosition = "28:23";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }

        [Test]
        public void EqualMethodsT2()
        {
            _testFileDir = $"{_testDir}\\equal_methods";
            _testNumber = 2;
            _instancePosition = "36:17";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void MethodCallT1()
        {
            _testFileDir = $"{_testDir}\\class_method_call";
            _testNumber = 1;
            _instancePosition = "27:19";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }

        [Test]
        public void SimpleClassesT1()
        {
            _testFileDir = $"{_testDir}\\simple_classes";
            _testNumber = 1;
            _instancePosition = "14:23";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void SimpleClassesT2()
        {
            _testFileDir = $"{_testDir}\\simple_classes";
            _testNumber = 2;
            _instancePosition = "14:29";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }
    }
}