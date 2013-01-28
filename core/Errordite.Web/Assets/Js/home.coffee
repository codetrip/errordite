
jQuery -> 
	$body = $('section#home');

	if $body.length > 0
		$body.delegate 'a.showplayer', 'click', () -> 
			$this = $ this
			$preview = $this.closest('div');
			$video = $preview.next('div.player');
			false
	