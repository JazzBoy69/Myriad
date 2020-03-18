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
    $("#toc").append('<li><a id="link" href="#top">Top of page</a></li>');
    var h2 = $(section).find("h3");
    $(h2).each(function (i) {
        var current = $(this);
        if (current.hasClass("sideheading") || current.text() === "Related Articles" || current.text() === "Cross references") {
            return true;
        }
        current.attr("id", "title" + i);
        $("#toc").append("<li><a id='link" + i + "' href='#title" +
            i + "'>" +
            current.html() + "</a></li>");
    });
}

function SetupIndex() {
    $('#possibilities a').not('.common').addClass('hidden');
    $('#possibilities a').click(function (event) {
        $('#searchField').val($(this).text() + ' ');
        $('#keys a.indexgroup').addClass('hidden');
        $('#keys a.indexnumber').removeClass('hidden');
        $('#level').text("1");
        if (!$('#go').hasClass('hidden')) {
            $('#go').addClass('hidden');
        }
    });
    $('#go').click(function (event) {
        $('#search').click();
    });
    $('#keys a.indexgroup').click(function (event) {
        var id = $(this).attr('id');
        var group = ".group" + id.substr(id.length - 1);
        $('#possibilities a').not(group).addClass('hidden');
        $('#possibilities a' + group).removeClass('hidden');
    });
    $('#keys a.indexnumber').click(function (event) {
        var text = $('#searchField').val()+$(this).text();
        $('#searchField').val(text);
        var level = +$('#level').text();
        level++;
        $('#level').text(level.toString());
        $('#go').removeClass('hidden');
    });
    $('#back').click(function (event) {
        var level = +$('#level').text();
        if (level === 0) {
            $('#possibilities a.common').removeClass('hidden');
            $('#possibilities a').not('.common').addClass('hidden');
            return;
        }
        if (level === 1) {
            $('#searchField').val("");
            $('#keys a.indexgroup').removeClass('hidden');
            $('#keys a.indexnumber').addClass('hidden');
            $('#level').text("0");
            return;
        }
        var reference = $('#searchField').val();
        reference = reference.substr(0, reference.length - 1);
        $('#searchField').val(reference);
        level--;
        $('#level').text(level.toString());
        if (level < 2) {
            $('#go').addClass('hidden');
        }
    });
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
    var overlay = $('#bibleindex');
    var article = $('article');
    if (overlay.hasClass('show')) {
        overlay.removeClass('show');
        article.removeClass('blur');
        $('.ellipsis').removeClass('hidden');
    }
    else {
        overlay.addClass('show');
        article.addClass('blur');
        $('.ellipsis').addClass('hidden');
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
    $(".hiddendetail").click(function (e) {
        $(this).addClass("showdetail").removeClass('hiddendetail');
        $(this).siblings(".hiddendetail").addClass("showdetail").removeClass("hiddendetail");
    });
}

function ScrollToTarget()
{
    var $mark = $('.target');
    if ($mark.parent().parent().hasClass('hiddendetail'))
    {
        $mark.parent().parent().addClass("showdetail").removeClass('hiddendetail');
    }
    var targetOffset = $mark.parent().offset().top+5;
    var h = $('header').height() + 40;
    $('html, body').animate({
        scrollTop: targetOffset - h
    }, 1000);
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
    $('#modal-overlay').css('height', window.innerHeight);
    $(window).resize(function () {
        $('#modal-overlay').css('height', window.innerHeight);
    });
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
	$('article img').click(function (e) {
		var img = $(e.target);
		$('#modal-image').attr('src', img.attr('src'));
		$('#modal-image-box').removeClass('hidden');
		$('#menuNext').addClass('hidden');
	});
	$('.zoomclose').click(function (e) {
		$('#modal-image-box').addClass('hidden');
		$('#menuNext').removeClass('hidden');
	});
}

function SetupPagination() {
    if (screen.width < 961) {
        $('article').hammer().on('swipeleft', function () {
            window.location = $('#nextLink').attr('href');
        });
        $('article').hammer().on('swiperight', function () {
            window.location = $('#previousLink').attr('href');
        });
    }
    shortcut.add("Ctrl+Shift+F12", function () {
        window.location.href = $('#previousLink').attr('href');
    });
    shortcut.add("Ctrl+F12", function () {
        window.location.href = $('#nextLink').attr('href');
    });
    $('#menuPrevious').css('height', window.innerHeight);
    $('#menuNext').css('height', window.innerHeight);
    $('#modal-overlay').css('height', window.innerHeight);
	$('#menuPrevious').mouseup(function () {
		if ($('#modal-image-box').hasClass('hidden'))
			window.location = $('#previousLink').attr('href');
		else 
			$('#modal-image-box').addClass('hidden');
    });
    $('#menuNext').mouseup(function () {
		if ($('#modal-image-box').hasClass('hidden'))
			window.location = $('#nextLink').attr('href');
		else
			$('#modal-image-box').addClass('hidden');
    });
    $(window).resize(function () {
        $('#menuPrevious').css('height', window.innerHeight);
        $('#menuNext').css('height', window.innerHeight);
        $('#modal-overlay').css('height', window.innerHeight);
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