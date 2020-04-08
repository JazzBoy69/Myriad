function SetupPartialPageLoad() {
    window.onpopstate = function (event) {
        LoadHistoryPage(document.location.href);
    }
}

function HandleLink(event) {
    history.replaceState(null, null, event.target.href);
    event.preventDefault(); 
    LoadPage(event.target.href);
}

function LoadPage(path) {
    if (performance.navigation.type === 1) {
        window.location = path;
        return;
    }
    if (path.indexOf('partial') === -1) path = AddQueryToPath(path, 'partial=true');
    LoadMainPane(path);
}

function HandleTOCClick(event) {
    event.preventDefault();
    showHideMenu();
    LoadIndexPane(event.target.href);
}

function LoadIndexPane(path) {
    postAjax(path, {},
        function (data) {
            var mainPane = document.getElementById('mainPane');
            mainPane.innerHTML = data;
            history.pushState(null, null, path);
            SetTitle();
        });
}

function LoadTOC() {
    path = AddQueryToPath(CurrentPath() + document.location.search, 'toc=1');
    postAjax(path, {},
        function (data) {
            var toc = document.getElementById('tocdiv');
            toc.innerHTML = data;
        });
}

function LoadHistoryPage(path) {
    if (performance.navigation.type === 1) {
        window.location = path;
        return;
    }
    var target = AddQueryToPath(path, 'partial=true');
    LoadMainPaneHistory(target);
}

function LoadMainPaneHistory(path) {
    postAjax(path, {},
        function (data) {
            var mainPane = document.getElementById('mainPane');
            mainPane.innerHTML = data;
            SetTitle();
        });
}

function LoadMainPane(path) {
    postAjax(path, {},
        function (data) {
            history.replaceState(null, null, AddQueryToPath(CurrentPath(), GetRangeQuery()));
            var mainPane = document.getElementById('mainPane');
            mainPane.innerHTML = data;
            history.pushState(null, null, AddQueryToPath(CurrentPath(), GetRangeQuery()));
            SetTitle();
        });
}

function AddQueryToPath(path, query) {
    if (HasQuery(path)) {
        return path+'&' + query;
    }
    return path + '?' + query;
}

function CurrentPath() {
    if ((document.location.pathname === '/') || (document.location.pathname === '/Index')) {
        return '/Index';
    }
    return document.location.pathname;
}

function HasQuery(path) {
    return path.indexOf('?') !== -1;
}

function SetupCopytoClipboardWithLabel() {
    document.addEventListener('copy', function (e) {
        var plaintext = document.getSelection().toString();
        var range = window.getSelection().getRangeAt(0);
        content = range.cloneContents();
        par = document.createElement('p');
        ref = document.createElement('a');
        ref.href = document.location;
        ref.innerHTML = document.title;
        space = document.createElement('span');
        space.innerHTML = ': ';
        par.appendChild(ref);
        par.appendChild(space);
        par.appendChild(content);
        var htmlContent = par.innerHTML;
        e.clipboardData.setData('text/html', htmlContent);
        e.clipboardData.setData('text/plain', plaintext);
        e.preventDefault();
    });
}

function CreateTableOfContents(section) {
    var toc = document.getElementsByClassName('#toc');
    toc.append('<li><a id="link" href="#top">Top of page</a></li>');
    var headings = document.getElementById(section).querySelectorAll("h3");
    for (var i = 0; i < headings.length; i++) {
        headings[i].id = "title" + i;

        toc.append("<li><a id='link" + i + "' href='#title" +
            i + "'>" +
            headings[i].innerHTML + "</a></li>");
        headings[i].onclick = TOCScroll;
    }
}

function TOCScroll(event) {
    const distanceToTop = el => Math.floor(el.getBoundingClientRect().top);
    e.preventDefault();
    var targetID = (respond) ? respond.getAttribute('href') : this.getAttribute('href');
    const targetAnchor = document.querySelector(targetID);
    if (!targetAnchor) return;
    const originalTop = distanceToTop(targetAnchor);
    window.scrollBy({ top: originalTop, left: 0, behavior: 'smooth' });
    const checkIfDone = setInterval(function () {
        const atBottom = window.innerHeight + window.pageYOffset >= document.body.offsetHeight - 2;
        if (distanceToTop(targetAnchor) === 0 || atBottom) {
            targetAnchor.tabIndex = '-1';
            targetAnchor.focus();
            window.history.pushState('', '', targetID);
            clearInterval(checkIfDone);
        }
    }, 100);
}

