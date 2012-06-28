
jQuery -> 
	$body = $('div#groups');

	if $body.length > 0
		group = null;

		$body.delegate('a.delete', 'click', () -> 
			$this = $ this
			this.group = new Group $this.closest('tr')
			this.group.delete()
			false	
		)	

		class Group
			constructor: ($appEl) -> 
				this.$appEl = $appEl

			delete: () -> 
				$appEl = this.$appEl
				if window.confirm "Are you sure you want to delete this group?"
					$appEl.prev('form').submit();

	