(function() {

  jQuery(function() {
    var $root, Dashboard, dashboard;
    $root = $('section#dashboard');
    if ($root.length > 0) {
      $root.delegate('select#ShowMe', 'change', function() {
        dashboard.update('feed', true);
        return true;
      });
      Dashboard = (function() {
        var fixWatermark;

        function Dashboard() {
          this.feedContainer = $('table#feed tbody');
        }

        Dashboard.prototype.update = function(mode, purge) {
          $.ajax({
            url: "/dashboard/update?mode=" + mode + "&showMe=" + $('select#ShowMe').val(),
            success: function(result) {
              if (result.success) {
                if (purge) {
                  dashboard.feedContainer.empty();
                }
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
            for (_i = 0, _len = errors.length; _i < _len; _i++) {
              e = errors[_i];
              dashboard.feedContainer.prepend(e);
            }
            dashboard.purgeItems();
          }
          return true;
        };

        Dashboard.prototype.purgeItems = function() {
          var count, _results;
          count = dashboard.feedContainer.find('tr').length;
          _results = [];
          while (count > 50) {
            dashboard.feedContainer.find('tr:last-child').remove();
            _results.push(count = dashboard.feedContainer.find('tr').length);
          }
          return _results;
        };

        Dashboard.prototype.showMostSignificantIssues = function(date) {
          window.Errordite.Spinner.disable();
          true;
          $.ajax({
            url: "/dashboard/issuebreakdown?dateFormat=" + date,
            success: function(result) {
              var modal;
              if (result.success) {
                modal = $root.find('div#issue-breakdown');
                dashboard.renderIssuePieChart(result.data);
                return modal.modal();
              } else {
                return dashboard.error();
              }
            },
            error: function() {
              return dashboard.error();
            },
            dataType: "json",
            complete: function() {
              return window.Errordite.Spinner.enable();
            }
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
                return dashboard.showMostSignificantIssues(event.item.dataContext.date);
              }
            });
            chart.dataProvider = chartdata;
            chart.categoryField = "date";
            chart.angle = 30;
            chart.depth3D = 20;
            categoryAxis = chart.categoryAxis;
            categoryAxis.parseDates = true;
            categoryAxis.equalSpacing = true;
            categoryAxis.minPeriod = "DD";
            categoryAxis.gridAlpha = 0.07;
            categoryAxis.axisColor = "#DADADA";
            categoryAxis.showFirstLabel = true;
            categoryAxis.showLastLabel = true;
            categoryAxis.startOnAxis = false;
            valueAxis = new AmCharts.ValueAxis();
            valueAxis.stackType = "3d";
            valueAxis.gridAlpha = 0.07;
            valueAxis.stackType = "3d";
            valueAxis.dashLength = 5;
            valueAxis.unit = "0";
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
            fixWatermark('graph');
          }
          return true;
        };

        Dashboard.prototype.renderPieChart = function(data) {
          var chartdata, legend, piechart;
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
            piechart = new AmCharts.AmPieChart();
            piechart.dataProvider = chartdata;
            piechart.titleField = "status";
            piechart.valueField = "count";
            piechart.labelsEnabled = false;
            piechart.urlField = "url";
            piechart.balloonText = "Click to view '[[title]]' issues: [[value]]";
            piechart.colors = ["#C2E0F2", "#92C7E7", "#95C0DF", "#729DB7", "#486C81"];
            piechart.startDuration = 0;
            legend = new AmCharts.AmLegend();
            legend.align = "right";
            legend.markerType = "circle";
            piechart.addLegend(legend);
            piechart.write("piechart");
            fixWatermark('piechart');
          }
          return true;
        };

        Dashboard.prototype.renderIssuePieChart = function(data) {
          var $table, issue, _i, _len;
          if (data !== null) {
            $table = $root.find('table#issues tbody');
            for (_i = 0, _len = data.length; _i < _len; _i++) {
              issue = data[_i];
              $table.append('<tr><td class="graph-fill"><a href="/issue/' + issue.Id + '">' + issue.Name + ' (' + issue.Count + ')</a></td></tr>');
            }
          }
          return true;
        };

        Dashboard.prototype.error = function() {
          console.log("error");
          return true;
        };

        fixWatermark = function(div) {
          var $rect, $text, $watermark;
          $watermark = $('div#' + div + ' svg g:last');
          $rect = $watermark.find('rect');
          $rect.removeAttr("height");
          $rect.removeAttr("y");
          $text = $watermark.find('tspan');
          $text.attr("y", "-1");
          return $text.attr("x", "-8");
        };

        return Dashboard;

      })();
      dashboard = new Dashboard();
      dashboard.update('graphs');
      return true;
    }
  });

}).call(this);
