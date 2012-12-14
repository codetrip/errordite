
jQuery -> 
	
	class Dashboard
		
		constructor: () ->
			console.log "init"
			this.issueContainer = $ 'div#issues'
			this.errorContainer = $ 'div#errors'
			this.lastPolled = new Date()
		poll: ->
			console.log "polling"
			date = dashboard.lastPolled
			dashboard.lastPolled = new Date()
			console.log date
			$.ajax
				url: "/dashboard/update?lastUpdated=" + date.toLocaleDateString() + ' ' + date.getUTCHours() + ':' + date.getUTCMinutes() + ':' + date.getUTCSeconds()
				success: (result) ->
					console.log "success"
					if result.success
						dashboard.bind(result.data)
					else
						dashboard.error()
				error: ->
					$this.error()
				dataType: "json"
				complete: dashboard.poll
				timeout: 10000
		bind: (data) ->
			console.log "binding"
			for i in data.issues
				dashboard.issueContainer.prepend i
			for e in data.errors
				dashboard.errorContainer.prepend e	
		error: -> 
			console.log "error"

#	dashboard = new Dashboard();
#	setTimeout dashboard.poll(), 10000