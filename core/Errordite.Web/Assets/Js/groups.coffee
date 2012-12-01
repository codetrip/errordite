
jQuery -> 
	$body = $('section#groups');

	if $body.length > 0
		group = null;

		$body.delegate('a.delete', 'click', () -> 
			$this = $ this
			this.group = new Group $('form#deleteGroup'), $this.data('val');
			this.group.delete()
			false	
		)	

		class Group
			constructor: ($form, groupId) -> 
				this.$form = $form
				this.groupId = groupId

			delete: () -> 
				$form = this.$form
				groupId = this.groupId
				if window.confirm "Are you sure you want to delete this group? " + groupId
					$('input#GroupId').val(groupId)
					$form.submit();