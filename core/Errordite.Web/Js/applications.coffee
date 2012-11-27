
jQuery -> 
	$body = $('div#applications');

	if $body.length > 0
		application = null;

		$body.delegate 'a.delete-application', 'click', (e) -> 
			$this = $ this
			application = new Application $this.closest('tr')
			application.delete()
			e.preventDefault()
		
		$body.delegate 'a.generate-error', 'click', (e) -> 
			$this = $ this
			new Application($this.closest('tr')).generateError()
			e.preventDefault()

		class Application
			constructor: ($appEl) -> 
				this.$appEl = $appEl

			delete: () -> 
				if window.confirm "Are you sure you want to delete this application, all associated errors will be deleted?"
					this.$appEl.find('form:has(.delete-application)').submit()

			generateError: () -> 
				this.$appEl.find('form:has(.generate-error)').submit()				

	