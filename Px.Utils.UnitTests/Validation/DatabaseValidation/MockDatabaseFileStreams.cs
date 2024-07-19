using Px.Utils.UnitTests.Validation.Fixtures;

namespace Px.Utils.UnitTests.Validation.DatabaseValidation
{
    internal static class MockDatabaseFileStreams
    {
        private readonly static string _databaseAliasFi = "database/Alias_fi.txt";
        private readonly static string _databaseAliasEn = "database/Alias_en.txt";
        private readonly static string _databaseAliasSv = "database/Alias_sv.txt";
        private readonly static string _databaseCategoryAliasFi = "database/category/Alias_fi.txt";
        private readonly static string _databaseCategoryAliasEn = "database/category/Alias_en.txt";
        private readonly static string _databaseCategoryAliasSv = "database/category/Alias_sv.txt";
        private readonly static string _databaseCategoryDirectoryAliasFi = "database/category/directory/Alias_fi.txt";
        private readonly static string _databaseCategoryDirectoryAliasEn = "database/category/directory/Alias_en.txt";
        private readonly static string _databaseCategoryDirectoryAliasSv = "database/category/directory/Alias_sv.txt";

        internal static Dictionary<string, string> FileStreams = new()
        {
            { _databaseAliasFi, _databaseAliasFi },
            { _databaseAliasEn, _databaseAliasEn },
            { _databaseAliasSv, _databaseAliasSv },
            { _databaseCategoryAliasFi, _databaseCategoryAliasFi },
            { _databaseCategoryAliasEn, _databaseCategoryAliasEn },
            { _databaseCategoryAliasSv, _databaseCategoryAliasSv },
            { _databaseCategoryDirectoryAliasFi, _databaseCategoryDirectoryAliasFi },
            { _databaseCategoryDirectoryAliasEn, _databaseCategoryDirectoryAliasEn },
            { _databaseCategoryDirectoryAliasSv, _databaseCategoryDirectoryAliasSv },
            { "database/category/directory/foo.px", PxFileFixtures.MINIMAL_PX_FILE },
            { "database/category/directory/bar.px", PxFileFixtures.MINIMAL_PX_FILE },
            { "database/category/directory/baz.px", PxFileFixtures.MINIMAL_PX_FILE },
            { "database/category/directory/invalid.px", PxFileFixtures.INVALID_PX_FILE }
        };
    }
}
