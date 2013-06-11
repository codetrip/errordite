
jQuery -> 

	$root = $('section#dashboard')

	if $root.length > 0	
		window.Errordite.Spinner.disable()
		$root.delegate 'select#ShowMe', 'change', () -> 
			dashboard.update 'feed', true
			true
		$root.delegate 'button#close-modal', 'click', () -> 
			dashboard.pollingEnabled = true
			dashboard.update
			true

		class Dashboard
		
			constructor: () ->
				this.feedContainer = $ 'table#feed tbody'
				this.pollingEnabled = true;
			update: (mode, purge) ->
				if not dashboard.pollingEnabled
					return true

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
						setTimeout dashboard.update, 5000
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
			showIssueBreakdown: (date) ->
				dashboard.pollingEnabled = false;
				$.ajax
					url: "/dashboard/issuebreakdown?dateFormat=" + date
					success: (result) ->
						if result.success
							modal = $root.find('div#issue-breakdown')
							dashboard.renderIssueBreakdown result.data, date
							modal.modal()
							modal.center()
						else
							dashboard.error()
					error: ->
						dashboard.error()
					dataType: "json"
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
							dashboard.showIssueBreakdown event.item.dataContext.date
						
#					chart.addListener "rollOverGraphItem", (event) ->
#						document.body.style.cursor="pointer";
#
#					chart.addListener "rollOutGraphItem", (event) ->
#						document.body.style.cursor="default";

					chart.dataProvider = chartdata
					chart.categoryField = "date"
					chart.angle = 30;
					chart.depth3D = 20;

					categoryAxis = chart.categoryAxis
					categoryAxis.parseDates = true
					categoryAxis.minPeriod = "DD"
					categoryAxis.gridAlpha = 0.07
					categoryAxis.axisColor = "#DADADA"
					categoryAxis.showFirstLabel = true
					categoryAxis.showLastLabel = true

					valueAxis = new AmCharts.ValueAxis()
					valueAxis.stackType = "3d";
					valueAxis.gridAlpha = 0.07
					valueAxis.stackType = "3d";
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
					fixWatermark('graph', "-8")
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

					
					chart = new AmCharts.AmSerialChart()
					chart.autoMarginOffset = 3
					chart.marginRight = 15
					chart.addListener "clickGraphItem", (event) ->
						window.location.href = event.item.dataContext.url;
					chart.dataProvider = chartdata
					chart.categoryField = "status"
					chart.angle = 30;
					chart.depth3D = 20;

					categoryAxis = chart.categoryAxis
					categoryAxis.showFirstLabel = true
					categoryAxis.showLastLabel = true
					categoryAxis.startOnAxis = false
					categoryAxis.labelRotation = 45;

					valueAxis = new AmCharts.ValueAxis()
					valueAxis.stackType = "3d";
					valueAxis.stackType = "3d";
					valueAxis.dashLength = 3;

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
					graph.valueField = "count"
					graph.lineAlpha = 1
					graph.lineColor = "#A9A9A8"
					graph.fillAlphas = 0.7
					graph.balloonText = "[[category]]: [[value]]"
					chart.addGraph(graph)

					chartCursor = new AmCharts.ChartCursor()
					chartCursor.cursorPosition = "mouse"
					chartCursor.zoomable = false
					chart.addChartCursor(chartCursor)
					chart.write("piechart")
					fixWatermark('piechart', "0")
				true
			
			renderIssueBreakdown: (data, date) -> 
				if data != null
					$table = $root.find('table#issues tbody')
					$table.empty()
					totalErrors = 0
					for i in data
						totalErrors += i.Count

					for issue in data
						$table.append('
							<tr>
								<td>
									<div class="graph-col">
										<div class="graph-fill"></div>
										<div class="graph-count">' + issue.Count + ' <span>-</span></div>
										<div class="graph-text">
											<a href="/issue/' + issue.Id + '">' + issue.Name.substring(0, 100) + '</a>
										</div>
									</div>
								</td>
							</tr>')
						$fill = $table.find('tr:last td div.graph-fill')
						$fill.animate({ width: (((issue.Count / totalErrors) * 100)  * 7) + 'px' }, 'slow');

					$root.find('div#issue-breakdown div.modal-header h4 span').text(date.toString('dddd, MMMM dd yyyy'))
				true
			error: -> 
				console.log "error"
				true
			fixWatermark = (div, x) ->
				$watermark = $('div#' + div + ' svg g:last')
				$rect = $watermark.find 'rect'
				$rect.removeAttr "height"
				$rect.removeAttr "y"
				$text = $watermark.find 'tspan'
				$text.attr "y", "-1"
				$text.attr "x", x

		dashboard = new Dashboard()
		dashboard.update 'graphs'
		true