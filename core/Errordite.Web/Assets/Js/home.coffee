
jQuery -> 
	$body = $('section#home');

	if $body.length > 0
		$body.delegate 'a#showplayer', 'click', () -> 
			$this = $ this
			$panel = $this.closest('div.video-panel')
			$preview = $this.closest('div.preview')
			$player = $panel.find('div.player')

			$preview.hide()
			$player.show()

			$panel.animate
				height: "635px"
			, 500, ->
				$player.find('iframe').show()
				$player.find('div.hide-button').show()
				homepageVideoPlayer.playVideo() if homepageVideoPlayer?

			false

		$body.delegate 'a#hideplayer', 'click', () -> 
			$this = $ this
			$panel = $this.closest('div.video-panel')
			$preview = $panel.find('div.preview')
			$player = $this.closest('div.player')

			$player.find('iframe').hide()
			$player.find('div.hide-button').hide()
			$player.hide()

			$panel.animate
				height: "135px"
			, 500, ->
				$preview.show()

			false
	