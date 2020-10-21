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
            testDir = $"{testRunnerDir.Parent?.Parent?.Parent?.FullName}\\Tests\\extract_variable";
        }

        protected string testDir;
        protected string testFileDir;
        protected int testNumber;
        protected string startRange;
        protected string endRange;

        protected string TestFilePath => $"{testFileDir}\\source.dfy";
        protected string TestOutputPath => $"{testFileDir}\\test{testNumber}.out";
        protected string TestExpectedPath => $"{testFileDir}\\test{testNumber}.expected";

        protected string[] Args => new[]
            {"apply-extract-variable", "-f", TestFilePath, "-s", startRange, "-e", endRange, "-o", TestOutputPath, "-v", "extractedVar"};

        [Test]
        public void ClassExprT1()
        {
            testFileDir = $"{testDir}\\class_expr";
            testNumber = 1;
            startRange = "17:24";
            endRange = "17:25";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void MethodCallT1()
        {
            testFileDir = $"{testDir}\\method_call";
            testNumber = 1;
            startRange = "9:26";
            endRange = "9:28";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void MethodCallT2()
        {
            testFileDir = $"{testDir}\\method_call";
            testNumber = 2;
            startRange = "9:26";
            endRange = "9:30";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void MethodCallT3()
        {
            testFileDir = $"{testDir}\\method_call";
            testNumber = 3;
            startRange = "9:32";
            endRange = "9:36";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void MethodCallT4()
        {
            testFileDir = $"{testDir}\\method_call";
            testNumber = 4;
            startRange = "9:32";
            endRange = "9:39";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void MethodCallT5()
        {
            testFileDir = $"{testDir}\\method_call";
            testNumber = 5;
            startRange = "9:27";
            endRange = "9:37";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }

        [Test]
        public void SimpleExprT1()
        {
            testFileDir = $"{testDir}\\simple_expr";
            testNumber = 1;
            startRange = "2:17";
            endRange = "2:19";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void SimpleExprT2()
        {
            testFileDir = $"{testDir}\\simple_expr";
            testNumber = 2;
            startRange = "2:17";
            endRange = "2:20";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void SimpleExprT3()
        {
            testFileDir = $"{testDir}\\simple_expr";
            testNumber = 3;
            startRange = "2:18";
            endRange = "2:39";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }

        [Test]
        public void SimpleExprT4()
        {
            testFileDir = $"{testDir}\\simple_expr";
            testNumber = 4;
            startRange = "2:17";
            endRange = "2:48";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void SimpleExprT5()
        {
            testFileDir = $"{testDir}\\simple_expr";
            testNumber = 5;
            startRange = "2:29";
            endRange = "2:34";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void SimpleExprT6()
        {
            testFileDir = $"{testDir}\\simple_expr";
            testNumber = 6;
            startRange = "2:24";
            endRange = "2:38";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }

        [Test]
        public void SimpleExprT7()
        {
            testFileDir = $"{testDir}\\simple_expr";
            testNumber = 7;
            startRange = "2:24";
            endRange = "2:39";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }
    }
}