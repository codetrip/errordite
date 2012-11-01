(function() {

  jQuery(function() {
    var $body;
    $body = $('div#audit');
    if ($body.length > 0) {
      return $body.find('input.daterangepicker').daterangepicker({
        dateFormat: 'D M d, yy'
      });
    }
  });

}).call(this);
