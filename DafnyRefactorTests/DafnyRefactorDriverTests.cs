using NUnit.Framework;
using System.IO;

namespace Microsoft.Dafny.Tests
{
    [TestFixture()]
    public class DafnyRefactorDriverTests
    {
        protected string testDir;
        protected string testFileDir;
        protected int testNumber;
        protected string TestFilePath => $"{testFileDir}\\source.dfy";
        protected string TestOutputPath => $"{testFileDir}\\test{testNumber}.out";
        protected string TestExpectedPath => $"{testFileDir}\\test{testNumber}.expected";

        [SetUp]
        protected void SetUp()
        {
            var testRunnerDir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            testDir = $"{testRunnerDir.Parent.Parent.Parent.FullName}\\Tests";

        }

        [Test()]
        public void IPMultiDeclT1()
        {
            testFileDir = $"{testDir}\\inline_temp\\multi_decl";
            testNumber = 1;

            int line = 2;
            int column = 14;
            var args = new string[] { "apply-inline-temp", TestFilePath, $"{line}", $"{column}", "-o", TestOutputPath };

            DafnyRefactorDriver.Main(args);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test()]
        public void IPMutableExprT1()
        {
            testFileDir = $"{testDir}\\inline_temp\\mutable_expr";
            testNumber = 1;

            int line = 2;
            int column = 7;
            var args = new string[] { "apply-inline-temp", TestFilePath, $"{line}", $"{column}", "-o", TestOutputPath };

            DafnyRefactorDriver.Main(args);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test()]
        public void IPScopeDiffT1()
        {
            testFileDir = $"{testDir}\\inline_temp\\scope_diff";
            testNumber = 1;

            int line = 2;
            int column = 7;
            var args = new string[] { "apply-inline-temp", TestFilePath, $"{line}", $"{column}", "-o", TestOutputPath };

            DafnyRefactorDriver.Main(args);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test()]
        public void IPSeparateDeclAssignT1()
        {
            testFileDir = $"{testDir}\\inline_temp\\separate_decl_assign";
            testNumber = 1;

            int line = 2;
            int column = 7;
            var args = new string[] { "apply-inline-temp", TestFilePath, $"{line}", $"{column}", "-o", TestOutputPath };

            DafnyRefactorDriver.Main(args);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }
    }
}