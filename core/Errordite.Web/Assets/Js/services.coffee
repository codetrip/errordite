
jQuery -> 

	$root = $("section#services")

	if $root.length > 0	
		
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

		$root.delegate "select#RavenInstanceId", "change", (e) ->
			e.preventDefault()
			$this = $(this)
			serviceManager.switchInstance $this.val()

		$root.delegate "a#refresh", "click", (e) ->
			e.preventDefault()
			serviceManager.reload()

		$root.delegate 'a.start-service', 'click', (e) ->
			e.preventDefault();
			$this = $ this;
			return serviceManager.serviceControl $this.data('service'),  $this.data('start');

		class ServiceManager

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
							alert "Failed to delete messages, please try again."
						true

					error: ->
						alert "Failed to delete messages, please try again."

					dataType: "json"
					type: "POST"

				true
			returnToSource: (serviceName) ->

				$.ajax
					url: "/system/services/returntosource"
					data:
						instanceId: $('select#RavenInstanceId').val()
						serviceName: serviceName

					success: (result) ->
						if result.success
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
			serviceControl: (serviceName, start) ->
				$.ajax
					url: "/system/services/servicecontrol"
					data:
						serviceName:serviceName
						start:start
					success: (result) ->
						if (result.success)
							return location.reload()
						else
							alert "Service failed to " + (start ? "start" : "stop") + ", please try again."
					
						true
					error: -> 
						alert "Service failed to " + (start ? "start" : "stop") + ", please try again."
					dataType: "json"
					type: "POST"

				
				true

		serviceManager = new ServiceManager();
		true