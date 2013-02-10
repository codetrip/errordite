jQuery -> 
	$issue = $('section#issue');

	if $issue.length > 0

		setReferenceLink = ->
			input = $(':input[name=Reference]')
			reference = input.val()
			$('#reference-link').empty()
			$('#reference-link').html($('<a>')
				.attr('href', reference)
				.attr('target', '_blank')
				.text('link')) if /^https?:\/\//.test(reference)			
		
		loadTabData = ($tab) ->
			if not $tab.data 'loaded'
				if $tab.data("val") == "reports"				
					renderReports()
				else if $tab.data("val") == "errors"
					renderErrors()
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
						highlighter:
							show: true
							sizeAdjust: 7.5

		renderErrors = () -> 
			$node = $issue.find('#error-items')
			url = '/issue/errors?IssueId=' + $issue.find('#IssueId').val()
			
			$.get url,
				(data) -> 
					$node.html(data)
					$('div.content').animate 
						scrollTop : 0,
						'slow'		
		
		loadTabData($ 'ul#issue-tabs li.active a.tablink')
			
		setReferenceLink()

		$issue.delegate 'form#reportform', 'submit', (e) ->
			e.preventDefault()
			renderReports()

		$issue.delegate ':input[name=Reference]', 'change', setReferenceLink

		$issue.delegate 'input[type="button"].confirm', 'click', () ->
			$this = $ this
			if confirm "Are you sure you want to delete all errors associated with this issue?" 
				$.post '/issue/purge', 'issueId=' + $this.attr('data-val'), (data) -> 
					renderErrors()
					$('span#instance-count').text "0"

		$issue.delegate 'form#errorsForm', 'submit', (e) ->
			e.preventDefault()
			$this = $ this
			renderErrors()

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

		$issue.delegate '.sort a[data-pgst]', 'click', (e) -> 
			e.preventDefault()
			$this = $ this
			$('#pgst').val $this.data('pgst')
			$('#pgsd').val $this.data('pgsd')
			renderErrors()
			false

		$issue.delegate '#apply-rules-confirmation input[name="WhatIf"]', 'click', (e) -> 
			e.preventDefault()
			$(this).closest('form').ajaxSubmit
				data:
					WhatIf: true
				success: (response) ->
					alert response.message
				
			

