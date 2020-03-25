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

function CreateTableOfContents(section)
{
    var toc = document.getElementsByClassName('#toc');
    toc.append('<li><a id="link" href="#top">Top of page</a></li>');
    var h2 = document.getElementById(section).querySelectorAll("h3");

    h2.forEach(function (current, i) {
        if (current.hasClass("sideheading") || current.text() === "Related Articles" || current.text() === "Cross references") {
            return true;
        }
        current.attr("id", "title" + i);
        toc.append("<li><a id='link" + i + "' href='#title" +
            i + "'>" +
            current.html() + "</a></li>");
        current.onclick = TOCScroll;
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

function SetupIndex() {
    var possibilityLinks = GetLinksInID('possibilities');
    var go = document.getElementById('go');
    var keys = document.getElementById('keys');
    var indexgroup = keys.getElementsByClassName('indexgroup');
    var indexnumber = keys.getElementsByClassName('indexnumber');

    for (var i = 0; i < possibilityLinks.length; i++)
    {
        possibilityLinks[i].onclick = HandlePossibilityClick;
    }
    go.onclick = function () {
        document.getElementById('search').click();
    };
    for (i = 0; i < indexgroup.length; i++)
    {
        indexgroup[i].onclick = HandleIndexGroupClick;
    }
    for (i = 0; i < indexnumber.length; i++) {
        indexnumber[i].onclick = HandleIndexNumberClick;
    }
    document.getElementById('back').onclick = HandleBackClick;
}

function CreateTableOfContentsFromMarkers(section) {
    $("#toc").append('<li><a id="link" href="#top">Top of page</a></li>');
    var markers = $(section).find(".marker");
    $(markers).each(function (i) {
        var current = $(this);

        current.attr("id", "title" + i);
        $("#toc").append("<li><a id='link" + i + "' href='#title" +
            i + "'>" +
            current.html() + "</a></li>");
    });
}

function filterPath(string) {
    return string
        .replace(/^\//, '')
        .replace(/(index|default).[a-zA-Z]{3,4}$/, '')
        .replace(/\/$/, '');
}

function showHideIndex() {
    var overlay = document.getElementById('bibleindex');
    var article = document.getElementById('mainPane');
    var ellipsis = document.getElementById('ellipsis');
    if (overlay.classList.contains('show')) {
        overlay.classList.remove('show');
        article.classList.remove('blur');
        if ((ellipsis !== null) && (ellipsis.classList !== null)) {
            ellipsis.classList.remove('hidden');

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
    var toc = $('#toc');
    var overlay = $('#modal-overlay');
    var article = $('article');
    if (toc.hasClass('hidden')) {
        toc.removeClass('hidden');
        toc.addClass('visible');
        overlay.addClass('show');
        article.addClass('blur');
    }
    else {
        toc.removeClass('visible');
        overlay.removeClass('show');
        toc.addClass('hidden');
        article.removeClass('blur');
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

function EditParagraph() {
    var editlink = $(this);
    var edittype = $(this).data('edittype');
    var ID = $(this).data('linkid');
    var index = $(this).data('index');
    $.ajax({
        url: "/Verse?handler=ParagraphPlainText",
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN",
                $('input:hidden[name="__RequestVerificationToken"]').val());
        },
        dataType: "json",
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify({ editType: edittype, commentID: ID, paragraphIndex: index }),
        success: function (data) {
            bootbox.prompt({
                title: "Edit paragraph",
                inputType: 'textarea',
                value: data,
                callback: function (result) {
                    if (result !== null && result.length>0) {
                        result = result.replace(/ '/g, ' ‘');
                        result = result.replace(/^'/g, '^‘');
                        result = result.replace(/\*\*'/g, '**‘');
                        result = result.replace(/\/\/'/g, '//‘');
                        result = result.replace(/“'"/g, '“‘');
                        result = result.replace(/—'/g, '—‘');
                        result = result.replace(/\('/g, '(‘');
                        result = result.replace(/\['/g, '[‘');

                        result = result.replace(/'/g, '’');
                        var jdata = "{ editType: '" + edittype + "', commentID: '" + ID + "', paragraphIndex: '" + index +
                            "', commentParagraph: '" + result + "' }";
                        $.ajax({
                            url: "/Verse?handler=ParagraphHTML",
                            beforeSend: function (xhr) {
                                xhr.setRequestHeader("XSRF-TOKEN",
                                    $('input:hidden[name="__RequestVerificationToken"]').val());
                            },
                            dataType: "json",
                            type: "POST",
                            contentType: "application/json; charset=utf-8",
                            data: JSON.stringify({ editType: edittype, commentID: ID, paragraphIndex: index, commentParagraph: result }),
                            success: function (data) {
                                editlink.siblings('.parcontent').html(data);
                            }
                        });
                    }
                }
            });
        }
    });
}

function HandleTabs() {
    $("ul.tabs li").click(function (e) {
        if (!$(this).hasClass("active")) {
            var tabNum = $(this).index();
            var tabID = $(this).attr('id') + "-tab";

            $(this).addClass("active").siblings().removeClass('active');
            $('#' + tabID).addClass("active").siblings().removeClass('active');
        }
    });
}

function HandleHiddenDetails()
{
    var details = document.getElementsByClassName('hiddendetail');
    details.forEach(detail => detail.onclick = function (e) {
        e.target.classList.add("showdetail");
        e.target.classList.remove('hiddendetail');
        var hiddenSiblings = e.target.parent.childNodes.getElementsByClassName("hiddendetail");
        AddClassToGroup(hiddenSiblings, "showdetail");
        RemoveClassFromGroup(hiddenSiblings, "hiddendetail");
    });
}

function ScrollToTarget()
{
    const marks = document.getElementsByClassName('target');
    marks.forEach(function (mark) {
        var grandparent = mark.parent().parent();
        if (grandparent.classList.contains('hiddendetail')) {
            grandparent.classList.add("showdetail");
            grandparent.classList.remove('hiddendetail');
        }
    });
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

function SetupOverlay() {
    SetOverlaySize();
    window.onresize = function () {
        SetOverlaySize();
    };
}

function SetOverlaySize() {
    document.getElementById('modal-overlay').style.height = window.innerHeight;
}

function HandleReadingView() {
	$('.scripture-header h3').click(function (e) {
		$('.scripture-text').addClass('hidden');
		$('.scripture-comment').removeClass('hidden');
		$('.scripture-comment-header').removeClass('hidden');
		$(this).parent().addClass('hidden');
		var $mark = $(this).parent().siblings('.scripture-comment');
		var targetOffset = $mark.parent().offset().top + 5;
		var h = $('header').height() + 40;
		$('html, body').animate({
			scrollTop: targetOffset - h
		}, 100);
	});
	$('.scripture-comment-header').click(function (e) {
		$('.scripture-text').removeClass('hidden');
		$('.scripture-comment').addClass('hidden');
		$(this).siblings('.scripture-header').removeClass('hidden');
		$('.scripture-comment-header').addClass('hidden');
		var $mark = $(this).siblings('.scripture-text');
		var targetOffset = $mark.parent().offset().top + 5;
		var h = $('header').height() + 40;
		$('html, body').animate({
			scrollTop: targetOffset - h
		}, 1000);
	});
	$('.scripture-text').click(function (e) {
		if ($(this).hasClass('expanded')) {
			$('.scripture-header,.versenumber').addClass('hidden');
			$('.scripture-text').removeClass('expanded');
		}
		else {
			$('.scripture-text').removeClass('expanded');
			$('.scripture-header').addClass('hidden');
			$(this).parent().find('.scripture-header').removeClass('hidden');
			$('.versenumber').removeClass('hidden');
			$(this).addClass('expanded');
		}
	});
}

function SetupModalPictures() {
    var images = document.getElementById('article').getElementsByTagName('img');
    images.forEach(image => image.onclick = function (e) {
		var img = e.target;
        document.getElementById('modal-image').src = img.src;
        document.getElementById('modal-image-box').classList.remove('hidden');
        document.getElementById('menuNext').classList.add('hidden');
	});
    document.getElementById('zoomclose').onclick = function (e) {
        document.getElementById('modal-image-box').classList.add('hidden');
        document.getElementById('menuNext').classList.remove('hidden');
	};
}

function SetupSuppressedParagraphs() {
    var suppressedParagraphs = document.getElementsByClassName('suppressed');
    var ellipsis = document.getElementById('ellipsis'); 
    suppressedParagraphs.forEach(function (paragraph) {
        paragraph.onclick = function (event) {
            event.target.removeClass('suppressed');
        };
        if (paragraph.length < 1) return;
        ellipsis.removeClass('hidden');
    });
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
    paragraphs.forEach(paragraph => paragraph.onclick = EditParagraph);
}

function SetupPagination() {
    if (screen.width < 961) {
        var hammertime = new Hammer.Manager(document.getElementById('article'));
        hammertime.on('swipeleft', function () {
            window.location = $('#nextLink').attr('href');
        });
        hammertime.on('swiperight', function () {
            window.location = $('#previousLink').attr('href');
        });
    }
    shortcut.add("Ctrl+Shift+F12", function () {
        window.location.href = document.getElementById('previousLink').attr('href');
    });
    shortcut.add("Ctrl+F12", function () {
        window.location.href = document.getElementById('nextLink').attr('href');
    });
    var prev = document.getElementById('menuPrevious').style.height = window.innerHeight;
    document.getElementById('menuNext').style.height =  window.innerHeight;
    document.getElementById('modal-overlay').style.height =  window.innerHeight;
    document.getElementById('menuPrevious').addEventListener('mouseup', function () {
        if (document.getElementById('modal-image-box').hasClass('hidden'))
            window.location = document.getElementById('previousLink').attr('href');
		else 
            document.getElementById('modal-image-box').addClass('hidden');
    });
    document.getElementById('menuNext').addEventListener('mouseup', function () {
        if (document.getElementById('modal-image-box').hasClass('hidden'))
            window.location = document.getElementById('nextLink').attr('href');
		else
            document.getElementById('modal-image-box').addClass('hidden');
    });
    window.addEventListener('resize', function () {
        document.getElementById('menuPrevious').style.height = window.innerHeight;
        document.getElementById('menuNext').style.height = window.innerHeight;
        document.getElementById('modal-overlay').style.height = window.innerHeight;
    });
}

function SetupVersePagination() {
    if (screen.width < 961) {
        $('article').hammer().on('swipeleft', function () {
            window.location = $('#nextLink').attr('href');
        });
        $('article').hammer().on('swiperight', function () {
            window.location = $('#previousLink').attr('href');
        });
    }
    $('#menuPrevious').css('height', window.innerHeight);
    $('#menuNext').css('height', window.innerHeight);
    $('#modal-overlay').css('height', 0);
    $('#menuPrevious').mouseup(function () {
        window.location = $('#previousLink').attr('href');
    });
    $('#menuNext').mouseup(function () {
        window.location = $('#nextLink').attr('href');
    });
    $(window).resize(function () {
        $('#menuPrevious').css('height', window.innerHeight);
        $('#menuNext').css('height', window.innerHeight);
    });


}