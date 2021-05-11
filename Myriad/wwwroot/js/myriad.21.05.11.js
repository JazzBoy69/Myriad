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
    if (path.indexOf('/Sidebar?') !== -1) {
        LoadSidebar(path);
        return;
    }
    HideSidebar();
    if (path.indexOf('partial') === -1) path = AddQueryToPath(path, 'partial=true');
    LoadMainPane(path);
}

function LoadSidebar(path) {
    postAjax(path, {},
        function (data) {
            var sidebar = document.getElementById('sidebar');
            sidebar.innerHTML = data;
            var article = document.getElementsByTagName('article');
            if (article[0].classList.contains('withsidebar')) {
                return;
            }
            sidebar.classList.remove('hidden');
            sidebar.classList.remove('closing');
            sidebar.classList.add('opening');
            var article = document.getElementsByTagName('article');
            article[0].classList.add('withsidebar');
            var nextButton = document.getElementById('menuNext');
            if (!nextButton.classList.contains('nexttosidebar')) {
                nextButton.classList.add('nexttosidebar');
            }
            setTimeout(function () {
                var sidebar = document.getElementById('sidebar');
                sidebar.classList.remove('opening');
            }, 700);
        });
}

function HideSidebar() {
    var sidebar = document.getElementById('sidebar');
    if (sidebar.classList.contains('hidden')) {
        return;
    }
    sidebar.classList.add('closing');
    var article = document.getElementsByTagName('article');
    article[0].classList.remove('withsidebar');
    article[0].classList.add('withoutsidebar');
    var nextButton = document.getElementById('menuNext');
    nextButton.classList.remove('nexttosidebar');
    setTimeout(function () {
        var sidebar = document.getElementById('sidebar');
        sidebar.classList.add('hidden');
        var article = document.getElementsByTagName('article');
        article[0].classList.remove('withoutsidebar');
    }, 180);
}

function LoadCompletePage(path) {
    window.location = path.replace("&partial=true", "");
    SetupPartialPageLoad();
    SetIcons();
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
            SetIcons();
        });
}

