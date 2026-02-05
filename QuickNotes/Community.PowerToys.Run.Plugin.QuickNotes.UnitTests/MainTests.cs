using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.Linq;
using System.Threading;
using Wox.Plugin;
using Community.PowerToys.Run.Plugin.QuickNotes.Properties;

namespace Community.PowerToys.Run.Plugin.QuickNotes.UnitTests
{
    [TestClass]
    public class MainTests
    {
        private Main main;

        [TestInitialize]
        public void TestInitialize()
        {
            main = new Main();
        }

        [TestMethod]
        public void Query_should_return_results()
        {
            var results = main.Query(new("search"));

            Assert.IsNotNull(results.First());
        }

        [TestMethod]
        public void LoadContextMenus_should_return_results()
        {
            var results = main.LoadContextMenus(new Result { ContextData = "search" });

            Assert.IsNotNull(results.First());
        }
    }

    [TestClass]
    public class LocalizationTests
    {
        private CultureInfo originalCulture;
        private CultureInfo originalUICulture;

        [TestInitialize]
        public void TestInitialize()
        {
            originalCulture = Thread.CurrentThread.CurrentCulture;
            originalUICulture = Thread.CurrentThread.CurrentUICulture;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Thread.CurrentThread.CurrentCulture = originalCulture;
            Thread.CurrentThread.CurrentUICulture = originalUICulture;
            Resources.Culture = null;
        }

        [TestMethod]
        public void Resources_English_PluginName_ShouldBeCorrect()
        {
            Resources.Culture = new CultureInfo("en-US");
            Assert.AreEqual("QuickNotes", Resources.PluginName);
        }

        [TestMethod]
        public void Resources_English_PluginDescription_ShouldBeCorrect()
        {
            Resources.Culture = new CultureInfo("en-US");
            Assert.AreEqual("Save, view, manage, search, tag, and pin quick notes", Resources.PluginDescription);
        }

        [TestMethod]
        public void Resources_Ukrainian_PluginName_ShouldBeCorrect()
        {
            Resources.Culture = new CultureInfo("uk");
            Assert.AreEqual("Швидкі нотатки", Resources.PluginName);
        }

        [TestMethod]
        public void Resources_Ukrainian_PluginDescription_ShouldBeCorrect()
        {
            Resources.Culture = new CultureInfo("uk");
            Assert.AreEqual("Зберігайте, переглядайте, керуйте, шукайте, тегуйте та закріплюйте швидкі нотатки", Resources.PluginDescription);
        }

        [TestMethod]
        public void Resources_Ukrainian_CommandHelp_ShouldBeCorrect()
        {
            Resources.Culture = new CultureInfo("uk");
            Assert.AreEqual("Показати довідку з доступними командами ℹ️", Resources.CommandHelp);
        }

        [TestMethod]
        public void Resources_Ukrainian_NoteSaved_ShouldBeCorrect()
        {
            Resources.Culture = new CultureInfo("uk");
            Assert.AreEqual("Нотатку збережено", Resources.NoteSaved);
        }

        [TestMethod]
        public void Resources_Ukrainian_NoteDeleted_ShouldBeCorrect()
        {
            Resources.Culture = new CultureInfo("uk");
            Assert.AreEqual("Нотатку видалено", Resources.NoteDeleted);
        }

        [TestMethod]
        public void Resources_Ukrainian_NotePinned_ShouldBeCorrect()
        {
            Resources.Culture = new CultureInfo("uk");
            Assert.AreEqual("Нотатку закріплено", Resources.NotePinned);
        }

        [TestMethod]
        public void Resources_Ukrainian_NoteUnpinned_ShouldBeCorrect()
        {
            Resources.Culture = new CultureInfo("uk");
            Assert.AreEqual("Нотатку відкріплено", Resources.NoteUnpinned);
        }

        [TestMethod]
        public void Resources_Ukrainian_Error_ShouldBeCorrect()
        {
            Resources.Culture = new CultureInfo("uk");
            Assert.AreEqual("Помилка", Resources.Error);
        }

        [TestMethod]
        public void Resources_Ukrainian_Cancel_ShouldBeCorrect()
        {
            Resources.Culture = new CultureInfo("uk");
            Assert.AreEqual("Скасувати", Resources.Cancel);
        }

        [TestMethod]
        public void Resources_ChineseSimplified_PluginName_ShouldBeCorrect()
        {
            Resources.Culture = new CultureInfo("zh-CN");
            Assert.AreEqual("快速笔记", Resources.PluginName);
        }

        [TestMethod]
        public void Resources_ChineseSimplified_PluginDescription_ShouldBeCorrect()
        {
            Resources.Culture = new CultureInfo("zh-CN");
            Assert.AreEqual("保存、查看、管理、搜索、标签和置顶快速笔记", Resources.PluginDescription);
        }

        [TestMethod]
        public void Resources_AllLocales_ShouldHaveNonEmptyPluginName()
        {
            var cultures = new[] { "en-US", "uk", "zh-CN" };
            foreach (var cultureName in cultures)
            {
                Resources.Culture = new CultureInfo(cultureName);
                Assert.IsFalse(string.IsNullOrEmpty(Resources.PluginName), $"PluginName should not be empty for culture {cultureName}");
            }
        }

        [TestMethod]
        public void Resources_AllLocales_ShouldHaveNonEmptyPluginDescription()
        {
            var cultures = new[] { "en-US", "uk", "zh-CN" };
            foreach (var cultureName in cultures)
            {
                Resources.Culture = new CultureInfo(cultureName);
                Assert.IsFalse(string.IsNullOrEmpty(Resources.PluginDescription), $"PluginDescription should not be empty for culture {cultureName}");
            }
        }
    }
}
