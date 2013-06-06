(function() {
  jQuery(function() {
    var $root, Dashboard, dashboard;

    $root = $('section#dashboard');
    if ($root.length > 0) {
      Dashboard = (function() {
        function Dashboard() {
          this.issueContainer = $('div#issues');
          this.showItems();
          Errordite.Spinner.disable();
        }

        Dashboard.prototype.refreshIssues = function() {
          dashboard.pollCount++;
          $.ajax({
            url: "/dashboard/update?applicationId=" + $('input#ApplicationId').val(),
            success: function(result) {
              console.log("success");
              if (result.success) {
                return dashboard.bind(result.data);
              } else {
                return dashboard.error();
              }
            },
            error: function() {
              return dashboard.error();
            },
            dataType: "json",
            complete: function() {
              return setTimeout(dashboard.refreshIssues, 15000);
            }
          });
          return true;
        };

        Dashboard.prototype.refreshGraph = function() {
          $.ajax({
            url: "/dashboard/getgraphdata?applicationId=" + $('input#ApplicationId').val(),
            success: function(data) {
              var $rect, $text, $watermark, categoryAxis, chart, chartCursor, chartdata, graph, guide, i, valueAxis;

              chart = new AmCharts.AmSerialChart();
              chart.pathToImages = "http://www.amcharts.com/lib/images/";
              chart.autoMarginOffset = 3;
              chart.marginRight = 15;
              chartdata = [];
              i = 0;
              while (i < data.x.length) {
                console.log(new Date(data.x[i]));
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
              graph.title = "red line";
              graph.valueField = "errors";
              graph.lineAlpha = 1;
              graph.lineColor = "#d1cf2a";
              graph.fillAlphas = 0.3;
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
              return $text.attr("x", "-8");
            },
            error: function() {
              return dashboard.error();
            },
            dataType: "json"
          });
          return true;
        };

        Dashboard.prototype.bind = function(data) {
          var i, _i, _len, _ref;

          console.log("binding");
          _ref = data.issues;
          for (_i = 0, _len = _ref.length; _i < _len; _i++) {
            i = _ref[_i];
            dashboard.issueContainer.prepend(i);
          }
          dashboard.showItems();
          return true;
        };

        Dashboard.prototype.error = function() {
          console.log("error");
          return true;
        };

        Dashboard.prototype.showItems = function() {
          return this.issueContainer.find('div.boxed-item:hidden').show('slow');
        };

        return Dashboard;

      })();
      dashboard = new Dashboard();
      dashboard.refreshGraph();
      setTimeout(dashboard.refreshGraph, 30000);
      setTimeout(dashboard.refreshIssues, 15000);
      return true;
    }
  });

}).call(this);
