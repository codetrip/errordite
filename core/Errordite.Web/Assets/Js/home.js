(function() {

  jQuery(function() {
    var $body;
    $body = $('section#home');
    if ($body.length > 0) {
      $body.delegate('a#showplayer', 'click', function() {
        var $panel, $player, $preview, $this;
        $this = $(this);
        $panel = $this.closest('div.video-panel');
        $preview = $this.closest('div.preview');
        $player = $panel.find('div.player');
        $preview.hide();
        $player.show();
        $panel.animate({
          height: "635px"
        }, 500, function() {
          $player.find('iframe').show();
          return $player.find('div.hide-button').show();
        });
        return false;
      });
      return $body.delegate('a#hideplayer', 'click', function() {
        var $panel, $player, $preview, $this;
        $this = $(this);
        $panel = $this.closest('div.video-panel');
        $preview = $panel.find('div.preview');
        $player = $this.closest('div.player');
        $player.find('iframe').hide();
        $player.find('div.hide-button').hide();
        $player.hide();
        $panel.animate({
          height: "135px"
        }, 500, function() {
          return $preview.show();
        });
        return false;
      });
    }
  });

}).call(this);
