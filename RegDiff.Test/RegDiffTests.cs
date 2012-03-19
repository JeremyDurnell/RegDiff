using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using RegDiff.Core;

namespace RegDiff.Test
{
    [TestFixture]
    public class RegDiffTests
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            _regDiff = new Core.RegDiff();
        }

        #endregion

        private Core.RegDiff _regDiff;

        [Test]
        public void GetDifferences()
        {
            var baseFile = new MemoryStream(Encoding.UTF8.GetBytes(Resource.RegTestBaseFile));
            var changedFile = new MemoryStream(Encoding.UTF8.GetBytes(Resource.RegTestChangesFile));

            IDictionary<string, Diff> changes = _regDiff.GetDifferences(baseFile, changedFile);

            Assert.AreEqual(4, changes.Count);
        }

        [Test]
        public void GetRegistry()
        {
            var regFile = new MemoryStream(Encoding.UTF8.GetBytes(Resource.RegTestBaseFile));

            IDictionary<string, string> dict;

            using (regFile)
            {
                dict = _regDiff.GetDictionary(regFile);
            }

            Assert.AreEqual(641, dict.Count);
        }
    }
}