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

			$.get "/issue/getreportdata?issueId=" + $issue.find('input#IssueId').val() + '&dateRange=' + $issue.find('input#DateRange').val() + '&token=' + $issue.find('input#Token').val(),
				(d) ->
					writeDateChart d.ByDate
					writeHourChart d.ByHour

		writeDateChart = (data) ->

			chartdata = []
			i = 0

			while i < data.x.length
				chartdata.push
					x: data.x[i]
					y: data.y[i]
				i++

			chart = new AmCharts.AmSerialChart()
			chart.pathToImages = "http://www.amcharts.com/lib/images/"
			chart.autoMarginOffset = 3
			chart.marginRight = 15

			chart.dataProvider = chartdata
			chart.categoryField = "x"

			categoryAxis = chart.categoryAxis
			categoryAxis.gridAlpha = 0.07
			categoryAxis.axisColor = "#DADADA"
			categoryAxis.showFirstLabel = true
			categoryAxis.showLastLabel = true
			categoryAxis.startOnAxis = true
			categoryAxis.parseDates = true
			categoryAxis.equalSpacing = true
			categoryAxis.minPeriod = "DD"

			valueAxis = new AmCharts.ValueAxis()
			valueAxis.stackType = "3d";
			valueAxis.gridAlpha = 0.07
			valueAxis.dashLength = 5;

			guide = new AmCharts.Guide();
			guide.value = 0;
			guide.toValue = 1000000;
			guide.fillColor = "#d7e5ee";
			guide.fillAlpha = 0.2;
			guide.lineAlpha = 0
			valueAxis.addGuide(guide);
			chart.addValueAxis(valueAxis)

			graph = new AmCharts.AmGraph()
			graph.type = "line"
			graph.title = "red line"
			graph.valueField = "y"
			graph.lineAlpha = 1
			graph.lineColor = "#d1cf2a"
			graph.fillAlphas = 0.3
			chart.addGraph(graph)

			chartCursor = new AmCharts.ChartCursor()
			chartCursor.cursorPosition = "mouse"
			chartCursor.categoryBalloonDateFormat = "DD MMMM"
			chart.addChartCursor(chartCursor)
			
			chart.write('date-graph')	
			fixWatermark('date-graph')

		writeHourChart = (data) ->

			chartdata = []
			i = 0

			while i < data.x.length
				chartdata.push
					x: data.x[i]
					y: data.y[i]
				i++

			chart = new AmCharts.AmSerialChart()
			chart.pathToImages = "http://www.amcharts.com/lib/images/"
			chart.autoMarginOffset = 3
			chart.marginRight = 15
			chart.startDuration = 1;
			chart.plotAreaFillAlphas = 0.2;
			chart.angle = 30;
			chart.depth3D = 20;

			chart.dataProvider = chartdata
			chart.categoryField = "x"

			categoryAxis = chart.categoryAxis
			categoryAxis.gridAlpha = 0.07
			categoryAxis.gridPosition = "start";
			categoryAxis.axisColor = "#DADADA"
			categoryAxis.showFirstLabel = true
			categoryAxis.showLastLabel = true
			categoryAxis.startOnAxis = true

			valueAxis = new AmCharts.ValueAxis()
			valueAxis.stackType = "3d";
			valueAxis.gridAlpha = 0.07
			valueAxis.dashLength = 5;

			guide = new AmCharts.Guide();
			guide.value = 0;
			guide.toValue = 1000000;
			guide.fillColor = "#d7e5ee";
			guide.fillAlpha = 0.2;
			guide.lineAlpha = 0
			valueAxis.addGuide(guide);
			chart.addValueAxis(valueAxis)

			graph = new AmCharts.AmGraph();
			graph.type = "column";
			graph.valueField = "y"
			graph.lineAlpha = 0;
			graph.lineColor = "#1A87C8";
			graph.fillAlphas = 1;
			chart.addGraph(graph)
			graph.balloonText = "Errors: [[value]]";

			chartCursor = new AmCharts.ChartCursor()
			chartCursor.cursorPosition = "mouse"
			chartCursor.categoryBalloonDateFormat = "DD MMMM"
			chart.addChartCursor(chartCursor)
			chart.write('hour-graph')	
			fixWatermark('hour-graph')

		fixWatermark = (div) ->
			$watermark = $('div#' + div + ' svg g:last')
			$rect = $watermark.find 'rect'
			$rect.removeAttr "height"
			$rect.removeAttr "y"
			$text = $watermark.find 'tspan'
			$text.attr "y", "-1"
			$text.attr "x", "-8"

		clearErrors = () ->
			$('div#error-items').clear();
						
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
			e.preventDefault()
			renderReports()

		$issue.delegate '.what-if-reprocess', 'click', (e) ->
			e.preventDefault()
			$(this).closest('form').ajaxSubmit
				data:
					WhatIf: true
				success: (data) ->
					$('p#reprocess-result').empty()
					msg = $('<span/>').addClass('reprocess-what-if-msg').html(data)
					$('p#reprocess-result').append msg
				error: ->
					Errordite.Alert.show('An error has occured, please try again.')

		$issue.delegate 'ul#action-list a.action', 'click', (e) ->
			e.preventDefault()
			$this = $ this
			action = $this.data('action')
			switch action
				when "delete", "purge"
					Errordite.Confirm.show($this.data('confirmtext'), { okCallBack: () -> 
						$this.closest('form').submit();
						true
					}, cancelCallBack: () -> 
						false
					)
					true
				when "reprocess"
					$reprocess = $('div#reprocess-modal')
					$reprocess.modal()
					true
				when "comment"
					$modal = $('div#add-comment')
					$modal.modal()
					true

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
			
				
			

