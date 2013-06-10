
jQuery -> 

	$root = $('section#dashboard')

	if $root.length > 0	

		$root.delegate 'select#ShowMe', 'change', () -> 
			dashboard.update 'feed', true
			true

		class Dashboard
		
			constructor: () ->
				this.feedContainer = $ 'table#feed tbody'
			update: (mode, purge) ->
				$.ajax
					url: "/dashboard/update?mode=" + mode + "&showMe=" + $('select#ShowMe').val()
					success: (result) ->
						if result.success
							if purge
								dashboard.feedContainer.empty()

							if result.liveErrorFeed
								dashboard.renderErrors result.data.feed
							else
								dashboard.renderIssues result.data.feed

							dashboard.renderGraph result.data.errors
							dashboard.renderPieChart result.data.stats
						else
							dashboard.error()
					error: ->
						dashboard.error()
					dataType: "json"
					complete: ->
						console.log 'poll'
						setTimeout dashboard.update, 30000
				true
			renderIssues: (issues) ->
				if issues != null
					dashboard.feedContainer.empty()

					for i in issues
						dashboard.feedContainer.append i

					dashboard.feedContainer.fadeIn(750)
				true
			renderErrors: (errors) ->
				if errors != null
					for e in errors
						dashboard.feedContainer.prepend e
					dashboard.purgeItems()
				true
			purgeItems: ->
				count = dashboard.feedContainer.find('tr').length
				while count > 50
					dashboard.feedContainer.find('tr:last-child').remove()
					count = dashboard.feedContainer.find('tr').length
			showMostSignificantIssues: (date) ->
				window.Errordite.Spinner.disable()
				true
				$.ajax
					url: "/dashboard/issuebreakdown?dateFormat=" + date
					success: (result) ->
						if result.success
							modal = $root.find('div#issue-breakdown')
							dashboard.renderIssuePieChart result.data
							modal.modal();
						else
							dashboard.error()
					error: ->
						dashboard.error()
					dataType: "json"
					complete: ->
						window.Errordite.Spinner.enable()
				true
			renderGraph: (data) ->
				if data != null

					chartdata = []
					i = 0

					while i < data.x.length
						chartdata.push
							date: new Date(data.x[i])
							errors: data.y[i]
						i++

					chart = new AmCharts.AmSerialChart()
					chart.autoMarginOffset = 3
					chart.marginRight = 15
					chart.addListener "clickGraphItem", (event) ->
						if event.item.dataContext.errors > 0
							dashboard.showMostSignificantIssues event.item.dataContext.date
					chart.dataProvider = chartdata
					chart.categoryField = "date"
					chart.angle = 30;
					chart.depth3D = 20;

					categoryAxis = chart.categoryAxis
					categoryAxis.parseDates = true
					categoryAxis.equalSpacing = true
					categoryAxis.minPeriod = "DD"
					categoryAxis.gridAlpha = 0.07
					categoryAxis.axisColor = "#DADADA"
					categoryAxis.showFirstLabel = true
					categoryAxis.showLastLabel = true
					categoryAxis.startOnAxis = false

					valueAxis = new AmCharts.ValueAxis()
					valueAxis.stackType = "3d";
					valueAxis.gridAlpha = 0.07
					valueAxis.stackType = "3d";
					valueAxis.dashLength = 5;
					valueAxis.unit = "0";

					guide = new AmCharts.Guide();
					guide.value = 0;
					guide.toValue = 1000000;
					guide.fillColor = "#d7e5ee";
					guide.fillAlpha = 0.2;
					guide.lineAlpha = 0
					valueAxis.addGuide(guide);
					chart.addValueAxis(valueAxis)

					graph = new AmCharts.AmGraph()
					graph.type = "column";
					graph.valueField = "errors"
					graph.lineAlpha = 1
					graph.lineColor = "#1A87C8"
					graph.fillAlphas = 0.7
					graph.balloonText = "Errors received: [[value]]"
					chart.addGraph(graph)

					chartCursor = new AmCharts.ChartCursor()
					chartCursor.cursorPosition = "mouse"
					chartCursor.categoryBalloonDateFormat = "DD MMMM"
					chartCursor.zoomable = false
					chart.addChartCursor(chartCursor)
					chart.write("graph")
					fixWatermark('graph')
				true
			renderPieChart: (data) -> 
				if data != null
					chartdata = []
					chartdata.push
						status: "Unacknowledged"
						count:data.Unacknowledged
						url: "/issues?Status=Unacknowledged"
					chartdata.push
						status: "Acknowledged"
						count:data.Acknowledged
						url: "/issues?Status=Acknowledged"
					chartdata.push
						status: "FixReady"
						count:data.FixReady
						url: "/issues?Status=FixReady"
					chartdata.push
						status: "Solved"
						count:data.Solved
						url: "/issues?Status=Solved"
					chartdata.push
						status: "Ignored"
						count:data.Ignored
						url: "/issues?Status=Ignored"

					piechart = new AmCharts.AmPieChart();
					piechart.dataProvider = chartdata;
					piechart.titleField = "status";
					piechart.valueField = "count";
					piechart.labelsEnabled = false;
					piechart.urlField = "url"
					piechart.balloonText = "Click to view '[[title]]' issues: [[value]]";
					piechart.colors = ["#C2E0F2", "#92C7E7", "#95C0DF", "#729DB7", "#486C81"]
					piechart.startDuration = 0;

					legend = new AmCharts.AmLegend()
					legend.align = "right"
					legend.markerType = "circle"
					piechart.addLegend(legend)

					piechart.write("piechart")
					fixWatermark('piechart')
				true
			
			renderIssuePieChart: (data) -> 
				if data != null
					$table = $root.find('table#issues tbody')
					for issue in data
						$table.append('<tr><td class="graph-fill"><a href="/issue/' + issue.Id + '">' + issue.Name + ' (' + issue.Count + ')</a></td></tr>')

				true
			error: -> 
				console.log "error"
				true
			fixWatermark = (div) ->
				$watermark = $('div#' + div + ' svg g:last')
				$rect = $watermark.find 'rect'
				$rect.removeAttr "height"
				$rect.removeAttr "y"
				$text = $watermark.find 'tspan'
				$text.attr "y", "-1"
				$text.attr "x", "-8"

		dashboard = new Dashboard()
		dashboard.update 'graphs'
		true