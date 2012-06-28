
jQuery -> 
	$body = $('div#users');

	if $body.length > 0
		user = null;

		$body.delegate('a.delete', 'click', () -> 
			$this = $ this
			this.user = new User $this.closest('tr')
			this.user.delete()
			false	
		)	

		class User
			constructor: ($appEl) -> 
				this.$appEl = $appEl

			delete: () -> 
				$appEl = this.$appEl
				if window.confirm "Are you sure you want to delete this user, any issues assigned to this user will be assigned to you!"
					$appEl.prev('form').submit();

	