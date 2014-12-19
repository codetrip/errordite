
jQuery -> 
	$root = $ 'section#test'

	if $root.length > 0
		$root.delegate 'select#ErrorId', 'change', () -> 
			$this = $ this
			$.ajax
				url: "/test/getjson?errorId=" + $this.val() + '&token=' + $("select#Token").val()
				success: (data) ->
					$("#Json").val(data)
				error: (e) ->
					console.log(e)
					Errordite.Alert.show('Something went wrong getting the error template, please try again.')

			false