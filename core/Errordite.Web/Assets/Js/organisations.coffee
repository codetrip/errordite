jQuery -> 
	$body = $('div#organisations');

	if $body.length > 0

		$body.delegate 'form#suspendForm', 'submit', (e) -> 
			e.preventDefault()
			$this = $ this

			$.post $this.attr('action'), $this.serialize(), (data) -> 
				window.location.reload()

		$body.delegate 'a.suspend', 'click', (e) -> 
			e.preventDefault()
			$this = $ this
			$modal = $body.find('div#suspend-modal')
			return null if $modal == null
			$modal.find('input[type=hidden]').val $this.data 'val'
			$modal.modal()

		$body.delegate 'input[type=submit].activate', 'click', (e) -> 
			$this = $ this

			if confirm "are you sure you want to activate this organisation?"
				return true

			e.preventDefault()
			false

		$body.delegate 'input[type=submit].delete', 'click', (e) -> 
			$this = $ this

			if confirm "are you sure you want to delete this organisation, all data will be permenantly deleted?"
				return true

			e.preventDefault()
			false
			