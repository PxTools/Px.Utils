using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Px.Utils.UnitTests.ModelBuilderTests.Fixtures
{
    internal static class PxFileMetaEntries_Robust_1_Language_With_Range_Time_Dimension
    {
        public static List<KeyValuePair<string, string>> Entries =
            [
                new("CHARSET", "\"ANSI\""),
                new("AXIS-VERSION", "\"2013\""),
                new("CODEPAGE", "\"iso-8859-15\""),
                new("LANGUAGE", "\"fi\""),
                new("CREATION-DATE", "\"20200121 09:00\""),
                new("NEXT-UPDATE", "\"20240131 08:00\""),
                new("TABLEID", "\"example_table_id_for_testing\""),
                new("DECIMALS", "0"),
                new("SHOWDECIMALS", "1"),
                new("MATRIX", "\"001_12ab_2022\""),
                new("SUBJECT-CODE", "\"ABCD\""),
                new("SUBJECT-AREA", "\"abcd\""),
                new("COPYRIGHT", "YES"),
                new("DESCRIPTION", "\"test_description_fi\""),
                new("TITLE", "\"test_title_fi\""),
                new("CONTENTS", "\"test_contents_fi\""),
                new("UNITS", "\"test_unit_fi\""),
                new("STUB", "\"Vuosi\",\"Alue\",\"Talotyyppi\""),
                new("HEADING", "\"Tiedot\""),
                new("CONTVARIABLE", "\"Tiedot\""),
                new("VALUES(\"Vuosi\")", "\"2015\",\"2016\",\"2017\",\"2018\",\"2019\",\"2020\",\"2021\",\"2022\""),
                new("VALUES(\"Alue\")", "\"Koko maa\",\"Pääkaupunkiseutu (PKS)\",\"Muu Suomi (koko maa pl. PKS)\",\"Helsinki\", \"Espoo-Kauniainen\",\"Vantaa\",\"Turku\""),
                new("VALUES(\"Talotyyppi\")", "\"Talotyypit yhteensä\",\"Rivitalot\",\"Kerrostalot\""),
                new("VALUES(\"Tiedot\")", "\"Indeksi (2015=100)\",\"Muutos edelliseen vuoteen (indeksi 2015=100)\",\"Kauppojen lukumäärä\""),
                new("TIMEVAL(\"Vuosi\")", "TLIST(A1, \"2015-2022\")"),
                new("CODES(\"Vuosi\")", "\"2015\",\"2016\",\"2017\",\"2018\",\"2019\",\"2020\",\"2021\",\"2022\""),
                new("CODES(\"Alue\")", "\"ksu\",\"pks\",\"msu\",\"091\",\"049\",\"092\",\"853\""),
                new("CODES(\"Talotyyppi\")", "\"0\",\"1\",\"3\""),
                new("CODES(\"Tiedot\")", "\"ketjutettu_lv\",\"vmuutos_lv\",\"lkm_julk_uudet\""),
                new("VARIABLE-TYPE(\"Vuosi\")", "\"Time\""),
                new("VARIABLE-TYPE(\"Alue\")", "\"Classificatory\""),
                new("VARIABLE-TYPE(\"Talotyyppi\")", "\"Classificatory\""),
                new("MAP(\"Alue\")", "\"Alue 2018\""),
                new("ELIMINATION(\"Talotyyppi\")", "\"Talotyypit yhteensä\""),
                new("PRECISION(\"Tiedot\",\"Muutos edelliseen vuoteen (indeksi 2015=100)\")", "1"),
                new("LAST-UPDATED(\"Indeksi (2015=100)\")", "\"20230131 08:00\""),
                new("LAST-UPDATED(\"Muutos edelliseen vuoteen (indeksi 2015=100)\")", "\"20230131 09:00\""),
                new("LAST-UPDATED(\"Kauppojen lukumäärä\")", "\"20230131 10:00\""),
                new("UNITS(\"Indeksi (2015=100)\")", "\"indeksipisteluku\""),
                new("UNITS(\"Muutos edelliseen vuoteen (indeksi 2015=100)\")", "\"%\""),
                new("UNITS", "\"lukumäärä\""), // table level units
                new("CONTACT(\"Indeksi (2015=100)\")", "\"test_contact1_fi\""),
                new("CONTACT(\"Muutos edelliseen vuoteen (indeksi 2015=100)\")", "\"test_contact2_fi\""),
                new("CONTACT(\"Kauppojen lukumäärä\")", "\"test_contact3_fi\""),
                new("SOURCE", "\"test_source_fi\""),
                new("OFFICIAL-STATISTICS", "YES"),
                new("NOTE", "\"test_note_fi\""),
                new("NOTE(\"Talotyyppi\")", "\"test_note_talotyyppi\""),
                new("VALUENOTE(\"Tiedot\",\"Indeksi (2015=100)\")", "\"test_value_note_tiedot_indeksi\""),
                new("VALUENOTE(\"Tiedot\",\"Muutos edelliseen vuoteen (indeksi 2015=100)\")", "\"test_value_note_tiedot_muutos\""),
                new("VALUENOTE(\"Tiedot\",\"Kauppojen lukumäärä\")", "\"test_value_note_tiedot_kauppojen_lukumäärä\"")
            ];
    }
}
