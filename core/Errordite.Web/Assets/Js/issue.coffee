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
						
		renderHistory = () -> 
			$node = $issue.find('#history-items')
			url = '/issue/history?IssueId=' + $issue.find('#IssueId').val()
			
			$.get url,
				(data) -> 
					$node.html(data.data)
					$('div.content').animate 
						scrollTop : 0,
						'slow'			
		
		loadTabData($ 'ul#issue-tabs li.active a.tablink')

		$issue.delegate 'form#reportform', 'submit', (e) ->
			e.preventDefault()
			renderReports()

		$issue.delegate 'input[type="button"].confirm', 'click', () ->
			$this = $ this
			if confirm "Are you sure you want to delete all errors associated with this issue?" 
				$.post '/issue/purge', 'issueId=' + $this.attr('data-val'), (data) -> 
					clearErrors()
					$('span#instance-count').text "0"

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
					alert 'Error. Please try again.'
		


		$issue.delegate 'select#Status', 'change', () -> 
			$this = $ this

			if $this.val() == 'Ignorable'
				$issue.find('li.checkbox').removeClass('hidden');
			else
				$issue.find('li.checkbox').addClass('hidden');		

		if $issue.find('select#Status').val() == 'Ignorable'
			$issue.find('li.checkbox').removeClass('hidden')

		$('#issue-tabs .tablink').bind 'shown', (e) -> 
			loadTabData $ e.currentTarget
			
				
			

