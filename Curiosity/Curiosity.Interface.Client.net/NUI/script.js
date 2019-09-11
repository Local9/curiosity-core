$(document).ready(function () {

    // setTimeout(() => {
    //     var iframe = window.parent.document.getElementsByName("curiosity-interface")[0];
    //     iframe.src = "https://forums.lifev.net";
    // }, 500);

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

    $(".forum").hide();

    //function UpdateCursorPos() {
    //    $('#cursorPointer').css('left', cursorX);
    //    $('#cursorPointer').css('top', cursorY);
    //}

    //function triggerClick(x, y) {
    //    var element = $(document.elementFromPoint(x, y));
    //    element.focus().click();
    //    return true;
    //}

    $('.forum-close').on('click', function (e) {
        e.preventDefault();
        $.post('http://curiosity-interface/closeForum', JSON.stringify({
            id: idEnt
        }));
    });

    // Listen for NUI Events
    window.addEventListener('message', function (event) {
        var element;

        if (event.data.hasOwnProperty("hideHud")) {
            if (event.data.hideHud) {
                $(".player-hud").hide();
                $(".icons").hide();
            } else {
                $(".player-hud").show();
                $(".icons").show();
            }
        } else if (event.data.hasOwnProperty("showForum")) {
            if (event.data.showForum) {
                $(".forum").show();
            } else {
                $(".forum").hide();
            }
        } else {

            var jobText = "unknown";

            if (!event.data.hasOwnProperty("job")) {
                $(".duty-job").text("no job active");
                $(".duty-state").text("not on active duty");
            } else {

                switch (event.data.job) {
                    case "police":
                        element = $(".police");
                        $(".medic").hide();
                        $(".fire").hide();
                        jobText = "Police Officer";
                        break;
                    case "fire":
                        element = $(".fire");
                        $(".medic").hide();
                        $(".police").hide();
                        jobText = "Firefighter";
                        break;
                    case "medic":
                        element = $(".medic");
                        $(".police").hide();
                        $(".fire").hide();
                        jobText = "Paramedic";
                        break;
                    default:
                        $(".medic").hide();
                        $(".police").hide();
                        $(".fire").hide();
                        $(".duty-job").text(event.data.job);
                        $(".duty-state").text(event.data.dutyActive ? "active" : "on break");
                        break;
                }

                if (element) {
                    if (event.data.duty) {
                        element.show();
                        $(".duty-job").text(jobText);
                        $(".duty-state").text(event.data.dutyActive ? "active" : "on break");
                    } else {
                        element.hide();
                        $(".duty-job").text("no job active");
                        $(".duty-state").text("not on active duty");
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
            }

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
