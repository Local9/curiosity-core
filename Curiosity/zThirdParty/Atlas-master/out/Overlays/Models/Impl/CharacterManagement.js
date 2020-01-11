$(document).ready(() => {
    const Viewmodel = new Vue({
        el: "#characters-overlay",
        data: {
            Overlay: Overlay,
            Header: {
                NONE: 0,
                SELECTION: 1,
                CREATION: 2
            },
            modal: {
                state: false,
                header: "",
                button_approve: "Yes",
                button_cancel: "No",
                callback: DefaultCallback
            },
            overlay: Overlay.NONE,
            header: 0,
            characters: [],
        },
        methods: {
            open_modal(header, callback, button_approve, button_cancel) {
                this.modal.state = true;
                this.modal.header = header;
                this.modal.button_approve = button_approve;
                this.modal.button_cancel = button_cancel;
                this.modal.callback = callback;
            },
            flush_modal() {
                this.modal.state = false;
                this.modal.header = "";
                this.modal.button_approve = "Yes";
                this.modal.button_cancel = "No";
                this.modal.callback = DefaultCallback
            },
            load_character: async (seed) => {
                Atlas.send("SELECT_CHAR", {0: seed})
            },
            delete_character: async (seed) => {
                const character = Viewmodel.characters.filter(self => self.Seed === seed)[0];

                Viewmodel.open_modal("Är du säker på att du vill radera `" + character.Name + " " + character.Surname + "`?", async () => {
                    Viewmodel.flush_modal();

                    await Atlas.task(500);

                    Viewmodel.characters = Viewmodel.characters.filter(self => self.Seed !== seed);

                    Atlas.send("DELETE_CHAR", {0: seed})
                }, "Radera", "Avbryt");
            },
            create_character: (state) => {
                if (!state) {
                    Viewmodel.Overlay = Overlay.CHARACTER_CREATION;
                    Viewmodel.header = Viewmodel.Header.CREATION;
                } else {
                    const firstname = Viewmodel.$refs.character_firstname.value;
                    const surname = Viewmodel.$refs.character_surname.value;
                    const dateofbirth = Viewmodel.$refs.character_dateofbirth.value;

                    if (firstname !== undefined && firstname.length > 0 && surname !== undefined && surname.length > 0 && dateofbirth !== undefined && dateofbirth.length > 0 && new RegExp("^(?:\\d{4}-\\d{2}-\\d{2})").test(dateofbirth)) {
                        Atlas.send("CREATE_CHAR", {0: firstname, 1: surname, 2: dateofbirth.substring(0, 10)});
                    }
                }
            }
        }
    });

    $(async () => {
        const load_characters = (characters) => {
            Viewmodel.characters = characters;
        };

        window.addEventListener("message", event => {
            switch (event.data["Operation"]) {
                case "LOAD_CHARACTERS":
                    Viewmodel.header = Viewmodel.Header.SELECTION;
                    Viewmodel.Overlay = Overlay.CHARACTER_SELECTION;

                    load_characters(event.data["Characters"]);

                    break;
                case "CLOSE_CHARACTERS":
                    Viewmodel.Overlay = Overlay.NONE;

                    break;
            }
        });
    });
});