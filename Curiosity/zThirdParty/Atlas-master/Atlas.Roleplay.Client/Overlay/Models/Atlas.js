const Atlas = {
    task: (milliseconds) => {
        return new Promise(((resolve, reject) => setTimeout(resolve, milliseconds)))
    },
    send: (event, data) => {
        $.post("http://atlas/" + event, JSON.stringify(data))
    },
    animate_statistic: (elementId) => {
        const element = $(elementId);

        if (!isNaN(element.text())) {
            const parts = element.text().match(/^(\d+)(.*)/);

            if (parts.length < 2) return;

            const scale = 20;
            const delay = 25;
            const end = 0 + parts[1];
            const suffix = parts[2];

            let next = 0;

            const func = function () {
                const show = Math.ceil(next);

                element.text('' + show + suffix);

                if (show === end) return;

                next = next + (end - next) / scale;

                window.setTimeout(func, delay);
            };

            func();
        } else {
            Atlas.__typewriter_step(element, element.text(), 0)
        }
    },
    __typewriter_step: (element, text, index) => {
        if (index < text.length) {
            element.text(text.substring(0, index + 1));
            
            setTimeout(() => {
                Atlas.__typewriter_step(element, text, index + 1);
            }, 100);
        }
    }
};