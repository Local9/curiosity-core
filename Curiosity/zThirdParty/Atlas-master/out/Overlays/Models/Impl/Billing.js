$(document).ready(() => {
    const Viewmodel = new Vue({
        el: "#billing-overlay",
        data: {
            Overlay: Overlay,
            Bill: {},
            overlay: 0
        },
        methods: {
            submit_bill() {
                const designation = this.$refs.designation.value.split("\n");
                const amountLines = this.$refs.amount.value.split("\n");
                let amount = 0;

                for (let i = 0; i < amountLines.length; i++) {
                    let entry = amountLines[i].toString();

                    if (entry.length > 0)
                        amount += !isNaN(entry) ? parseInt(entry) : 0;
                }

                this.Bill.Receiver.Name = this.$refs.receiver.value;
                this.Bill.Designation = designation;
                this.Bill.AmountLines = amountLines;
                this.Bill.Amount = amount;
                this.Bill._Receiver = JSON.stringify(this.Bill.Receiver);
                this.Bill._Designation = JSON.stringify(this.Bill.Designation);
                this.Bill._AmountLines = JSON.stringify(this.Bill.AmountLines);

                Atlas.send("SUBMIT_BILL", {0: JSON.stringify(this.Bill)});

                this.overlay = this.Overlay.NONE;
            },
            pay_bill() {
                Atlas.send("PAY_BILL", {0: this.Bill.Seed});

                this.overlay = this.Overlay.NONE;
            },
            destroy_bill() {
                Atlas.send("DESTROY_BILL", {0: this.Bill.Seed});

                this.overlay = this.Overlay.NONE;
            },
            close() {
                this.overlay = this.Overlay.NONE;

                Atlas.send("CLOSE_BILL", {})
            }
        }
    });

    let math_random = (min, max) => {
        return Math.floor(Math.random() * (max - min)) + min;
    };

    let create_bill = () => {
        Viewmodel.Bill.BillNumber = math_random(10000, 99999);
        Viewmodel.Bill.ClientNumber = math_random(100000000, 999999999);
        Viewmodel.Bill.OrderNumber = math_random(1000000, 9999999);
    };

    window.addEventListener("message", (event) => {
        switch (event.data["Operation"]) {
            case "SHOW_BILL": {
                const bill = event.data["Bill"];

                Viewmodel.Bill = bill;

                if (!bill.IsCreated) create_bill();

                Viewmodel.Overlay = Overlay.BILLING;

                break;
            }
        }
    });

    create_bill();
});