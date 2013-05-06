(function ($) {

    var Guid = function () {
        var self = this;
        self.newGuid = function () {
            return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) { var r = Math.random() * 16 | 0, v = c == 'x' ? r : r & 0x3 | 0x8; return v.toString(16); });
        };
    };
    
    var Alert = function () {
        var self = this;
        self.show = function (message, args) {
            var defaultArgs = {
                title: "Ooops!",
                message: message,
                okCallBack: null,
                okButtonText: "Ok"
            };

            if (typeof args == "undefined") {
                args = defaultArgs;
            } else {
                for (var i in defaultArgs)
                    if (typeof args[i] == "undefined")
                        args[i] = defaultArgs[i];
            }

            var id = Errordite.Guid.newGuid();
            var markup = '<div id="' + id + '" class="modal hide fade" data-backdrop="static">';
            markup += '<div class="modal-header">';
            markup += '<button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>';
            markup += '<h4 class="formodal">';
            markup += args["title"];
            markup += '</h4>';
            markup += '</div>';
            markup += '<div class="modal-body"><p>';
            markup += args["message"];
            markup += '</p></div>';
            markup += '<div class="modal-footer">';
            markup += '<a href="#" data-dismiss="modal" class="btn btn-small btn-blue ok">' + args["okButtonText"] + '</a>';
            markup += '</div>';
            markup += '</div>';
            $("body").append(markup);

            var theModal = $("#" + id);

            theModal.modal();
            theModal.on('hidden', function () {
                if (args["okCallBack"]) {
                    args["okCallBack"]();
                }
                $(this).remove();
            });
        };
    };

    var Confirm = function () {
        var self = this;
        self.show = function (message, args) {
            var defaultArgs = {
                title: "Confirm",
                message: message,
                okCallBack: null,
                cancelCallBack: null,
                okButtonText: "Ok",
                cancelButtonText: "Cancel",
                isMultiLine: false,
                placeholder: "input required..."
            };

            if (typeof args == "undefined") {
                args = defaultArgs;
            } else {
                for (var i in defaultArgs)
                    if (typeof args[i] === "undefined")
                        args[i] = defaultArgs[i];
            }

            var id = Errordite.Guid.newGuid();
            var markup = '<div id="' + id + '" class="modal hide fade" data-backdrop="static" data-keyboard="false">';
            markup += '<div class="modal-header">';
            markup += '<button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>';
            markup += '<h4 class="formodal">';
            markup += args["title"];
            markup += '</h4>';
            markup += '</div>';
            markup += '<div class="modal-body">';
            markup += '<p>' + args["message"] + '</p>';
            markup += '</div>';
            markup += '<div class="modal-footer">';
            markup += '<a href="#" data-dismiss="modal" class="btn btn-small btn-blue ok">' + args["okButtonText"] + '</a>';
            markup += '<a href="#" data-dismiss="modal" class="btn btn-small btn-grey cancel">' + args["cancelButtonText"] + '</a>';
            markup += '</div>';
            markup += '</div>';
            $("body").append(markup);

            var theModal = $("#" + id);

            theModal.find("a.ok").click(function () {
                if (args["okCallBack"]) {
                    args["okCallBack"]();
                }
            });

            theModal.find(".cancel, .close").click(function () {
                if (args["cancelCallBack"]) {
                    args["cancelCallBack"]();
                }
            });

            theModal.modal();
            theModal.on('hidden', function () {
                $(this).remove();
            });
        };
    };

    var Prompt = function () {
        var self = this;
        self.show = function (message, args) {
            var defaultArgs = {
                title: "Prompt",
                message: message,
                okCallBack: null,
                cancelCallBack: null,
                okButtonText: "Ok",
                cancelButtonText: "Cancel",
                isMultiLine: false,
                placeholder: "input required...",
                isMandatory: false,
                validationText: ""
            };

            if (typeof args == "undefined") {
                args = defaultArgs;
            } else {
                for (var i in defaultArgs)
                    if (typeof args[i] === "undefined")
                        args[i] = defaultArgs[i];
            }

            var id = Errordite.Guid.newGuid();
            var markup = '<div id="' + id + '" class="modal hide fade" data-backdrop="static" data-keyboard="false">';
            markup += '<div class="modal-header">';
            markup += '<button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>';
            markup += '<h4 class="formodal">';
            markup += args["title"];
            markup += '</h4>';
            markup += '</div>';
            markup += '<div class="modal-body">';
            if (args["isMandatory"]) {
                markup += '<div class="alert alert-error hidden">';
                markup += '<span>';
                markup += args["validationText"];
                markup += '</span>';
                markup += '</div>';
            }
            markup += '<label>' + args["message"] + '</label>';
            var require = args["isMandatory"] ? " required " : "";
            if (args["isMultiLine"]) {
                markup += '<textarea class="prompt-text" placeholder="' + args["placeholder"] + '"' + require + '></textarea>';
            } else {
                markup += '<input type="text" class="prompt-text" placeholder="' + args["placeholder"] + '"' + require + '></textarea>';
            }
            markup += '</div>';
            markup += '<div class="modal-footer">';
            markup += '<a href="#" data-dismiss="modal" class="btn btn-small btn-blue ok">' + args["okButtonText"] + '</a>';
            markup += '<a href="#" data-dismiss="modal" class="btn btn-small btn-grey cancel">' + args["cancelButtonText"] + '</a>';
            markup += '</div>';
            markup += '</div>';
            $("body").append(markup);

            var theModal = $("#" + id);

            theModal.find(".prompt-text").keypress(function (event) {
                if (event.which == 13) {
                    event.preventDefault();
                    theModal.find("a.ok").trigger('click');
                }
            });

            theModal.find("a.ok").click(function () {
                var text = theModal.find(".prompt-text").val();
                if (args["isMandatory"] && text == "") {
                    theModal.find(".alert").removeClass("hidden");
                } else {
                    if (args["okCallBack"]) {
                        args["okCallBack"](text);
                    }
                    theModal.modal('hide');
                }
            });

            theModal.find(".cancel, .close").click(function () {
                if (args["cancelCallBack"]) {
                    args["cancelCallBack"]();
                }
            });

            theModal.modal();
            theModal.on('hidden', function () {
                $(this).remove();
            });
        };
    };

    window.Errordite.Guid = new Guid();
    window.Errordite.Alert = new Alert();
    window.Errordite.Confirm = new Confirm();
    window.Errordite.Prompt = new Prompt();

})(jQuery);
