function setActive(id) {
    let ele = document.querySelector(`#${id}`);
    if (ele) {
        ele.classList.add("active");
    }
}

function submitRequest(url, option) {
    if (option.form) {
        if (option.form.classList.contains("needs-validation")) {
            if (option.form.checkValidity()) {
                option.form.classList.add("was-validated");
            } else {
                option.form.classList.add("was-validated");
                return;
            }

        }

        const fieldset = option.form.closest("fieldset");
        if (fieldset) {
            fieldset.disabled = true;
        }
    }

    if (option.preAction) {
        option.preAction();
    }

    const method = option.method ?? "POST";
    const contentType = option.contentType ?? "application/json";
    const body = option.body ?? "";
    const formPostAction = function () {
        if (option.form) {
            if (option.form.classList.contains("was-validated")) {
                option.form.classList.remove("was-validated");
            }

            const fieldset = option.form.closest("fieldset");
            if (fieldset) {
                fieldset.disabled = false;
            }
        }
    };

    window.fetch(url,
        {
            method: method,
            headers: {
                "Content-Type": contentType
            },
            body: body
        }).then(response => response.json()).then(result => {
            if (!result.ok) {
                showErrorMessageModal(result.message);
                formPostAction();
            } else {
                if (result.redirectTo) {
                    window.location.href = result.redirectTo;
                } else {
                    if (option.okAction) {
                        option.okAction(result.content);
                    }
                }
            }

            if (option.postAction) {
                formPostAction();
                option.postAction();
            }
        }).catch(error => {
        showErrorMessageModal(error);
            formPostAction();
        });
}

function showMessageModal(title, bodyHtml, footerHtml) {
    let messageModalEl = document.querySelector("#messageModal");
    if (!messageModalEl) {
        return;
    }

    let messageModal = bootstrap.Modal.getOrCreateInstance(messageModalEl);
    messageModal.show();

    let messageModalTitle = document.querySelector("#messageModalTitle");
    if (messageModalTitle) {
        messageModalTitle.innerHTML = title;
    }

    let messageModalBody = document.querySelector("#messageModalBody");
    if (messageModalBody) {
        messageModalBody.innerHTML = bodyHtml;
    }

    let messageModalFooter = document.querySelector("#messageModalFooter");
    if (messageModalFooter) {
        footerHtml = footerHtml ??
            "<button type=\"button\" class=\"btn btn-info\" data-bs-dismiss=\"modal\">Ok</button>";
        messageModalFooter.innerHTML = footerHtml;
    }
}

function showInfoMessageModal(message) {
    showMessageModal(`<i class="fa-solid fa-circle-info text-info"></i> Info`, `<p>${message}</p>`);
}

function showErrorMessageModal(message) {
    showMessageModal(`<i class="fa-solid fa-circle-exclamation text-danger"></i> Error`, `<p>${message}</p>`);
}

function showConfirmMessageModal(message, yesHandler) {
    if (!yesHandler) {
        return;
    }

    let yesId = makeId(8);
    let yesButton = `<button type="button" class="btn btn-danger" data-bs-dismiss="modal" id="${yesId}"><span class="px-2">Yes</span></button>`;
    let noButton = "<button type=\"button\" class=\"btn btn-dark\" data-bs-dismiss=\"modal\"><span class=\"px-2\">No</span></button>";
    showMessageModal(`<i class="fa-solid fa-circle-question text-warning"></i> Question`, `<p>${message}</p>`, yesButton + noButton);
    document.querySelector(`#${yesId}`).addEventListener("click", function () { yesHandler(); });
}

function makeId(length) {
    var result = "";
    const characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
    const charactersLength = characters.length;
    for (let i = 0; i < length; i++) {
        result += characters.charAt(Math.floor(Math.random() *
            charactersLength));
    }
    return result;
}

function runTask(id) {
    submitRequest(`/api/run/${id}`,
        {
            method: "PUT"
        });
}

function stopRun(runId) {
    showConfirmMessageModal(`Are your sure to stop run #${runId}?`,
        function () {
            submitRequest(`/api/run/${runId}/stop`,
                {
                    method: "POST",
                    okAction: function (res) {
                        window.location.reload();
                    }
                });
        });
}

function escapeHTML(str) {
    return new Option(str).innerHTML;
}

function startApp(id) {
    submitRequest(`/api/app/${id}/start`,
        {
            method: "POST",
            okAction: function (res) {
                showInfoMessageModal(`<pre style="background:white;color:black;padding:3px;">${res}</pre>`);
            }
        });
}

function stopApp(id) {
    submitRequest(`/api/app/${id}/stop`,
        {
            method: "POST",
            okAction: function (res) {
                showInfoMessageModal(`<pre style="background:white;color:black;padding:3px;">${res}</pre>`);
            }
        });
}