$(document).ready(() => {
    const Viewmodel = new Vue({
        el: "#menu-overlay",
        data: {
            Overlay: Overlay,
            overlay: Overlay.NONE,
            container: {
                Header: "Society",
                Type: 0,
                Selected: 0,
                Items: [],
                Profile: {
                    Standard: "Standard"
                }
            }
        },
        methods: {
            post_dialog_menu() {
                Atlas.send("MENU_DIALOG_UPDATE", {0: this.$refs.menu_dialog.value})
            }
        }
    });

    $(async () => {
        let menu_dialog_process = function () {
            Viewmodel.$nextTick(() => {
                const dom = Viewmodel.$refs.menu_dialog;
                const element = $(dom);

                element.focus();
                element.off();
                element.on("keydown", (event) => {
                    if (event.keyCode === 13) {
                        event.preventDefault();

                        Atlas.send("MENU_DIALOG_SELECT", {})
                    }
                });
            })
        };

        window.addEventListener("message", event => {
            switch (event.data["Operation"]) {
                case "OPEN_MENU":
                    Viewmodel.Overlay = Overlay.MENU;
                    Viewmodel.container = event.data["Metadata"];

                    if (event.data["Metadata"]["Type"] === 1) menu_dialog_process();

                    break;
                case "UPDATE_MENU":
                    Viewmodel.container = event.data["Metadata"];

                    if (event.data["Metadata"]["Type"] === 1) menu_dialog_process();

                    break;
                case "CLOSE_MENU":
                    Viewmodel.Overlay = Overlay.NONE;

                    break;
            }
        });
    });
});