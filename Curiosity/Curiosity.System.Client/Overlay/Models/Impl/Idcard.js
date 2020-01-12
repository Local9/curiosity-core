$(document).ready(() => {
    const Viewmodel = new Vue({
        el: "#idcard-overlay",
        data: {
            Overlay: Overlay,
            overlay: Overlay.NONE,
            Character: {},
            SSN: "0"
        }
    });

    $(async () => {
        window.addEventListener("message", (event) => {
            switch (event.data["Operation"]) {
                case "SHOW_ID_CARD":
                    Viewmodel.Overlay = Overlay.ID;
                    Viewmodel.Character = event.data["Character"];
                    Viewmodel.SSN = event.data["SSN"];

                    break;
                case "CLOSE_ID_CARD":
                    Viewmodel.Overlay = Overlay.NONE;

                    break;
            }
        });
    });
});