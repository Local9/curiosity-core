$(document).ready(function () {
    // Mouse Controls
    var documentWidth = document.documentElement.clientWidth;
    var documentHeight = document.documentElement.clientHeight;
    var cursor = $('#cursorPointer');
    var cursorX = documentWidth / 2;
    var cursorY = documentHeight / 2;
    var idEnt = 0;

    function UpdateCursorPos() {
        $('#cursorPointer').css('left', cursorX);
        $('#cursorPointer').css('top', cursorY);
    }

    function triggerClick(x, y) {
        var element = $(document.elementFromPoint(x, y));
        element.focus().click();
        return true;
    }

    // Listen for NUI Events
    window.addEventListener('message', function (event) {
        // Crosshair
        if (event.data.crosshair) {
            $(".crosshair").addClass('fadeIn');
        }
        if (!event.data.crosshair) {
            $(".crosshair").removeClass('fadeIn');
        }

        // Menu
        if (event.data.menu == 'vehicle') {
            $(".crosshair").addClass('active');
            $(".menu-car").addClass('fadeIn');
            idEnt = event.data.idEntity;
            if (event.data.showDutyMenu) {
                $(".openDutyMenu").find('.text').text(event.data.dutyMenuText);
                $(".openDutyMenu").show();
            } else {
                $(".openDutyMenu").hide();
            }
        }
        if (event.data.menu == 'user') {
            $(".crosshair").addClass('active');
            $(".menu-user").addClass('fadeIn');
            idEnt = event.data.idEntity;
        }
        if ((event.data.menu == false)) {
            $(".crosshair").removeClass('active');
            $(".menu").removeClass('fadeIn');
            idEnt = 0;
        }

        // Click
        if (event.data.type == "click") {
            triggerClick(cursorX - 1, cursorY - 1);
        }
    });

    // Mousemove
    $(document).mousemove(function (event) {
        cursorX = event.pageX;
        cursorY = event.pageY;
        UpdateCursorPos();
    });

    // Click Menu

    // Functions
    // Vehicle
    $('.openCoffre').on('click', function (e) {
        e.preventDefault();
        $.post('http://curiosity-context/togglecoffre', JSON.stringify({
            id: idEnt
        }));
        $(this).find('.text').text($(this).find('.text').text() == 'Ouvrir le coffre' ? 'Fermer le coffre' : 'Ouvrir le coffre');
    });

    $('.openCarboot').on('click', function (e) {
        e.preventDefault();
        $.post('http://curiosity-context/togglecarboot', JSON.stringify({
            id: idEnt
        }));
        $(this).find('.text').text($(this).find('.text').text() == 'Open the boot' ? 'Close the boot' : 'Open the boot');
    });

    $('.lock').on('click', function (e) {
        e.preventDefault();
        $.post('http://curiosity-context/togglelock', JSON.stringify({
            id: idEnt
        }));
        $(this).find('.text').text($(this).find('.text').text() == 'Lock' ? 'Unlock' : 'Lock');
    });

    $('.openDutyMenu').on('click', function (e) {
        e.preventDefault();
        $.post('http://curiosity-context/openDutyMenu', JSON.stringify({
            id: idEnt
        }));
    });

    // Functions
    // User
    $('.cheer').on('click', function (e) {
        e.preventDefault();
        $.post('http://curiosity-context/cheer', JSON.stringify({
            id: idEnt
        }));
    });


    // Click Crosshair
    $('.crosshair').on('click', function (e) {
        e.preventDefault();
        $(".crosshair").removeClass('fadeIn').removeClass('active');
        $(".menu").removeClass('fadeIn');
        $.post('http://curiosity-context/disablenuifocus', JSON.stringify({
            nuifocus: false
        }));
    });
    $(document).keypress(function (e) {
        if (e.which == 101) { // if "E" is pressed
            $(".crosshair").removeClass('fadeIn').removeClass('active');
            $(".menu").removeClass('fadeIn');
            $.post('http://curiosity-context/disablenuifocus', JSON.stringify({
                nuifocus: false
            }));
        }
    });

});
