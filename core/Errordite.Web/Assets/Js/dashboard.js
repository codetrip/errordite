(function() {
  jQuery(function() {
    var $root, Dashboard, dashboard;

    $root = $('section#dashboard');
    if ($root.length > 0) {
      $root.delegate('select#SortId', 'change', function() {
        dashboard.refreshIssues();
        return true;
      });
      Dashboard = (function() {
        function Dashboard() {
          this.issueContainer = $('table#issues tbody');
          this.graphSpinner = $root.find('div#graph-spinner');
          this.pieChartSpinner = $root.find('div#piechart-spinner');
          this.issuesSpinner = $root.find('div#issues-spinner');
          Errordite.Spinner.disable();
        }

        Dashboard.prototype.refreshIssues = function() {
          dashboard.issuesSpinner.show();
          $.ajax({
            url: "/dashboard/update?applicationId=" + $('input#ApplicationId').val() + '&sortId=' + $('select#SortId').val(),
            success: function(result) {
              var i, _i, _len, _ref;

              if (result.success) {
                dashboard.issueContainer.empty();
                dashboard.issueContainer.hide();
                _ref = result.data.issues;
                for (_i = 0, _len = _ref.length; _i < _len; _i++) {
                  i = _ref[_i];
                  dashboard.issueContainer.append(i);
                }
                dashboard.issueContainer.fadeIn(750);
                return dashboard.issuesSpinner.hide();
              } else {
                dashboard.error();
                return dashboard.issuesSpinner.hide();
              }
            },
            error: function() {
              dashboard.error();
              return dashboard.issuesSpinner.hide();
            },
            dataType: "json",
            complete: function() {
              return setTimeout(dashboard.refreshIssues, 30000);
            }
          });
          return true;
        };

        Dashboard.prototype.refreshGraph = function() {
          dashboard.graphSpinner.show();
          $.ajax({
            url: "/dashboard/getgraphdata?applicationId=" + $('input#ApplicationId').val(),
            success: function(data) {
              var $rect, $text, $watermark, categoryAxis, chart, chartCursor, chartdata, graph, guide, i, valueAxis;

              chart = new AmCharts.AmSerialChart();
              chart.pathToImages = "http://www.amcharts.com/lib/images/";
              chart.autoMarginOffset = 3;
              chart.marginRight = 15;
              chart.startEffect = "elastic";
              chart.startDuration = 0.5;
              chartdata = [];
              i = 0;
              while (i < data.x.length) {
                chartdata.push({
                  date: new Date(data.x[i]),
                  errors: data.y[i]
                });
                i++;
              }
              chart.dataProvider = chartdata;
              chart.categoryField = "date";
              categoryAxis = chart.categoryAxis;
              categoryAxis.parseDates = true;
              categoryAxis.equalSpacing = true;
              categoryAxis.minPeriod = "DD";
              categoryAxis.gridAlpha = 0.07;
              categoryAxis.axisColor = "#DADADA";
              categoryAxis.showFirstLabel = true;
              categoryAxis.showLastLabel = false;
              categoryAxis.startOnAxis = false;
              valueAxis = new AmCharts.ValueAxis();
              valueAxis.gridAlpha = 0.07;
              valueAxis.dashLength = 5;
              guide = new AmCharts.Guide();
              guide.value = 0;
              guide.toValue = 1000000;
              guide.fillColor = "#d7e5ee";
              guide.fillAlpha = 0.2;
              guide.lineAlpha = 0;
              valueAxis.addGuide(guide);
              chart.addValueAxis(valueAxis);
              graph = new AmCharts.AmGraph();
              graph.type = "line";
              graph.valueField = "errors";
              graph.lineAlpha = 1;
              graph.lineColor = "#d1cf2a";
              graph.fillAlphas = 0.3;
              graph.balloonText = "Errors received: [[value]]";
              chart.addGraph(graph);
              chartCursor = new AmCharts.ChartCursor();
              chartCursor.cursorPosition = "mouse";
              chartCursor.categoryBalloonDateFormat = "DD MMMM";
              chart.addChartCursor(chartCursor);
              chart.write("graph");
              $watermark = $('div#graph svg g:last');
              $rect = $watermark.find('rect');
              $rect.removeAttr("height");
              $rect.removeAttr("y");
              $text = $watermark.find('tspan');
              $text.attr("y", "-1");
              $text.attr("x", "-8");
              return dashboard.graphSpinner.hide();
            },
            error: function() {
              dashboard.error();
              return dashboard.graphSpinner.hide();
            },
            dataType: "json",
            complete: function() {
              return setTimeout(dashboard.refreshGraph, 30000);
            }
          });
          return true;
        };

        Dashboard.prototype.refreshPieChart = function(data) {
          dashboard.pieChartSpinner.show();
          $.ajax({
            url: "/dashboard/updatepiechart?applicationId=" + $('input#ApplicationId').val(),
            success: function(data) {
              var $rect, $text, $watermark, chartdata, legend, piechart;

              chartdata = [];
              chartdata.push({
                status: "Unacknowledged",
                count: data.data.Unacknowledged,
                url: "/issues?Status=Unacknowledged"
              });
              chartdata.push({
                status: "Acknowledged",
                count: data.data.Acknowledged,
                url: "/issues?Status=Acknowledged"
              });
              chartdata.push({
                status: "FixReady",
                count: data.data.FixReady,
                url: "/issues?Status=FixReady"
              });
              chartdata.push({
                status: "Solved",
                count: data.data.Solved,
                url: "/issues?Status=Solved"
              });
              chartdata.push({
                status: "Ignored",
                count: data.data.Ignored,
                url: "/issues?Status=Ignored"
              });
              piechart = new AmCharts.AmPieChart();
              piechart.dataProvider = chartdata;
              piechart.titleField = "status";
              piechart.valueField = "count";
              piechart.labelsEnabled = false;
              piechart.urlField = "url";
              piechart.sequencedAnimation = true;
              piechart.startRadius = "100%";
              piechart.startEffect = '>';
              piechart.balloonText = "Click to view '[[title]]' issues: [[value]]";
              piechart.colors = ["#C2E0F2", "#92C7E7", "#95C0DF", "#729DB7", "#486C81"];
              legend = new AmCharts.AmLegend();
              legend.align = "right";
              legend.markerType = "circle";
              piechart.addLegend(legend);
              piechart.write("piechart");
              $watermark = $('div#piechart svg g:last');
              $rect = $watermark.find('rect');
              $rect.removeAttr("height");
              $rect.removeAttr("y");
              $text = $watermark.find('tspan');
              $text.attr("y", "-1");
              $text.attr("x", "-8");
              return dashboard.pieChartSpinner.hide();
            },
            error: function() {
              dashboard.error();
              return dashboard.pieChartSpinner.hide();
            },
            dataType: "json",
            complete: function() {
              return setTimeout(dashboard.refreshPieChart, 30000);
            }
          });
          return true;
        };

        Dashboard.prototype.error = function() {
          console.log("error");
          return true;
        };

        return Dashboard;

      })();
      dashboard = new Dashboard();
      dashboard.refreshGraph();
      dashboard.refreshPieChart();
      setTimeout(dashboard.refreshGraph, 30000);
      setTimeout(dashboard.refreshIssues, 30000);
      setTimeout(dashboard.refreshPieChart, 30000);
      return true;
    }
  });

}).call(this);
