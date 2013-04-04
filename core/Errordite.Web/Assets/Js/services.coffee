
jQuery -> 

	$root = $("section#services")

	if $root.length > 0	
		
		$root.delegate "a.purge", "click", (e) ->
			e.preventDefault()
			$this = $(this)
			return false  unless confirm("Are you sure you want to delete all messages?")
			serviceManager.deleteMessages $this.data("queue"), $this.data("service")

		$root.delegate "a.retry", "click", (e) ->
			e.preventDefault()
			$this = $(this)
			return false  unless confirm("Are you sure you want to retry all messages?")
			serviceManager.returnToSource $this.data("queue"), $this.data("service")

		$root.delegate "select#RavenInstanceId", "change", (e) ->
			e.preventDefault()
			$this = $(this)
			serviceManager.switchInstance $this.val()

		class ServiceManager

			deleteMessages: ->
				deleteMessages = (queueName, serviceName) ->
				$.ajax
					url: "/system/services/deletemessages"
					data:
						instanceId: $('select#RavenInstanceId').val()
						queueName: queueName
						serviceName: serviceName

					success: (result) ->
						if result.Success
							location.reload()
						else
							alert "Failed to delete messages, please try again."
						true

					error: ->
						alert "Failed to delete messages, please try again."

					dataType: "json"
					type: "POST"

				true
			returnToSource: (queueName, serviceName) ->
				console.log 'instance: ' + $('select#RavenInstanceId').val()
				console.log 'queue:' + queueName
				console.log 'service: ' + serviceName
				$.ajax
					url: "/system/services/returntosource"
					data:
						instanceId: $('select#RavenInstanceId').val()
						queueName: queueName
						serviceName: serviceName

					success: (result) ->
						if result.Success
							location.reload()
						else
							alert "Failed to return error messages to their source queue, please try again."
						true

					error: ->
						alert "Failed to return error messages to their source queue, please try again."

					dataType: "json"
					type: "POST"

				true
			reload: () ->
				window.location = "/system/services?ravenInstanceId=" + $('select#RavenInstanceId').val()
				true
			switchInstance: (instanceId) -> 
				window.location = "/system/services?ravenInstanceId=" + instanceId
				true

		serviceManager = new ServiceManager();
		true