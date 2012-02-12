using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;

namespace bleistift.QuickTestSwitcher
{
    public class Switcher
    {
        static readonly string[] suffixes = new[] { "Test", "Scenario", "Spec" };

        readonly Project proj;
        readonly Document doc;

        public Switcher(Project proj, Document doc)
        {
            this.proj = proj;
            this.doc = doc;
        }

        public bool IsTestProject
        {
            get
            {
                var name = proj.Name;
                return suffixes.Any(s => name.EndsWith(s) || name.EndsWith(s + "s"));
            }
        }

        public Project TargetProject
        {
            get
            {
                Func<Project, bool> isProductionProject =
                    p => proj.Name.StartsWith(p.Name);
                Func<Project, bool> isTestProject =
                    p => p.Name.StartsWith(proj.Name);

                // テストプロジェクトの場合は製品プロジェクトを、そうでない場合はテストプロジェクトを取得したい
                var isTargetProj = IsTestProject ? isProductionProject : isTestProject;

                var sol = proj.DTE.Solution;
                foreach (Project p in sol.Projects)
                {
                    if (p.Name == proj.Name)
                        continue;

                    if (isTargetProj(p))
                        return p;
                }
                throw new TargetProjectNotFound();
            }
        }

        public ProjectItem TargetProjectItem
        {
            get
            {
                Func<string, bool> just =
                    str => suffixes.Any(s => str.StartsWith(s));
                Func<string, string> substrBeforeLastDot =
                    str =>
                    {
                        var i = str.LastIndexOf(".");
                        return i == -1 ? "" : str.Substring(0, str.LastIndexOf("."));
                    };

                Func<ProjectItem, bool> isProductionProjItem =
                    pi =>
                    {
                        var name = substrBeforeLastDot(pi.Name);
                        return doc.Name.StartsWith(name) && just(doc.Name.Substring(name.Length));
                    };
                Func<ProjectItem, bool> isTestProjItem =
                    pi =>
                    {
                        var name = substrBeforeLastDot(doc.Name);
                        return pi.Name.StartsWith(name) && just(pi.Name.Substring(name.Length));
                    };

                // テストプロジェクトの場合は製品コードを、そうでない場合はテストコードを取得したい
                var isTargetProjItem = IsTestProject ? isProductionProjItem : isTestProjItem;

                Func<ProjectItems, ProjectItem> f = null;
                f = items =>
                {
                    foreach (ProjectItem pi in items)
                    {
                        if (pi.ProjectItems.Count != 0 && !pi.Name.EndsWith(".tt"))
                        {
                            var res = f(pi.ProjectItems);
                            if (res != null)
                                return res;
                        }

                        if (isTargetProjItem(pi))
                            return pi;
                    }
                    return null;
                };

                var r = f(TargetProject.ProjectItems);
                if (r != null)
                    return r;
                throw new TargetProjectItemNotFound();
            }
        }
    }
}
