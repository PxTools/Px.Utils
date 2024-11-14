using Px.Utils.ModelBuilders;

namespace Px.Utils.UnitTests.ModelBuilderTests.ValueParserUtilitiesTests
{
    [TestClass]
    public class GetTimeValValueListTests
    {
        [TestMethod]
        public void GetTimeValValueListTestEmptyInputReturnsEmptyList()
        {
            string input = "TLIST(A1)";
            List<string> expected = [];
            List<string> actual = ValueParserUtilities.GetTimeValValueList(input);

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetTimeValValueListTestRangeInputReturnsEmptyList()
        {
            string input = "TLIST(A1, 9000-9001)";
            List<string> expected = [];
            List<string> actual = ValueParserUtilities.GetTimeValValueList(input);

            CollectionAssert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void GetTimeValValueListTestListOfOneInputReturnsList()
        {
            string input = "TLIST(A1), \"9001\"";
            List<string> expected = ["9001"];
            List<string> actual = ValueParserUtilities.GetTimeValValueList(input);

            CollectionAssert.AreEqual(expected, actual);
        }
        
        [TestMethod]
        public void GetTimeValValueListTestListOfFiveInputReturnsList()
        {
            string input = "TLIST(A1), \"9000\", \"9001\", \"9002\", \"9003\", \"9004\"";
            List<string> expected = ["9000", "9001", "9002", "9003", "9004"];
            List<string> actual = ValueParserUtilities.GetTimeValValueList(input);

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
