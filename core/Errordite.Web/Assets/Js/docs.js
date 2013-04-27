(function() {

  jQuery(function() {
    var $body;
    $body = $('section#clients');
    if ($body.length > 0) {
      return $body.delegate('[data-toggle="tab"]', 'click', function(e) {
        var $container, $this;
        e.preventDefault();
        $this = $(this);
        $container = $this.closest('.sidenav');
        $container.find('div.sidenav-tab').removeClass('active');
        $container.find('li.active').removeClass('active');
        $container.find('div.sidenav-tab#' + $this.data('tab')).addClass('active');
        $this.closest('li').addClass('active');
        return false;
      });
    }
  });

}).call(this);
