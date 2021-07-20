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
    <meta name='viewport' content='user-scalable=no,width=device-width, initial-scale=1, maximum-scale=1' />";
        public const string header =
@"<link rel='stylesheet' href='css/myriad.21.07.22.css' />
    <link rel='shortcut icon' href='images/MyriadIcon.png'>

</head>
<body>
    <div class='container'>
        <header>
            <div class='menu'>
                <!--SEARCHBAR-->
                <div id='searchbar'> 
                    <form id='searchForm' class='section' onsubmit='event.preventDefault(); HandleSearch();'>
                        <div id='toolbarSearchField' >
                            <input id='searchField' name='q' type='search' autocomplete='off' autocapitalize='off' maxlength='256' />
                        </div>
                        <div id='toolbarSearchButton'><img src='images/search.png' id='search' onclick=HandleSearch()></img></div>
                    </form>
                </div>
                <!--Navigation Controls-->
                <div id='menuSpacer' class='hidden'></div>
                <ul>
                    <li id='menuHome' class='hidden'><a href='/Index?name=home'><span class='icon'><img src='images/home.png' /></span></a></li>
                    <li id='menuPrevious' class='hidden' onclick=GoToPreceding()><span id='previousLink'><span class='icon'><img src='images/left.png' /></span></a></li>
                    <li id='menuUp' class='hidden' onclick=GoUp()><a id='upLink'><span class='icon'><img src='images/up.png' /></span></a></li>
                    <li id='menuNext' class='hidden' onclick=GoToNext()><a id='nextLink'><span class='icon'><img src='images/right.png' /></span></a></li>
                    <li id='menuTOC'>
                        <a onclick='showHideMenu()'><span class='icon'><img src='images/toc.png' /></span></a>
                    </li>
                    <li id='menuEdit'><a id='editButton' onclick=Edit()><span class='icon'><img src='images/icons8-edit-48.png' /></span></a></li>
                    <li id='menuOriginalWord' class='hidden'><a id='originalWordButton' onclick=EditOriginalWords()><span class='icon'><img src='images/Aleph.png' /></span></a></li>
                    <li id='menuChrono' class='hidden'><a id='chronoButton' onclick=ShowChrono()><span class='icon'><img src='images/calendar.png' /></span></a></li>
                    <li id='menuCancel' class='hidden'><a id='cancelEdit' onclick=HandleCancel()><span class='icon'><img src='images/icons8-unavailable.png' /></span></a></li>
                    <li id='menuAccept' class='hidden'><a id='acceptButton'><span class='icon'><img src='images/icons8-checkmark-52.png' /></span></a></li>
                </ul>
                <a onclick='showHideIndex()'><span class='indexicon'><img src='images/downarrow.png' /></span></a>
                <img src='images/icons8-ellipsis-30.png' id='ellipsis' class='hidden' />
            </div>
            <div id=level class='hidden'>0</div>
            <div id='bibleindex'>
                <section id='possibilities' class='buttongrid'>
                    <a id=ge class='indexbutton group1 hidden'>Ge</a>
                    <a id=ex class='indexbutton group1 hidden'>Ex</a>
                    <a id=le class='indexbutton group1 hidden'>Le</a>
                    <a id=nu class='indexbutton group1 hidden'>Nu</a>
                    <a id=de class='indexbutton group1 hidden'>De</a>
                    <a id=jos class='indexbutton group1 hidden'>Jos</a>
                    <a id=jg class='indexbutton group1 hidden'>Jg</a>
                    <a id=ru class='indexbutton group1 hidden'>Ru</a>
                    <a id=1sa class='indexbutton group2 hidden'>1Sa</a>
                    <a id=2sa class='indexbutton group2 hidden'>2Sa</a>
                    <a id=1ki class='indexbutton group2 hidden'>1Ki</a>
                    <a id=2ki class='indexbutton group2 hidden'>2Ki</a>
                    <a id=1ch class='indexbutton group2 hidden'>1Ch</a>
                    <a id=2ch class='indexbutton group2 hidden'>2Ch</a>
                    <a id=ezr class='indexbutton group2 hidden'>Ezr</a>
                    <a id=ne class='indexbutton group2 hidden'>Ne</a>
                    <a id=es class='indexbutton group2  hiddenhidden'>Es</a>
                    <a id=job class='indexbutton group3'>Job</a>
                    <a id=ps class='indexbutton common group3'>Ps</a>
                    <a id=pr class='indexbutton common group3'>Pr</a>
                    <a id=ec class='indexbutton group3 hidden'>Ec</a>
                    <a id=ca class='indexbutton group3 hidden'>Ca</a>
                    <a id=isa class='indexbutton group4 hidden'>Isa</a>
                    <a id=jer class='indexbutton group4 hidden'>Jer</a>
                    <a id=la class='indexbutton group4 hidden'>La</a>
                    <a id=eze class='indexbutton group4 hidden'>Eze</a>
                    <a id=da class='indexbutton group4 hidden'>Da</a>
                    <a id=ho class='indexbutton group5 hidden'>Ho</a>
                    <a id=joe class='indexbutton group5 hidden'>Joel</a>
                    <a id=am class='indexbutton group5 hidden'>Am</a>
                    <a id=ob class='indexbutton short group5 hidden'>Ob</a>
                    <a id=jon class='indexbutton group5 hidden'>Jon</a>
                    <a id=mic class='indexbutton group5 hidden'>Mic</a>
                    <a id=na class='indexbutton group5 hidden'>Na</a>
                    <a id=hab class='indexbutton group5 hidden'>Hab</a>
                    <a id=zep class='indexbutton group5 hidden'>Zep</a>
                    <a id=hag class='indexbutton group5 hidden'>Hag</a>
                    <a id=zec class='indexbutton group5'>Zec</a>
                    <a id=mal class='indexbutton group5 hidden'>Mal</a>
                    <a id=mt class='indexbutton common group6'>Mt</a>
                    <a id=mr class='indexbutton common group6'>Mr</a>
                    <a id=lu class='indexbutton common group6'>Lu</a>
                    <a id=joh class='indexbutton common group6'>Joh</a>
                    <a id=ac class='indexbutton group6 hidden'>Ac</a>
                    <a id=ro class='indexbutton common group7'>Ro</a>
                    <a id=1co class='indexbutton common group7'>1Co</a>
                    <a id=2co class='indexbutton group7 hidden'>2Co</a>
                    <a id=ga class='indexbutton group7 hidden'>Ga</a>
                    <a id=eph class='indexbutton group7 hidden'>Eph</a>
                    <a id=php class='indexbutton group7 hidden'>Php</a>
                    <a id=col class='indexbutton group7 hidden'>Col</a>
                    <a id=1th class='indexbutton group8 hidden'>1Th</a>
                    <a id=2th class='indexbutton group8 hidden'>2Th</a>
                    <a id=1ti class='indexbutton group8 hidden'>1Ti</a>
                    <a id=2ti class='indexbutton group8 hidden'>2Ti</a>
                    <a id=tit class='indexbutton group8 hidden'>Tit</a>
                    <a id=phm class='indexbutton short group8 hidden'>Phm</a>
                    <a id=heb class='indexbutton common group8'>Heb</a>
                    <a id=jas class='indexbutton group9 hidden'>Jas</a>
                    <a id=1pe class='indexbutton group9 hidden'>1Pe</a>
                    <a id=2pe class='indexbutton group9 hidden'>2Pe</a>
                    <a id=1jo class='indexbutton group9 hidden'>1Jo</a>
                    <a id=2jo class='indexbutton group9 short hidden'>2Jo</a>
                    <a id=3jo class='indexbutton group9 short hidden'>3Jo</a>
                    <a id=jude class='indexbutton group9 short hidden'>Jude</a>
                    <a id=re class='indexbutton group9 hidden'>Re</a>
                </section>
                <section id=keys class=buttongrid>
                    <a id=key1 class='indexbutton indexgroup'>Ge-Ruth</a>
                    <a id=key2 class='indexbutton indexgroup'>1Sa-Es</a>
                    <a id=key3 class='indexbutton indexgroup'>Poetic</a>
                    <a id=key4 class='indexbutton indexgroup'>Isa-Da</a>
                    <a id=key5 class='indexbutton indexgroup'>Hos-Mal</a>
                    <a id=key6 class='indexbutton indexgroup'>Mt-Acts</a>
                    <a id=key7 class='indexbutton indexgroup'>Ro-Col</a>
                    <a id=key8 class='indexbutton indexgroup'>1Th-Heb</a>
                    <a id=key9 class='indexbutton indexgroup'>Jas-Re</a>
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
                    <a id=numBang class='indexbutton indexnumber hidden'>!</a>
                    <a id=numDash class='indexbutton indexnumber hidden'>-</a>
                    <a id=back class='indexbutton'>Back</a>
                    <a id=go class='indexbutton hidden'>Go</a>
                </section>
            </div>
        </header>
        <div id=sidebar class='hidden'>
        </div>
        <div id='modal-image-box' class='hidden'>
            <img class='zoom' id='modal-image'>
        </div>
<div id=top></div>
<div id=editFormContainer class='flexTabs hidden fixedposition fillHeight'>
    <div id='editTabsHeader' class=flexTabsHeader>
        <ul class='tabs'>
            <li class='active'>Header 1</li>
            <li>Header 2</li>
        </ul>
    </div>
    <div id='editTabs' class=flexTabsTabs>
        <ul id='editTabList' class=tab>
            <li id=edittab-0 class='active editcontent'>
                <div id=editForm contenteditable=true data-pos=0 data-id=0 data-index=0 data-edittype=0></div>
            </li>
        </ul>
    </div>
</div>
<article>
<div id='mainPane'>
";
        public const string close = @"</div></article></div>";
        public const string tocdiv = "<div id=tocdiv class=hidden></div>";
        public const string modalOverlay = "<div id='modal-overlay'></div>";
        public const string myriadJavaScript = "<script src='js/myriad.21.07.19" +
            ".js'></script>";
        public const string endofBody = "</body></html>";
    }
}
