function HandleShortcut(e) {
    e = e || window.event;
    if (e.keyCode) code = e.keyCode;
    else if (e.which) code = e.which;
    var ctrlPressed = false;
    if (e.ctrlKey) ctrlPressed = true;
}