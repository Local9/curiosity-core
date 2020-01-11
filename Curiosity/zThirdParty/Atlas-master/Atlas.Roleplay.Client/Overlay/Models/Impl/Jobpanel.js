$(document).ready(() => {
    const Viewmodel = new Vue({
        el: "#jobpanel-overlay",
        data: {
            Overlay: Overlay,
            overlay: Overlay.NONE,
            panel: "NONE",
            Job: {},
            Profiles: [],
            Statistics: {},
            Context: {
                RolesDropdown: false,
                Employee: {},
                EmployeeRole: []
            },
            modal: {
                state: false,
                header: "",
                button_approve: "Yes",
                button_cancel: "No",
                type: 0,
                callback: DefaultCallback
            },
        },
        methods: {
            open_modal(header, callback, button_approve, button_cancel, type) {
                this.modal.state = true;
                this.modal.header = header;
                this.modal.button_approve = button_approve;
                this.modal.button_cancel = button_cancel;
                this.modal.callback = callback;
                this.modal.type = type;
            },
            flush_modal() {
                this.modal.state = false;
                this.modal.header = "";
                this.modal.button_approve = "Yes";
                this.modal.button_cancel = "No";
                this.modal.callback = DefaultCallback;
                this.modal.type = 0;
            },
            update_panel(panelId) {
                this.panel = panelId;

                if (panelId === "OVERVIEW") {
                    Vue.nextTick(() => {
                        Atlas.animate_statistic("#panel-stat-money");
                        Atlas.animate_statistic("#panel-stat-days");
                        Atlas.animate_statistic("#panel-stat-top-employee");
                    });
                }
            },
            update_role(role) {
                this.Context.EmployeeRole = role;
                this.Context.RolesDropdown = false
            },
            save_role_settings(employee, role) {
                employee.Role = role;

                const employees = this.Job.Employees.filter(self => self.Seed !== employee.Seed);

                employees.push(employee);

                this.Job.Employees = employees;

                Atlas.send("EMPLOYEE_UPDATE", {0: employee.Seed, 1: employee.Role[0], 2: employee.Salary})
            },
            save_salary_settings(employee) {
                let salary = this.$refs.employee_salary.value;

                salary = !isNaN(salary) ? parseInt(salary) : 0;

                employee.Salary = salary;

                const employees = this.Job.Employees.filter(self => self.Seed !== employee.Seed);

                employees.push(employee);

                this.Job.Employees = employees;

                Atlas.send("EMPLOYEE_UPDATE", {0: employee.Seed, 1: employee.Role[0], 2: employee.Salary})
            },
            kick_employee(employee) {
                this.open_modal("Vill du sparka " + employee.Name + "?", () => {
                    this.flush_modal();

                    Atlas.send("EMPLOYEE_KICK", {0: employee.Seed, 1: this.Job.Label});

                    this.Job.Employees = this.Job.Employees.filter(self => self.Seed !== employee.Seed);
                }, "Ja", "Avbryt", 0);
            },
            hire() {
                this.open_modal("Anställ person", () => {
                    this.flush_modal();

                    const person = this.$refs.modal_input.value;

                    Atlas.send("EMPLOYEE_HIRE", {0: person, 1: this.Job.Attachment, 2: this.Job.Label})
                }, "Anställ", "Avbryt", 1)
            }
        }
    });

    $(async () => {
        window.addEventListener("message", (event) => {
            switch (event.data["Operation"]) {
                case "OPEN_PANEL": {
                    Viewmodel.flush_modal();
                    Viewmodel.Overlay = Overlay.JOBPANEL;
                    Viewmodel.Job = event.data["Job"];
                    Viewmodel.Profiles = event.data["Profiles"];
                    Viewmodel.Statistics = event.data["Statistics"];
                    Viewmodel.update_panel("OVERVIEW");

                    break;
                }
            }
        });

        window.addEventListener("keydown", event => {
            if (Overlay === Overlay.JOBPANEL && event.keyCode === 27) {
                event.preventDefault();

                Viewmodel.Overlay = Overlay.NONE;

                Atlas.send("CLOSE_JOB_PANEL", {});
            }
        }, false);
    });
});