function LoadMainPaneAfterEdit(path) {
    path = AddQueryToPath(path, 'partial=true');
    postAjax(path, {},
        function (data) {
            var mainPane = document.getElementById('mainPane');
            mainPane.innerHTML = data;
            SetTitle();
            SetIcons();
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
    var editForm = document.getElementById('editFormContainer');
    if (!editForm.classList.contains('hidden')) {
        history.pushState(null, null, CurrentPath());
        CloseEditForm();
        return;
    }
    if (performance.navigation.type === 1) {
        window.location = path;
        HandleAdditionalSearchTasks();
        HandleHiddenDetails();
        SetIcons();
        return;
    }
    var target = AddQueryToPath(path, 'partial=true');
    LoadMainPaneHistory(target);
}

function LoadMainPaneHistory(path) {
    postAjax(path, {},
        function (data) {
            UpdateMainPane(data);
            SetTitle();
            HandleAdditionalSearchTasks();
            HandleHiddenDetails();
            ScrollWhenReady();
            SetIcons();
        });
}

function HandleAdditionalSearchTasks() {
    var queryElement = document.getElementById('querystring');
    if (queryElement === null) return;
    SetSearchFieldText();
    LoadSynonymSearchResults();
    SetupPartialPageLoad();
}

function LoadMainPane(path) {
    postAjax(path, {},
        function (data) {
            WriteMainPane(data);
        });
}

function WriteMainPane(data) {
    UpdateMainPane(data);
    var path = CurrentPath();
    if (path.indexOf('/Search') > -1) {
        SetSearchFieldText();
    }
    history.pushState(null, null, path);
    SetTitle();
    HandleAdditionalSearchTasks();
    HandleHiddenDetails();
    SetupPartialPageLoad();
    SetIcons();
    ScrollWhenReady();
}

function SetIcons() {
    SetTOCButton();
    SetPaginationButtons();
    SetEditButton();
    SetOriginalWordButton();
    SetChronoButton();
}

function SetModal() {
    var search = document.getElementById('search');
    search.disabled = true;
    var home = document.querySelector('#menuHome a');
    home.removeAttribute('href');
    HidePaginationButtons();
}

function ResetModal() {
    var search = document.getElementById('search');
    search.disabled = false;
    var home = document.querySelector('#menuHome a');
    home.href = '/Index?name=home';
}

function SetTOCButton() {
    var hasTOC = document.getElementById('hastoc');
    var tocButton = document.getElementById('menuTOC');
    if (hasTOC === null) {
        if (!tocButton.classList.contains('hidden')) {
            tocButton.classList.add('hidden');
        }
        return;
    }
    if (tocButton.classList.contains('hidden')) {
        tocButton.classList.remove('hidden');
    }
}

function SetPaginationButtons() {
    var paginate = document.getElementById('paginate');
    if (paginate === null) {
        HidePaginationButtons();
        return;
    }
    ShowPaginationButtons();
}

function ShowPaginationButtons() {
    var upButton = document.getElementById('menuUp');
    var nextButton = document.getElementById('menuNext');
    var previousButton = document.getElementById('menuPrevious');
    if (upButton.classList.contains('hidden')) {
        upButton.classList.remove('hidden');
    }
    if (nextButton.classList.contains('hidden')) {
        nextButton.classList.remove('hidden');
    }
    if (previousButton.classList.contains('hidden')) {
        previousButton.classList.remove('hidden');
    }
}

function HidePaginationButtons() {
    var upButton = document.getElementById('menuUp');
    var nextButton = document.getElementById('menuNext');
    var previousButton = document.getElementById('menuPrevious');
    if (!upButton.classList.contains('hidden')) {
        upButton.classList.add('hidden');
    }
    if (!nextButton.classList.contains('hidden')) {
        nextButton.classList.add('hidden');
    }
    if (!previousButton.classList.contains('hidden')) {
        previousButton.classList.add('hidden');
    }
}

function SetEditButton() {
    var editdata = document.getElementById('editdata');
    var editButton = document.getElementById('menuEdit');
    if (editdata === null) {
        if (!editButton.classList.contains('hidden'))
            editButton.classList.add('hidden');
        return;
    }
    if (editButton.classList.contains('hidden'))
        editButton.classList.remove('hidden');
}

function SetOriginalWordButton() {
    var originalWords = document.getElementsByClassName('originalword');
    var originalWordButton = document.getElementById('menuOriginalWord');
    if ((originalWords === null) || (originalWords.length === 0)) {
        if (!originalWordButton.classList.contains('hidden')) {
            originalWordButton.classList.add('hidden');
        }
        return;
    }
    if (originalWordButton.classList.contains('hidden')) {
        originalWordButton.classList.remove('hidden');
    }
}

function SetChronoButton() {
    var chrono = document.getElementById('chrono');
    var chronoButton = document.getElementById('menuChrono');
    if ((chrono === null) || (chrono === 'undefined')) {
        if (!chronoButton.classList.contains('hidden')) {
            chronoButton.classList.add('hidden');
        }
        return;
    }
    if (chronoButton.classList.contains('hidden')) {
        chronoButton.classList.remove('hidden');
    }
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

function ShowChrono() {
    var path = ChronoPath();
    if (path === null) return;
    var chrono = document.getElementById('chrono');
    if ((chrono === null) || (chrono === 'undefined')) return;
    var id = 'id=' + chrono.innerText;
    path = AddQueryToPath(path, id);
    LoadPage(path);
}

function ChronoPath() {
    var activeTab = document.querySelector('.active.rangedata');
    if ((activeTab === null) || (typeof activeTab === 'undefined')) return null;
    var path = '/Chrono';
    path = AddQueryToPath(path, "tgstart=" + activeTab.getAttribute('data-start'));
    path = AddQueryToPath(path, "tgend=" + activeTab.getAttribute('data-end'));
    return path;
}

function ActivePath() {
    if (CurrentPath().indexOf('/Chrono') > -1) return CurrentPath();
    var activeTab = document.querySelector('.active.rangedata');
    if ((activeTab === null) || (typeof activeTab === 'undefined')) return CurrentPath();
    var pageURL = document.getElementById('pageUrlData');
    if (pageURL !== null) {
        var url = pageURL.innerText;
        var p = url.indexOf('?');
        var path = url.substr(0, p);
        path = AddQueryToPath(path, "start=" + activeTab.getAttribute('data-start'));
        path = AddQueryToPath(path, "end=" + activeTab.getAttribute('data-end'));
        return path;
    }
    if ((document.location.pathname === '/') || (document.location.pathname === '/Index')) {
        return '/Index';
    }
    var location = document.location.pathname;
    location = AddQueryToPath(location, "start=" + activeTab.getAttribute('data-start'));
    location = AddQueryToPath(location, "end=" + activeTab.getAttribute('data-end'));
    return location;
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
    var path = AddQueryToPath('/Search', 'q=' + searchField.value.trim());
    HideIndex();
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
    if (path.indexOf('/Verse') > -1) {
        ScrollToTop();
        return;
    }
    var targets = document.getElementsByClassName('target');
    var target = (targets === null) || (targets.length === 0) ?
        document.getElementById('top') :
        targets[0].closest('p');
    var targetOffset = target.offsetTop;
    var h = document.getElementsByTagName('header')[0].offsetHeight;
    window.scrollTo({ top: (targetOffset - h), left: 0, behavior: 'smooth' });
}

function ScrollWhenReady() {
    var path = CurrentPath();
    if (path.indexOf('/Verse') > -1) {
        ScrollToTop();
        return;
    }
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
    return document.getElementById(id).querySelectorAll('a');
}

function AddClassToGroup(g, c) {
    for (var i = g.length-1; i >= 0; i--) {
        g[i].classList.add(c);
    }
}

function RemoveClassFromGroup(g, c) {
    for (var i = g.length-1; i >= 0; i--) {
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
        SaveIndexLevel(0);
        document.getElementById('go').classList.add('hidden');
        return;
    }
    var reference = searchField.value;
    reference = reference.substr(0, reference.length - 1);
    searchField.value = reference;
    level--;
    SaveIndexLevel(level);
}

function ResetIndex() {
    var searchField = document.getElementById('searchField');
    var possibilityLinks = GetLinksInID('possibilities');
    var indexgroup = keys.getElementsByClassName('indexgroup');
    var indexnumber = keys.getElementsByClassName('indexnumber');
    for (i = 0; i < possibilityLinks.length; i++) {
        if (possibilityLinks[i].classList.contains('common'))
            possibilityLinks[i].classList.remove('hidden');
        else
            possibilityLinks[i].classList.add('hidden');
    }
    searchField.value = "";
    RemoveClassFromGroup(indexgroup, 'hidden');
    AddClassToGroup(indexnumber, 'hidden');
    document.getElementById('go').classList.add('hidden');
    SaveIndexLevel(0);
}

function SaveIndexLevel(level) {
    document.getElementById('level').textContent = level.toString();
}

function showHideIndex() {
    var overlay = document.getElementById('bibleindex');
    if (overlay.classList.contains('show')) {
        HideIndex();
    }
    else {
        var article = document.getElementById('mainPane');
        var ellipsis = document.getElementById('ellipsis');
        overlay.classList.add('show');
        article.classList.add('blur');
        if ((ellipsis !== null) && (ellipsis.classList !== null)) {
                ellipsis.classList.add('hidden');
        }
        ResetIndex();
    }
}

function HideIndex() {
    var overlay = document.getElementById('bibleindex');
    if (!overlay.classList.contains('show')) return;
    var article = document.getElementById('mainPane');
    var ellipsis = document.getElementById('ellipsis');
    overlay.classList.remove('show');
    article.classList.remove('blur');
    if ((ellipsis !== null) && (ellipsis.classList !== null)) {
        var suppressedParagraphs = document.getElementsByClassName('suppressed');
        for (var i = 0; i < suppressedParagraphs.length; i++) {
            if (suppressedParagraphs[i].length < 1) return;
            ellipsis.classList.remove('hidden');
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

function ResetEditWindow() {
    var scriptureTabs = document.getElementById('editTabList');
    scriptureTabs.innerHTML = '<li id=edittab-0 class=\'active editcontent\'><div id=editForm contenteditable=true data-pos=0 data-id=0 data-index=0 data-edittype=0></div></li>';
}

function EditOriginalWords() {
    var path = AddQueryToPath(CurrentPath(), 'originalword=true');
    var menuAccept = document.getElementById('menuAccept');
    var menuOriginalWord = document.getElementById('menuOriginalWord');
    menuOriginalWord.classList.add('hidden');
    menuAccept.onclick = AcceptEditOriginalWord;

    postAjax(path,{},
        function (data) { ShowEditWindow(data); }
    );
}

function ShowEditWindow(data) {
    HideSidebar();
    var mainPane = document.getElementById('mainPane');
    var menuCancel = document.getElementById('menuCancel');
    var menuEdit = document.getElementById('menuEdit');
    var menuAccept = document.getElementById('menuAccept');
    var menuOriginalWord = document.getElementById('menuOriginalWord');
    var menuChrono = document.getElementById('menuChrono');
    var editForm = document.getElementById('editForm');
    editForm.innerText = data;
    var scrollPos = document.documentElement.scrollTop;
    editForm.setAttribute('data-pos', scrollPos);
    mainPane.classList.add('hidden');
    menuEdit.classList.add('hidden');
    menuOriginalWord.classList.add('hidden');
    menuChrono.classList.add('hidden');
    menuCancel.classList.remove('hidden');
    menuAccept.classList.remove('hidden');
    var editFormContainer = document.getElementById('editFormContainer');
    editFormContainer.classList.remove('hidden');
    SetModal();
    SetEditTabs();
}

function SetEditTabs() {
    var section = GetScriptureSection();
    var header = ((section === null) || (typeof section === 'undefined')) ?
        null :
        section.getElementsByClassName('scripture-header');
    if ((header !== null) && (header.length>0)){
        ShowTabsHeader(header[0]);
        AppendScriptureTabs(section);
        return;
    }
    HideEditTabsHeader();
}

function GetScriptureSection() {
    var paragraph = document.getElementsByClassName('updating');
    if ((paragraph !== null) && (paragraph.length > 0)) {
        return paragraph[0].closest('.scripture-section');
    }
    return document.getElementsByClassName('scripture-section')[0];
}

function ShowTabsHeader(header) {
    var listItems = header.getElementsByTagName('li');
    if (listItems.length === 0) {
        ShowSingleTextTab(header);
        return;
    }
    var editTabsHeader = document.getElementById('editTabsHeader');
    var tabs = '<ul class=tabs><li data-index=0 class=active onclick=SwitchEditTab(this)>Comment</li>';
    for (var i = 0; i < listItems.length; i++) {
        tabs += '<li data-index=' + (i+1) +' onclick=SwitchEditTab(this)>' + listItems[i].innerText + '</li>';
    }
    editTabsHeader.innerHTML = tabs + '</ul>';
    editTabsHeader.classList.remove('hidden');
    var editTabs = document.getElementById('editTabs');
    editTabs.classList.remove('complete');
}

function AppendScriptureTabs(section) {
    var existingTabs = section.getElementsByClassName('tab');
    if ((existingTabs === null) || (existingTabs.length === 0)) {
        AppendSingleTab(section);
        return;
    }
    var list = existingTabs[0].getElementsByTagName('li');
    var listHTML = '';
    for (var i = 0; i < list.length; i++) {
        listHTML += '<li id=edittab-' + (i + 1) + '><p>' + list[i].getElementsByClassName('cleanquote')[0].innerText + '</p></li>';
    }
    var scriptureTabs = document.getElementById('editTabList');
    scriptureTabs.innerHTML += listHTML;
}

function AppendSingleTab(section) {
    var quote = section.getElementsByClassName('cleanquote')[0];
    var scriptureTabs = document.getElementById('editTabList');
    scriptureTabs.innerHTML += '<li id=edittab-1><p>' + quote.innerText+'</p></li>';
}

function SwitchEditTab(element) {
    if (element.classList.contains('active')) return;
    var tabIndex = element.getAttribute('data-index');
    var siblings = getSiblings(element);
    RemoveClassFromGroup(siblings, 'active');
    element.classList.add('active');
    var tabToActivate = document.getElementById('edittab-' + tabIndex);
    tabToActivate.classList.add('active');
    var tabSiblings = getSiblings(tabToActivate);
    RemoveClassFromGroup(tabSiblings, 'active');
}

function SelectMainEditTab() {
    var firstTab = document.querySelector('#editTabsHeader ul li');
    SwitchEditTab(firstTab);
}

function ShowSingleTextTab(header) {
    var heading = header.getElementsByTagName('h3');
    var text = heading[0].innerText;
    var p = text.indexOf('(');
    var scripture = text.substr(p + 1, text.length - p - 2);
    var editTabsHeader = document.getElementById('editTabsHeader');
    var tabs = '<ul class=tabs><li data-index=0 class=active onclick=SwitchEditTab(this)>Comment</li><li data-index=1 onclick=SwitchEditTab(this)>' + scripture + '</li></ul>';
    editTabsHeader.innerHTML = tabs;
    editTabsHeader.classList.remove('hidden');
    var editTabs = document.getElementById('editTabs');
    editTabs.classList.remove('complete');
}

function HideEditTabsHeader() {
    var editTabsHeader = document.getElementById('editTabsHeader');
    if (!editTabsHeader.classList.contains('hidden')) {
        editTabsHeader.classList.add('hidden');
    }
    var editTabs = document.getElementById('editTabs');
    if (!editTabs.classList.contains('complete')) {
        editTabs.classList.add('complete');
    }
}

function CloseEditForm() {
    var editFormContainer = document.getElementById('editFormContainer');
    var mainPane = document.getElementById('mainPane'); 
    editFormContainer.classList.add('hidden');
    var editForm = document.getElementById('editForm');
    var scrollPos = editForm.getAttribute('data-pos');
    ResetEditWindow();
    menuCancel.classList.add('hidden');
    mainPane.classList.remove('hidden');
    SetIcons();
    ResetModal();
    menuAccept.classList.add('hidden');
    document.documentElement.scrollTop = scrollPos;
}

function AcceptEditParagraph() {
    SelectMainEditTab();
    var editForm = document.getElementById('editForm');
    var edittype = editForm.getAttribute('data-edittype');
    var ID = editForm.getAttribute('data-id');
    var index = editForm.getAttribute('data-index');
    postAjax('/EditParagraph/SetData',
        {
            editType: edittype,
            ID: ID,
            paragraphIndex: index,
            text: editForm.innerText.trim()
        },
        function (data) { RefreshEditedParagraph(data); }
    );
    CloseEditForm();
}

function AcceptEditOriginalWord() {
    var editForm = document.getElementById('editForm');
    var path = AddQueryToPath(CurrentPath(), 'originalword=true&accept=true');
    postAjax(path,
        {
            text: editForm.innerText
        },
        function (data) {
            WriteMainPane(data);
        }
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
            var hiddenSiblings = e.target.parentNode.parentNode.getElementsByClassName("hiddendetail");
            AddClassToGroup(hiddenSiblings, "showdetail");
            RemoveClassFromGroup(hiddenSiblings, "hiddendetail");
        }
    }
    SetupSuppressedParagraphs();
    HandleExtraInfo();
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
    var section = event.target.closest('.scripture-section');
    var mark = section.getElementsByClassName('scripture-comment-header')[0];
    var targetOffset = mark.offsetTop;
    var h = document.getElementsByTagName('header')[0].offsetHeight;
    window.scrollTo({ top: targetOffset - h, left: 0});
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
    window.scrollTo({ top: targetOffset - h, left: 0});
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
        textSection.classList.add('expanded');
    }
}

function HandleExtraInfo() {
    var extrainfo = document.getElementsByClassName('extrainfo');
    if (extrainfo.length == 0) return;
    SetupEllipsis(false, ShowHideExtraInfo);
}

function ShowHideExtraInfo() {
    var extrainfo = document.getElementsByClassName('extrainfo');
    if (extrainfo[0].classList.contains('hidden')) {
        RemoveClassFromGroup(extrainfo, 'hidden');
        return;
    }
    AddClassToGroup(extrainfo, 'hidden');
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
    var hideEllipsis = true;
    for (var i = 0; i < suppressedParagraphs.length; i++) {
        suppressedParagraphs[i].onclick = function (event) {
            var paragraph = event.target.closest('.suppressed');
            if (paragraph !== null)
                paragraph.classList.remove('suppressed');
        };
        if (suppressedParagraphs[i].length < 1) return;
        hideEllipsis = false;
    }
    SetupEllipsis(hideEllipsis, ShowHiddenParagraphs);
}

function SetupEllipsis(hideEllipsis, clickHandler) {
    var ellipsis = document.getElementById('ellipsis');
    if (hideEllipsis) {
        if (!ellipsis.classList.contains('hidden')) {
            ellipsis.classList.add('hidden');
        }
        return;
    }
    if (ellipsis.classList.contains('hidden')) {
        ellipsis.classList.remove('hidden');
    }
    ellipsis.onclick = clickHandler;
}

function ShowHiddenParagraphs(event) {
    var extraInfo = document.getElementsByClassName('extrainfo');
    var suppressed = document.getElementsByClassName('suppressed');
    AddClassToGroup(suppressed, 'transition');
    RemoveClassFromGroup(suppressed, 'suppressed');
    AddClassToGroup(extraInfo, 'suppressed');
    RemoveClassFromGroup(extraInfo, 'extrainfo');
    var transition = document.getElementsByClassName('transition');
    AddClassToGroup(transition, 'extrainfo');
    RemoveClassFromGroup(transition, 'transition');
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
            var tg = a + '&tgstart=' + dta.getAttribute('data-start') + '&tgend=' + dta.getAttribute('data-end')
            location.href = tg;
        }
    }
}

function GoUp() {
    var path = ActivePath();
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
    var path = ActivePath();
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
    let key = "";
    if (code === 113) key = "F2";
    if (code === 121) key = "F10";
    if (code === 123) key = "F12";
    if (code === 131) key = "F20";
    if (key === "") return true;
    if (key === "F20") {
        GoUp();
        return;
    }
    if (key === "F2") {
        Edit();
        return;
    }
    if (!e.ctrlKey) return true;
    e.preventDefault();
    if (key === "F10") {
        document.getElementById('searchField').focus();
        return false;
    }
    var editForm = document.getElementById('editFormContainer');
    if (!editForm.classList.contains('hidden')) {
        AcceptOrCancelEditForm(!e.shiftKey);
        return false;
    }
    var paginate = document.getElementById('paginate');
    if (paginate === null) return true;
    if (e.shiftKey) {
        GoToPreceding();
        return false;
    }
    GoToNext();
    return false
}

function AcceptOrCancelEditForm(accept) {
    if (accept) {
        var acceptButton = document.getElementById('menuAccept');
        acceptButton.click();
        return;
    }
    CloseEditForm();
}

function Edit() {
    var editdata = document.getElementById('editdata');
    if (editdata === null) return;
    var path = editdata.innerText; 
    postAjax(path, {},
        function (data) {
            ShowEditWindow(data);
            menuAccept.onclick = AcceptEdit;
        });
}

function AcceptEdit() {
    SelectMainEditTab();
    var editdata = document.getElementById('editdata');
    var path = AddQueryToPath(editdata.innerText, "accept=true");
    var editForm = document.getElementById('editForm');
    postAjax(path,
        {
            text: editForm.innerText
        },
        function (data) {
            let evaluatePath = new RegExp('(\\/)([A-Z][a-z]*?)(\\?)');
            var pageURL = document.getElementById('pageUrlData').innerText;
            var editURL = document.getElementById('editdata').innerText;
            var page = pageURL.match(evaluatePath)[0];
            var editPage = editURL.match(evaluatePath)[0];
            if (page === editPage) {
                mainPane.innerHTML = data; 
                return;
            }
            LoadMainPaneAfterEdit(pageURL);
        }
    );
    CloseEditForm();
}

function UpdateMainPane(data) {
    var mainPane = document.getElementById('mainPane');
    mainPane.innerHTML = data;
}