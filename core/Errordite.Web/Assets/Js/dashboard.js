(function() {

  jQuery(function() {
    var $root, Dashboard, dashboard;
    $root = $('section#dashboard');
    if ($root.length > 0) {
      window.Errordite.Spinner.disable();
      $root.delegate('select#ShowMe', 'change', function() {
        dashboard.update('feed');
        return true;
      });
      $root.delegate('select#PageSize', 'change', function() {
        dashboard.update('feed');
        return true;
      });
      $root.delegate('button#close-modal', 'click', function() {
        dashboard.pollingEnabled = true;
        dashboard.update;
        return true;
      });
      Dashboard = (function() {
        var fixWatermark;

        function Dashboard() {
          this.feedContainer = $('table#feed tbody');
          this.pollingEnabled = true;
        }

        Dashboard.prototype.update = function(mode) {
          if (!dashboard.pollingEnabled) {
            return true;
          }
          $.ajax({
            url: "/dashboard/update?mode=" + mode + "&showMe=" + $('select#ShowMe').val() + '&pageSize=' + $('select#PageSize').val(),
            success: function(result) {
              if (result.success) {
                if (result.liveErrorFeed) {
                  dashboard.renderErrors(result.data.feed);
                } else {
                  dashboard.renderIssues(result.data.feed);
                }
                dashboard.renderGraph(result.data.errors);
                return dashboard.renderPieChart(result.data.stats);
              } else {
                return dashboard.error();
              }
            },
            error: function() {
              return dashboard.error();
            },
            dataType: "json",
            complete: function() {
              console.log('poll');
              return setTimeout(dashboard.update, 30000);
            }
          });
          return true;
        };

        Dashboard.prototype.renderIssues = function(issues) {
          var i, _i, _len;
          if (issues !== null) {
            dashboard.feedContainer.empty();
            for (_i = 0, _len = issues.length; _i < _len; _i++) {
              i = issues[_i];
              dashboard.feedContainer.append(i);
            }
            dashboard.feedContainer.fadeIn(750);
          }
          return true;
        };

        Dashboard.prototype.renderErrors = function(errors) {
          var e, _i, _len;
          if (errors !== null) {
            dashboard.feedContainer.empty();
            for (_i = 0, _len = errors.length; _i < _len; _i++) {
              e = errors[_i];
              dashboard.feedContainer.prepend(e);
            }
          }
          return true;
        };

        Dashboard.prototype.showIssueBreakdown = function(date) {
          dashboard.pollingEnabled = false;
          $.ajax({
            url: "/dashboard/issuebreakdown?dateFormat=" + date,
            success: function(result) {
              var modal;
              if (result.success) {
                modal = $root.find('div#issue-breakdown');
                dashboard.renderIssueBreakdown(result.data, date);
                modal.modal();
                return modal.center();
              } else {
                return dashboard.error();
              }
            },
            error: function() {
              return dashboard.error();
            },
            dataType: "json"
          });
          return true;
        };

        Dashboard.prototype.renderGraph = function(data) {
          var categoryAxis, chart, chartCursor, chartdata, graph, guide, i, valueAxis;
          if (data !== null) {
            chartdata = [];
            i = 0;
            while (i < data.x.length) {
              chartdata.push({
                date: new Date(data.x[i]),
                errors: data.y[i]
              });
              i++;
            }
            chart = new AmCharts.AmSerialChart();
            chart.autoMarginOffset = 3;
            chart.marginRight = 15;
            chart.addListener("clickGraphItem", function(event) {
              if (event.item.dataContext.errors > 0) {
                return dashboard.showIssueBreakdown(event.item.dataContext.date);
              }
            });
            chart.dataProvider = chartdata;
            chart.categoryField = "date";
            chart.angle = 30;
            chart.depth3D = 20;
            chart.startDuration = 1;
            chart.plotAreaFillAlphas = 0.2;
            categoryAxis = chart.categoryAxis;
            categoryAxis.parseDates = true;
            categoryAxis.minPeriod = "DD";
            categoryAxis.gridAlpha = 0.07;
            categoryAxis.axisColor = "#d7e5ee";
            categoryAxis.showFirstLabel = true;
            categoryAxis.showLastLabel = true;
            valueAxis = new AmCharts.ValueAxis();
            valueAxis.stackType = "3d";
            valueAxis.gridAlpha = 0.07;
            valueAxis.stackType = "3d";
            valueAxis.dashLength = 5;
            valueAxis.axisColor = "#d7e5ee";
            guide = new AmCharts.Guide();
            guide.value = 0;
            guide.toValue = 1000000;
            guide.fillColor = "#d7e5ee";
            guide.fillAlpha = 0.2;
            guide.lineAlpha = 0;
            valueAxis.addGuide(guide);
            chart.addValueAxis(valueAxis);
            graph = new AmCharts.AmGraph();
            graph.type = "column";
            graph.valueField = "errors";
            graph.lineAlpha = 1;
            graph.lineColor = "#1A87C8";
            graph.fillAlphas = 0.7;
            graph.balloonText = "Errors received: [[value]]";
            chart.addGraph(graph);
            chartCursor = new AmCharts.ChartCursor();
            chartCursor.cursorPosition = "mouse";
            chartCursor.categoryBalloonDateFormat = "DD MMMM";
            chartCursor.zoomable = false;
            chart.addChartCursor(chartCursor);
            chart.write("graph");
            fixWatermark('graph', "-8");
          }
          return true;
        };

        Dashboard.prototype.renderPieChart = function(data) {
          var categoryAxis, chart, chartCursor, chartdata, graph, guide, valueAxis;
          if (data !== null) {
            chartdata = [];
            chartdata.push({
              status: "Unacknowledged",
              count: data.Unacknowledged,
              url: "/issues?Status=Unacknowledged"
            });
            chartdata.push({
              status: "Acknowledged",
              count: data.Acknowledged,
              url: "/issues?Status=Acknowledged"
            });
            chartdata.push({
              status: "FixReady",
              count: data.FixReady,
              url: "/issues?Status=FixReady"
            });
            chartdata.push({
              status: "Solved",
              count: data.Solved,
              url: "/issues?Status=Solved"
            });
            chartdata.push({
              status: "Ignored",
              count: data.Ignored,
              url: "/issues?Status=Ignored"
            });
            chart = new AmCharts.AmSerialChart();
            chart.autoMarginOffset = 3;
            chart.marginRight = 15;
            chart.addListener("clickGraphItem", function(event) {
              return window.location.href = event.item.dataContext.url;
            });
            chart.dataProvider = chartdata;
            chart.categoryField = "status";
            chart.angle = 30;
            chart.depth3D = 20;
            chart.startDuration = 1;
            chart.plotAreaFillAlphas = 0.2;
            categoryAxis = chart.categoryAxis;
            categoryAxis.showFirstLabel = true;
            categoryAxis.showLastLabel = true;
            categoryAxis.startOnAxis = false;
            categoryAxis.labelRotation = 45;
            categoryAxis.axisColor = "#d7e5ee";
            valueAxis = new AmCharts.ValueAxis();
            valueAxis.stackType = "3d";
            valueAxis.stackType = "3d";
            valueAxis.dashLength = 3;
            valueAxis.axisColor = "#d7e5ee";
            guide = new AmCharts.Guide();
            guide.value = 0;
            guide.toValue = 1000000;
            guide.fillColor = "#d7e5ee";
            guide.fillAlpha = 0.2;
            guide.lineAlpha = 0;
            valueAxis.addGuide(guide);
            chart.addValueAxis(valueAxis);
            graph = new AmCharts.AmGraph();
            graph.type = "column";
            graph.valueField = "count";
            graph.lineAlpha = 1;
            graph.lineColor = "#A9A9A8";
            graph.fillAlphas = 0.7;
            graph.balloonText = "[[category]]: [[value]]";
            chart.addGraph(graph);
            chartCursor = new AmCharts.ChartCursor();
            chartCursor.cursorPosition = "mouse";
            chartCursor.zoomable = false;
            chart.addChartCursor(chartCursor);
            chart.write("piechart");
            fixWatermark('piechart', "0");
          }
          return true;
        };

        Dashboard.prototype.renderIssueBreakdown = function(data, date) {
          var $fill, $table, i, issue, totalErrors, _i, _j, _len, _len1;
          if (data !== null) {
            $table = $root.find('table#issues tbody');
            $table.empty();
            totalErrors = 0;
            for (_i = 0, _len = data.length; _i < _len; _i++) {
              i = data[_i];
              totalErrors += i.Count;
            }
            for (_j = 0, _len1 = data.length; _j < _len1; _j++) {
              issue = data[_j];
              $table.append('\
							<tr>\
								<td>\
									<div class="graph-col">\
										<div class="graph-fill"></div>\
										<div class="graph-count">' + issue.Count + ' <span>-</span></div>\
										<div class="graph-text">\
											<a href="/issue/' + issue.Id + '">' + issue.Name.substring(0, 100) + '</a>\
										</div>\
									</div>\
								</td>\
							</tr>');
              $fill = $table.find('tr:last td div.graph-fill');
              $fill.animate({
                width: (((issue.Count / totalErrors) * 100) * 7) + 'px'
              }, 'slow');
            }
            $root.find('div#issue-breakdown div.modal-header h4 span').text(date.toString('dddd, MMMM dd yyyy'));
          }
          return true;
        };

        Dashboard.prototype.error = function() {
          console.log("error");
          return true;
        };

        fixWatermark = function(div, x) {
          var $rect, $text, $watermark;
          $watermark = $('div#' + div + ' svg g:last');
          $rect = $watermark.find('rect');
          $rect.removeAttr("height");
          $rect.removeAttr("y");
          $text = $watermark.find('tspan');
          $text.attr("y", "-1");
          return $text.attr("x", x);
        };

        return Dashboard;

      })();
      dashboard = new Dashboard();
      dashboard.update('graphs');
      return true;
    }
  });

}).call(this);
