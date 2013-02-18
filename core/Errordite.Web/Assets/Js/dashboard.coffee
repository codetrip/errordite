
jQuery -> 

	$root = $('section#dashboard')

	if $root.length > 0	
		class Dashboard
		
			constructor: () ->
				this.issueContainer = $ 'div#issues'
				this.errorContainer = $ 'div#errors'
				this.lastError = $('input#LastErrorDisplayed').val()
				this.lastIssue = $('input#LastIssueDisplayed').val()
				this.showItems()
			poll: ->
				$.ajax
					url: "/dashboard/update?lastErrorDisplayed=" + dashboard.lastError + '&lastIssueDisplayed=' + dashboard.lastIssue + '&applicationId=' + $('input#ApplicationId').val()
					success: (result) ->
						console.log "success"
						if result.success
							dashboard.bind(result.data)
						else
							dashboard.error()
					error: ->
						dashboard.error()
					dataType: "json"
					complete: ->
						setTimeout dashboard.poll, 10000
				true
			rendergraph: ->
				$.ajax
					url: "/dashboard/getgraphdata?applicationId=" + $('input#ApplicationId').val()
					success: (data) ->
						$.jqplot 'date-graph', 
							[_.zip data.x, data.y],
							seriesDefaults:
								renderer:$.jqplot.LineRenderer
							axes:
								xaxis:
									renderer: $.jqplot.DateAxisRenderer
									tickOptions:
										formatString:'%a %#d %b'
								yaxis:
									min: 0
									tickInterval: if (_.max data.y > 3) then null else 1									
										
							highlighter:
								show: true
								sizeAdjust: 7.5
					error: ->
						dashboard.error()
					dataType: "json"
				true
			bind: (data) ->
				console.log "binding"
				for i in data.issues
					dashboard.issueContainer.prepend i
				for e in data.errors
					dashboard.errorContainer.prepend e	

				dashboard.lastError = data.lastErrorDisplayed
				dashboard.lastIssue = data.lastIssueDisplayed
				dashboard.showItems()
				true
			error: -> 
				console.log "error"
				true
			showItems: ->
				this.issueContainer.find('div.boxed-item:hidden').show('slow')
				this.errorContainer.find('div.boxed-item:hidden').show('slow')
				this.purgeItems this.issueContainer
				this.purgeItems this.errorContainer
			purgeItems: ($container) ->
				count = $container.find(' > div').length
				while count > 100
					$container.find(' > div:last-child').remove()
					count = $container.find(' > div').length

		dashboard = new Dashboard();
		dashboard.rendergraph();

		setTimeout dashboard.poll, 10000
		true