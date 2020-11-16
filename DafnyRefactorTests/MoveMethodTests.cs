using System.IO;
using Microsoft.DafnyRefactor;
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
            "apply-move-method", "-f", TestFilePath, "-i", instancePosition, "-o", TestOutputPath
        };

        [Test]
        public void ComplexClassesT1()
        {
            testFileDir = $"{testDir}\\complex_classes";
            testNumber = 1;
            instancePosition = "24:23";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void ComplexClassesT2()
        {
            testFileDir = $"{testDir}\\complex_classes";
            testNumber = 2;
            instancePosition = "7:16";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }

        [Test]
        public void ComplexClassesT3()
        {
            testFileDir = $"{testDir}\\complex_classes";
            testNumber = 3;
            instancePosition = "32:17";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }

        [Test]
        public void EqualMethodsT1()
        {
            testFileDir = $"{testDir}\\equal_methods";
            testNumber = 1;
            instancePosition = "28:23";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }

        [Test]
        public void EqualMethodsT2()
        {
            testFileDir = $"{testDir}\\equal_methods";
            testNumber = 2;
            instancePosition = "36:17";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void MethodCallT1()
        {
            testFileDir = $"{testDir}\\method_call";
            testNumber = 1;
            instancePosition = "27:19";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }

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

        [Test]
        public void SimpleClassesT2()
        {
            testFileDir = $"{testDir}\\simple_classes";
            testNumber = 2;
            instancePosition = "14:29";

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }
    }
}