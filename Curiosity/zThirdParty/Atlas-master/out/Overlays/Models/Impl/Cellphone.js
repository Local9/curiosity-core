$(document).ready(() => {
    const Viewmodel = new Vue({
        el: "#cellphone-overlay",
        data: {
            Overlay: Overlay,
            overlay: Overlay.NONE,
            panel: 0,
            contacts: [
                {
                    Alias: "=)",
                    Number: 123123123123
                }
            ],
            conversations: [
                {
                    Target: "..",
                    TargetNumber: 123123123,
                    History: [
                        {
                            IsSender: false,
                            Message: "tjo man vad göriiii",
                            Date: "2019/03/12 13:11"
                        },{
                            IsSender: true,
                            Message: "tjo man vad kucken",
                            Date: "2019/03/12 13:12"
                        }
                    ],
                    /**
                     * @return {string}
                     */
                    LastMessage() {
                        return this.History[this.History.length - 1].Message;
                    }
                }
            ],
            conversation: {}
        },
        methods: {
            update_panel(panelId) {
                this.panel = panelId;
            },
            open_conversation(number) {
                this.conversation = this.conversations.filter(self => self.TargetNumber === number)[0];
                this.update_panel(3)
            }
        }
    }); 
});