using System.Collections.Generic;



namespace Myriad.Library
{
    public class Bible
    {
        internal static List<string> BookNames = new List<string>
        {
             "GE", "EX", "LE", "NU", "DE", "JOS", "JG", "RU",    //0-7
            "1SA", "2SA", "1KI", "2KI", "1CH", "2CH", "EZR",    //8-14
            "NE", "ES", "JOB", "PS", "PR", "EC", "CA", "ISA",   //15-22
            "JER", "LA", "EZE", "DA", "HO", "JOE", "AM", "OB",  //23-30
            "JON", "MIC", "NA", "HAB", "ZEP", "HAG", "ZEC",     //31-37
            "MAL", "MT", "MAT", "MR", "LU", "JOH", "AC", "RO", "1CO",  //38-45
            "2CO", "GA", "EPH", "PHP", "COL", "1TH", "2TH",     //46-52
            "1TI", "2TI", "TIT", "PHM", "HEB", "JAS", "1PE",    //53-59
            "2PE", "1JO", "2JO", "3JO", "JUDE", "RE",
            "GENESIS", "EXODUS", "LEVITICUS", "NUMBERS", "DEUTERONOMY",
            "JOSHUA", "JUDGES", "RUTH", "1 SAMUEL", "2 SAMUEL", "1 KINGS",
            "2 KINGS", "1 CHRONICLES", "2 CHRONICLES", "EZRA", "NEHEMIAH",
            "ESTHER", "JOB", "PSALMS", "PROVERBS", "ECCLESIASTES",
            "SONG OF SOLOMON", "ISAIAH", "JEREMIAH", "LAMENTATIONS",
            "EZEKIEL", "DANIEL", "HOSEA", "JOEL", "AMOS", "OBADIAH",
            "JONAH", "MICAH", "NAHUM", "HABAKKUK", "ZEPHANIAH", "HAGGAI",
            "ZECHARIAH", "MALACHI", "MATTHEW", "MARK", "LUKE", "JOHN",
            "ACTS", "ROMANS", "1 CORINTHIANS", "2 CORINTHIANS", "GALATIANS",
            "EPHESIANS", "PHILIPPIANS", "COLOSSIANS", "1 THESSALONIANS",
            "2 THESSALONIANS", "1 TIMOTHY", "2 TIMOTHY", "TITUS", "PHILEMON",
            "HEBREWS", "JAMES", "1 PETER", "2 PETER", "1 JOHN", "2 JOHN",
            "3 JOHN", "JUDE", "REVELATION"
        };
        internal static Dictionary<string, int> NamesTitleCaseIndex = new Dictionary<string, int>
        {
            {"Ge", 0 }, {"Ex", 1 }, {"Le", 2 }, {"Nu", 3 }, { "De",4 }, { "Jos", 5 }, { "Jg", 6 }, { "Ru",7 },    //0-7
            { "1Sa", 8 }, { "2Sa", 9}, {"1Ki", 10}, { "2Ki", 11}, {"1Ch", 12}, {"2Ch", 13}, {"Ezr", 14},    //8-14
            { "Ne", 15}, {"Es", 16},  { "Ps", 18}, {"Pr", 19}, { "Ec", 20}, {"Ca", 21}, {"Isa", 22 },  //15-22
            { "Jer", 23}, {"La",24}, { "Eze", 25}, {"Da", 26}, {"Ho", 27}, {"Joe", 28}, {"Am", 29}, {"Ob", 30 }, //23-30
            { "Jon", 31}, {"Mic", 32}, {"Na", 33}, {"Hab", 34}, {"Zep", 35}, {"Hag", 36}, {"Zec",37 },     //31-37
            { "Mal", 38}, {"Mt", 39}, {"Mr", 40}, {"Lu", 41}, {"Joh", 42}, {"Ac", 43}, {"Ro", 44}, {"1Co", 45 }, //38-45
            { "2Co", 46}, {"Ga", 47}, {"Eph", 48}, {"Php", 49}, {"Col", 50}, {"1Th", 51}, {"2Th", 52 },    //46-52
            { "1Ti", 53}, {"2Ti", 54}, {"Tit", 55}, {"Phm", 56}, {"Heb", 57}, {"Jas", 58}, {"1Pe", 59 },   //53-59
            { "2Pe", 60}, {"1Jo", 61}, {"2Jo", 62}, {"3Jo", 63}, {"Re", 65 },
            { "Genesis",0}, { "Exodus",1}, { "Leviticus",2}, { "Numbers",3}, { "Deuteronomy",4},
            { "Joshua", 5}, {"Judges",6}, { "Ruth", 7}, {"1 Samuel", 8}, {"First Samuel", 8 },
            { "2 Samuel", 9}, {"Second Samuel", 9 }, {"1 Kings",10}, {"First Kings", 10 }, {"Second Kings", 11 },
            { "2 Kings", 11}, {"First Chronicles", 12 }, {"1 Chronicles", 12}, {"Second Chronicles", 13 },
            { "2 Chronicles",13}, { "Ezra", 14}, {"Nehemiah",15},
            { "Esther", 16}, {"Job", 17 }, { "Psalms", 18}, {"Psalm", 18 }, {"Proverbs",19}, { "Ecclesiastes",20},
            { "Song of Solomon",21}, { "Isaiah", 22}, {"Jeremiah", 23}, {"Lamentations",24},
            { "Ezekiel", 25}, {"Daniel", 26}, {"Hosea", 27}, {"Joel", 28}, {"Amos", 29}, {"Obadiah",30},
            { "Jonah", 31}, {"Micah", 32}, {"Nahum", 33}, {"Habakkuk", 34}, {"Zephaniah", 35}, {"Haggai",36},
            { "Zechariah",37}, { "Malachi", 38}, {"Matthew", 39}, {"Mark", 40}, {"Luke", 41}, {"John",42},
            { "Acts", 43}, {"Romans",44}, { "1 Corinthians", 45}, {"First Corinthians", 45 },
            {"Second Corinthians", 46 }, { "2 Corinthians", 46}, {"Galatians",47},
            { "Ephesians", 48}, {"Philippians", 49}, {"Colossians",50}, {"First Thessalonians", 51 }, { "1 Thessalonians",51},
            { "2 Thessalonians", 52}, {"Second Thessalonians", 52 }, {"1 Timothy", 53}, {"First Timothy", 53 },
            { "2 Timothy", 54}, {"Second Timothy", 54 }, {"Titus", 55}, {"Philemon",56},
            { "Hebrews", 57}, {"James", 58}, {"First Peter", 59 }, {"1 Peter",59}, {"Second Peter", 60 },
            { "2 Peter", 60}, {"1 John", 61}, {"First John", 61 }, {"2 John",62}, {"Second John", 62 },
            { "3 John", 63}, {"Third John", 63 }, {"Jude", 64}, {"Revelation", 65},
            { "Gen.", 0 }, {"Ex.", 1 }, {"Lev.", 2 }, {"Num.", 3 }, {"Deut.", 4 }, {"Josh.",5}, { "Judg.", 6},         //0-7
            { "1 Sam.", 8}, {"2 Sam.", 9}, {"1 Ki.", 10}, {"2 Ki.", 11}, { "1 Chron.", 12}, {"2 Chron.", 13},     //8-14
            { "Neh.", 15}, { "Ps.", 18}, {"Prov.", 19}, {"Eccl.", 20}, {"Song of Sol.",21}, { "Isa.", 22}, //15-22
            { "Jer.", 23}, {"Lam.", 24}, {"Ezek.", 25}, {"Dan.", 26}, {"Hos.",27}, {"Obad.", 30},           //23-30
            {"Mic.", 32}, {"Nah.", 33}, {"Hab.", 34}, {"Zeph.", 35}, {"Hag.", 36}, {"Zech.", 37},                  //31-37
            { "Mal.", 38}, {"Matt.", 39}, { "Rom.", 44}, {"1 Cor.", 45},           //38-45
            { "2 Cor.", 46}, {"Gal.", 47}, {"Eph.", 48}, {"Phil.", 49}, {"Col.",50}, { "1 Thess.",51}, { "2 Thess.",52},           //46-52
            { "1 Tim.", 53 }, {"2 Tim.", 54}, { "Philem.",56}, { "Heb.", 57}, {"Jas.", 58}, {"1 Pet.", 59},           //53-59
            { "2 Pet.",60}, { "Rev.", 65}
    };
        internal static Dictionary<string, int> NamesIndex = new Dictionary<string, int>
        {
            {"GE", 0 }, {"EX", 1 }, {"LE", 2 }, {"NU", 3 }, { "DE",4 }, { "JOS", 5 }, { "JG", 6 }, { "RU",7 },    //0-7
            { "1SA", 8 }, { "2SA", 9}, {"1KI", 10}, { "2KI", 11}, {"1CH", 12}, {"2CH", 13}, {"EZR", 14},    //8-14
            { "NE", 15}, {"ES", 16},  { "PS", 18}, {"PR", 19}, { "EC", 20}, {"CA", 21}, {"ISA", 22 },  //15-22
            { "JER", 23}, {"LA",24}, { "EZE", 25}, {"DA", 26}, {"HO", 27}, {"JOE", 28}, {"AM", 29}, {"OB", 30 }, //23-30
            { "JON", 31}, {"MIC", 32}, {"NA", 33}, {"HAB", 34}, {"ZEP", 35}, {"HAG", 36}, {"ZEC",37 },     //31-37
            { "MAL", 38}, {"MT", 39}, {"MAT", 39 }, {"MR", 40}, {"LU", 41}, {"JOH", 42}, {"AC", 43}, {"RO", 44}, {"1CO", 45 }, //38-45
            { "2CO", 46}, {"GA", 47}, {"EPH", 48}, {"PHP", 49}, {"COL", 50}, {"1TH", 51}, {"2TH", 52 },    //46-52
            { "1TI", 53}, {"2TI", 54}, {"TIT", 55}, {"PHM", 56}, {"HEB", 57}, {"JAS", 58}, {"1PE", 59 },   //53-59
            { "2PE", 60}, {"1JO", 61}, {"2JO", 62}, {"3JO", 63}, {"RE", 65 },
            { "GENESIS",0}, { "EXODUS",1}, { "LEVITICUS",2}, { "NUMBERS",3}, { "DEUTERONOMY",4},
            { "JOSHUA", 5}, {"JUDGES",6}, { "RUTH", 7}, {"1 SAMUEL", 8}, {"FIRST SAMUEL", 8 },
            { "2 SAMUEL", 9}, {"SECOND SAMUEL", 9 }, {"1 KINGS",10}, {"FIRST KINGS", 10 }, {"SECOND KINGS", 11 },
            { "2 KINGS", 11}, {"FIRST CHRONICLES", 12 }, {" 1 CHRONICLES", 12}, {"SECOND CHRONICLES", 13 },
            { "2 CHRONICLES",13}, { "EZRA", 14}, {"NEHEMIAH",15},
            { "ESTHER", 16}, {"JOB", 17 }, { "PSALMS", 18}, {"PSALM", 18 }, {"PROVERBS",19}, { "ECCLESIASTES",20},
            { "SONG OF SOLOMON",21}, { "ISAIAH", 22}, {"JEREMIAH", 23}, {"LAMENTATIONS",24},
            { "EZEKIEL", 25}, {"DANIEL", 26}, {"HOSEA", 27}, {"JOEL", 28}, {"AMOS", 29}, {"OBADIAH",30},
            { "JONAH", 31}, {"MICAH", 32}, {"NAHUM", 33}, {"HABAKKUK", 34}, {"ZEPHANIAH", 35}, {"HAGGAI",36},
            { "ZECHARIAH",37}, { "MALACHI", 38}, {"MATTHEW", 39}, {"MARK", 40}, {"LUKE", 41}, {"JOHN",42},
            { "ACTS", 43}, {"ROMANS",44}, { "1 CORINTHIANS", 45}, {"FIRST CORINTHIANS", 45}, {"2 CORINTHIANS", 46}, {"SECOND CORINTHIANS", 46}, {"GALATIANS",47},
            { "EPHESIANS", 48}, {"PHILIPPIANS", 49}, {"COLOSSIANS",50}, {"FIRST THESSALONIANS", 51 }, { "1 THESSALONIANS",51},
            { "2 THESSALONIANS", 52}, {"SECOND THESSALONIANS", 52 }, {"1 TIMOTHY", 53}, {"FIRST TIMOTHY", 53 },
            { "2 TIMOTHY", 54}, {"SECOND TIMOTHY", 54 }, {"TITUS", 55}, {"PHILEMON",56},
            { "HEBREWS", 57}, {"JAMES", 58}, {"FIRST PETER", 59 }, {"1 PETER",59}, {"SECOND PETER", 60 },
            { "2 PETER", 60}, {"1 JOHN", 61}, {"FIRST JOHN", 61 }, {"2 JOHN",62}, {"SECOND JOHN", 62 },
            { "3 JOHN", 63}, {"THIRD JOHN", 63 }, {"JUDE", 64}, {"REVELATION", 65},
            { "GEN.", 0 }, {"EX.", 1 }, {"LEV.", 2 }, {"NUM.", 3 }, {"DEUT.", 4 }, {"JOSH.",5}, { "JUDG.", 6},         //0-7
            { "1 SAM.", 8}, {"2 SAM.", 9}, {"1 KI.", 10}, {"2 KI.", 11}, { "1 CHRON.", 12}, {"2 CHRON.", 13},     //8-14
            { "NEH.", 15}, { "PS.", 18}, {"PROV.", 19}, {"ECCL.", 20}, {"SONG OF SOL.",21}, { "ISA.", 22}, //15-22
            { "JER.", 23}, {"LAM.", 24}, {"EZEK.", 25}, {"DAN.", 26}, {"HOS.",27}, {"OBAD.", 30},           //23-30
            {"MIC.", 32}, {"NAH.", 33}, {"HAB.", 34}, {"ZEPH.", 35}, {"HAG.", 36}, {"ZECH.", 37},                  //31-37
            { "MAL.", 38}, {"MATT.", 39}, { "ROM.", 44}, {"1 COR.", 45},           //38-45
            { "2 COR.", 46}, {"GAL.", 47}, {"EPH.", 48}, {"PHIL.", 49}, {"COL.",50}, { "1 THESS.",51}, { "2 THESS.",52},           //46-52
            { "1 TIM.", 53 }, {"2 TIM.", 54}, { "PHILEM.",56}, { "HEB.", 57}, {"JAS.", 58}, {"1 PET.", 59},           //53-59
            { "2 PET.",60}, { "REV.", 65}    };
        internal static Dictionary<string, string> QueryBibleNames = new Dictionary<string, string>
        {
            {"GE", "GE" }, {"EX", "EX" }, {"LE", "LE" }, {"NU", "NU" }, { "DE", "DE" }, { "JOS", "JOS" }, { "JG", "JG" },
            { "RU","RU" },    //0-7
            { "1SA", "1SA" }, { "2SA", "2SA"}, {"1KI", "1KI"}, { "2KI", "2KI"}, {"1CH", "1CH"}, {"2CH", "2CH"}, {"EZR", "EZR"},    //8-14
            { "NE", "NE"}, {"ES", "ES"},  { "PS", "PS"}, {"PR", "PR"}, { "EC", "EC"}, {"CA", "CA"}, {"ISA", "ISA" },  //15-22
            { "JER", "JER"}, {"LA","LA"}, { "EZE", "EZE"}, {"DA", "DA"}, {"DAN", "DA" }, {"HO", "HO"}, {"HOS", "HO" },
            { "JOE", "JOE"}, {"AM", "AM"}, {"OB", "OB" }, //23-30
            { "JON", "JON"}, {"MIC", "MIC"}, {"NA", "NA"}, {"HAB", "HAB"}, {"ZEP", "ZEP"}, {"HAG", "HAG"}, {"ZEC","ZEC" },     //31-37
            { "MAL", "MAL"}, {"MT", "MT"}, {"MR", "MR"}, {"LU", "LU"}, {"JOH", "JOH"}, {"AC", "AC"},
            { "RO", "RO"}, {"1CO", "1CO" }, //38-45
            { "2CO", "2CO"}, {"GA", "GA"}, {"GAL", "GA" }, {"GL", "GA" }, {"EPH", "EPH"}, {"PHP", "PHP"}, {"COL", "COL"}, {"1TH", "1TH"},
            { "2TH", "2TH" },    //46-52
            { "1TI", "1TI"}, {"2TI", "2TI"}, {"TIT", "TIT"}, {"PHM", "PHM"}, {"PLM", "PHM" }, {"HEB", "HEB"}, {"JAS", "JAS"},
            { "1PE", "1PE" },   //53-59
            { "2PE", "2PE"}, {"1JO", "1JO"}, {"2JO", "2JO"}, {"3JO", "3JO"}, {"RE", "RE" },
            { "GENESIS", "GE"}, { "EXODUS", "EX"}, { "LEVITICUS", "LE"}, { "NUMBERS", "NU"}, { "DEUTERONOMY", "DE"},
            { "JOSHUA", "JOS"}, {"JUDGES","JG"}, { "RUTH", "RU"}, {"1 SAMUEL", "1SA"}, {"FIRST SAMUEL", "1SA"},
            { "2 SAMUEL", "2SA"}, {"SECOND SAMUEL", "2SA"}, {"1 KINGS", "1KI"}, {"FIRST KINGS", "1KI" }, {"SECOND KINGS", "2KI" },
            { "2 KINGS", "2KI"}, {"FIRST CHRONICLES", "2CH" }, {" 1 CHRONICLES", "1CH"}, {"SECOND CHRONICLES", "2CH" },
            { "2 CHRONICLES","2CH"}, { "EZRA", "EZR"}, {"NEHEMIAH","NE"},
            { "ESTHER", "ES"}, {"JOB", "JOB" }, { "PSALMS", "PS"}, {"PSALM", "PS" }, {"PROVERBS","PR"}, { "ECCLESIASTES","EC"},
            { "SONG OF SOLOMON","CA"}, { "ISAIAH", "ISA"}, {"JEREMIAH", "JER"}, {"LAMENTATIONS","LA"},
            { "EZEKIEL", "EZE"}, {"DANIEL", "DA"}, {"HOSEA", "HO"}, {"JOEL", "JOE"}, {"AMOS", "AM"}, {"OBADIAH","OB"},
            { "JONAH", "JON"}, {"MICAH", "MIC"}, {"NAHUM", "NA"}, {"HABAKKUK", "HAB"}, {"ZEPHANIAH", "ZEP"}, {"HAGGAI","HAG"},
            { "ZECHARIAH","ZEC"}, { "MALACHI", "MAL"}, {"MATTHEW", "MT"}, {"MARK", "MR"}, {"LUKE", "LU"}, {"JOHN","JOH"},
            { "ACTS", "AC"}, {"ROMANS","RO"}, { "1 CORINTHIANS", "1CO"}, {"FIRST CORINTHIANS", "1CO"},
            { "2 CORINTHIANS", "2CO"}, {"SECOND CORINTHIANS", "2CO"}, {"GALATIANS","GA"},
            { "EPHESIANS", "EPH"}, {"PHILIPPIANS", "PHP"}, {"COLOSSIANS","COL"}, {"FIRST THESSALONIANS", "1TH" },
            { "1 THESSALONIANS","1TH"},
            { "2 THESSALONIANS", "2TH"}, {"SECOND THESSALONIANS", "2TH" }, {"1 TIMOTHY", "1TI"}, {"FIRST TIMOTHY", "1TI" },
            { "2 TIMOTHY", "2TI"}, {"SECOND TIMOTHY", "2TI" }, {"TITUS", "TIT"}, {"PHILEMON","PHM"},
            { "HEBREWS", "HEB"}, {"JAMES", "JAS"}, {"FIRST PETER", "1PE" }, {"1 PETER","1PE"}, {"SECOND PETER", "2PE" },
            { "2 PETER", "2PE"}, {"1 JOHN", "1JO"}, {"FIRST JOHN", "1JO" }, {"2 JOHN","2JO"}, {"SECOND JOHN", "2JO" },
            { "3 JOHN", "3JO"}, {"THIRD JOHN", "3JO" }, {"JUDE", "JUDE"}, {"REVELATION", "RE"}, {"REV", "RE"},
            { "GEN.", "GE" }, {"EX.", "EX" }, {"LEV.", "LE" }, {"NUM.", "NU" }, {"DEUT.", "DE" }, {"JOSH.", "JOS"},
            { "JUDG.", "JG"},         //0-7
            { "1 SAM.", "1SA"}, {"2 SAM.", "2SA"}, {"1 KI.", "1KI"}, {"2 KI.", "2KI"}, { "1 CHRON.", "1CH"}, {"2 CHRON.", "2CH"},     //8-14
            { "NEH.", "NE"}, { "PS.", "PS"}, {"PROV.", "PR"}, {"ECCL.", "EC"}, {"SONG OF SOL.","CA"}, { "ISA.", "ISA"}, //15-22
            { "JER.", "JER"}, {"LAM.", "LA"}, {"EZEK.", "EZE"}, {"DAN.", "DA"}, {"HOS.","HO"}, {"OBAD.", "OB"},           //23-30
            {"MIC.", "MIC"}, {"NAH.", "NA"}, {"HAB.", "HAB"}, {"ZEPH.", "ZEP"}, {"HAG.", "HAG"}, {"ZECH.", "ZEC"},                  //31-37
            { "MAL.", "MAL"}, {"MATT.", "MT"}, { "ROM.", "RO"}, {"1 COR.", "1CO"},           //38-45
            { "2 COR.", "2CO"}, {"GAL.", "GA"}, {"EPH.", "EPH"}, {"PHIL.", "PHP"}, {"COL.","COL"}, { "1 THESS.","1TH"},
            { "2 THESS.","2TH"},           //46-52
            { "1 TIM.", "1TI" }, {"2 TIM.", "2TI"}, { "PHILEM.","PLM"}, { "HEB.", "HEB"}, {"JAS.", "JAS"}, {"1 PET.", "1PE"},           //53-59
            { "2 PET.","2PE"}, { "REV.", "RE"}
        };
        internal static List<string> Abbreviations = new List<string> {
            "GE", "EX", "LE", "NU", "DE", "JOS", "JG", "RU",    //0-7
            "1SA", "2SA", "1KI", "2KI", "1CH", "2CH", "EZR",    //8-14
            "NE", "ES", "JOB", "PS", "PR", "EC", "CA", "ISA",   //15-22
            "JER", "LA", "EZE", "DA", "HO", "JOE", "AM", "OB",  //23-30
            "JON", "MIC", "NA", "HAB", "ZEP", "HAG", "ZEC",     //31-37
            "MAL", "MT", "MR", "LU", "JOH", "AC", "RO", "1CO",  //38-45
            "2CO", "GA", "EPH", "PHP", "COL", "1TH", "2TH",     //46-52
            "1TI", "2TI", "TIT", "PHM", "HEB", "JAS", "1PE",    //53-59
            "2PE", "1JO", "2JO", "3JO", "JUDE", "RE"};
        internal static List<string> AbbreviationsTitleCase = new List<string> {
            "Ge", "Ex", "Le", "Nu", "De", "Jos", "Jg", "Ru",    //0-7
            "1Sa", "2Sa", "1Ki", "2Ki", "1Ch", "2Ch", "Ezr",    //8-14
            "Ne", "Es", "Job", "Ps", "Pr", "Ec", "Ca", "Isa",   //15-22
            "Jer", "La", "Eze", "Da", "Ho", "Joe", "Am", "Ob",  //23-30
            "Jon", "Mic", "Na", "Hab", "Zep", "Hag", "Zec",     //31-37
            "Mal", "Mt", "Mr", "Lu", "Joh", "Ac", "Ro", "1Co",  //38-45
            "2Co", "Ga", "Eph", "Php", "Col", "1Th", "2Th",     //46-52
            "1Ti", "2Ti", "Tit", "Phm", "Heb", "Jas", "1Pe",    //53-59
            "2Pe", "1Jo", "2Jo", "3Jo", "Jude", "Re"};
        internal static List<string> Leaders = new List<string>()
        {
            "1", "2", "3", "Song", "Song of", "First", "Second", "Third"
        };
        internal static List<string> WordsToSkip = new List<string>()
        {
            "See", "see", "Read", "compare", "Compare", "also", "at"
        };
        internal static List<string> NamesAllCaps = new List<string> {
            "GENESIS", "EXODUS", "LEVITICUS", "NUMBERS", "DEUTERONOMY",
            "JOSHUA", "JUDGES", "RUTH", "1 SAMUEL", "2 SAMUEL", "1 KINGS",
            "2 KINGS", "1 CHRONICLES", "2 CHRONICLES", "EZRA", "NEHEMIAH",
            "ESTHER", "JOB", "PSALMS", "PROVERBS", "ECCLESIASTES",
            "SONG OF SOLOMON", "ISAIAH", "JEREMIAH", "LAMENTATIONS",
            "EZEKIEL", "DANIEL", "HOSEA", "JOEL", "AMOS", "OBADIAH",
            "JONAH", "MICAH", "NAHUM", "HABAKKUK", "ZEPHANIAH", "HAGGAI",
            "ZECHARIAH", "MALACHI", "MATTHEW", "MARK", "LUKE", "JOHN",
            "ACTS", "ROMANS", "1 CORINTHIANS", "2 CORINTHIANS", "GALATIANS",
            "EPHESIANS", "PHILIPPIANS", "COLOSSIANS", "1 THESSALONIANS",
            "2 THESSALONIANS", "1 TIMOTHY", "2 TIMOTHY", "TITUS", "PHILEMON",
            "HEBREWS", "JAMES", "1 PETER", "2 PETER", "1 JOHN", "2 JOHN",
            "3 JOHN", "JUDE", "REVELATION"
            };
        internal static List<string> NamesTitleCase = new List<string> {
            "Genesis", "Exodus", "Leviticus", "Numbers", "Deuteronomy",
            "Joshua", "Judges", "Ruth", "1 Samuel", "2 Samuel", "1 Kings",
            "2 Kings", "1 Chronicles", "2 Chronicles", "Ezra", "Nehemiah",
            "Esther", "Job", "Psalms", "Proverbs", "Ecclesiastes",
            "Song of Solomon", "Isaiah", "Jeremiah", "Lamentations",
            "Ezekiel", "Daniel", "Hosea", "Joel", "Amos", "Obadiah",
            "Jonah", "Micah", "Nahum", "Habakkuk", "Zephaniah", "Haggai",
            "Zechariah", "Malachi", "Matthew", "Mark", "Luke", "John",
            "Acts", "Romans", "1 Corinthians", "2 Corinthians", "Galatians",
            "Ephesians", "Philippians", "Colossians", "1 Thessalonians",
            "2 Thessalonians", "1 Timothy", "2 Timothy", "Titus", "Philemon",
            "Hebrews", "James", "1 Peter", "2 Peter", "1 John", "2 John",
            "3 John", "Jude", "Revelation"
            };
        internal static List<string> LongAbbreviations = new List<string> {
            "GEN.", "EX.", "LEV.", "NUM.", "DEUT.", "JOSH.", "JUDG.", "RUTH",           //0-7
            "1 SAM.", "2 SAM.", "1 KI.", "2 KI.", "1 CHRON.", "2 CHRON.", "EZRA",       //8-14
            "NEH.", "ESTHER", "JOB", "PS.", "PROV.", "ECCL.", "SONG OF SOL.", "ISA.", //15-22
            "JER.", "LAM.", "EZEK.", "DAN.", "HOS.", "JOEL", "AMOS", "OBAD.",           //23-30
            "JONAH", "MIC.", "NAH.", "HAB.", "ZEPH.", "HAG.", "ZECH.",                  //31-37
            "MAL.", "MATT.", "MARK", "LUKE", "JOHN", "ACTS", "ROM.", "1 COR.",          //38-45
            "2 COR.", "GAL.", "EPH.", "PHIL.", "COL.", "1 THESS.", "2 THESS.",          //46-52
            "1 TIM.", "2 TIM.", "TITUS", "PHILEM.", "HEB.", "JAS.", "1 PET.",           //53-59
            "2 PET.", "1 JOHN", "2 JOHN", "3 JOHN", "JUDE", "REV."};                    //60-65 



