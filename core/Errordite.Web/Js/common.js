
$(function () {

    var $filter = $('.filter-link');

    if ($filter.length > 0) {

        $filter.each(function () {
            var $this = $(this);

            $this.toggle(function () {
                $this.find('ul').show();
            }, function () {
                $this.find('ul').hide();
            });

            $this.find('ul li a').click(function () {
                var href = $(this).attr("href");
                if (href === '#') {
                    $this.find('#filter-top-link').text($(this).text());
                    $('#' + $(this).attr("data-fld")).val($(this).attr("data-val"));
                } else {
                    window.location = $(this).attr("href");
                }
            });
        });
    }
    
    var $cache = $('div#caching');

    if ($cache.length > 0) {
        $cache.delegate('select#CacheEngine', 'change', function () {
            var $this = $(this);
            var index = window.location.href.indexOf('?');
            if (index === -1)
                window.location = window.location.href + '?engine=' + $this.val();
            else
                window.location = window.location.href.substring(0, index) + '?engine=' + $this.val();
        });
    }

});


