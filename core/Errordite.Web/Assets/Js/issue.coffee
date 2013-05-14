jQuery -> 
	$issue = $('section#issue');

	if $issue.length > 0

		paging = new window.Paging('/issue/errors?Id=' + $issue.find('#IssueId').val() + '&')
		paging.init()
		
		loadTabData = ($tab) ->
			if not $tab.data 'loaded'
				if $tab.data("val") == "reports"				
					renderReports()
				else if $tab.data("val") == "history"
					renderHistory()
				$tab.data 'loaded', true	

		renderReports = () -> 
			$('div#date-graph').empty()
			$('div#hour-graph').empty()

			$.get "/issue/getreportdata?issueId=" + $issue.find('input#IssueId').val() + '&dateRange=' + $issue.find('input#DateRange').val(),
				(d) -> 										
					$.jqplot 'hour-graph', [d.ByHour.y], 
						seriesDefaults: 
							renderer:$.jqplot.BarRenderer						
						axes: 
							xaxis: 
								renderer: $.jqplot.CategoryAxisRenderer
								ticks: d.ByHour.x							
							yaxis:
								min: 0
								tickInterval: if (_.max d.ByHour.y) > 3 then null else 1
									
					$.jqplot 'date-graph', 
						[_.zip d.ByDate.x, d.ByDate.y],
						seriesDefaults:
							renderer:$.jqplot.LineRenderer
						axes:
							xaxis:
								renderer: $.jqplot.DateAxisRenderer
								tickOptions:
									formatString:'%a %#d %b'
							yaxis:
								min: 0
								tickInterval: if (_.max d.ByDate.y) > 3 then null else 1

						highlighter:
							show: true
							sizeAdjust: 7.5

		clearErrors = () ->
			$('div#error-items').clear();

		setStatus = (status) ->
			$.post('/issue/setstatus', {issueId: $issue.find('input#IssueId').val(), status:status}, (data) -> 
				if data.redirect != ''
					window.location.href = data.redirect
				else if data.success
					$issue.find('div#status').text(status)
					Errordite.Notification.show(data.message)
					$issue.find('select#Status').val(status)

					switch status
						when 'Solved'
							$issue.find('li#solved').hide()
							$issue.find('li#ignored').show()
							$issue.find('li#acknowledged').hide()
							$issue.find('li#fixready').hide()
						when 'Acknowledged'
							$issue.find('li#acknowledged').hide()
							$issue.find('li#ignored').show()
							$issue.find('li#solved').show()
							$issue.find('li#fixready').show()
						when 'FixReady'
							$issue.find('li#acknowledged').hide()
							$issue.find('li#fixready').hide()
							$issue.find('li#ignored').show()
							$issue.find('li#solved').show()
						when 'Ignored'
							$issue.find('li#acknowledged').hide()
							$issue.find('li#fixready').hide()
							$issue.find('li#ignored').hide()
							$issue.find('li#solved').show()
				else
					Errordite.Alert.show('An error occured while trying to update the issue, please try again')
			).error((e) -> 
				Errordite.Alert.show('An error occured while trying to update the issue, please try again')
			)	
						
		renderHistory = () -> 
			$node = $issue.find('table.history tbody')
			url = '/issue/history?IssueId=' + $issue.find('#IssueId').val()
			
			$.get url,
				(data) -> 
					$node.append(data.data)
					$('div.content').animate 
						scrollTop : 0,
						'slow'			
		
		loadTabData($ 'ul#issue-tabs li.active a.tablink')

		$issue.delegate 'form#reportform', 'submit', (e) ->
		$issue.delegate 'form#reportform', 'submit', (e) ->
			e.preventDefault()
			renderReports()

		$issue.delegate '.what-if-reprocess', 'click', (e) ->
			e.preventDefault()
			$(this).closest('form').ajaxSubmit
				data:
					WhatIf: true
				success: (data) ->
					$('.reprocess-what-if-msg').remove()
					msg = $('<span/>').addClass('reprocess-what-if-msg').html(data)
					$(e.currentTarget).after msg
#					setTimeout -> msg.fadeOut(500), 
#					5000 
				error: ->
					Errordite.Alert.show('An error has occured, please try again.')

		$issue.delegate 'ul#action-list a.action', 'click', (e) ->
			e.preventDefault()
			$this = $ this
			setStatus($this.data('val'))

		$issue.delegate 'ul#action-list a.form', 'click', (e) ->
			e.preventDefault()
			$this = $ this
			Errordite.Confirm.show($this.data('confirmtext'), { okCallBack: () -> 
				$this.closest('form').submit();
				true
			}, cancelCallBack: () -> 
				false
			)

		$issue.delegate 'select#Status', 'change', () -> 
			$this = $ this

			if $this.val() == 'Ignored'
				$issue.find('li.inline').removeClass('hidden');
			else
				$issue.find('li.inline').addClass('hidden');		

		if $issue.find('select#Status').val() == 'Ignored'
			$issue.find('li.inline').removeClass('hidden')

		$('#issue-tabs .tablink').bind 'shown', (e) -> 
			loadTabData $ e.currentTarget
			
				
			

