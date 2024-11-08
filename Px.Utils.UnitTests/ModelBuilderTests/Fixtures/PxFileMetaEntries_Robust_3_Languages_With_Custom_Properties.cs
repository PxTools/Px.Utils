﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Px.Utils.UnitTests.ModelBuilderTests.Fixtures
{
    internal static class PxFileMetaEntries_Robust_3_Languages_With_Custom_Properties
    {
        public static List<KeyValuePair<string, string>> Entries =
           [
               new("CHARSET", "\"ANSI\""),
               new("AXIS-VERSION", "\"2013\""),
               new("CODEPAGE", "\"iso-8859-15\""),
               new("LANGUAGE", "\"fi\""),
               new("LANGUAGES", "\"fi\",\"sv\",\"en\""),
               new("CREATION-DATE", "\"20200121 09:00\""),
               new("NEXT-UPDATE", "\"20240131 08:00\""),
               new("TABLEID", "\"example_table_id_for_testing\""),
               new("DECIMALS", "1"),
               new("SHOWDECIMALS", "0"),
               new("MATRIX", "\"001_12ab_2022\""),
               new("SUBJECT-CODE", "\"ABCD\""),
               new("SUBJECT-AREA", "\"abcd\""),
               new("SUBJECT-AREA[sv]", "\"abcd\""),
               new("SUBJECT-AREA[en]", "\"abcd\""),
               new("COPYRIGHT", "YES"),
               new("DESCRIPTION", "\"test_description_fi\""),
               new("DESCRIPTION[sv]", "\"test_description_sv\""),
               new("DESCRIPTION[en]", "\"test_description_en\""),
               new("TITLE", "\"test_title_fi\""),
               new("TITLE[sv]", "\"test_title_sv\""),
               new("TITLE[en]", "\"test_title_en\""),
               new("CONTENTS", "\"test_contents_fi\""),
               new("CONTENTS[sv]", "\"test_contents_sv\""),
               new("CONTENTS[en]", "\"test_content_en\""),
               new("UNITS", "\"test_unit_fi\""),
               new("UNITS[sv]", "\"test_unit_sv\""),
               new("UNITS[en]", "\"test_unit_en\""),
               new("STUB", "\"Vuosi\",\"Alue\",\"Talotyyppi\""),
               new("STUB[sv]", "\"År\",\"Område\",\"Hustyp\""),
               new("STUB[en]", "\"Year\",\"Region\",\"Building type\""),
               new("HEADING", "\"Tiedot\""),
               new("HEADING[sv]", "\"Uppgifter\""),
               new("HEADING[en]", "\"Information\""),
               new("CONTVARIABLE", "\"Tiedot\""),
               new("CONTVARIABLE[sv]", "\"Uppgifter\""),
               new("CONTVARIABLE[en]", "\"Information\""),
               new("VALUES(\"Vuosi\")", "\"2015\",\"2016\",\"2017\",\"2018\",\"2019\",\"2020\",\"2021\",\"2022\""),
               new("VALUES[sv](\"År\")", "\"2015\",\"2016\",\"2017\",\"2018\",\"2019\",\"2020\",\"2021\",\"2022\""),
               new("VALUES[en](\"Year\")", "\"2015\",\"2016\",\"2017\",\"2018\",\"2019\",\"2020\",\"2021\",\"2022\""),
               new("VALUES(\"Alue\")", "\"Koko maa\",\"Pääkaupunkiseutu (PKS)\",\"Muu Suomi (koko maa pl. PKS)\",\"Helsinki\", \"Espoo-Kauniainen\",\"Vantaa\",\"Turku\""),
               new("VALUES[sv](\"Område\")", "\"Hela landet\",\"Huvudstadsregionen\", \"Övriga Finland (Hela landet utan Huvudstadsregionen)\",\"Helsingfors\",\"Esbo-Grankulla\",\"Vanda\",\"Åbo\""),
               new("VALUES[en](\"Region\")", "\"Whole country\",\"Greater Helsinki\",\"Whole country excluding Greater Helsinki\",\"Helsinki\",\"Espoo-Kauniainen\",\"Vantaa\",\"Turku\""),
               new("VALUES(\"Talotyyppi\")", "\"Talotyypit yhteensä\",\"Rivitalot\",\"Kerrostalot\""),
               new("VALUES[sv](\"Hustyp\")", "\"Hustyp totalt\",\"Radhus\",\"Flervåningshus\""),
               new("VALUES[en](\"Building type\")", "\"Building types total\",\"Terraced houses\",\"Blocks of flats\""),
               new("VALUES(\"Tiedot\")", "\"Indeksi (2015=100)\",\"Muutos edelliseen vuoteen (indeksi 2015=100)\",\"Kauppojen lukumäärä\""),
               new("VALUES[sv](\"Uppgifter\")", "\"Index (2015=100)\",\"Årsförändring (index 2015=100)\",\"Antal\""),
               new("VALUES[en](\"Information\")", "\"Index (2015=100)\",\"Annual change (index 2015=100)\",\"Number\""),
               new("TIMEVAL(\"Vuosi\")", "TLIST(A1),\"2015\",\"2016\",\"2017\",\"2018\",\"2019\",\"2020\",\"2021\",\"2022\""),
               new("TIMEVAL[sv](\"År\")", "TLIST(A1),\"2015\",\"2016\",\"2017\",\"2018\",\"2019\",\"2020\",\"2021\",\"2022\""),
               new("TIMEVAL[en](\"Year\")", "TLIST(A1),\"2015\",\"2016\",\"2017\",\"2018\",\"2019\",\"2020\",\"2021\",\"2022\""),
               new("CODES(\"Vuosi\")", "\"2015\",\"2016\",\"2017\",\"2018\",\"2019\",\"2020\",\"2021\",\"2022\""),
               new("CODES[sv](\"År\")", "\"2015\",\"2016\",\"2017\",\"2018\",\"2019\",\"2020\",\"2021\",\"2022\""),
               new("CODES[en](\"Year\")", "\"2015\",\"2016\",\"2017\",\"2018\",\"2019\",\"2020\",\"2021\",\"2022\""),
               new("CODES(\"Alue\")", "\"ksu\",\"pks\",\"msu\",\"091\",\"049\",\"092\",\"853\""),
               new("CODES[sv](\"Område\")", "\"ksu\",\"pks\",\"msu\",\"091\",\"049\",\"092\",\"853\""),
               new("CODES[en](\"Region\")", "\"ksu\",\"pks\",\"msu\",\"091\",\"049\",\"092\",\"853\""),
               new("CODES(\"Talotyyppi\")", "\"0\",\"1\",\"3\""),
               new("CODES[sv](\"Hustyp\")", "\"0\",\"1\",\"3\""),
               new("CODES[en](\"Building type\")", "\"0\",\"1\",\"3\""),
               new("CODES(\"Tiedot\")", "\"ketjutettu_lv\",\"vmuutos_lv\",\"lkm_julk_uudet\""),
               new("CODES[sv](\"Uppgifter\")", "\"ketjutettu_lv\",\"vmuutos_lv\",\"lkm_julk_uudet\""),
               new("CODES[en](\"Information\")", "\"ketjutettu_lv\",\"vmuutos_lv\",\"lkm_julk_uudet\""),
               new("VARIABLE-TYPE(\"Vuosi\")", "\"Time\""),
               new("VARIABLE-TYPE[sv](\"År\")", "\"Time\""),
               new("VARIABLE-TYPE[en](\"Year\")", "\"Time\""),
               new("VARIABLE-TYPE(\"Alue\")", "\"Classificatory\""),
               new("VARIABLE-TYPE[sv](\"Område\")", "\"Classificatory\""),
               new("VARIABLE-TYPE[en](\"Region\")", "\"Classificatory\""),
               new("VARIABLE-TYPE(\"Talotyyppi\")", "\"Classificatory\""),
               new("VARIABLE-TYPE[sv](\"Hustyp\")", "\"Classificatory\""),
               new("VARIABLE-TYPE[en](\"Building type\")", "\"Classificatory\""),
               new("MAP(\"Alue\")", "\"Alue 2018\""),
               new("MAP[sv](\"Område\")", "\"Alue 2018\""),
               new("MAP[en](\"Region\")", "\"Alue 2018\""),
               new("ELIMINATION(\"Talotyyppi\")", "\"Talotyypit yhteensä\""),
               new("ELIMINATION[sv](\"Hustyp\")", "\"Hustyp totalt\""),
               new("ELIMINATION[en](\"Building type\")", "\"Building types total\""),
               new("PRECISION(\"Tiedot\",\"Indeksi (2015=100)\")", "1"),
               new("PRECISION[sv](\"Uppgifter\",\"Index (2015=100)\")", "1"),
               new("PRECISION[en](\"Information\",\"Index (2015=100)\")", "1"),
               new("PRECISION(\"Tiedot\",\"Muutos edelliseen vuoteen (indeksi 2015=100)\")", "1"),
               new("PRECISION[sv](\"Uppgifter\",\"Årsförändring (index 2015=100)\")", "1"),
               new("PRECISION[en](\"Information\",\"Annual change (index 2015=100)\")", "1"),
               new("LAST-UPDATED(\"Indeksi (2015=100)\")", "\"20230131 08:00\""),
               new("LAST-UPDATED[sv](\"Index (2015=100)\")", "\"20230131 08:00\""),
               new("LAST-UPDATED[en](\"Index (2015=100)\")", "\"20230131 08:00\""),
               new("LAST-UPDATED(\"Muutos edelliseen vuoteen (indeksi 2015=100)\")", "\"20230131 09:00\""),
               new("LAST-UPDATED[sv](\"Årsförändring (index 2015=100)\")", "\"20230131 09:00\""),
               new("LAST-UPDATED[en](\"Annual change (index 2015=100)\")", "\"20230131 09:00\""),
               new("LAST-UPDATED(\"Kauppojen lukumäärä\")", "\"20230131 10:00\""),
               new("LAST-UPDATED[sv](\"Antal\")", "\"20230131 10:00\""),
               new("LAST-UPDATED[en](\"Number\")", "\"20230131 10:00\""),
               new("UNITS(\"Indeksi (2015=100)\")", "\"indeksipisteluku\""),
               new("UNITS[sv](\"Index (2015=100)\")", "\"indextal\""),
               new("UNITS[en](\"Index (2015=100)\")", "\"index point\""),
               new("UNITS(\"Muutos edelliseen vuoteen (indeksi 2015=100)\")", "\"%\""),
               new("UNITS[sv](\"Årsförändring (index 2015=100)\")", "\"%\""),
               new("UNITS[en](\"Annual change (index 2015=100)\")", "\"%\""),
               new("UNITS(\"Kauppojen lukumäärä\")", "\"lukumäärä\""),
               new("UNITS[sv](\"Antal\")", "\"antal\""),
               new("UNITS[en](\"Number\")", "\"number\""),
               new("CONTACT(\"Indeksi (2015=100)\")", "\"test_contact1_fi\""),
               new("CONTACT[sv](\"Index (2015=100)\")", "\"test_contact1_sv\""),
               new("CONTACT[en](\"Index (2015=100)\")", "\"test_contact1_en\""),
               new("CONTACT(\"Muutos edelliseen vuoteen (indeksi 2015=100)\")", "\"test_contact2_fi\""),
               new("CONTACT[sv](\"Årsförändring (index 2015=100)\")", "\"test_contact2_sv\""),
               new("CONTACT[en](\"Annual change (index 2015=100)\")", "\"test_contact2_en\""),
               new("CONTACT(\"Kauppojen lukumäärä\")", "\"test_contact3_fi\""),
               new("CONTACT[sv](\"Antal\")", "\"test_contact3_sv\""),
               new("CONTACT[en](\"Number\")", "\"test_contact3_en\""),
               new("SOURCE", "\"test_source_fi\""),
               new("SOURCE[sv]", "\"test_source_sv\""),
               new("SOURCE[en]", "\"test_source_en\""),
               new("OFFICIAL-STATISTICS", "YES"),
               new("NOTE", "\"test_note_fi\""),
               new("NOTE[sv]", "\"test_note_sv\""),
               new("NOTE[en]", "\"test_note_en\""),
               new("NOTE(\"Talotyyppi\")", "\"test_note_talotyyppi\""),
               new("NOTE[sv](\"Hustyp\")", "\"test_note_hustyp\""),
               new("NOTE[en](\"Building type\")", "\"test_note_building_type\""),
               new("VALUENOTE(\"Tiedot\",\"Indeksi (2015=100)\")", "\"test_value_note_tiedot_indeksi\""),
               new("VALUENOTE[sv](\"Uppgifter\",\"Index (2015=100)\")", "\"test_value_note_uppgifter_index\""),
               new("VALUENOTE[en](\"Information\",\"Index (2015=100)\")", "\"test_value_note_information_index\""),
               new("VALUENOTE(\"Tiedot\",\"Muutos edelliseen vuoteen (indeksi 2015=100)\")", "\"test_value_note_tiedot_muutos\""),
               new("VALUENOTE[sv](\"Uppgifter\",\"Årsförändring (index 2015=100)\")", "\"test_value_note_uppgifter_årsförändring\""),
               new("VALUENOTE[en](\"Information\",\"Annual change (index 2015=100)\")", "\"test_value_note_information_change\""),
               new("VALUENOTE(\"Tiedot\",\"Kauppojen lukumäärä\")", "\"test_value_note_tiedot_kauppojen_lukumäärä\""),
               new("VALUENOTE[sv](\"Uppgifter\",\"Antal\")", "\"test_value_note_uppgifter_antal\""),
               new("VALUENOTE[en](\"Information\",\"Number\")", "\"test_value_note_information_number\""),
               new("TEXTPROPERTY", "\"foo\""),
               new("MULTILANGUAGETEXTPROPERTY", "\"foo\""),
               new("MULTILANGUAGETEXTPROPERTY[sv]", "\"foo(sv)\""),
               new("MULTILANGUAGETEXTPROPERTY[en]", "\"foo(en)\""),
               new("NUMBERPROPERTY", "1"),
               new("BOOLEANPROPERTY", "YES"),
               new("BOOLEANTEXTPROPERTY", "NO"),
               new("TEXTARRAYPROPERTY", "\"foo\",\"bar\",\"baz\""),
               new("MULTILANGUAGETEXTARRAYPROPERTY", "\"foo\",\"bar\",\"baz\""),
               new("MULTILANGUAGETEXTARRAYPROPERTY[sv]", "\"foo\",\"bar\",\"baz (sv)\""),
               new("MULTILANGUAGETEXTARRAYPROPERTY[en]", "\"foo\",\"bar\",\"baz (en)\""),
               new("SINGLEITEMTEXTARRAYPROPERTY", "\"foo\""),
               new("SINGLEITEMMULTILANGUAGETEXTARRAYPROPERTY", "\"foo\""),
               new("SINGLEITEMMULTILANGUAGETEXTARRAYPROPERTY[sv]", "\"foo(sv)\""),
               new("SINGLEITEMMULTILANGUAGETEXTARRAYPROPERTY[en]", "\"foo(en)\""),
           ];
    }
}
