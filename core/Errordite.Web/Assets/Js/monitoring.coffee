
jQuery -> 

	$root = $("section#monitoring")

	if $root.length > 0	

		$('th :checkbox').on 'click', () -> 
			$(this).closest('table').find('td :checkbox').prop('checked', $(this).is(':checked'))
			monitoring.maybeEnableActions()
		
		$root.delegate "a.purge", "click", (e) ->
			e.preventDefault()
			$this = $(this)
			return false  unless confirm("Are you sure you want to delete all messages?")
			serviceManager.deleteMessages $this.data("service")

		$root.delegate "a.retry", "click", (e) ->
			e.preventDefault()
			$this = $(this)
			return false  unless confirm("Are you sure you want to retry all messages?")
			serviceManager.returnToSource $this.data("service")

		class Monitoring

			deleteMessages: ->
				deleteMessages = (serviceName) ->
				$.ajax
					url: "/system/services/deletemessages"
					data:
						instanceId: $('select#RavenInstanceId').val()
						serviceName: serviceName

					success: (result) ->
						if result.success
							location.reload()
						else
							return Errordite.Alert.show("Failed to delete messages, please try again.")
						true

					error: ->
						alert "Failed to delete messages, please try again."

					dataType: "json"
					type: "POST"

				true
			retryMessages: (serviceName) ->

				$.ajax
					url: "/system/services/returntosource"
					data:
						instanceId: $('select#RavenInstanceId').val()
						serviceName: serviceName

					success: (result) ->
						if result.success
							location.reload()
						else
							Errordite.Alert.show("Failed to return error messages to their source queue, please try again.")
						true

					error: ->
						Errordite.Alert.show("Failed to return error messages to their source queue, please try again.")

					dataType: "json"
					type: "POST"

				true
			maybeEnableActions: () ->
				$('ul#action-list').toggle !!$(':checkbox:checked[name=envelopes]').length
				true

		monitoring = new Monitoring()
		monitoring.maybeEnableActions()
		true