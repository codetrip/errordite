
jQuery -> 
	$body = $('section#users');

	if $body.length > 0
		user = null;

		$body.delegate('a.delete', 'click', () -> 
			$this = $ this
			this.user = new User $this.closest('form')
			this.user.delete()
			false	
		)	

		$body.delegate('a.invite', 'click', () -> 
			$this = $ this
			$this.closest('form').submit()
			false	
		)	

		class User
			constructor: ($form) -> 
				this.$form = $form

			delete: () -> 
				if window.confirm "Are you sure you want to delete this user, any issues assigned to this user will be assigned to you!"
					this.$form.submit();

	