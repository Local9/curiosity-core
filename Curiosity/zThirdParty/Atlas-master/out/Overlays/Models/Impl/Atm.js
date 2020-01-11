$(document).ready(() => {
    const Viewmodel = new Vue({
        el: "#atm-overlay",
        data: {
            Overlay: Overlay,
            Cash: 100,
            Bank: 100,
            History: [],
            overlay: Overlay.NONE,
            panel: 0
        },
        methods: {
            deposit() {
                this.overlay = 0;
                this.panel = 0;

                const field = this.$refs.deposit_field.value;
                const amount = !isNaN(field) ? parseInt(field) : 1;

                Atlas.send("DEPOSIT_ATM", {0: amount.toString()})
            },
            withdraw() {
                this.overlay = 0;
                this.panel = 0;

                const field = this.$refs.withdraw_field.value;
                const amount = !isNaN(field) ? parseInt(field) : 1;

                Atlas.send("WITHDRAW_ATM", {0: amount.toString()})
            }
        }
    });

    $(async () => {
        window.addEventListener("message", event => {
            switch (event.data["Operation"]) {
                case "OPEN_ATM": {
                    Viewmodel.Overlay = Overlay.ATM;
                    Viewmodel.panel = 0;
                    Viewmodel.Cash = event.data["Cash"];
                    Viewmodel.Bank = event.data["Bank"];
                    Viewmodel.History = event.data["History"];
                }
            }
        });

        window.addEventListener("keydown", event => {
            if (Overlay === Overlay.ATM && event.keyCode === 27) {
                event.preventDefault();

                Viewmodel.Overlay = Overlay.NONE;

                Atlas.send("CLOSE_ATM", {});
            }
        }, false);
    });
});