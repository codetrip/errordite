(function() {

  jQuery(function() {
    var $body;
    $body = $('section#home');
    if ($body.length > 0) {
      return $body.delegate('a.showplayer', 'click', function() {
        var $preview, $this, $video;
        $this = $(this);
        $preview = $this.closest('div');
        $video = $preview.next('div.player');
        return false;
      });
    }
  });

}).call(this);
