
(function ($) {
    function Loader(target, p1) {
        if (target) {
            var image;
            if (typeof p1 == "string") {
                image = (p1 == "") ? "../images/zoomloader.gif" : p1;
            }
            else image = "../images/zoomloader.gif";
            var jNode = $('<div style="-moz-opacity:0.8;opacity:0.8;filter:alpha(opacity=80);color:#333;font-size:12px;font-family:Tahoma;'
				+ 'text-align:center;background-image:url(' + image + ');position:absolute;'
				+ 'background-repeat:no-repeat;background-position:43px 30px;width:100px;height:55px;z-index:999;'
				+ '">Loading</div>').appendTo("body");
            this.target = $(target).data("Loader", this).load(function () {
                jNode.remove();
                $(this).data("Loader").callBack();
            });
            var t = parseInt(this.target.offset().top + (this.target.height() - jNode.height()) / 2);
            var l = parseInt(this.target.offset().left + (this.target.width() - jNode.width()) / 2);
            this.jNode = jNode.css({ top: t, left: l });
        }
    }

    Loader.prototype.callBack = function() {
        if (typeof this.callBack == "function") this.callBack();
    };
    Loader.prototype.load = function (href, callBack) {
        var cb = this.callBack = callBack, jNode = this.jNode;
        if (this.target[0].nodeName.toLowerCase() == "img") this.target.attr("src", href);
        else
            this.target.load(href, function () {
                jNode.remove();
                if (typeof cb == "function") cb();
            });
    };
    $.fn.addLoader = function (image, callBack) { new Loader(this, image, callBack); };
    $.fn.useLoader = function (href, callBack) { this.data("Loader").load(href, callBack); };
})(jQuery);