function GetLinksInID(id)
{
    return document.getElementById(id).childNodes;
}

function AddClassToGroup(g, c) {
    for (var i = 0; i < g.length; i++) {
        g[i].classList.add(c);
    }
}

function RemoveClassFromGroup(g, c) {
    for (var i = 0; i < g.length; i++) {
        g[i].classList.remove(c);
    }
}


function SetupIndex() {
    var possibilityLinks = GetLinksInID('possibilities');
    var go = document.getElementById('go');
    var keys = document.getElementById('keys');
    var indexgroup = keys.getElementsByClassName('indexgroup');
    var indexnumber = keys.getElementsByClassName('indexnumber');

    for (var i = 0; i < possibilityLinks.length; i++) {
        possibilityLinks[i].onclick = HandlePossibilityClick;
    }
    go.onclick = function () {
        document.getElementById('search').click();
    };
    for (i = 0; i < indexgroup.length; i++) {
        indexgroup[i].onclick = HandleIndexGroupClick;
    }
    for (i = 0; i < indexnumber.length; i++) {
        indexnumber[i].onclick = HandleIndexNumberClick;
    }
    document.getElementById('back').onclick = HandleBackClick;
}

function HandlePossibilityClick(event) {
    if ((event.target === null) || (event.target.textContent === null)) return;
    var searchField = document.getElementById('searchField');
    var indexgroup = keys.getElementsByClassName('indexgroup');
    var indexnumber = keys.getElementsByClassName('indexnumber');
    var go = document.getElementById('go');

    searchField.value = event.target.textContent + ' ';
    AddClassToGroup(indexgroup, 'hidden');
    RemoveClassFromGroup(indexnumber, 'hidden');
    document.getElementById('level').textContent = "1";
    if (!go.classList.contains('hidden')) {
        go.classList.add('hidden');
    }
}

function HandleIndexGroupClick(event) {
    if (event === null) return;
    var possibilityLinks = GetLinksInID('possibilities');
    var id = event.target.id;
    if (id === null) return;
    alert(id);
    var group = "group" + id.substr(id.length - 1);
    for (i = 0; i < possibilityLinks.length; i++) {
        if (possibilityLinks[i].classList.contains(group))
            possibilityLinks[i].classList.remove('hidden');
        else
            possibilityLinks[i].classList.add('hidden');
    }
}

function HandleIndexNumberClick(event) {
    var searchField = document.getElementById('searchField');
    var text = searchField.value + event.target.textContent;
    searchField.value = text;
    var levelElement = document.getElementById('level');
    var level = parseInt(levelElement.textContent);
    level++;
    levelElement.textContent = level.toString();
    document.getElementById('go').classList.remove('hidden');
}

function HandleBackClick(event) {
    var searchField = document.getElementById('searchField');
    var possibilityLinks = GetLinksInID('possibilities');
    var indexgroup = keys.getElementsByClassName('indexgroup');
    var indexnumber = keys.getElementsByClassName('indexnumber');
    var level = parseInt(document.getElementById('level').textContent);
    if (level === 0) {
        for (i = 0; i < possibilityLinks.length; i++) {
            if (possibilityLinks[i].classList.contains('common'))
                possibilityLinks[i].classList.remove('hidden');
            else
                possibilityLinks[i].classList.add('hidden');
        }
        return;
    }
    if (level === 1) {
        searchField.value = "";
        RemoveClassFromGroup(indexgroup, 'hidden');
        AddClassToGroup(indexnumber, 'hidden');
        document.getElementById('level').textContent = "0";
        document.getElementById('go').classList.add('hidden');
        return;
    }
    var reference = searchField.value;
    reference = reference.substr(0, reference.length - 1);
    searchField.value = reference;
    level--;
    document.getElementById('level').textContent = level.toString();
}


