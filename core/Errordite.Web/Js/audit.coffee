jQuery -> 
	$body = $('div#audit');

	if $body.length > 0	
		$body.find('input.daterangepicker').daterangepicker({
			dateFormat: 'D M d, yy',
		});

		
				