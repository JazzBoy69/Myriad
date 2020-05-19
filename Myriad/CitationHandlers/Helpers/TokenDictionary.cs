using System.Collections.Generic;
using Feliciana.Library;

namespace Myriad.CitationHandlers.Helpers
{
    public static class TokenDictionary
    {
        public const int SetFirstBook = 0;
        public const int SetFirstChapter = 1;
        public const int SetFirstVerse = 2;
        public const int SetSecondChapter = 5;
        public const int SetFirstChapterAndAddResults = 0x11;
        public const int SetFirstVerseAndAddResults = 0x12;
        public const int SetFirstWordAndAddResults = 0x13;
        public const int SetSecondChapterAndAddResults = 0x15;
        public const int SetSecondVerseAndAddResults = 0x16;
        public const int DeferWordIndex = 0x18;
        public const int AddBrokenCommaMarker = 0x19;


        static readonly Dictionary<char, byte> tokenTable = new Dictionary<char, byte>()
        {
            { '–', 2 },
            { '-', 2 },
            { ',', 3 },
            { ';', 4 },
            { ':', 5 },
            { '!', 6 },
            { '.', 7 },
            { ' ', 8 },
            { '~', 9 }
        };
        static readonly Dictionary<int, int> table = new Dictionary<int, int>()
        {//Key: Alpha or number (A or 1) || nextToLastToken (0 if doesn't matter) || last token || token
            //1 to add citation to results; 0 continue || index to set
            { 0xA048, SetFirstBook },                  // Mt 24:14 => ; Mt 
            { 0xA574, DeferWordIndex },                // Mt 24:14.preached => :xx.preached
            { 0xA448, SetFirstBook },                  // Mt 24:14 => ; ; Mt              
            { 0xA488, SetFirstBook },                  // Mt 24:14 => ; ; Mt                    
            { 0x1025, SetSecondChapter },              //2Co 6:14-7:1 => -7:  
            { 0x1042, SetFirstVerse },                 //Mt 24:14, 16-18 => xx, 16-xx
            { 0x1048, SetFirstBook },                  //Mt 24:14; Mr 13:10 => ; Mr_
            { 0x1045, SetFirstChapter },               //Mt 24:14; 28:19, 20 => :xx; 28:
            { 0x1052, SetFirstVerse },                 //Mt 24:45-47 => :45-            
            { 0x1056, SetFirstVerseAndAddResults },    //Mr 2:1! => :1! 
            { 0x1084, SetFirstChapterAndAddResults },  //Mt 24; =>  _24;
            { 0x1085, SetFirstChapter },               //Mt 24:14 =>  _24:
            { 0x1232, SetFirstVerse },                 //Ge 30:13, 17-20, 22-24; => -xx, 22-
            { 0x1233, SetFirstVerse },                 //Mt 2:4-6, 14, 15 => -xx, 14,
            { 0x1234, SetFirstVerseAndAddResults },    //Mt 25:31-33, 40; => -xx, 40;
            { 0x1253, SetSecondVerseAndAddResults },   //Ge 29:32–30:13, 17-20 => xx-xx:13,
            { 0x1254, SetSecondVerseAndAddResults },   //Joh 18:29–19:16  => xx-xx:16
            { 0x1323, SetSecondVerseAndAddResults },   //Ge 30:13, 17-20, 22-24; => xx, xx-20,
            { 0x1324, SetSecondVerseAndAddResults },   //Mt 24:14, 16-18 =>  , xx-18;
            { 0x1332, SetFirstVerse },                 //Mt 2:4-6, 14, 15, 19-23; xx, xx, 19-
            { 0x1333, SetSecondVerseAndAddResults },   //Mt 2:4-6, 14, 15, 19-23; xx, xx, 15,
            { 0x1334, SetSecondVerseAndAddResults },   //Mt 24:14, 16, 18 => , xx, 18;
            { 0x1393, SetFirstVerse },                 //Ge 36:2, 14, 18, 20, 24, 25 => , xx, 20,
            { 0x1394, SetFirstVerseAndAddResults },    //Ge 36:2, 4, 10, 12 => , xx, 12
            { 0x1424, SetSecondVerseAndAddResults },   //Mt 24:14, 16, 18 => , xx, 18;
            { 0x1432, AddBrokenCommaMarker },          //Mt 3:1, 6, 13-17; => xx, xx, 13-
            { 0x1434, SetSecondVerseAndAddResults },   //Joh 8:26, 28, 38; => , xx, 38;
            { 0x1452, SetFirstVerse },                 //Mt 6:33; 24:45-47 =>; xx:45-
            { 0x1453, SetFirstVerse },                 //Mt 24:14; 28:19, 20 => ; xx:19,
            { 0x1454, SetFirstVerseAndAddResults },    //Mt 24:14; 28:20; => ; xx:20;
            { 0x1456, SetFirstVerseAndAddResults },    //Mr 2:1! => ; xx:1!
            { 0x1482, SetFirstChapter },
            { 0x1483, SetFirstChapter },               //Ge 6, 7 => ;xx_6,
            { 0x1484, SetFirstChapterAndAddResults },  //Mt 24; => ;xx_24;
            { 0x1485, SetFirstChapter },               //Mt 24:14 => ;xx _24:
            { 0x1523, SetSecondVerseAndAddResults },   //Re 16:14-16, 18; => :xx-16,
            { 0x1524, SetSecondVerseAndAddResults },   //Mt 24:45-47 => :xx-47;
            { 0x1525, SetSecondChapter },              //2Co 6:14-7:1 => :xx-7:
            { 0x1533, SetSecondVerseAndAddResults },   //Re 16:14, 16, 18; => :xx, 16,
            { 0x1534, SetSecondVerseAndAddResults },   //Re 16:14, 16; => :xx, 16;
            { 0x1532, SetSecondVerseAndAddResults },   //Re 16:14, 16-18 => :xx, 16-
            { 0x1543, SetFirstVerse },                 //Mt 3:1, 6, 13-17; => :xx, 6,
            { 0x1544, SetFirstVerseAndAddResults },    //Mt 2:12, 22 => :xx, 22
            { 0x1574, SetFirstWordAndAddResults },     //Mt 24:14.8 => :xx.8
            { 0x1592, SetFirstVerse },                 //Mr 6:1, 4-6; => :xx, 4- 
            { 0x1593, SetFirstVerse },                 //Joh 8:26, 28, 38; => :xx, 28,
            { 0x1594, SetFirstVerseAndAddResults },    //Jas 1:13, 17 => :xx, 17
            { 0x1832, SetSecondVerseAndAddResults },   //2Jo 10, 12-14 => _xx, 12-
            { 0x1824, SetSecondChapterAndAddResults },
            { 0x1833, SetSecondVerseAndAddResults },   //2Jo 10, 12, 14 => _xx, 12,
            { 0x1834, SetSecondChapterAndAddResults }, //Ge 6, 7; => _xx, 7;
            { 0x1844, SetSecondVerseAndAddResults },   //2Jo 10, 12
            { 0x1852, SetFirstVerse },                 //Mt 24:45-47 => _xx:45- 
            { 0x1853, SetFirstVerse },                 //Mt 28:19, 20 => _xx:19,
            { 0x1854, SetFirstVerseAndAddResults },    //Mt 24:14; => _xx:14;
            { 0x1856, SetFirstVerseAndAddResults },    //Mr 2:1! => _xx:1!
            { 0x1857, SetFirstVerse },                 // Mt 24:14.preached => xx:14.
            { 0x1892, SetFirstVerse },                 //3Jo 1, 5-8 => _xx, 5-
            { 0x1923, SetSecondVerseAndAddResults },   //Ge 36:2, 5-8, 14 => , xx-8,
            { 0x1924, SetSecondVerseAndAddResults },    //Mt 3:1, 6, 13-17; => , xx-17;
            { 0x1932, AddBrokenCommaMarker },          //Mt 3:1, 6, 13-17; => xx, xx, 13-
            { 0x1933, SetSecondVerseAndAddResults },                 //Mt 5:3, 10, 19, 20 => xx, 19,
            { 0x1934, SetSecondVerseAndAddResults },   //Joh 8:26, 28, 38; => , xx, 38;
            { 0x1993, SetFirstVerse },
            { 0x1994, SetFirstVerseAndAddResults }   //Joh 8:26, 28, 38; => , xx, 38;
        };

        public static int Lookup(char nextToLastToken, char lastToken, char token, int count)
        {
            int key = (count == Result.notfound) ?
                40960 + 
                256 * tokenTable[nextToLastToken]  + 
                16 * tokenTable[lastToken] + 
                tokenTable[token] 
                :
                4096 + 
                256 * tokenTable[nextToLastToken] + 
                16 * tokenTable[lastToken] + 
                tokenTable[token];
            int wildcardKey = (count == Result.notfound) ?
                40960 +
                16 * tokenTable[lastToken] + 
                tokenTable[token] 
                :
                4096 +
                16 * tokenTable[lastToken] + 
                tokenTable[token];
            if (!table.ContainsKey(key))
            {
                return (table.ContainsKey(wildcardKey)) ?
                table[wildcardKey] : Result.notfound;
            }
            return table[key];
        }
    }
}
