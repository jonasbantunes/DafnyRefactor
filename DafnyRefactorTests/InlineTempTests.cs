using System.IO;
using DafnyRefactor;
using NUnit.Framework;

namespace DafnyRefactorTests
{
    [TestFixture]
    public class InlineTempTests
    {
        [SetUp]
        protected void SetUp()
        {
            var testRunnerDir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            testDir = $"{testRunnerDir.Parent?.Parent?.Parent?.FullName}\\Tests\\inline_temp";
        }

        protected string testDir;
        protected string testFileDir;
        protected int testNumber;
        protected int line;
        protected int column;
        protected string TestFilePath => $"{testFileDir}\\source.dfy";
        protected string TestOutputPath => $"{testFileDir}\\test{testNumber}.out";
        protected string TestExpectedPath => $"{testFileDir}\\test{testNumber}.expected";

        protected string[] Args => new[]
            {"apply-inline-temp", "-f", TestFilePath, "-l", $"{line}", "-c", $"{column}", "-o", TestOutputPath};

        [Test]
        public void MultiDeclT1()
        {
            testFileDir = $"{testDir}\\multi_decl";
            testNumber = 1;
            line = 2;
            column = 14;

            DafnyRefactorDriver.Main(Args);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void MultiDeclT2()
        {
            testFileDir = $"{testDir}\\multi_decl";
            testNumber = 2;
            line = 3;
            column = 7;

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }

        [Test]
        public void MultiDeclT3()
        {
            testFileDir = $"{testDir}\\multi_decl";
            testNumber = 3;
            line = 4;
            column = 7;

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }


        [Test]
        public void MutableExprT1()
        {
            testFileDir = $"{testDir}\\mutable_expr";
            testNumber = 1;
            line = 2;
            column = 7;

            DafnyRefactorDriver.Main(Args);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void MutableExprT2()
        {
            testFileDir = $"{testDir}\\mutable_expr";
            testNumber = 2;
            line = 3;
            column = 7;

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }

        [Test]
        public void MutableExprT3()
        {
            testFileDir = $"{testDir}\\mutable_expr";
            testNumber = 3;
            line = 4;
            column = 7;

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }

        [Test]
        public void ScopeDiffT1()
        {
            testFileDir = $"{testDir}\\scope_diff";
            testNumber = 1;
            line = 2;
            column = 7;

            DafnyRefactorDriver.Main(Args);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void ScopeDiffT2()
        {
            testFileDir = $"{testDir}\\scope_diff";
            testNumber = 2;
            line = 9;
            column = 10;

            DafnyRefactorDriver.Main(Args);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void ScopeDiffT3()
        {
            testFileDir = $"{testDir}\\scope_diff";
            testNumber = 3;
            line = 7;
            column = 9;

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }

        [Test]
        public void SeparateDeclAssignT1()
        {
            testFileDir = $"{testDir}\\separate_decl_assign";
            testNumber = 1;
            line = 7;
            column = 17;

            DafnyRefactorDriver.Main(Args);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void SeparateDeclAssignT2()
        {
            testFileDir = $"{testDir}\\separate_decl_assign";
            testNumber = 2;
            line = 4;
            column = 7;

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }

        [Test]
        public void SeparateDeclAssignT3()
        {
            testFileDir = $"{testDir}\\separate_decl_assign";
            testNumber = 3;
            line = 7;
            column = 10;

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }

        [Test]
        public void SeparateDeclAssignT4()
        {
            testFileDir = $"{testDir}\\separate_decl_assign";
            testNumber = 4;
            line = 3;
            column = 6;

            DafnyRefactorDriver.Main(Args);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void SeparateDeclAssignT5()
        {
            testFileDir = $"{testDir}\\separate_decl_assign";
            testNumber = 5;
            line = 4;
            column = 10;

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }
    }
}