jQuery -> 
	$body = $('div#issues');
	$activeModal = null

	if $body.length > 0

		init = new Initalisation()
		init.datepicker($body)

		$body.delegate 'form#actionForm', 'submit', (e) -> 
			e.preventDefault();
			$this = $ this

			$.post($this.attr('action'), $this.serialize(), (data) -> 
				window.location.reload();
			).error((e) -> 
				if $activeModal?
					$activeModal.find('div.alert').removeClass('hidden');
					$activeModal.find("div.alert h4").text "An error occurred, please close the modal window and try again."
			)
			
		$body.delegate 'div.dropdown-small ul.dropdown-menu li input', 'click', (e) -> 
			e.stopPropagation()

		$body.delegate 'div.dropdown-small ul.dropdown-menu li a', 'click', (e) -> 
			e.preventDefault()
			$(this).closest('ul').find('li :checkbox').prop('checked', true)

		$body.delegate 'div.dropdown-small ul.dropdown-menu li', 'click', (e) ->
			$this = $ this
			$chk = $this.closest('li').children('input');
			$chk.attr('checked', !$chk.attr('checked'));
			false

		$body.delegate 'div.action ul.dropdown-menu li a', 'click', () -> 
			$this = $ this
			$modal = $body.find('div#' + $this.attr('data-val-modal'))
			return null if $modal == null

			$body.find('input[type="hidden"]#Action').val($modal.attr("id"))

			$modal.find('.batch-issue-count').text $(':checkbox:checked[name=issueIds]').length
			$modal.find('.batch-issue-plural').toggle $(':checkbox:checked[name=issueIds]').length > 1
			
			if $modal.find('.batch-issue-status').length > 0
				$modal.find('.batch-issue-status').text $this.attr('data-val-status')
				$body.find('input[type="hidden"]#Status').val($this.attr('data-val-status').replace(' ', ''))

			$activeModal = $modal;
			$modal.modal()
		
		$('th :checkbox').on 'click', () -> 
			$(this).closest('table').find('td :checkbox').prop('checked', $(this).is(':checked'))
			maybeEnableBatchStatus()

		maybeEnableBatchStatus = () -> 
			$('div.action').toggle !!$(':checkbox:checked[name=issueIds]').length
		
		$body.delegate ':checkbox[name=issueIds]', 'click', () -> maybeEnableBatchStatus()			

		maybeEnableBatchStatus()