function showHideIndex() {
    var overlay = document.getElementById('bibleindex');
    var article = document.getElementById('mainPane');
    var ellipsis = document.getElementById('ellipsis');
    if (overlay.classList.contains('show')) {
        overlay.classList.remove('show');
        article.classList.remove('blur');
        if ((ellipsis !== null) && (ellipsis.classList !== null)) {
            var suppressedParagraphs = document.getElementsByClassName('suppressed');
            for (var i = 0; i < suppressedParagraphs.length; i++) {
                if (suppressedParagraphs[i].length < 1) return;
                ellipsis.removeClass('hidden');
            }
        }
    }
    else {
        overlay.classList.add('show');
        article.classList.add('blur');
        if ((ellipsis !== null) && (ellipsis.classList !== null)) {
                ellipsis.classList.add('hidden');
        }
    }
}

function showHideMenu() {
    var toc = document.getElementById('tocdiv');
    var overlay = document.getElementById('modal-overlay');
    var article = document.getElementById('mainPane');
    if (toc.classList.contains('hidden')) {
        LoadTOC();
        toc.classList.remove('hidden');
        toc.classList.add('visible');
        overlay.classList.add('show');
        article.classList.add('blur');
    }
    else {
        article.classList.remove('blur');
        toc.classList.remove('visible');
        overlay.classList.remove('show');
        toc.classList.add('hidden');
        var tocListShowing = document.getElementById('toc');
        tocListShowing.classList.add('hidden');
        tocListShowing.classList.remove('visible');
    }
    return true;
}

