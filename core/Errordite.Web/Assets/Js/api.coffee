
jQuery -> 
	$body = $('section#api');

	if $body.length > 0
		$body.delegate 'a#showplayer', 'click', () -> 
			$this = $ this
			false

		$body.delegate 'a#hideplayer', 'click', () -> 
			$this = $ this
			false
	