        internal static readonly List<byte[]> Chapters = new List<byte[]>() {


new byte[] {                //Genesis
                31, 31, 25, 24, 26, 32, 22, 24, 22, 29,
                32, 32, 20, 18, 24, 21, 16, 27, 33, 38,
                18, 34, 24, 20, 67, 34, 35, 46, 22, 35,
                43, 55, 32, 20, 31, 29, 43, 36, 30, 23,
                23, 57, 38, 34, 34, 28, 34, 31, 22, 33,
                26
            }
        ,
new byte[]             {   //Exodus             
                22, 22, 25, 22, 31, 23, 30, 25, 32, 35,
                29, 10, 51, 22, 31, 27, 36, 16, 27, 25,
                26, 36, 31, 33, 18, 40, 37, 21, 43, 46,
                38, 18, 35, 23, 35, 35, 38, 29, 31, 43,
                38
            },
            new byte[] {       //Leviticus         
                17, 17, 16, 17, 35, 19, 30, 38, 36, 24,
                20, 47,  8, 59, 57, 33, 34, 16, 30, 37,
                27, 24, 33, 44, 23, 55, 46, 34
            },
            new byte[] {         //Numbers       
                54, 54, 34, 51, 49, 31, 27, 89, 26, 23,
                36, 35, 16, 33, 45, 41, 50, 13, 32, 22,
                29, 35, 41, 30, 25, 18, 65, 23, 31, 40,
                16, 54, 42, 56, 29, 34, 13
            },
            new byte[] {         //Deuteronomy       
                46, 46, 37, 29, 49, 33, 25, 26, 20, 29,
                22, 32, 32, 18, 29, 23, 22, 20, 22, 21,
                20, 23, 30, 25, 22, 19, 19, 26, 68, 29,
                20, 30, 52, 29, 12
            },
            new byte[] {           //Joshua     
                18, 18, 24, 17, 24, 15, 27, 26, 35, 27,
                43, 23, 24, 33, 15, 63, 10, 18, 28, 51,
                 9, 45, 34, 16, 33
            },
            new byte[] {             //Judges   
                36, 36, 23, 31, 24, 31, 40, 25, 35, 57,
                18, 40, 15, 25, 20, 20, 31, 13, 31, 30,
                48, 25
            },
            new byte[] {               //Ruth 
                22, 22, 23, 18, 22
            },
            new byte[] {              //1 Samuel  
                28, 28, 36, 21, 22, 12, 21, 17, 22, 27,
                27, 15, 25, 23, 52, 35, 23, 58, 30, 24,
                42, 15, 23, 29, 22, 44, 25, 12, 25, 11,
                31, 13
            },
            new byte[] {              //2 Samuel  
                27, 27, 32, 39, 12, 25, 23, 29, 18, 13,
                19, 27, 31, 39, 33, 37, 23, 29, 33, 43,
                26, 22, 51, 39, 25
            },
            new byte[] {             //1 Kings   
                53, 53, 46, 28, 34, 18, 38, 51, 66, 28,
                29, 43, 33, 34, 31, 34, 34, 24, 46, 21,
                43, 29, 53
            },
            new byte[] {            //2 Kings     
                18, 18, 25, 27, 44, 27, 33, 20, 29, 37,
                36, 21, 21, 25, 29, 38, 20, 41, 37, 37,
                21, 26, 20, 37, 20, 30
            },
            new byte[] {           //1 Chronicles     
                54,
                54, 55, 24, 43, 26, 81, 40, 40, 44,
                14, 47, 40, 14, 17, 29, 43, 27, 17, 19,
                 8, 30, 19, 32, 31, 31, 32, 34, 21, 30
            },
            new byte[] {            //2 Chronicles    
                17, 17, 18, 17, 22, 14, 42, 22, 18, 31,
                19, 23, 16, 22, 15, 19, 14, 19, 34, 11,
                37, 20, 12, 21, 27, 28, 23,  9, 27, 36,
                27, 21, 33, 25, 33, 27, 23
            },
            new byte[] {           //Ezra     
                11, 11, 70, 13, 24, 17, 22, 28, 36, 15,
                44
            },
            new byte[] {           //Nehemiah     
                11, 11, 20, 32, 23, 19, 19, 73, 18, 38,
                39, 36, 47, 31
            },
            new byte[] {          //Esther      
                22, 22, 23, 15, 17, 14, 14, 10, 17, 32,
                 3
            },
            new byte[] {         //Job       
                22, 22, 13, 26, 21, 27, 30, 21, 22, 35,
                22, 20, 25, 28, 22, 35, 22, 16, 21, 29,
                29, 34, 30, 17, 25,  6, 14, 23, 28, 25,
                31, 40, 22, 33, 37, 16, 33, 24, 41, 30,
                24, 34, 17
            },
            new byte[] {       //Psalms         
                 6,  6, 12,  8,  8, 12, 10, 17,  9, 20,
                18,  7,  8,  6,  7,  5, 11, 15, 50, 14,
                 9, 13, 31,  6, 10, 22, 12, 14,  9, 11,
                12, 24, 11, 22, 22, 28, 12, 40, 22, 13,
                17, 13, 11,  5, 26, 17, 11,  9, 14, 20,
                23, 19,  9,  6,  7, 23, 13, 11, 11, 17,
                12,  8, 12, 11, 10, 13, 20,  7, 35, 36,
                 5, 24, 20, 28, 23, 10, 12, 20, 72, 13,
                19, 16,  8, 18, 12, 13, 17,  7, 18, 52,
                17, 16, 15,  5, 23, 11, 13, 12,  9,  9,
                 5,  8, 28, 22, 35, 45, 48, 43, 13, 31,
                 7, 10, 10,  9,  8, 18, 19,  2, 29, 176,
                 7,  8,  9,  4,  8,  5,  6,  5,  6,  8,
                 8,  3, 18,  3,  3, 21, 26,  9,  8, 24,
                13, 10,  7, 12, 15, 21, 10, 20, 14,  9,
                 6
            },
            new byte[] {          //Proverbs      
                33, 33, 22, 35, 27, 23, 35, 27, 36, 18,
                32, 31, 28, 25, 35, 33, 33, 28, 24, 29,
                30, 31, 29, 35, 34, 28, 28, 27, 28, 27,
                33, 31
            },
            new byte[] {           //Ecclesiastes    
                18, 18, 26, 22, 16, 20, 12, 29, 17, 18,
                20, 10, 14
            },
            new byte[] {             //Song of Solomon   
                17, 17, 17, 11, 16, 16, 13, 13, 14
            },
            new byte[] {        //Isaiah        
                31, 31, 22, 26,  6, 30, 13, 25, 22, 21,
                34, 16,  6, 22, 32,  9, 14, 14,  7, 25,
                 6, 17, 25, 18, 23, 12, 21, 13, 29, 24,
                33,  9, 20, 24, 17, 10, 22, 38, 22,  8,
                31, 29, 25, 28, 28, 25, 13, 15, 22, 26,
                11, 23, 15, 12, 17, 13, 12, 21, 14, 21,
                22, 11, 12, 19, 12, 25, 24
            },
            new byte[] {         //Jeremiah       
                19, 19, 37, 25, 31, 31, 30, 34, 22, 26,
                25, 23, 17, 27, 22, 21, 21, 27, 23, 15,
                18, 14, 30, 40, 10, 38, 24, 22, 17, 32,
                24, 40, 44, 26, 22, 19, 32, 21, 28, 18,
                16, 18, 22, 13, 30,  5, 28,  7, 47, 39,
                46, 64, 34
            },
            new byte[] { //Lamentations
                22, 22, 22, 66, 22, 22
            },
            new byte[] {  //Ezekiel
                28, 28, 10, 27, 17, 17, 14, 27, 18, 11,
                22, 25, 28, 23, 23,  8, 63, 24, 32, 14,
                49, 32, 31, 49, 27, 17, 21, 36, 26, 21,
                26, 18, 32, 33, 31, 15, 38, 28, 23, 29,
                49, 26, 20, 27, 31, 25, 24, 23, 35
            },
            new byte[] { //Daniel
                21, 21, 49, 30, 37, 31, 28, 28, 27, 27,
                21, 45, 13
            },
            new byte[] { //Hosea
                11, 11, 23, 5, 19, 15, 11, 16, 14, 17,
                15, 12, 14, 16, 9
            },
            new byte[] { //Joel
                20, 20, 32, 21
            },
            new byte[] { //Amos
                15, 15, 16, 15, 13, 27, 14, 17, 14, 15
            },
            new byte[] { //Obadiah
                21, 21
            },
            new byte[] { //Jonah
                17, 17, 10, 10, 11
            },
            new byte[] { //Micah
                16, 16, 13, 12, 13, 15, 16, 20
            },
            new byte[] {  //Nahum
                15, 15, 13, 19
            },
            new byte[] {  //Habakkuk
                17, 17, 20, 19
            },
            new byte[] {  //Zephaniah
                18, 18, 15, 20
            },
            new byte[] {  //Haggai
                15, 15, 23
            },
            new byte[] {  //Zechariah
                21, 21, 13, 10, 14, 11, 15, 14, 23, 17,
                12, 17, 14, 9, 21
            },
            new byte[] {  //Malachi
                14, 14, 17, 18, 6
            },
            new byte[] {  //Matthew
                25, 25, 23, 17, 25, 48, 34, 29, 34, 38,
                42, 30, 50, 58, 36, 39, 28, 27, 35, 30,
                34, 46, 46, 39, 51, 46, 75, 66, 20
            },
            new byte[] {  //Mark
                45, 45, 28, 35, 41, 43, 56, 37, 38, 50,
                52, 33, 44, 37, 72, 47, 8
            },
            new byte[] {  //Luke
                80, 80, 52, 38, 44, 39, 49, 50, 56, 62,
                42, 54, 59, 35, 35, 32, 31, 37, 43, 48,
                47, 38, 71, 56, 53
            },
            new byte[] {  //John
                51, 51, 25, 36, 54, 47, 71, 52, 59, 41,
                42, 57, 50, 38, 31, 27, 33, 26, 40, 42,
                31, 25
            },
            new byte[] {  //Acts
                26, 26, 47, 26, 37, 42, 15, 60, 40, 43,
                48, 30, 25, 52, 28, 41, 40, 34, 28, 41,
                38, 40, 30, 35, 27, 27, 32, 44, 31
            },
            new byte[] {  //Romans
                32, 32, 29, 31, 25, 21, 23, 25, 39, 33,
                21, 36, 21, 14, 23, 33, 27
            },
            new byte[] {  //1 Corinthians
                31, 31, 16, 23, 21, 13, 20, 40, 13, 27,
                33, 34, 31, 13, 40, 58, 24
            },
            new byte[] {  //2 Corinthians
                24, 24, 17, 18, 18, 21, 18, 16, 24, 15,
                18, 33, 21, 14
            },
            new byte[] {  //Galatians
                24, 24, 21, 29, 31, 26, 18
            },
            new byte[] {  //Ephesians
                23, 23, 22, 21, 32, 33, 24
            },
            new byte[] {  //Philippians
                30, 30, 30, 21, 23
            },
            new byte[] {  //Colossians
                29, 29, 23, 25, 18
            },
            new byte[] {  //1 Thessalonians
                10, 10, 20, 13, 18, 28
            },
            new byte[] {  //2 Thessalonians
                12, 12, 17, 18
            },
            new byte[] {  //1 Timothy
                20, 20, 15, 16, 16, 25, 21
            },
            new byte[] {  //2 Timothy
                18, 18, 26, 17, 22
            },
            new byte[] {  //Titus
                16, 16, 15, 15
            },
            new byte[] {  //Philemon
                25, 25
            },
            new byte[] {  //Hebrews
                14, 14, 18, 19, 16, 14, 20, 28, 13, 28,
                39, 40, 29, 25
            },
            new byte[] {  //James
                27, 27, 26, 18, 17, 20
            },
            new byte[] { //1 Peter
                25, 25, 25, 22, 19, 14
            },
            new byte[] {  //2 Peter
                21, 21, 22, 18
            },
            new byte[] {  //1 John
                10, 10, 29, 24, 21, 21
            },
            new byte[] {  //2 John
                13, 13
            },
            new byte[] {  //3 John
                14, 14
            },
            new byte[] {  //Jude 
                25, 25
            },
            new byte[] {  // Revelation
                20, 20, 29, 22, 11, 14, 17, 17, 13, 21,
                11, 19, 17, 18, 20,  8, 21, 18, 24, 21,
                15, 27, 21
            }};
        public const byte bookCount = 66;

    }
}
