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
            testDir = $"{testRunnerDir.Parent?.Parent?.Parent?.FullName}\\Tests\\move_method";
        }

        protected string testDir;
        protected string testFileDir;
        protected int testNumber;
        protected string instancePosition;

        protected string TestFilePath => $"{testFileDir}\\source.dfy";
        protected string TestOutputPath => $"{testFileDir}\\test{testNumber}.out";
        protected string TestExpectedPath => $"{testFileDir}\\test{testNumber}.expected";

        protected string[] Args => new[]
        {
            "apply-move-method", "-f", TestFilePath, "-i", instancePosition, "-o", TestOutputPath,
        };

        [Test]
        public void SimpleClassesT1()
        {
            testFileDir = $"{testDir}\\simple_classes";
            testNumber = 1;
            instancePosition = "14:23";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }
    }
}