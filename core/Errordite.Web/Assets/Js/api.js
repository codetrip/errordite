(function() {

  jQuery(function() {
    var $body;
    $body = $('section#api');
    if ($body.length > 0) {
      $body.delegate('a#showplayer', 'click', function() {
        var $this;
        $this = $(this);
        return false;
      });
      return $body.delegate('a#hideplayer', 'click', function() {
        var $this;
        $this = $(this);
        return false;
      });
    }
  });

}).call(this);
