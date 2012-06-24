
jQuery -> 
	$body = $('div#dashboard-errors');

	if $body.length > 0
		$body.delegate('select#ApplicationId', 'change', () -> 
			$this = $ this
			if $this.val() != ''
				window.location = $this.val();
			false	
		)	

	