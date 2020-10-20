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
            startRange = "17:27";
            endRange = "17:39";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }

        [Test]
        public void SimpleExprT1()
        {
            testFileDir = $"{testDir}\\simple_expr";
            testNumber = 1;
            startRange = "2:18";
            endRange = "2:20";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }
    }
}