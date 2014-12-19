(function() {
  jQuery(function() {
    var $issue, clearErrors, fixWatermark, loadTabData, paging, renderHistory, renderReports, writeDateChart, writeHourChart;

    $issue = $('section#issue');
    if ($issue.length > 0) {
      paging = new window.Paging('/issue/errors?Id=' + $issue.find('#IssueId').val() + '&');
      paging.init();
      loadTabData = function($tab) {
        if (!$tab.data('loaded')) {
          if ($tab.data("val") === "reports") {
            renderReports();
          } else if ($tab.data("val") === "history") {
            renderHistory();
          }
          return $tab.data('loaded', true);
        }
      };
      renderReports = function() {
        $('div#date-graph').empty();
        $('div#hour-graph').empty();
        return $.get("/issue/getreportdata?issueId=" + $issue.find('input#IssueId').val() + '&dateRange=' + $issue.find('input#DateRange').val() + '&token=' + $issue.find('input#Token').val(), function(d) {
          writeDateChart(d.ByDate);
          return writeHourChart(d.ByHour);
        });
      };
      writeDateChart = function(data) {
        var categoryAxis, chart, chartCursor, chartdata, graph, guide, i, valueAxis;

        chartdata = [];
        i = 0;
        while (i < data.x.length) {
          chartdata.push({
            x: data.x[i],
            y: data.y[i]
          });
          i++;
        }
        chart = new AmCharts.AmSerialChart();
        chart.pathToImages = "http://www.amcharts.com/lib/images/";
        chart.autoMarginOffset = 3;
        chart.marginRight = 15;
        chart.dataProvider = chartdata;
        chart.categoryField = "x";
        categoryAxis = chart.categoryAxis;
        categoryAxis.gridAlpha = 0.07;
        categoryAxis.axisColor = "#DADADA";
        categoryAxis.showFirstLabel = true;
        categoryAxis.showLastLabel = true;
        categoryAxis.startOnAxis = true;
        categoryAxis.parseDates = true;
        categoryAxis.equalSpacing = true;
        categoryAxis.minPeriod = "DD";
        valueAxis = new AmCharts.ValueAxis();
        valueAxis.stackType = "3d";
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
        graph.valueField = "y";
        graph.lineAlpha = 1;
        graph.lineColor = "#d1cf2a";
        graph.fillAlphas = 0.3;
        chart.addGraph(graph);
        chartCursor = new AmCharts.ChartCursor();
        chartCursor.cursorPosition = "mouse";
        chartCursor.categoryBalloonDateFormat = "DD MMMM";
        chart.addChartCursor(chartCursor);
        chart.write('date-graph');
        return fixWatermark('date-graph');
      };
      writeHourChart = function(data) {
        var categoryAxis, chart, chartCursor, chartdata, graph, guide, i, valueAxis;

        chartdata = [];
        i = 0;
        while (i < data.x.length) {
          chartdata.push({
            x: data.x[i],
            y: data.y[i]
          });
          i++;
        }
        chart = new AmCharts.AmSerialChart();
        chart.pathToImages = "http://www.amcharts.com/lib/images/";
        chart.autoMarginOffset = 3;
        chart.marginRight = 15;
        chart.startDuration = 1;
        chart.plotAreaFillAlphas = 0.2;
        chart.angle = 30;
        chart.depth3D = 20;
        chart.dataProvider = chartdata;
        chart.categoryField = "x";
        categoryAxis = chart.categoryAxis;
        categoryAxis.gridAlpha = 0.07;
        categoryAxis.gridPosition = "start";
        categoryAxis.axisColor = "#DADADA";
        categoryAxis.showFirstLabel = true;
        categoryAxis.showLastLabel = true;
        categoryAxis.startOnAxis = true;
        valueAxis = new AmCharts.ValueAxis();
        valueAxis.stackType = "3d";
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
        graph.type = "column";
        graph.valueField = "y";
        graph.lineAlpha = 0;
        graph.lineColor = "#1A87C8";
        graph.fillAlphas = 1;
        chart.addGraph(graph);
        graph.balloonText = "Errors: [[value]]";
        chartCursor = new AmCharts.ChartCursor();
        chartCursor.cursorPosition = "mouse";
        chartCursor.categoryBalloonDateFormat = "DD MMMM";
        chart.addChartCursor(chartCursor);
        chart.write('hour-graph');
        return fixWatermark('hour-graph');
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
      clearErrors = function() {
        return $('div#error-items').clear();
      };
      renderHistory = function() {
        var $node, url;

        $node = $issue.find('table.history tbody');
        url = '/issue/history?IssueId=' + $issue.find('#IssueId').val();
        return $.get(url, function(data) {
          $node.append(data.data);
          return $('div.content').animate({
            scrollTop: 0
          }, 'slow');
        });
      };
      loadTabData($('ul#issue-tabs li.active a.tablink'));
      $issue.delegate('form#reportform', 'submit', function(e) {
        e.preventDefault();
        return renderReports();
      });
      $issue.delegate('.what-if-reprocess', 'click', function(e) {
        e.preventDefault();
        return $(this).closest('form').ajaxSubmit({
          data: {
            WhatIf: true
          },
          success: function(data) {
            var msg;

            $('p#reprocess-result').empty();
            msg = $('<span/>').addClass('reprocess-what-if-msg').html(data);
            return $('p#reprocess-result').append(msg);
          },
          error: function() {
            return Errordite.Alert.show('An error has occured, please try again.');
          }
        });
      });
      $issue.delegate('ul#action-list a.action', 'click', function(e) {
        var $modal, $reprocess, $this, action;

        e.preventDefault();
        $this = $(this);
        action = $this.data('action');
        switch (action) {
          case "delete":
          case "purge":
            Errordite.Confirm.show($this.data('confirmtext'), {
              okCallBack: function() {
                $this.closest('form').submit();
                return true;
              }
            }, {
              cancelCallBack: function() {
                return false;
              }
            });
            return true;
          case "reprocess":
            $reprocess = $('div#reprocess-modal');
            $reprocess.modal();
            return true;
          case "comment":
            $modal = $('div#add-comment');
            $modal.modal();
            return true;
        }
      });
      $issue.delegate('select#Status', 'change', function() {
        var $this;

        $this = $(this);
        if ($this.val() === 'Ignored') {
          return $issue.find('li.inline').removeClass('hidden');
        } else {
          return $issue.find('li.inline').addClass('hidden');
        }
      });
      if ($issue.find('select#Status').val() === 'Ignored') {
        $issue.find('li.inline').removeClass('hidden');
      }
      return $('#issue-tabs .tablink').bind('shown', function(e) {
        return loadTabData($(e.currentTarget));
      });
    }
  });

}).call(this);
