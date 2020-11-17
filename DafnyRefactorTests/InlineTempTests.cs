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
            _testDir = $"{testRunnerDir.Parent?.Parent?.Parent?.FullName}\\Tests";
        }

        private string _testDir;
        private string _testFileDir;
        private int _testNumber;
        private int _line;
        private int _column;
        private string TestFilePath => $"{_testFileDir}\\source.dfy";
        private string TestOutputPath => $"{_testFileDir}\\test{_testNumber}.out";
        private string TestExpectedPath => $"{_testFileDir}\\test{_testNumber}.expected";

        private string[] Args => new[]
            {"apply-inline-temp", "-f", TestFilePath, "-l", $"{_line}", "-c", $"{_column}", "-o", TestOutputPath};

        [Test]
        public void LinkedListT1()
        {
            _testFileDir = $"{_testDir}\\linked_list";
            _testNumber = 1;
            _line = 25;
            _column = 9;

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void LinkedListExternalMethodT1()
        {
            _testFileDir = $"{_testDir}\\linked_list_external_method";
            _testNumber = 1;
            _line = 31;
            _column = 9;

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void LinkedListExternalMethodT2()
        {
            _testFileDir = $"{_testDir}\\linked_list_external_method";
            _testNumber = 2;
            _line = 34;
            _column = 5;

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }

        [Test]
        public void LinkedListExternalMethodT3()
        {
            _testFileDir = $"{_testDir}\\linked_list_external_method";
            _testNumber = 3;
            _line = 36;
            _column = 15;

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }

        [Test]
        public void LinkedListUncertainT1()
        {
            _testFileDir = $"{_testDir}\\linked_list_uncertain";
            _testNumber = 1;
            _line = 25;
            _column = 9;

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }

        [Test]
        public void LinkedListValueT1()
        {
            _testFileDir = $"{_testDir}\\linked_list_value";
            _testNumber = 1;
            _line = 34;
            _column = 22;

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(0, exitCode);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void LinkedListWrongT1()
        {
            _testFileDir = $"{_testDir}\\linked_list_wrong";
            _testNumber = 1;
            _line = 25;
            _column = 9;

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }

        [Test]
        public void MultiDeclT1()
        {
            _testFileDir = $"{_testDir}\\multi_decl";
            _testNumber = 1;
            _line = 2;
            _column = 14;

            DafnyRefactorDriver.Main(Args);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void MultiDeclT2()
        {
            _testFileDir = $"{_testDir}\\multi_decl";
            _testNumber = 2;
            _line = 3;
            _column = 7;

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }

        [Test]
        public void MultiDeclT3()
        {
            _testFileDir = $"{_testDir}\\multi_decl";
            _testNumber = 3;
            _line = 4;
            _column = 7;

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }


        [Test]
        public void MutableExprT1()
        {
            _testFileDir = $"{_testDir}\\mutable_expr";
            _testNumber = 1;
            _line = 2;
            _column = 7;

            DafnyRefactorDriver.Main(Args);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void MutableExprT2()
        {
            _testFileDir = $"{_testDir}\\mutable_expr";
            _testNumber = 2;
            _line = 3;
            _column = 7;

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }

        [Test]
        public void MutableExprT3()
        {
            _testFileDir = $"{_testDir}\\mutable_expr";
            _testNumber = 3;
            _line = 4;
            _column = 7;

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }

        [Test]
        public void ScopeDiffT1()
        {
            _testFileDir = $"{_testDir}\\scope_diff";
            _testNumber = 1;
            _line = 2;
            _column = 7;

            DafnyRefactorDriver.Main(Args);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void ScopeDiffT2()
        {
            _testFileDir = $"{_testDir}\\scope_diff";
            _testNumber = 2;
            _line = 9;
            _column = 10;

            DafnyRefactorDriver.Main(Args);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void ScopeDiffT3()
        {
            _testFileDir = $"{_testDir}\\scope_diff";
            _testNumber = 3;
            _line = 7;
            _column = 9;

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }

        [Test]
        public void SeparateDeclAssignT1()
        {
            _testFileDir = $"{_testDir}\\separate_decl_assign";
            _testNumber = 1;
            _line = 7;
            _column = 17;

            DafnyRefactorDriver.Main(Args);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void SeparateDeclAssignT2()
        {
            _testFileDir = $"{_testDir}\\separate_decl_assign";
            _testNumber = 2;
            _line = 4;
            _column = 7;

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }

        [Test]
        public void SeparateDeclAssignT3()
        {
            _testFileDir = $"{_testDir}\\separate_decl_assign";
            _testNumber = 3;
            _line = 7;
            _column = 10;

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }

        [Test]
        public void SeparateDeclAssignT4()
        {
            _testFileDir = $"{_testDir}\\separate_decl_assign";
            _testNumber = 4;
            _line = 3;
            _column = 6;

            DafnyRefactorDriver.Main(Args);
            FileAssert.AreEqual(TestExpectedPath, TestOutputPath);
        }

        [Test]
        public void SeparateDeclAssignT5()
        {
            _testFileDir = $"{_testDir}\\separate_decl_assign";
            _testNumber = 5;
            _line = 4;
            _column = 10;

            var exitCode = DafnyRefactorDriver.Main(Args);
            Assert.AreEqual(2, exitCode);
        }
    }
}