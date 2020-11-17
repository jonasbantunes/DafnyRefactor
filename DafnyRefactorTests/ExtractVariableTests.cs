using System.IO;
using DafnyRefactor;
using NUnit.Framework;

namespace DafnyRefactorTests
{
    [TestFixture]
    public class ExtractVariableTests
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
        private string _startRange;
        private string _endRange;

        private string TestFilePath => $"{_testFileDir}\\source.dfy";
        private string TestOutputPath => $"{_testFileDir}\\test{_testNumber}.out";
        private string TestExpectedPath => $"{_testFileDir}\\test{_testNumber}.expected";

        private string[] Args => new[]
        {
            "apply-extract-variable", "-f", TestFilePath, "-s", _startRange, "-e", _endRange, "-o", TestOutputPath,
            "-v", "extractedVar"
        };

        [Test]
        public void ClassExprT1()
        {
            _testFileDir = $"{_testDir}\\class_expr";
            _testNumber = 1;
            _startRange = "17:24";
            _endRange = "17:28";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void ClassExprT2()
        {
            _testFileDir = $"{_testDir}\\class_expr";
            _testNumber = 2;
            _startRange = "18:14";
            _endRange = "18:15";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void ClassExprT3()
        {
            _testFileDir = $"{_testDir}\\class_expr";
            _testNumber = 3;
            _startRange = "14:17";
            _endRange = "14:27";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }

        [Test]
        public void IteExprT1()
        {
            _testFileDir = $"{_testDir}\\ite_expr";
            _testNumber = 1;
            _startRange = "3:19";
            _endRange = "3:42";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void IteExprT2()
        {
            _testFileDir = $"{_testDir}\\ite_expr";
            _testNumber = 2;
            _startRange = "3:22";
            _endRange = "3:28";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void IteExprT3()
        {
            _testFileDir = $"{_testDir}\\ite_expr";
            _testNumber = 3;
            _startRange = "3:21";
            _endRange = "3:42";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }

        [Test]
        public void MethodCallT1()
        {
            _testFileDir = $"{_testDir}\\method_call";
            _testNumber = 1;
            _startRange = "9:26";
            _endRange = "9:28";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void MethodCallT2()
        {
            _testFileDir = $"{_testDir}\\method_call";
            _testNumber = 2;
            _startRange = "9:26";
            _endRange = "9:30";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void MethodCallT3()
        {
            _testFileDir = $"{_testDir}\\method_call";
            _testNumber = 3;
            _startRange = "9:32";
            _endRange = "9:36";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void MethodCallT4()
        {
            _testFileDir = $"{_testDir}\\method_call";
            _testNumber = 4;
            _startRange = "9:32";
            _endRange = "9:39";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void MethodCallT5()
        {
            _testFileDir = $"{_testDir}\\method_call";
            _testNumber = 5;
            _startRange = "9:27";
            _endRange = "9:37";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }

        [Test]
        public void ScopeDiff2T1()
        {
            _testFileDir = $"{_testDir}\\scope_diff_2";
            _testNumber = 1;
            _startRange = "3:18";
            _endRange = "3:23";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void ScopeDiff2T2()
        {
            _testFileDir = $"{_testDir}\\scope_diff_2";
            _testNumber = 2;
            _startRange = "8:23";
            _endRange = "8:28";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void ScopeDiff2T3()
        {
            _testFileDir = $"{_testDir}\\scope_diff_2";
            _testNumber = 3;
            _startRange = "5:9";
            _endRange = "5:16";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void SimpleExprT1()
        {
            _testFileDir = $"{_testDir}\\simple_expr";
            _testNumber = 1;
            _startRange = "2:17";
            _endRange = "2:19";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void SimpleExprT2()
        {
            _testFileDir = $"{_testDir}\\simple_expr";
            _testNumber = 2;
            _startRange = "2:17";
            _endRange = "2:20";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void SimpleExprT3()
        {
            _testFileDir = $"{_testDir}\\simple_expr";
            _testNumber = 3;
            _startRange = "2:18";
            _endRange = "2:39";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }

        [Test]
        public void SimpleExprT4()
        {
            _testFileDir = $"{_testDir}\\simple_expr";
            _testNumber = 4;
            _startRange = "2:17";
            _endRange = "2:48";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void SimpleExprT5()
        {
            _testFileDir = $"{_testDir}\\simple_expr";
            _testNumber = 5;
            _startRange = "2:29";
            _endRange = "2:34";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void SimpleExprT6()
        {
            _testFileDir = $"{_testDir}\\simple_expr";
            _testNumber = 6;
            _startRange = "2:24";
            _endRange = "2:38";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }

        [Test]
        public void SimpleExprT7()
        {
            _testFileDir = $"{_testDir}\\simple_expr";
            _testNumber = 7;
            _startRange = "2:24";
            _endRange = "2:39";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void SimpleExprT8()
        {
            _testFileDir = $"{_testDir}\\simple_expr";
            _testNumber = 8;
            _startRange = "2:16";
            _endRange = "2:48";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }
    }
}