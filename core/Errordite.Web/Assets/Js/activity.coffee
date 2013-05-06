
jQuery -> 

	$root = $('section#activity')

	if $root.length > 0	

		$root.delegate 'button#activity-load-more', 'click', (e) -> 
			e.preventDefault()
			activity.loadModeItems()
			false

		class Activity
		
			constructor: () ->
				this.nextPage = 2
				this.table = $root.find('table.history tbody')
			loadModeItems: ->
				$.ajax
					url: "/dashboard/getnextactivitypage?pagenumber=" + activity.nextPage
					success: (data) ->
						activity.nextPage++
						activity.table.append(data, {})
					error: (e) ->
						console.log(e)
						Errordite.Alert.show('Something went wrong getting the next page, please try again.')
				true

		activity = new Activity();
		true