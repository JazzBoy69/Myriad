using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Myriad.Pages
{
    public struct LayoutHTML
    {
        public const string startOfPage = @"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1, maximum-scale=1' />";
        public const string header =
@"<link rel='stylesheet' href='css/felicianadialog.01.20.css' />
    <link rel='stylesheet' href='css/myriad.03.11.css' />
    <link rel='shortcut icon' href='images/MyriadIcon.png'>
</head>
<body>
    <div class='container'>
        <header>
            <div class='menu'>
                <!--SEARCHBAR-->
                <div id='searchbar' 
                    <form id='searchForm' class='section' method='GET' action='Search'>
                        <div id='toolbarSearchField' >
                            <input id='searchField' name='q' type='search' autocomplete='off' autocapitalize='off' maxlength='256' />
                        </div>
                        <div id='toolbarSearchButton'><button id='search' type='submit'>Search</button></div>
                    </form>
                </div>
                <!--Navigation Controls-->
                <ul>
                    <li id='menuHome'><a href='/Index?name=home'><span class='icon'><img src='images/home.png' /></span></a></li>
                        <li id='menuPrevious'><a id='previousLink'><span class='icon'><img src='images/left.png' /></span></a></li>
                           <li id='menuUp'><a id='upLink'><span class='icon'><img src='images/up.png' /></span></a></li>
                              <li id='menuNext'><a id='nextLink'><span class='icon'><img src='images/right.png' /></span></a></li>
                                 <li id='menuTOC'>
                        <a onclick='showHideMenu()'><span class='icon'><img src='images/toc.png' /></span></a>
                            <ul id='toc' class='hidden'></ul>
                    </li>
                </ul>
                <a onclick='showHideIndex()'><span class='indexicon'><img src='images/downarrow.png' /></span></a>
                    <img src='images/icons8-ellipsis-30.png' class='ellipsis hidden' />
            </div>
            <div id='modal-overlay'><</div>
            <div id=level class='hidden'>0</div>
            <div id='bibleindex'>
                <section id=possibilities class='buttongrid'>
                    <a id=ge class='indexbutton group1 hidden'>Genesis</a>
                    <a id=ex class='indexbutton group1 hidden'>Exodus</a>
                    <a id=le class='indexbutton group1 hidden'>Leviticus</a>
                    <a id=nu class='indexbutton group1 hidden'>Numbers</a>
                    <a id=de class='indexbutton group1 hidden'>Deuteronomy</a>
                    <a id=jos class='indexbutton group1 hidden'>Joshua</a>
                    <a id=jg class='indexbutton group1 hidden'>Judges</a>
                    <a id=ru class='indexbutton group1 hidden'>Ruth</a>
                    <a id=1sa class='indexbutton group2 hidden'>1 Samuel</a>
                    <a id=2sa class='indexbutton group2 hidden'>2 Samuel</a>
                    <a id=1ki class='indexbutton group2 hidden'>1 Kings</a>
                    <a id=2ki class='indexbutton group2 hidden'>2 Kings</a>
                    <a id=1ch class='indexbutton group2 hidden'>1 Chronicles</a>
                    <a id=2ch class='indexbutton group2 hidden'>2 Chronicles</a>
                    <a id=ezr class='indexbutton group2 hidden'>Ezra</a>
                    <a id=ne class='indexbutton group2 hidden'>Nehemiah</a>
                    <a id=es class='indexbutton group2  hiddenhidden'>Esther</a>
                    <a id=job class='indexbutton group3'>Job</a>
                    <a id=ps class='indexbutton common group3'>Psalms</a>
                    <a id=pr class='indexbutton common group3'>Proverbs</a>
                    <a id=ec class='indexbutton group3 hidden'>Ecclesiastes</a>
                    <a id=ca class='indexbutton group3 hidden'>Song of Solomon</a>
                    <a id=isa class='indexbutton group4 hidden'>Isaiah</a>
                    <a id=jer class='indexbutton group4 hidden'>Jeremiah</a>
                    <a id=la class='indexbutton group4 hidden'>Lamentations</a>
                    <a id=eze class='indexbutton group4 hidden'>Ezekiel</a>
                    <a id=da class='indexbutton group4 hidden'>Daniel</a>
                    <a id=ho class='indexbutton group5 hidden'>Hosea</a>
                    <a id=joe class='indexbutton group5 hidden'>Joel</a>
                    <a id=am class='indexbutton group5 hidden'>Amos</a>
                    <a id=ob class='indexbutton short group5 hidden'>Obadiah</a>
                    <a id=jon class='indexbutton group5 hidden'>Jonah</a>
                    <a id=mic class='indexbutton group5 hidden'>Micah</a>
                    <a id=na class='indexbutton group5 hidden'>Nahum</a>
                    <a id=hab class='indexbutton group5 hidden'>Habakkuk</a>
                    <a id=zep class='indexbutton group5 hidden'>Zephaniah</a>
                    <a id=hag class='indexbutton group5 hidden'>Haggai</a>
                    <a id=zec class='indexbutton group5'>Zechariah</a>
                    <a id=mal class='indexbutton group5 hidden'>Malachi</a>
                    <a id=mt class='indexbutton common group6'>Matthew</a>
                    <a id=mr class='indexbutton common group6'>Mark</a>
                    <a id=lu class='indexbutton common group6'>Luke</a>
                    <a id=joh class='indexbutton common group6'>John</a>
                    <a id=ac class='indexbutton group6 hidden'>Acts</a>
                    <a id=ro class='indexbutton common group7'>Romans</a>
                    <a id=1co class='indexbutton common group7'>1 Corinthians</a>
                    <a id=2co class='indexbutton group7 hidden'>2 Corinthians</a>
                    <a id=ga class='indexbutton group7 hidden'>Galatians</a>
                    <a id=eph class='indexbutton group7 hidden'>Ephesians</a>
                    <a id=php class='indexbutton group7 hidden'>Philippians</a>
                    <a id=col class='indexbutton group7 hidden'>Colossians</a>
                    <a id=1th class='indexbutton group8 hidden'>1 Thessalonians</a>
                    <a id=2th class='indexbutton group8 hidden'>2 Thessalonians</a>
                    <a id=1ti class='indexbutton group8 hidden'>1 Timothy</a>
                    <a id=2ti class='indexbutton group8 hidden'>2 Timothy</a>
                    <a id=tit class='indexbutton group8 hidden'>Titus</a>
                    <a id=phm class='indexbutton short group8 hidden'>Philemon</a>
                    <a id=heb class='indexbutton common group8'>Hebrews</a>
                    <a id=jas class='indexbutton group9 hidden'>James</a>
                    <a id=1pe class='indexbutton group9 hidden'>1 Peter</a>
                    <a id=2pe class='indexbutton group9 hidden'>2 Peter</a>
                    <a id=1jo class='indexbutton group9 hidden'>1 John</a>
                    <a id=2jo class='indexbutton group9 short hidden'>2 John</a>
                    <a id=3jo class='indexbutton group9 short hidden'>3 John</a>
                    <a id=jude class='indexbutton group9 short hidden'>Jude</a>
                    <a id=re class='indexbutton group9 hidden'>Revelation</a>
                </section>
                <section id=keys class=buttongrid>
                    <a id=key1 class='indexbutton indexgroup'>Genesis-Ruth</a>
                    <a id=key2 class='indexbutton indexgroup'>1 Samuel-Esther</a>
                    <a id=key3 class='indexbutton indexgroup'>Job-Song of Solomon</a>
                    <a id=key4 class='indexbutton indexgroup'>Isaiah-Daniel</a>
                    <a id=key5 class='indexbutton indexgroup'>Hosea-Malachi</a>
                    <a id=key6 class='indexbutton indexgroup'>Matthew-Acts</a>
                    <a id=key7 class='indexbutton indexgroup'>Romans-Colossians</a>
                    <a id=key8 class='indexbutton indexgroup'>1Th-Hebrews</a>
                    <a id=key9 class='indexbutton indexgroup'>James-Revelation</a>
                    <a id=num1 class='indexbutton indexnumber hidden'>1</a>
                    <a id=num2 class='indexbutton indexnumber hidden'>2</a>
                    <a id=num3 class='indexbutton indexnumber hidden'>3</a>
                    <a id=num4 class='indexbutton indexnumber hidden'>4</a>
                    <a id=num5 class='indexbutton indexnumber hidden'>5</a>
                    <a id=num6 class='indexbutton indexnumber hidden'>6</a>
                    <a id=num7 class='indexbutton indexnumber hidden'>7</a>
                    <a id=num8 class='indexbutton indexnumber hidden'>8</a>
                    <a id=num9 class='indexbutton indexnumber hidden'>9</a>
                    <a id=num0 class='indexbutton indexnumber hidden'>0</a>
                    <a id=numColon class='indexbutton indexnumber hidden'>:</a>
                    <a id=back class='indexbutton'>Back</a>
                    <a id=go class='indexbutton hidden'>Go</a>
                </section>
            </div>
        </header>
        <div id='modal-image-box' class='hidden'>
            <span class='zoomclose'>&times;</span>
            <img class='modal-content zoom' id='modal-image'>
        </div>
<div id=top></div><article>
";
        public const string close = @"</article>
    </div>

    <script src='js/myriad27.01.20.js'></script>
    <script src='js/Hammer.min.js'></script>
    <script src='js/shortcuts.js'></script>";
        public const string endofBody = "</body></html>";
    }
}


/*
 *     <script src='js/jquery-2.1.3.min.js' defer></script>
 *     
    <script src='js/bootstrap.js' defer></script>
    <script src='js/bootbox.min.js' defer></script>
        <script src='js/jquery.hammer.js' defer></script>
*/
