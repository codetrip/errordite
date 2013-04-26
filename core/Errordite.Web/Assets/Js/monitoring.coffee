
jQuery -> 

	$root = $("section#monitoring")

	if $root.length > 0	
		
		$root.delegate "ul.dropdown-menu li a", "click", (e) ->
			e.preventDefault()
			$this = $(this)
			Errordite.Confirm.show($this.data('check'), 
				okCallBack: () -> 
					monitoring.performAction $this.data('action')
			)

		class Monitoring

			performAction: (action)->
				$root.find('input#OrgId').val($root.find('input#OrganisationId').val())
				$root.find('input#Svc').val($root.find('select#Service').val())

				$form = $root.find('form#actionForm');
				$form.attr('action', $form.attr('action').replace('delete', action))
				$form.submit()
				true

		monitoring = new Monitoring()
		true