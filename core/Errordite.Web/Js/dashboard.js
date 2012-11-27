(function() {
  jQuery(function() {
    var $body;
    $body = $('div#dashboard-errors');
    if ($body.length > 0) {
      return $body.delegate('select#ApplicationId', 'change', function() {
        var $this;
        $this = $(this);
        if ($this.val() !== '') {
          window.location = $this.val();
        }
        return false;
      });
    }
  });
}).call(this);
