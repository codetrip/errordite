jQuery -> 
	$orgroot = $('section#organisations');
	$cacheroot = $('section#caching');

	if $cacheroot.length > 0
		$cacheroot.delegate "select#CacheEngine", "change", ->
			$this = $(this)
			index = window.location.href.indexOf("?")

			if index is -1
				window.location = window.location.href + "?engine=" + $this.val()
			else
				window.location = window.location.href.substring(0, index) + "?engine=" + $this.val()


	if $orgroot.length > 0

		$orgroot.delegate 'form#suspendForm', 'submit', (e) -> 
			e.preventDefault()
			$this = $ this

			$.post $this.attr('action'), $this.serialize(), (data) -> 
				window.location.reload()

		$orgroot.delegate 'a.suspend', 'click', (e) -> 
			e.preventDefault()
			$this = $ this
			$modal = $orgroot.find('div#suspend-modal')
			return null if $modal == null
			$modal.find('input[type=hidden]').val $this.data 'val'
			$modal.modal()

		$orgroot.delegate 'a.activate', 'click', (e) -> 
			$this = $ this

			if confirm "are you sure you want to activate this organisation?"
				$form = $this.closest('form')
				$form.submit();

			e.preventDefault()
			false

		$orgroot.delegate 'a.delete', 'click', (e) -> 
			$this = $ this

			if confirm "are you sure you want to delete this organisation, all data will be permenantly deleted?"
				$form = $this.closest('form')
				$form.submit();

			e.preventDefault()
			false
			