function ScrollToHeading() {
    var locationPath = filterPath(location.pathname);
    var scrollElem = scrollableElement('html', 'body');

    $('a[href*=#]').each(function () {
        var thisPath = filterPath(this.pathname) || locationPath;
        if (locationPath === thisPath
            && (location.hostname === this.hostname || !this.hostname)
            && this.hash.replace(/#/, '')) {
            var $target = $(this.hash), target = this.hash;
            if (target) {
                $(this).click(function (event) {
                    showHideMenu();
                    var $t = $(this.hash), target = this.hash;
                    var targetOffset = $t.offset().top;
                    event.preventDefault();
                    var h = $('header').height() + 20;
                    $(scrollElem).animate({ scrollTop: targetOffset - h }, 600); 
                });
            }
        }
    });
}

function ScrollToMarker() {
    var locationPath = filterPath(location.pathname);
    var scrollElem = scrollableElement('html', 'body');

    $('a[href*=#]').each(function () {
        var thisPath = filterPath(this.pathname) || locationPath;
        if (locationPath === thisPath
            && (location.hostname === this.hostname || !this.hostname)
            && this.hash.replace(/#/, '')) {
            var $target = $(this.hash), target = this.hash;
            if (target) {
                $(this).click(function (event) {
                    showHideMenu();
                    var $t = $(this.hash), target = this.hash;
                    var targetOffset = $t.next().offset().top;
                    event.preventDefault();
                    var h = $('header').height() + 20;
                    $(scrollElem).animate({ scrollTop: targetOffset - h }, 600); 
                });
            }
        }
    });
}


function ScrollToTop(event) {
        event.preventDefault();
        var scrollElem = scrollableElement('html', 'body');
        showHideMenu();
        $(scrollElem).animate({ scrollTop: 0 }, 600);
}

function scrollableElement(els) {
    for (var i = 0, argLength = arguments.length; i < argLength; i++) {
        var el = arguments[i],
            $scrollElement = $(el);
        if ($scrollElement.scrollTop() > 0) {
            return el;
        } else {
            $scrollElement.scrollTop(1);
            var isScrollable = $scrollElement.scrollTop() > 0;
            $scrollElement.scrollTop(0);
            if (isScrollable) {
                return el;
            }
        }
    }
    return [];
}

function EditParagraph(editlink) {
    var edittype = editlink.getAttribute('data-edittype');
    var ID = editlink.getAttribute('data-id');
    var index = editlink.getAttribute('data-index');
    var editSpan = editlink.parentElement.getElementsByClassName('parcontent')[0];
    editSpan.classList.add('updating');
    var editForm = document.getElementById('editForm');
    editForm.setAttribute('data-edittype', edittype);
    editForm.setAttribute('data-id', ID);
    editForm.setAttribute('data-index', index);
    var menuAccept = document.getElementById('menuAccept');
    menuAccept.onclick = AcceptEditParagraph;
    postAjax('/EditParagraph/GetData',
        {
        editType: edittype,
        ID: ID,
        paragraphIndex: index
    },
        function (data) { ShowEditWindow(data); }
    );
}

function ShowEditWindow(data) {
    var editForm = document.getElementById('editForm');
    var mainPane = document.getElementById('mainPane');
    var menuCancel = document.getElementById('menuCancel');
    var menuEdit = document.getElementById('menuEdit');
    var menuAccept = document.getElementById('menuAccept');
    editForm.innerText = data;
    var scrollPos = document.documentElement.scrollTop;
    editForm.setAttribute('data-pos', scrollPos);
    mainPane.classList.add('hidden');
    menuEdit.classList.add('hidden');
    menuCancel.classList.remove('hidden');
    menuAccept.classList.remove('hidden');
    editForm.classList.remove('hidden');
}

function CloseEditForm() {
    var editForm = document.getElementById('editForm');
    var mainPane = document.getElementById('mainPane'); 
    editForm.classList.add('hidden');
    menuCancel.classList.add('hidden');
    mainPane.classList.remove('hidden');
    menuEdit.classList.remove('hidden');
    menuAccept.classList.add('hidden');
    var scrollPos = editForm.getAttribute('data-pos');
    document.documentElement.scrollTop = scrollPos;
}

function AcceptEditParagraph() {
    var editForm = document.getElementById('editForm');
    var edittype = editForm.getAttribute('data-edittype');
    var ID = editForm.getAttribute('data-id');
    var index = editForm.getAttribute('data-index');
    postAjax('/EditParagraph/SetData',
        {
            editType: edittype,
            ID: ID,
            paragraphIndex: index,
            text: editForm.innerText
        },
        function (data) { RefreshEditedParagraph(data); }
    );
    CloseEditForm();
}

function RefreshEditedParagraph(data) {
    var editSpan = document.getElementsByClassName('updating')[0];
    editSpan.classList.remove('updating');
    editSpan.innerHTML = data;
}

function postAjax(url, data, success) {
    var params = typeof data === 'string' ? data : Object.keys(data).map(
        function (k) { return encodeURIComponent(k) + '=' + encodeURIComponent(data[k]) }
    ).join('&');

    var xhr = window.XMLHttpRequest ? new XMLHttpRequest() : new ActiveXObject("Microsoft.XMLHTTP");
    xhr.open('POST', url);
    xhr.onreadystatechange = function () {
        if (xhr.readyState > 3 && xhr.status === 200) { success(xhr.responseText); }
    };
    xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
    xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
    xhr.send(params);
    return xhr;
}


function HandleTabClick(tabClicked) {
    if (!tabClicked.classList.contains('active')) {
        var scriptureTabID = tabClicked.id + '-tab';
        tabClicked.classList.add('active');
        var clickedSiblings = getSiblings(tabClicked);
        RemoveClassFromGroup(clickedSiblings, 'active');
        var scriptureTab = document.getElementById(scriptureTabID);
        scriptureTab.classList.add('active');
        var scriptureSiblings = getSiblings(scriptureTab);
        RemoveClassFromGroup(scriptureSiblings, 'active');
    }
}

function getChildren(n, skipMe) {
    var r = [];
    for (; n; n = n.nextSibling)
        if (n.nodeType === 1 && n !== skipMe)
            r.push(n);
    return r;
};

function getSiblings(n) {
    return getChildren(n.parentNode.firstChild, n);
}

function HandleHiddenDetails()
{
    var details = document.getElementsByClassName('hiddendetail');
    for (var i = 0; i < details.length; i++) {
        details[i].onclick = function (e) {
            e.target.classList.add("showdetail");
            e.target.classList.remove('hiddendetail');
            var hiddenSiblings = e.target.parent.childNodes.getElementsByClassName("hiddendetail");
            AddClassToGroup(hiddenSiblings, "showdetail");
            RemoveClassFromGroup(hiddenSiblings, "hiddendetail");
        }
    }
}

function ScrollToTarget()
{
    const marks = document.getElementsByClassName('target');
    for (var i = 0; i < marks.length; i++) {
        var grandparent = marks[i].parent().parent();
        if (grandparent.classList.contains('hiddendetail')) {
            grandparent.classList.add("showdetail");
            grandparent.classList.remove('hiddendetail');
        }
    }
            
    const distanceToTop = el => Math.floor(el.getBoundingClientRect().top);
    const targetAnchor = marks[0];
    if (!targetAnchor) return;
    const originalTop = distanceToTop(targetAnchor);
    window.scrollBy({ top: originalTop, left: 0, behavior: 'smooth' });
    const checkIfDone = setInterval(function () {
        const atBottom = window.innerHeight + window.pageYOffset >= document.body.offsetHeight - 2;
        if (distanceToTop(targetAnchor) === 0 || atBottom) {
            targetAnchor.tabIndex = '-1';
            targetAnchor.focus();
            window.history.pushState('', '', targetID);
            clearInterval(checkIfDone);
        }
    }, 100);
}


function ScrollToScriptureTarget() {
	var $mark = $('.scripture.target,mark.target');
	var targetOffset = $mark.parent().offset().top + 5;
	var h = $('header').height() + 40;
	$('html, body').animate({
		scrollTop: targetOffset - h
	}, 1000);
}


function HandleReadingView() {
    HandleScriptureHeaderClicks();
    HandleCommentHeaderClicks();
    HandleScriptureTextClicks();
}

function HandleScriptureHeaderClicks() {
    var headers = document.querySelectorAll('.scripture-header h3');
    for (var i = 0; i < headers.length; i++) {
        headers[i].onclick = function (event) {
            var texts = document.getElementsByClassName('scripture-text');
            AddClassToGroup(texts, 'hidden');
            var comments = document.getElementsByClassName('scripture-comment');
            RemoveClassFromGroup(comments, 'hidden');
            var commentheaders = document.getElementsByClassName('scripture-comment-header');
            RemoveClassFromGroup(commentheaders, 'hidden');
            event.target.parent().classList.add('hidden');
            var mark = event.target.parent().siblingsgetElementsByClassName('scripture-comment')[0];
            var targetOffset = mark.parent().offset().top + 5;
            var h = document.getElementById('header').height + 40;
            window.scrollBy({ top: targetOffset - h, left: 0, behavior: 'smooth' });
        }
    }
}

function HandleCommentHeaderClicks() {
    var headers = document.querySelectorAll('scripture-comment-header');
    for (var i = 0; i < headers.length; i++) {
        headers[i].onclick = function (event) {
            var texts = document.getElementsByClassName('scripture-text');
            RemoveClassFromGroup(texts, 'hidden');
            var comments = document.getElementsByClassName('scripture-comment');
            AddClassToGroup(comments, 'hidden');
            var siblings = event.target.siblings.getElementsByClassName('.scripture-header');
            RemoveClassFromGroup(siblings, 'hidden');
            var commentheaders = document.getElementsByClassName('scripture-comment-header');
            AddClassToGroup(commentheaders, 'hidden');
            var mark = event.target.siblings.getElementsByClassName('scripture-text')[0];
            var targetOffset = mark.parent().offset().top + 5;
            var h = document.getElementById('header').height + 40;
            window.scrollBy({ top: targetOffset - h, left: 0, behavior: 'smooth' });
        }
    }
}

function HandleScriptureTextClicks() {
    var texts = document.getElementsByClassName('scripture-text');
    for (var i = 0; i < texts.length; i++) {
        texts[i].onclick = function (event) {
            if (event.target.classList.contains('expanded')) {
                var headers = document.getElementsByClassName('scripture-header');
                var verseNumbers = document.getElementsByClassName('versenumber');
                AddClassToGroup(headers, 'hidden');
                AddClassToGroup(verseNumbers, 'hidden');
                var texts = document.getElementsByClassName('scripture-text');
                RemoveClassFromGroup(texts, 'expanded');
            }
            else {
                var expandedtexts = document.getElementsByClassName('scripture-text');
                RemoveClassFromGroup(expandedtexts, 'expanded');
                var expandedheaders = document.getElementsByClassName('scripture-header');
                AddClassToGroup(expandedheaders, 'hidden');
                var parentheader = event.target.parent.querySelector('.scripture-header');
                parentheader.classList.remove('hidden');
                var expandedVerseNumbers = document.getElementsByClassName('versenumber');
                RemoveClassFromGroup(expandedVerseNumbers, 'hidden');
                event.target.classList.add('expanded');
            }
        }
    }
}

function HandleTabClicks() {
    var tabs = document.querySelectorAll('ul.tabs li');
    for (var i = 0; i < tabs.length; i++) {
        tabs[i].onclick = function (event) {
            if (!event.target.classList.contains('active')) {
                var tabNum = event.target.index();
                var tabID = event.target.attr('id') + "-tab";
                event.target.addClass("active").siblings().removeClass('active');
                var theTab = document.getElementById('#' + tabID);
                theTab.classList.add('active');
                var siblings = theTab.siblings();
                RemoveClassFromGroup(siblings, 'active');

                var start = theTab.data - start;
                var end = parseInt(theTab.data - end);
                var url = '/Text?';
                var previousLink = document.getElementById('previousLink');
                previousLink.href = url + 'end=' + (start - 1);
                var nextLink = document.getElementById('nextLink');
                nextLink.href = url + 'start=' + (end + 1);
                var upLink = document.getElementById('upLink');
                upLink.href = '/Chapter?start=' + start;
                var chronoLink = document.getElementById('#chronoLink');
                if (chronoLink) {
                    var chronoid = chronoLink.data - id;
                    chronoLink.href = '/Chrono?id=' + chronoid + "&start=" + start + "&end=" + end;
                }
            }
        }
    }
}

function OpenModalPicture(event) {
    var img = event.target;
    document.getElementById('modal-image').src = img.src;
    document.getElementById('modal-image-box').classList.remove('hidden');
    document.getElementById('menuNext').classList.add('hidden');
}

function CloseModalPicture(event) {
    document.getElementById('modal-image-box').classList.add('hidden');
    document.getElementById('menuNext').classList.remove('hidden');
}

function SetupSuppressedParagraphs() {
    var suppressedParagraphs = document.getElementsByClassName('suppressed');
    var ellipsis = document.getElementById('ellipsis');
    for (var i = 0; i < suppressedParagraphs.length; i++) {
        suppressedParagraphs[i].onclick = function (event) {
            event.target.removeClass('suppressed');
        };
        if (paragraph.length < 1) return;
        ellipsis.removeClass('hidden');
    }
    ellipsis.onclick = function (event) {
        var extraInfo = document.getElementsByClassName('extrainfo');
        var suppressed = document.getElementsByClassName('suppressed');
        RemoveClassFromGroup(suppressed, 'suppressed');
        AddClassToGroup(suppressed, 'extrainfo');
        RemoveClassFromGroup(extraInfo, 'extrainfo');
        AddClassToGroup(extraInfo, 'suppressed');
     };
    $('.suppressed').click(function (event) {
        $(this).removeClass('suppressed');
    });
}

function SetupEditParagraph() {
    var paragraphs = document.getElementsByClassName('editparagraph');
    for (var i = 0; i < paragraphs.length; i++) {
        paragraphs[i].onclick = EditParagraph;
    }
}

function SetThisVerseAsTarget() {
    var links = document.getElementsByClassName("link");
    for (var i = 0; i < links.length; i++) {
        links[i].onclick = function (event) {
            var a = event.target.href;
            var dta = document.querySelector('.active.rangedata');
            e.preventDefault();
            var tg = a + '&tgstart=' + dta.attr('data-start') + '&tgend=' + dta.attr('data-end')
            location.href = tg;
        }
    }
}

function GoToNext() {
    TurnPage('next');
}

function GoToPreceding() {
    TurnPage('preceding');
}

function TurnPage(direction) {
    var path = AddQueryToPath(CurrentPath(), GetRangeQuery());
    path = AddQueryToPath(path, direction + '=true');
    LoadPage(path);
}

function GetRangeQuery() {
    var rangeData = document.querySelector('.rangedata.active');
    if (rangeData === null) return '';
    var start = rangeData.getAttribute('data-start');
    var end = rangeData.getAttribute('data-end');
    return 'start=' + start + '&end=' + end;
}

function SetTitle() {
    var titleData = document.getElementById('pageTitle');
    document.title = titleData.innerText;
}

function HandleNext() {
    if (document.getElementById('modal-image-box').hasClass('hidden'))
        GoToNext();
    else
        document.getElementById('modal-image-box').addClass('hidden');
}


function HandlePrevious() {
    if (document.getElementById('modal-image-box').hasClass('hidden'))
        GoToPreceding();
    else
        document.getElementById('modal-image-box').addClass('hidden');
}

function AddShortcut() {
    document.onkeydown = HandleShortcut;
}

function HandleShortcut(e) {
    if (e.keyCode) code = e.keyCode;
    else if (e.which) code = e.which;
    if (code !== 121) return true; //not F10
    if (!e.ctrlKey) return true; //Ctrl not pressed
    e.preventDefault();
    document.getElementById('searchField').focus();
    return false;
}