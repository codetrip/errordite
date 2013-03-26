(function() {

  jQuery(function() {
    var $body, hiddenPanelCss;
    $body = $('section#home');
    hiddenPanelCss = null;
    if ($body.length > 0) {
      $body.delegate('a#showplayer', 'click', function() {
        var $panel, $player, $preview, $this;
        $this = $(this);
        $panel = $this.closest('div.video-panel');
        $preview = $this.closest('div.preview');
        $player = $panel.find('div.player');
        $preview.hide();
        $player.show();
        hiddenPanelCss = {
          height: $panel.css('height'),
          width: $panel.css('width'),
          'margin-left': $panel.css('margin-left')
        };
        $panel.animate({
          height: "635px",
          width: "974px",
          "margin-left": "0"
        }, 500, function() {
          $player.find('iframe').show();
          $player.find('div.hide-button').show();
          if (typeof homepageVideoPlayer !== "undefined" && homepageVideoPlayer !== null) {
            return homepageVideoPlayer.playVideo();
          }
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
        $panel.animate(hiddenPanelCss, 500, function() {
          return $preview.show();
        });
        return false;
      });
    }
  });

}).call(this);
