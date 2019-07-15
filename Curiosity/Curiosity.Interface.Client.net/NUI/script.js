$(document).ready(function () {
    // Mouse Controls
    var documentWidth = document.documentElement.clientWidth;
    var documentHeight = document.documentElement.clientHeight;
    //var cursor = $('#cursorPointer');
    var cursorX = documentWidth / 2;
    var cursorY = documentHeight / 2;
    var idEnt = 0;

    $(".police").hide();
    $(".medic").hide();
    $(".fire").hide();

    //function UpdateCursorPos() {
    //    $('#cursorPointer').css('left', cursorX);
    //    $('#cursorPointer').css('top', cursorY);
    //}

    //function triggerClick(x, y) {
    //    var element = $(document.elementFromPoint(x, y));
    //    element.focus().click();
    //    return true;
    //}

    // Listen for NUI Events
    window.addEventListener('message', function (event) {
        var element;

        if (event.data.hideHud) {
            $(".player-hud").hide();
            $(".icons").hide();
        } else {
            $(".player-hud").show();
            $(".icons").show();
        }

        switch (event.data.job) {
            case "police":
                element = $(".police");
                $(".medic").hide();
                $(".fire").hide();
                break;
            case "fire":
                element = $(".fire");
                $(".medic").hide();
                $(".police").hide();
                break;
            case "medic":
                element = $(".medic");
                $(".police").hide();
                $(".fire").hide();
                break;
        }

        if (element) {
            if (event.data.duty) {
                element.show();
            } else {
                element.hide();
            }

            if (event.data.dutyActive) {
                element.addClass('active');
            }
            if (!event.data.dutyActive) {
                element.removeClass('active');
            }
        } else {
            $(".police").hide();
            $(".medic").hide();
            $(".fire").hide();
        }

        // Crosshair
        //if (event.data.crosshair) {
        //    $(".crosshair").addClass('fadeIn');
        //}
        //if (!event.data.crosshair) {
        //    $(".crosshair").removeClass('fadeIn');
        //}

        //if ((event.data.menu == false)) {
        //    $(".crosshair").removeClass('active');
        //    $(".menu").removeClass('fadeIn');
        //    idEnt = 0;
        //}

        //// Click
        //if (event.data.type == "click") {
        //    triggerClick(cursorX - 1, cursorY - 1);
        //}
    });

    // Mousemove
    //$(document).mousemove(function (event) {
    //    cursorX = event.pageX;
    //    cursorY = event.pageY;
    //    UpdateCursorPos();
    //});

    // Click Menu

    // Functions
    // Vehicle
    //$('.openCoffre').on('click', function (e) {
    //    e.preventDefault();
    //    $.post('http://curiosity-context/togglecoffre', JSON.stringify({
    //        id: idEnt
    //    }));
    //    $(this).find('.text').text($(this).find('.text').text() == 'Ouvrir le coffre' ? 'Fermer le coffre' : 'Ouvrir le coffre');
    //});
});
