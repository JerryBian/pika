function setActive(id) {
    let ele = document.querySelector(`#${id}`);
    if (ele) {
        ele.classList.add("active");
    }
}

async function startSignalRConnection(endpoint) {
    let connection = new signalR.HubConnectionBuilder()
        .withUrl(endpoint)
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();
    try {
        await connection.start();
        return connection;
    } catch (err) {
        console.error(err);
        return null;
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
    // only set contentType header when caller explicitly provided one
    const contentType = option.hasOwnProperty('contentType') ? option.contentType : undefined;
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

    const headers = {};
    if (contentType) {
        headers["Content-Type"] = contentType;
    }

    window.fetch(url,
        {
            method: method,
            headers: Object.keys(headers).length ? headers : undefined,
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

function showMessageModal(title, bodyHtml, footerHtml, isLargeScreen, onHidden) {
    let messageModalEl = document.querySelector("#messageModal");
    if (!messageModalEl) {
        return;
    }

    if (isLargeScreen) {
        document.querySelector(".modal-dialog").classList.add("modal-xl");
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

    messageModalEl.addEventListener('hidden.bs.modal', event => {
        if (isLargeScreen) {
            document.querySelector(".modal-dialog").classList.remove("modal-xl");
        }

        if (onHidden) {
            onHidden();
        }
    })
}

function showInfoMessageModal(message, onHidden) {
    showMessageModal(`<i class="fa-solid fa-circle-info text-info"></i> Info`, `<p>${message}</p>`, null, null, onHidden);
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

function runScript(id) {
    submitRequest(`/api/script/${id}/run`,
        {
            method: "PUT"
        });
}

function stopRun(runId) {
    showConfirmMessageModal(`Are your sure to stop run #${runId}?`,
        function () {
            submitRequest(`/api/script/run/${runId}/stop`,
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

function showContainer() {
    let ele = document.querySelector("#container");
    if (ele.classList.contains("d-none")) {
        ele.classList.remove("d-none");
    }
}

function hideContainer() {
    let ele = document.querySelector("#container");
    if (!ele.classList.contains("d-none")) {
        ele.classList.add("d-none");
    }
}

function showSpinner() {
    let ele = document.querySelector("#spinner");
    if (ele.classList.contains("d-none")) {
        ele.classList.remove("d-none");
    }
}

function hideSpinner() {
    let ele = document.querySelector("#spinner");
    if (!ele.classList.contains("d-none")) {
        ele.classList.add("d-none");
    }
}