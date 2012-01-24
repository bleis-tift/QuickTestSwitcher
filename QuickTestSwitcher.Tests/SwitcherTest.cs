using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using EnvDTE;
using Moq;
using bleistift.QuickTestSwitcher;

namespace QuickTestSwitcher.Tests
{
    [TestFixture]
    public class SwitcherTest
    {
        static ProjectItems EmptyProjItems()
        {
            var mock = new Mock<ProjectItems>();
            mock.Setup(_ => _.Count).Returns(0);
            return mock.Object;
        }

        static ProjectItem[] ProjItems(params string[] names)
        {
            return names.Select(n =>
            {
                var mock = new Mock<ProjectItem>();
                mock.Setup(_ => _.Name).Returns(n);
                mock.Setup(_ => _.ProjectItems).Returns(EmptyProjItems());
                return mock.Object;
            }).ToArray();
        }

        static ProjectItem[] ProjItems(string name)
        {
            switch (name)
            {
                case "Hoge":
                    return ProjItems("Dir", "A.cs", "B.cs");
                case "Hoge.Test":
                    return ProjItems("ATest.cs", "BScenario.cs");
                case "Piyo":
                    return ProjItems("A.fs", "B.fs");
                case "Piyo.Tests":
                    return ProjItems("ATest.fs", "BScenario.fs");
                case "Foo":
                    return ProjItems("Hoge.cs", "HogeHoge.cs");
                case "FooTest":
                    return ProjItems("HogeHogeTest.fs", "HogeTest.fs");
                default:
                    return new ProjectItem[0];
            }
        }

        static Project[] Projects()
        {
            return new[] { "Hoge", "Piyo", "Piyo.Tests", "Hoge.Test", "Foo", "FooTest" }.Select(n =>
            {
                var projItemsMock = new Mock<ProjectItems>();
                projItemsMock.Setup(_ => _.GetEnumerator()).Returns(ProjItems(n).GetEnumerator());
                projItemsMock.Setup(_ => _.Count).Returns(0);

                var mock = new Mock<Project>();
                mock.Setup(_ => _.Name).Returns(n);
                mock.Setup(_ => _.ProjectItems).Returns(projItemsMock.Object);
                return mock.Object;
            }).ToArray();
        }

        static Project NewProjectMock(string activeProjectName)
        {
            var projsMock = new Mock<Projects>();
            projsMock.Setup(_ => _.GetEnumerator())
                .Returns(Projects().GetEnumerator());

            var solMock = new Mock<Solution>();
            solMock.Setup(_ => _.Projects).Returns(projsMock.Object);

            var dteMock = new Mock<DTE>();
            dteMock.Setup(_ => _.Solution).Returns(solMock.Object);

            var mock = new Mock<Project>();
            mock.Setup(_ => _.Name).Returns(activeProjectName);
            mock.Setup(_ => _.DTE).Returns(dteMock.Object);

            return mock.Object;
        }

        static Document NewDocumentMock(string activeDocName)
        {
            var mock = new Mock<Document>();
            mock.Setup(_ => _.Name).Returns(activeDocName);
            return mock.Object;
        }

        public class IsTestProject
        {
            [TestCase("Hoge", false)]
            [TestCase("Hoge.Test", true)]
            [TestCase("HogeTest", true)]
            [TestCase("Hoge.Tests", true)]
            [TestCase("Hoge.Scenario", true)]
            [TestCase("Hoge.Scenarios", true)]
            public void テストプロジェクトかどうか判定できる(string projectName, bool expected)
            {
                var mock = new Mock<Project>();
                mock.Setup(_ => _.Name).Returns(projectName);

                var proj = mock.Object;
                var switcher = new Switcher(proj, null);
                Assert.That(switcher.IsTestProject, Is.EqualTo(expected));
            }
        }

        public class TargetProject
        {
            [TestCase("Hoge", "Hoge.Test")]
            [TestCase("Piyo", "Piyo.Tests")]
            [TestCase("Hoge.Test", "Hoge")]
            [TestCase("Piyo.Tests", "Piyo")]
            public void 対象のプロジェクトを取得できる(string activeProjectName, string expected)
            {
                var proj = NewProjectMock(activeProjectName);
                var switcher = new Switcher(proj, null);
                Assert.That(switcher.TargetProject.Name, Is.EqualTo(expected));
            }
        }

        public class TargetProjectItem
        {
            [TestCase("Hoge", "A.cs", "ATest.cs")]
            [TestCase("Hoge", "B.cs", "BScenario.cs")]
            [TestCase("Hoge.Test", "ATest.cs", "A.cs")]
            [TestCase("Hoge.Test", "BScenario.cs", "B.cs")]
            [TestCase("Piyo", "A.fs", "ATest.fs")]
            [TestCase("Piyo", "B.fs", "BScenario.fs")]
            [TestCase("Piyo.Tests", "ATest.fs", "A.fs")]
            [TestCase("Piyo.Tests", "BScenario.fs", "B.fs")]
            [TestCase("Foo", "Hoge.cs", "HogeTest.fs")]
            [TestCase("Foo", "HogeHoge.cs", "HogeHogeTest.fs")]
            [TestCase("FooTest", "HogeTest.fs", "Hoge.cs")]
            [TestCase("FooTest", "HogeHogeTest.fs", "HogeHoge.cs")]
            public void 対象のプロジェクトアイテムを取得できる(string activeProjectName, string activeDocName, string expected)
            {
                var proj = NewProjectMock(activeProjectName);
                var doc = NewDocumentMock(activeDocName);
                var switcher = new Switcher(proj, doc);
                Assert.That(switcher.TargetProjectItem.Name, Is.EqualTo(expected));
            }
        }
    }
}
