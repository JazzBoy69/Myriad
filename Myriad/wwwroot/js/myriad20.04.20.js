function SetupPartialPageLoad() {
    var pageURL = document.getElementById('pageUrlData');
    if (pageURL !== null) {
        history.replaceState(null, null, pageURL.innerText);
    }
    window.onpopstate = function (event) {
        LoadHistoryPage(document.location.href);
    }
}

function HandleLink(event) {
    event.preventDefault();
    LoadPage(event.target.closest('a').href);
}

function LoadPage(path) {
    if (performance.navigation.type === 1) {
        LoadCompletePage(path);
        HandleAdditionalSearchTasks();
        return;
    }
    if (path.indexOf('partial') === -1) path = AddQueryToPath(path, 'partial=true');
    LoadMainPane(path);
}

function LoadCompletePage(path) {
    window.location = path.replace("&partial=true", "");
}

function HandleTOCClick(event) {
    event.preventDefault();
    showHideMenu();
    var path = event.target.href;
    var hash = path.indexOf('#');
    if (hash !== -1) {
        ScrollToHeading(path.substr(hash+1));
        return;
    }
    LoadIndexPane(path);
}

function LoadIndexPane(path) {
    postAjax(path, {},
        function (data) {
            var mainPane = document.getElementById('mainPane');
            mainPane.innerHTML = data;
            history.pushState(null, null, CurrentPath());
            SetTitle();
        });
}

function LoadTOC() {
    path = AddQueryToPath(CurrentPath(), 'toc=1');
    postAjax(path, {},
        function (data) {
            if (data.length === 0) return;
            var toc = document.getElementById('tocdiv');
            var overlay = document.getElementById('modal-overlay');
            var article = document.getElementById('mainPane');
            toc.innerHTML = data;
            toc.classList.remove('hidden');
            toc.classList.add('visible');
            overlay.classList.add('show');
            article.classList.add('blur');
        });
}

function LoadHistoryPage(path) {
    if (performance.navigation.type === 1) {
        window.location = path;
        HandleAdditionalSearchTasks();
        HandleHiddenDetails();
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
            HandleAdditionalSearchTasks();
            HandleHiddenDetails();
            ScrollWhenReady();
        });
}

function HandleAdditionalSearchTasks() {
    var queryElement = document.getElementById('querystring');
    if (queryElement === null) return;
    SetSearchFieldText();
    LoadSynonymSearchResults();
}

function LoadMainPane(path) {
    postAjax(path, {},
        function (data) {
            var mainPane = document.getElementById('mainPane');
            mainPane.innerHTML = data;
            var path = CurrentPath();
            if (path.indexOf('/Search')>-1) {
                SetSearchFieldText();
            }
            history.pushState(null, null, path);
            SetTitle();
            HandleAdditionalSearchTasks();
            HandleHiddenDetails();
            ScrollWhenReady();
        });
}

function AddQueryToPath(path, query) {
    if (HasQuery(path)) {
        return path+'&' + query;
    }
    return path + '?' + query;
}

function CurrentPath() {
    var pageURL = document.getElementById('pageUrlData');
    if (pageURL !== null) {
        return pageURL.innerText;
    }
    if ((document.location.pathname === '/') || (document.location.pathname === '/Index')) {
        return '/Index';
    }
    return document.location.pathname + document.location.search;
}

function HasQuery(path) {
    return path.indexOf('?') !== -1;
}

function DefinitionTabClick(target) {
    if (!target.classList.contains('active')) {
        var tabID = target.id + "-tab";
        target.classList.add('active');
        var siblings = getSiblings(target);
        RemoveClassFromGroup(siblings, 'active');
        var relatedTab = document.getElementById(tabID);
        relatedTab.classList.add('active');
        var relatedSiblings = getSiblings(relatedTab);
        RemoveClassFromGroup(relatedSiblings, 'active');
    }
}

function HandleSearch() {
    var searchField = document.getElementById('searchField');
    var path = AddQueryToPath('/Search', 'q=' + searchField.value);
    LoadPage(path);
    return false;
}


function SetSearchFieldText() {
    var queryElement = document.getElementById('querystring');
    if (queryElement === null) return;
    var query = queryElement.innerText;
    var searchField = document.getElementById('searchField');
    searchField.value = query;
}

function LoadSynonymSearchResults() {
    var query = document.getElementById('searchqueryinfo');
    var path = '/SynonymSearch' + query.innerText;
    postAjax(path, {},
        function (data) {
            var synResults = document.getElementById('synresults');
            synResults.innerHTML = data;
        });
}

function ScrollToTarget() {
    var path = CurrentPath();
    if (path.indexOf('/Verse')>-1) return;
    var targets = document.getElementsByClassName('target');
    var target = (targets === null) || (targets.length === 0) ?
        document.getElementById('top') :
        targets[0];
    var targetOffset = target.offsetTop;
    var h = document.getElementsByTagName('header')[0].offsetHeight;
    window.scrollTo({ top: targetOffset - h, left: 0, behavior: 'smooth' });
}

function ScrollWhenReady() {
    var images = mainPane.getElementsByTagName('img');
    var ready = images.length === 0;
    if (!ready) {
        ready = true;
        for (var i = 0; i < images.length; i++) {
            if (!images[i].complete) {
                images[i].onload = ScrollWhenReady;
                return;
            }
        }
    }
    ScrollToTarget();
}

function ScrollToTop() {
    window.scrollTo(0, 0);
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
        return;
    }
    if (toc.classList.contains('visible')) {
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

function ScrollToHeading(path) {
    var header =
        document.getElementById(path);
    var targetOffset = header.offsetTop;
    var h = document.getElementsByTagName('header')[0].offsetHeight;
    window.scrollTo({ top: targetOffset - h, left: 0, behavior: 'smooth' });
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


function HandleTabClick(e) {
    var tabClicked = e.target;
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
    var marks = document.getElementsByClassName('target');
    for (var i = 0; i < marks.length; i++) {
        var grandparent = marks[i].parentNode.parentNode;
        if (grandparent.classList.contains('hiddendetail')) {
            grandparent.classList.add("showdetail");
            grandparent.classList.remove('hiddendetail');
        }
    }
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
    SetupSuppressedParagraphs();
}


function HandleReadingView() {
    HandleScriptureHeaderClicks();
    HandleCommentHeaderClicks();
    HandleScriptureTextClicks();
}

function HandleScriptureHeaderClicks() {
    var texts = document.getElementsByClassName('scripture-text');
    AddClassToGroup(texts, 'hidden');
    var headers = document.getElementsByClassName('scripture-header');
    AddClassToGroup(headers, 'hidden');
    AddClassToGroup(headers, 'visible');
    var comments = document.getElementsByClassName('scripture-comment');
    RemoveClassFromGroup(comments, 'hidden');
    var commentheaders = document.getElementsByClassName('scripture-comment-header');
    RemoveClassFromGroup(commentheaders, 'hidden');
    event.target.parentNode.classList.add('hidden');
    var mark = event.target.closest('.scripture-section').getElementsByClassName('scripture-comment')[0];
    var targetOffset = mark.offsetTop;
    var h = document.getElementsByTagName('header')[0].offsetHeight+40;
    window.scrollTo({ top: targetOffset - h, left: 0, behavior: 'smooth' });
}

function HandleCommentHeaderClicks() {
    var texts = document.getElementsByClassName('scripture-text');
    RemoveClassFromGroup(texts, 'hidden');
    var headers = document.getElementsByClassName('scripture-header');
    RemoveClassFromGroup(headers, 'hidden');
    RemoveClassFromGroup(headers, 'visible');
    var comments = document.getElementsByClassName('scripture-comment');
    AddClassToGroup(comments, 'hidden');
    var commentheaders = document.getElementsByClassName('scripture-comment-header');
    AddClassToGroup(commentheaders, 'hidden');
    var section = event.target.closest('.scripture-section');
    var header = section.getElementsByClassName('scripture-header')[0];
    header.classList.add('visible');
    var targetOffset = header.offsetTop;
    var h = document.getElementsByTagName('header')[0].offsetHeight;
    window.scrollTo({ top: targetOffset - h, left: 0, behavior: 'smooth' });
}

function ExpandReadingViewText(event) {
    var textSection = event.target.closest('.scripture-text');
    if (textSection.classList.contains('expanded')) {
        var headers = document.getElementsByClassName('scripture-header');
        var verseNumbers = document.getElementsByClassName('versenumber');
        RemoveClassFromGroup(headers, 'visible');
        RemoveClassFromGroup(verseNumbers, 'visible');
        var texts = document.getElementsByClassName('scripture-text');
        RemoveClassFromGroup(texts, 'expanded');
    }
    else {
        var expandedtexts = document.getElementsByClassName('scripture-text');
        RemoveClassFromGroup(expandedtexts, 'expanded');
        var expandedheaders = document.getElementsByClassName('scripture-header');
        RemoveClassFromGroup(expandedheaders, 'visible');
        var parentheader = textSection.closest('.scripture-section').querySelector('.scripture-header');
        parentheader.classList.add('visible');
        var expandedVerseNumbers = document.getElementsByClassName('versenumber');
        AddClassToGroup(expandedVerseNumbers, 'visible');
        event.target.parentNode.parentNode.classList.add('expanded');
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
    var hideEllipsis = true;
    for (var i = 0; i < suppressedParagraphs.length; i++) {
        suppressedParagraphs[i].onclick = function (event) {
            var paragraph = event.target.closest('.suppressed');
            paragraph.classList.remove('suppressed');
        };
        if (suppressedParagraphs[i].length < 1) return;
        hideEllipsis = false;
    }
    if (hideEllipsis) {
        if (!ellipsis.classList.contains('hidden')) {
            ellipsis.classList.add('hidden');
        }
        return;
    }
    if (ellipsis.classList.contains('hidden')) {
        ellipsis.classList.remove('hidden');
    }
    ellipsis.onclick = function (event) {
        var extraInfo = document.getElementsByClassName('extrainfo');
        var suppressed = document.getElementsByClassName('suppressed');
        RemoveClassFromGroup(suppressed, 'suppressed');
        AddClassToGroup(suppressed, 'extrainfo');
        RemoveClassFromGroup(extraInfo, 'extrainfo');
        AddClassToGroup(extraInfo, 'suppressed');
     };
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

function GoUp() {
    var path = CurrentPath();
    path = path.replace('&navigating=true', '');
    path = AddQueryToPath(path, 'up=true&navigating=true');
    LoadPage(path);
}

function GoToNext() {
    TurnPage('next');
}

function GoToPreceding() {
    TurnPage('preceding');
}

function TurnPage(direction) {
    var path = CurrentPath();
    path = AddQueryToPath(path, direction + '=true&navigating=true');
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